using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;

public abstract class BaseGroupController : MonoBehaviour, IGroupController
{
	private const float RandomOffsetRange = 100f;
	private const float AngleOffsetMultiplier = 0.1f;
	private const float FullCircleDegrees = 360f;
	private const float DegToRad = Mathf.Deg2Rad;
	private const float NoiseRangeMultiplier = 2f;
	private const float MaxGroupAngleOffset = 90f;
	private const float NoiseOffset = 1f;
	private const float PhantomTimeOffset = 50f;
	private const float BaseMultiplier = 1f;
	private const float MinDangerMultiplier = 1f;
	private const float MinSeparationDistance = 1.0f;
	private const float SeparationStrengthMultiplier = 1.0f;
	private const float MinInfluenceStrength = 0.1f;

	private const float LightBlueColorR = 0.5f;
	private const float LightBlueColorG = 0.8f;
	private const float LightBlueColorB = 1f;
	private const float LightBlueColorA = 0.3f;
	private const float SquareSize = 0.15f;
	private const float SphereSize = 0.02f;

	[Header("Swarm Zone Settings")]
	[SerializeField, MinValue(0.5f)] protected float _optimalDistance = 2.5f;
	[SerializeField, MinValue(0.1f)] protected float _targetZoneRadius = 0.8f;
	[SerializeField, MinValue(0.1f)] protected float _dangerZoneRadius = 1.2f;
	[SerializeField, MinValue(0.5f)] protected float _influenceRadius = 4.0f;

	[Header("Movement Settings")]
	[SerializeField, Range(0.1f, 3.0f)] protected float _targetWobbleSpeed = 1.5f;
	[SerializeField, Range(0.1f, 2.0f)] protected float _targetWobbleAmount = 0.6f;
	[SerializeField, Range(1.0f, 5.0f)] protected float _dangerZoneMultiplier = 2.5f;
	[Space]
	[SerializeField, Range(0.1f, 10.0f)] protected float _baseInfluenceStrength = 3f;
	[SerializeField, Range(1.0f, 10.0f)] protected float _distanceCompensationMultiplier = 3f;
	[SerializeField, Range(0.1f, 10.0f)] protected float _separationForce = 4f;
	[Space]
	[SerializeField, Range(0.1f, 5.0f)] protected float _smoothFollowSpeed = 2f;
	[SerializeField, Range(0.1f, 2.0f)] protected float _maxFollowDistance = 1.5f;
	[SerializeField, Range(0.01f, 0.5f)] protected float _minMovementThreshold = 0.05f;

	[Header("Components")]
	[SerializeField, Required] private MonoBehaviour _followerLogic;

	[Header("Debug")]
	[SerializeField] private bool _showZonesInEditor = true;

	[Header("Debug Info")]
	[SerializeField, ReadOnly] private bool _debugIsGroupLeader = false;
	[SerializeField, ReadOnly] private int _debugGroupMembersCount = 0;
	[SerializeField, ReadOnly] private int _debugGroupId = -1;

	protected IFollower _follower;
	protected List<IGroupController> _groupMembers = new List<IGroupController>();
	protected bool _isGroupLeader = false;
	protected int _groupId = -1;

	private Dictionary<IGroupController, float> _memberOffsets = new Dictionary<IGroupController, float>();
	private Dictionary<IGroupController, Vector2> _memberFollowPositions = new Dictionary<IGroupController, Vector2>();

	private float _groupBaseAngleOffset;

	public bool IsGroupLeader => _isGroupLeader;
	public List<IGroupController> GroupMembers => _groupMembers;
	public int GroupId => _groupId;

	protected virtual void Awake()
	{
		_follower = GetComponent<IFollower>();
	}

