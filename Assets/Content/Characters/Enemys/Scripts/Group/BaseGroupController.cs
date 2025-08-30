using System.Collections.Generic;
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
	private const float NoiseOffset = 1f;
	private const float PhantomTimeOffset = 50f;
	private const float BaseMultiplier = 1f;
	private const float MinDangerMultiplier = 1f;
	private const float MinSeparationDistance = 1.0f;
	private const float SeparationStrengthMultiplier = 1.0f;

	private const float LightBlueColorR = 0.5f;
	private const float LightBlueColorG = 0.8f;
	private const float LightBlueColorB = 1f;
	private const float LightBlueColorA = 0.3f;
	private const float SquareSize = 0.15f;
	private const float SphereSize = 0.02f;

	[Header("Swarm Zone Settings")]
	[SerializeField, MinValue(0.5f)] protected float _optimalDistance = 2.5f; // Синий круг - оптимальная дистанция
	[SerializeField, MinValue(0.1f)] protected float _targetZoneRadius = 0.8f; // Голубой круг - зона target точки
	[SerializeField, MinValue(0.1f)] protected float _dangerZoneRadius = 1.2f; // Красный круг - зона опасности
	[SerializeField, MinValue(0.5f)] protected float _influenceRadius = 4.0f; // Зеленый круг - зона влияния

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

	protected IFollower _follower;
	protected List<IGroupController> _groupMembers = new List<IGroupController>();
	protected bool _isGroupLeader = false;

	private Dictionary<IGroupController, float> _memberOffsets = new Dictionary<IGroupController, float>();
	private Dictionary<IGroupController, Vector2> _memberFollowPositions = new Dictionary<IGroupController, Vector2>();

	public bool IsGroupLeader => _isGroupLeader;
	public List<IGroupController> GroupMembers => _groupMembers;

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
		if (_isGroupLeader && _groupMembers.Count > 0)
		{
			ApplyGroupBehavior();
		}
	}

	public virtual void InitializeGroup(List<IGroupController> groupMembers)
	{
		_groupMembers.Clear();
		_groupMembers.AddRange(groupMembers);
		_isGroupLeader = true;

		InitializeSwarmSystem();
	}

	public virtual void ClearGroup()
	{
		_groupMembers.Clear();
		_isGroupLeader = false;

		_memberOffsets.Clear();
		_memberFollowPositions.Clear();
	}

	public virtual IFollower GetFollower()
	{
		return _follower;
	}

	public virtual Transform GetTransform()
	{
		return transform;
	}

	protected virtual void ApplyGroupBehavior()
	{
		if (!CanControlled())
			return;

		Vector2 leaderPos = transform.position;
		int processedMembers = 0;

		foreach (var member in _groupMembers)
		{
			if (member?.GetFollower() == null || !member.GetFollower().IsMovementEnabled)
				continue;

			Vector2 memberPos = member.GetTransform().position;
			float distanceToLeaderSqr = (memberPos - leaderPos).sqrMagnitude;
			float influenceRadiusSqr = _influenceRadius * _influenceRadius;

			if (distanceToLeaderSqr > influenceRadiusSqr)
				continue;

			Vector2 targetPos = GetOrCreateTargetPosition(member, leaderPos);

			Vector2 smoothTargetPos = GetOrCreateSmoothFollowPosition(member, targetPos, memberPos);

			float distanceToLeader = Mathf.Sqrt(distanceToLeaderSqr);
			float influenceStrength = CalculateInfluenceStrength(memberPos, smoothTargetPos, distanceToLeader);

			Vector2 separation = CalculateSeparation(member, memberPos);

			Vector2 directionToTarget = (smoothTargetPos + separation - memberPos).normalized;
			float distanceToTarget = Vector2.Distance(memberPos, smoothTargetPos + separation);

			// Применяем влияние только если расстояние больше threshold
			if (distanceToTarget > _minMovementThreshold)
			{
				member.GetFollower().AddInfluence(directionToTarget, influenceStrength);
			}

			processedMembers++;
		}
	}

	private Vector2 GetOrCreateTargetPosition(IGroupController member, Vector2 leaderPos)
	{
		Vector2 baseTargetPos = CalculateBaseTargetPosition(member, leaderPos);
		Vector2 wobbleOffset = CalculateTargetWobble(member);

		if (wobbleOffset.magnitude > _targetZoneRadius)
		{
			wobbleOffset = wobbleOffset.normalized * _targetZoneRadius;
		}

		Vector2 idealPoint = baseTargetPos + wobbleOffset;

		return idealPoint;
	}

	private Vector2 GetOrCreateSmoothFollowPosition(IGroupController member, Vector2 targetPos, Vector2 currentPos)
	{
		if (!_memberFollowPositions.TryGetValue(member, out Vector2 followPos))
		{
			followPos = currentPos;
			_memberFollowPositions[member] = followPos;
		}

		Vector2 directionToTarget = (targetPos - followPos).normalized;
		float distanceToTarget = Vector2.Distance(followPos, targetPos);

		if (distanceToTarget > _maxFollowDistance)
		{
			followPos = targetPos - directionToTarget * _maxFollowDistance;
		}
		else
		{
			float moveDistance = _smoothFollowSpeed * Time.fixedDeltaTime;
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
			Debug.LogWarning($"[{nameof(BaseGroupController)}] Member not found in group list!");
			return leaderPos + Vector2.right * _optimalDistance;
		}

		float angleStep = FullCircleDegrees / _groupMembers.Count;
		float baseAngle = angleStep * index + offset * AngleOffsetMultiplier;

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
			float dangerMultiplier = BaseMultiplier + (_dangerZoneMultiplier - MinDangerMultiplier) * (MinDangerMultiplier - distanceToLeader / _dangerZoneRadius);
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
		_memberOffsets.Clear();
		int validMembers = 0;

		foreach (var member in _groupMembers)
		{
			if (member != null)
			{
				_memberOffsets[member] = Random.Range(0f, RandomOffsetRange);
				validMembers++;
			}
			else
			{
				Debug.LogWarning($"[{nameof(BaseGroupController)}] Null member found in group during initialization!");
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

	protected virtual void OnReturnedToPool(PooledEnemy pooledEnemy)
	{
		ClearGroup();
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