	protected virtual void OnValidate()
	{
		if (_followerLogic != null)
		{
			if (_followerLogic is IFollower)
			{
				_follower = _followerLogic.GetComponent<IFollower>();
			}
		}
		else
		{
			_followerLogic = null;
			Debug.LogError($"[{nameof(BaseGroupController)}] FollowerLogic is not a {nameof(IFollower)} on {gameObject.name}!");
		}

		if (_isGroupLeader && _groupMembers.Count > 0)
		{
			ReinitializeSwarmSystem();
		}
	}

	protected virtual void OnEnable()
	{
		if (TryGetComponent<PooledEnemy>(out var pooledEnemy))
		{
			pooledEnemy.ReturnedToPool += OnReturnedToPool;
		}
	}

	protected virtual void OnDisable()
	{
		if (TryGetComponent<PooledEnemy>(out var pooledEnemy))
		{
			pooledEnemy.ReturnedToPool -= OnReturnedToPool;
		}

		ClearGroup();
	}

	protected virtual void FixedUpdate()
	{
		UpdateDebugInfo();

		if (_isGroupLeader && _groupMembers.Count > 0)
		{
			ApplyGroupBehavior();
		}
	}

	private void UpdateDebugInfo()
	{
		_debugIsGroupLeader = _isGroupLeader;
		_debugGroupMembersCount = _groupMembers.Count;
		_debugGroupId = _groupId;
	}

	public virtual void InitializeGroup(int groupId, bool isLeader)
	{
		if (groupId <= 0)
		{
			Debug.LogWarning($"[BaseGroupController] InitializeGroup: Invalid groupId {groupId}");
			return;
		}

		var group = GroupRegister.GetGroup(groupId);
		if (group == null)
		{
			Debug.LogError($"[BaseGroupController] InitializeGroup: Group {groupId} not found in register");
			return;
		}

		_groupId = groupId;

		if (!isLeader)
		{
			var leader = group.Keys.First();
			var members = group[leader];

			if (!members.Contains(this))
			{
				members.Add(this);
				group[leader] = members;
				GroupRegister.ReinitializeGroupMembers(groupId);
			}

			CheckAndActivateGroupState();
			return;
		}

		var leaderMembers = group[this];

		_groupMembers.Clear();
		_memberOffsets.Clear();
		_memberFollowPositions.Clear();
		_groupBaseAngleOffset = Random.Range(0f, MaxGroupAngleOffset);

		foreach (var member in leaderMembers)
		{
			if (member != null)
			{
				_groupMembers.Add(member);
			}
		}
		_isGroupLeader = true;

		foreach (var member in _groupMembers)
		{
			if (member != null)
			{
				// Включаем контроль только если член группы не занят (не в knockback)
				if (member.CanControlled())
				{
					EnableMemberControl(member);
				}
				DistributeGroupIdToMember(member, groupId);
			}
		}

		InitializeSwarmSystem();
		CleanupInactiveMembers();
	}

	public virtual void InitializeSwarm()
	{
		if (_groupId <= 0 || !_isGroupLeader)
		{
			Debug.LogWarning($"[BaseGroupController] InitializeSwarm: GroupId={_groupId}, IsGroupLeader={_isGroupLeader} - cannot initialize swarm");
			return;
		}

		var group = GroupRegister.GetGroup(_groupId);
		if (group == null)
		{
			Debug.LogError($"[BaseGroupController] InitializeSwarm: Group {_groupId} not found in register");
			return;
		}

		var currentMembers = group[this];
		_groupMembers.Clear();
		_memberOffsets.Clear();
		_memberFollowPositions.Clear();
		_groupBaseAngleOffset = Random.Range(0f, MaxGroupAngleOffset);

		foreach (var member in currentMembers)
		{
			if (member != null)
			{
				_groupMembers.Add(member);
			}
		}

		foreach (var member in _groupMembers)
		{
			if (member != null)
			{
				// Включаем контроль только если член группы не занят (не в knockback)
				if (member.CanControlled())
				{
					EnableMemberControl(member);
				}
				DistributeGroupIdToMember(member, _groupId);
			}
		}

		InitializeSwarmSystem();
		CleanupInactiveMembers();
	}

	private void DistributeGroupIdToMember(IGroupController member, int groupId)
	{
		if (member is BaseGroupController baseMember)
		{
			baseMember.SetGroupId(groupId);
		}
	}

	public virtual void SetGroupId(int groupId)
	{
		_groupId = groupId;
	}

	public virtual void ClearGroup()
	{
		foreach (var member in _groupMembers)
		{
			DisableMemberControl(member);
		}

		_groupMembers.Clear();
		_isGroupLeader = false;
		_groupId = -1;


		_memberOffsets.Clear();
		_memberFollowPositions.Clear();
		_groupBaseAngleOffset = 0f;
	}

	public virtual void TransferLeadership(IGroupController newLeader)
	{
		if (newLeader == null || !_isGroupLeader || _groupId <= 0)
		{
			return;
		}

		var group = GroupRegister.GetGroup(_groupId);
		if (group == null)
		{
			return;
		}

		var currentMembers = group[this];
		currentMembers.Remove(newLeader);
		currentMembers.Add(this);

		GroupRegister.SetRandomLeader(_groupId);
		ClearGroup();
	}

	public virtual void TransferGroupTo(IGroupController newMember)
	{
		if (newMember == null || !_isGroupLeader || _groupId <= 0)
		{
			return;
		}

		var group = GroupRegister.GetGroup(_groupId);
		if (group == null)
		{
			return;
		}

		var currentMembers = group[this];
		currentMembers.Add(this);
		currentMembers.Remove(newMember);

		newMember.InitializeGroup(_groupId, true);
		ClearGroup();
	}

	public virtual void TransferGroupIdToSuccessor(IGroupController successor)
	{
		if (successor == null || _groupId <= 0)
		{
			return;
		}

		GroupRegister.ReplaceLeader(_groupId, successor);
		ClearGroup();
	}

	public virtual void AddMemberToGroup(IGroupController member)
	{
		if (member != null && !_groupMembers.Contains(member))
		{
			_groupMembers.Add(member);

			if (!_memberOffsets.ContainsKey(member))
			{
				_memberOffsets[member] = Random.Range(0f, RandomOffsetRange);
			}

			_memberFollowPositions.Remove(member);

			DistributeGroupIdToMember(member, _groupId);

			ReinitializeSwarmSystem();
		}
		else if (member != null && _groupMembers.Contains(member))
		{
			// Член уже в группе
		}
		else
		{
			// Попытка добавить null члена
		}
	}

	public virtual void RemoveFromGroup()
	{
		if (_follower != null && _follower.IsControlOverridden)
		{
			_follower.SetControlOverride(false);
		}

		ClearGroup();
	}

	public virtual void OnMemberAddedToGroup(IGroupController member)
	{
		if (member != null && !_groupMembers.Contains(member))
		{
			AddMemberToGroup(member);
		}
	}

	public virtual IFollower GetFollower()
	{
		return _follower;
	}

	public virtual Transform GetTransform()
	{
		return transform;
	}

	private void EnableMemberControl(IGroupController member)
	{
		if (member?.GetFollower() != null && !member.GetFollower().IsControlOverridden)
		{
			member.GetFollower().SetControlOverride(true);
		}
	}

	private void DisableMemberControl(IGroupController member)
	{
		if (member?.GetFollower() != null && member.GetFollower().IsControlOverridden)
		{
			member.GetFollower().SetControlOverride(false);
		}
	}

	private void CleanupInactiveMembers()
	{
		_groupMembers.RemoveAll(member =>
			member == null ||
			!member.GetTransform().gameObject.activeInHierarchy);

		var membersToRemove = new List<IGroupController>();

		foreach (var member in _memberOffsets.Keys.ToList())
		{
			if (!_groupMembers.Contains(member))
			{
				membersToRemove.Add(member);
			}
		}

		foreach (var member in membersToRemove)
		{
			_memberOffsets.Remove(member);
			_memberFollowPositions.Remove(member);
		}
	}

	protected virtual void ApplyGroupBehavior()
	{
		CleanupInactiveMembers();

		Vector2 leaderPos = transform.position;

		foreach (var member in _groupMembers)
		{
			if (member?.GetFollower() == null || member?.GetTransform() == null)
				continue;

			Vector2 memberPos = member.GetTransform().position;
			float distanceToLeaderSqr = (memberPos - leaderPos).sqrMagnitude;
			float influenceRadiusSqr = _influenceRadius * _influenceRadius;

			Vector2 targetPos = GetOrCreateTargetPosition(member, leaderPos);
			Vector2 smoothTargetPos = GetOrCreateSmoothFollowPosition(member, targetPos, memberPos);

			if (member.GetFollower().IsMovementEnabled &&
				member.GetTransform().gameObject.activeInHierarchy &&
				member.CanControlled() &&
				distanceToLeaderSqr <= influenceRadiusSqr)
			{
				if (!member.GetFollower().IsControlOverridden)
				{
					EnableMemberControl(member);
				}

				float distanceToLeader = Mathf.Sqrt(distanceToLeaderSqr);
				float influenceStrength = CalculateInfluenceStrength(memberPos, smoothTargetPos, distanceToLeader);

				Vector2 separation = CalculateSeparation(member, memberPos);
				Vector2 directionToTarget = (smoothTargetPos + separation - memberPos).normalized;

				ApplyInfluenceToMember(member, directionToTarget, influenceStrength);
			}
			else
			{
				if (member.GetFollower().IsControlOverridden)
				{
					DisableMemberControl(member);
				}
			}
		}
	}

	private void ApplyInfluenceToMember(IGroupController member, Vector2 direction, float strength)
	{
		if (strength < MinInfluenceStrength)
		{
			strength = MinInfluenceStrength;
		}

		member.GetFollower().AddInfluence(direction, strength);
	}

	private Vector2 GetOrCreateTargetPosition(IGroupController member, Vector2 leaderPos)
	{
		Vector2 baseTargetPos = CalculateBaseTargetPosition(member, leaderPos);
		Vector2 wobbleOffset = CalculateTargetWobble(member);

		if (wobbleOffset.magnitude > _targetZoneRadius)
		{
			wobbleOffset = wobbleOffset.normalized * _targetZoneRadius;
		}

		return baseTargetPos + wobbleOffset;
	}

	private Vector2 GetOrCreateSmoothFollowPosition(IGroupController member, Vector2 targetPos, Vector2 currentPos)
	{
		if (!_memberFollowPositions.TryGetValue(member, out Vector2 followPos))
		{
			followPos = currentPos;
			_memberFollowPositions[member] = followPos;
		}

		Vector2 directionToTarget = (targetPos - followPos).normalized;
		float distanceToTargetSqr = (followPos - targetPos).sqrMagnitude;
		float maxFollowDistanceSqr = _maxFollowDistance * _maxFollowDistance;

		if (distanceToTargetSqr > maxFollowDistanceSqr)
		{
			followPos = targetPos - directionToTarget * _maxFollowDistance;
		}
		else
		{
			float moveDistance = _smoothFollowSpeed * Time.fixedDeltaTime;
			float distanceToTarget = Mathf.Sqrt(distanceToTargetSqr);
			if (distanceToTarget > moveDistance)
			{
				followPos += directionToTarget * moveDistance;
			}
			else
			{
				followPos = targetPos;
			}
		}

		_memberFollowPositions[member] = followPos;
		return followPos;
	}

	private Vector2 CalculateBaseTargetPosition(IGroupController member, Vector2 leaderPos)
	{
		if (!_memberOffsets.TryGetValue(member, out float offset))
		{
			offset = Random.Range(0f, RandomOffsetRange);
			_memberOffsets[member] = offset;
		}

		int index = _groupMembers.IndexOf(member);
		if (index == -1)
		{
			return leaderPos + Vector2.right * _optimalDistance;
		}

		float angleStep = FullCircleDegrees / _groupMembers.Count;
		float baseAngle = angleStep * index + offset * AngleOffsetMultiplier + _groupBaseAngleOffset;

		Vector2 direction = new Vector2(
			Mathf.Cos(baseAngle * DegToRad),
			Mathf.Sin(baseAngle * DegToRad)
		);

		return leaderPos + direction * _optimalDistance;
	}

	private Vector2 CalculateTargetWobble(IGroupController member)
	{
		if (!_memberOffsets.TryGetValue(member, out float offset))
		{
			offset = Random.Range(0f, RandomOffsetRange);
			_memberOffsets[member] = offset;
		}

		float time = Time.time * _targetWobbleSpeed + offset;
		float noiseX = Mathf.PerlinNoise(time, 0f) * NoiseRangeMultiplier - NoiseOffset;
		float noiseY = Mathf.PerlinNoise(0f, time) * NoiseRangeMultiplier - NoiseOffset;

		return new Vector2(noiseX, noiseY) * _targetWobbleAmount;
	}

	private Vector2 CalculatePhantomWobble()
	{
		float time = Time.time * _targetWobbleSpeed + PhantomTimeOffset;
		float noiseX = Mathf.PerlinNoise(time, 0f) * NoiseRangeMultiplier - NoiseOffset;
		float noiseY = Mathf.PerlinNoise(0f, time) * NoiseRangeMultiplier - NoiseOffset;

		Vector2 wobble = new Vector2(noiseX, noiseY) * _targetWobbleAmount;

		if (wobble.magnitude > _targetZoneRadius)
		{
			wobble = wobble.normalized * _targetZoneRadius;
		}

		return wobble;
	}

	private float CalculateInfluenceStrength(Vector2 memberPos, Vector2 targetPos, float distanceToLeader)
	{
		float baseStrength = _baseInfluenceStrength;

		if (distanceToLeader < _dangerZoneRadius)
		{
			float dangerMultiplier = BaseMultiplier + (_dangerZoneMultiplier - MinDangerMultiplier) * (1f - distanceToLeader / _dangerZoneRadius);
			baseStrength *= dangerMultiplier;
		}

		float distanceToTargetSqr = (memberPos - targetPos).sqrMagnitude;
		float distanceToTarget = Mathf.Sqrt(distanceToTargetSqr);
		float distanceCompensation = BaseMultiplier + (distanceToTarget / _optimalDistance) * _distanceCompensationMultiplier;

		return baseStrength * distanceCompensation;
	}

	private Vector2 CalculateSeparation(IGroupController currentMember, Vector2 currentPos)
	{
		Vector2 separation = Vector2.zero;

		foreach (var other in _groupMembers)
		{
			if (other == currentMember || other?.GetTransform() == null)
				continue;

			Vector2 otherPos = other.GetTransform().position;
			float distSqr = (currentPos - otherPos).sqrMagnitude;
			float minDistSqr = MinSeparationDistance * MinSeparationDistance;

			if (distSqr < minDistSqr)
			{
				float dist = Mathf.Sqrt(distSqr);
				Vector2 away = (currentPos - otherPos).normalized;
				separation += away * (SeparationStrengthMultiplier - dist) * _separationForce;
			}
		}

		return separation;
	}

	private void InitializeSwarmSystem()
	{
		_groupBaseAngleOffset = Random.Range(0f, MaxGroupAngleOffset);

		foreach (var member in _groupMembers)
		{
			if (member != null)
			{
				if (!_memberOffsets.ContainsKey(member))
				{
					_memberOffsets[member] = Random.Range(0f, RandomOffsetRange);
				}
			}
		}
	}

	public virtual void ReinitializeSwarmSystem()
	{
		InitializeSwarmSystem();
	}

	public virtual bool CanControlled()
	{
		return ShouldSkipGroupBehavior() == false;
	}

	protected abstract bool ShouldSkipGroupBehavior();

	public virtual void CheckAndActivateGroupState()
	{
		if (_groupId <= 0)
		{
			return;
		}

		var group = GroupRegister.GetGroup(_groupId);
		if (group == null)
		{
			ClearGroup();
			return;
		}

		var leader = group.Keys.First();
		var members = group[leader];

		if (leader.GetTransform() == this.transform)
		{
			_isGroupLeader = true;
			InitializeGroup(_groupId, true);
		}
		else if (members.Contains(this))
		{
			_isGroupLeader = false;
			GroupRegister.ReinitializeGroupMembers(_groupId);
		}
		else
		{
			ClearGroup();
		}
	}

	protected virtual void OnReturnedToPool(PooledEnemy pooledEnemy)
	{
		if (_isGroupLeader && _groupId > 0)
		{
			GroupRegister.LeaderDied(_groupId);
		}
		else if (_groupId > 0)
		{
			NotifyLeaderAboutMemberDeath();
		}

		ClearGroup();
	}

	protected virtual void NotifyLeaderAboutMemberDeath()
	{
		var group = GroupRegister.GetGroup(_groupId);
		if (group == null)
		{
			return;
		}

		var leader = group.Keys.First();
		var members = group[leader];

		if (members.Contains(this))
		{
			members.Remove(this);
			group[leader] = members;
		}
	}

	protected virtual void OnDrawGizmosSelected()
	{
		Vector3 center = transform.position;

#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			if (_showZonesInEditor || _isGroupLeader)
			{
				DrawBasicZones(center);
			}

			if (_showZonesInEditor)
			{
				DrawPhantomMember(center);
			}
		}
		else
		{
			if (_isGroupLeader)
			{
				DrawBasicZones(center);
				DrawGroupDetails(center);
			}
		}
#else
		if (_isGroupLeader)
		{
			DrawBasicZones(center);
			DrawGroupDetails(center);
		}
#endif
	}

	private void DrawBasicZones(Vector3 center)
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(center, _influenceRadius);

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(center, _optimalDistance);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(center, _dangerZoneRadius);
	}

	private void DrawGroupDetails(Vector3 center)
	{
		foreach (var member in _groupMembers)
		{
			if (member?.GetTransform() == null)
				continue;

			Vector2 baseTargetPos = CalculateBaseTargetPosition(member, center);

			Gizmos.color = new Color(LightBlueColorR, LightBlueColorG, LightBlueColorB, LightBlueColorA);
			Gizmos.DrawWireSphere(baseTargetPos, _targetZoneRadius);

			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(center, baseTargetPos);

			Vector2 currentIdealPoint = GetOrCreateTargetPosition(member, center);

			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(currentIdealPoint, Vector3.one * SquareSize);

			Gizmos.color = Color.white;
			Gizmos.DrawSphere(currentIdealPoint, SphereSize);
		}

		Gizmos.color = Color.yellow;
		foreach (var member in _groupMembers)
		{
			if (member?.GetTransform() != null)
			{
				Gizmos.DrawLine(center, member.GetTransform().position);
			}
		}
	}

#if UNITY_EDITOR
	private void DrawPhantomMember(Vector3 center)
	{
		Vector2 phantomBasePos = center + Vector3.right * _optimalDistance;

		Gizmos.color = new Color(LightBlueColorR, LightBlueColorG, LightBlueColorB, LightBlueColorA);
		Gizmos.DrawWireSphere(phantomBasePos, _targetZoneRadius);

		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(center, phantomBasePos);

		Vector2 phantomWobble = CalculatePhantomWobble();
		Vector2 phantomIdealPoint = phantomBasePos + phantomWobble;

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube(phantomIdealPoint, Vector3.one * SquareSize);

		Gizmos.color = Color.white;
		Gizmos.DrawSphere(phantomIdealPoint, SphereSize);
	}
#endif
}

