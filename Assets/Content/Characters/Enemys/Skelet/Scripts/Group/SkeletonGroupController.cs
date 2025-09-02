using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;
using System.Collections;

[RequireComponent(typeof(Skelet))]
public class SkeletonGroupController : BaseGroupController
{
	[SerializeField, Required] private SoulSpawnerRequested _soulRequested;

	[Header("Skeleton Group Settings")]
	[SerializeField, MinValue(0.1f)] private float _attackChainDelay = 0.5f;

	[Header("Debug")]
	[SerializeField, ReadOnly] private int _debugQueueCount = 0;
	[SerializeField, ReadOnly] private int _debugActiveChainsCount = 0;

	private List<Skelet> _skeletonMembers = new List<Skelet>();
	private List<AttackChain> _activeAttackChains = new List<AttackChain>();
	private Skelet _skeleton;
	private WaitForSeconds _attackChainDelayWait;

	private static int _nextChainId = 1;

	protected override void Awake()
	{
		base.Awake();

		_skeleton = GetComponent<Skelet>();

		_attackChainDelayWait = new WaitForSeconds(_attackChainDelay);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_skeleton != null)
		{
			_skeleton.Attacked += OnLeaderAttacked;
		}
		if (_soulRequested != null)
		{
			_soulRequested.SpawnedSoul += OnSoulSpawned;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (_skeleton != null)
		{
			_skeleton.Attacked -= OnLeaderAttacked;
		}
		if (_soulRequested != null)
		{
			_soulRequested.SpawnedSoul -= OnSoulSpawned;
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		_attackChainDelayWait = new WaitForSeconds(_attackChainDelay);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		UpdateDebugInfo();
	}

	private void UpdateDebugInfo()
	{
		_debugQueueCount = _activeAttackChains.Sum(chain => chain.AttackQueue.Count);
		_debugActiveChainsCount = _activeAttackChains.Count;
	}

	public override void InitializeGroup(int groupId, bool isLeader)
	{
		base.InitializeGroup(groupId, isLeader);
		if (isLeader)
		{
			InitializeSkeletonGroup();
		}
	}

	private void InitializeSkeletonGroup()
	{
		_skeletonMembers.Clear();
		_skeletonMembers.Add(_skeleton);

		foreach (var member in _groupMembers)
		{
			if (member != null && member.GetTransform() != null)
			{
				var skeleton = member.GetTransform().GetComponent<Skelet>();
				if (skeleton != null)
				{
					_skeletonMembers.Add(skeleton);
				}
			}
		}
	}

	private void OnLeaderAttacked()
	{
		if (_isGroupLeader)
		{
			StartAttackChain();
		}
	}

	private void OnSoulSpawned(PooledEnemy spawnedSoul)
	{
		if (!IsGroupLeader && GroupId > 0)
		{
			NotifyLeaderAboutMemberDeath();
		}

		if (spawnedSoul.TryGetComponent<SoulGroupController>(out var soulGroupController))
		{
			if (IsGroupLeader)
			{
				TransferGroupIdToSuccessor(soulGroupController);
			}
			else if (GroupId > 0)
			{
				soulGroupController.InitializeGroup(GroupId, false);
			}
		}
	}

	public override bool CanControlled()
	{
		return _skeleton.IsCanAttack;
	}

	private void StartAttackChain()
	{
		var attackQueue = CreateRandomAttackQueue();
		var chainId = _nextChainId++;
		var attackChain = new AttackChain(chainId, attackQueue);
		_activeAttackChains.Add(attackChain);
		attackChain.Coroutine = StartCoroutine(AttackChainCoroutine(attackChain));
	}

	private Queue<Skelet> CreateRandomAttackQueue()
	{
		var attackQueue = new Queue<Skelet>();
		var memberSkeletons = _skeletonMembers.Where(s => s != _skeleton).ToList();

		for (int i = memberSkeletons.Count - 1; i > 0; i--)
		{
			int randomIndex = Random.Range(0, i + 1);
			var temp = memberSkeletons[i];
			memberSkeletons[i] = memberSkeletons[randomIndex];
			memberSkeletons[randomIndex] = temp;
		}

		foreach (var skeleton in memberSkeletons)
		{
			if (skeleton != null)
			{
				attackQueue.Enqueue(skeleton);
			}
		}

		return attackQueue;
	}

	private IEnumerator AttackChainCoroutine(AttackChain chain)
	{
		while (chain.AttackQueue.Count > 0 && chain.IsActive)
		{
			bool attackExecuted = ExecuteNextAttack(chain);
			if (attackExecuted)
			{
				yield return _attackChainDelayWait;
			}
		}

		EndAttackChain(chain);
	}

	private bool ExecuteNextAttack(AttackChain chain)
	{
		if (chain.AttackQueue.Count == 0)
		{
			return false;
		}

		var currentSkeleton = chain.AttackQueue.Dequeue();
		var skeletonId = GetSkeletonId(currentSkeleton);

		if (currentSkeleton == null)
		{
			return false;
		}

		if (currentSkeleton.IsCanAttack)
		{
			currentSkeleton.StartAttack();
			return true;
		}

		return false;
	}

	private int GetSkeletonId(Skelet skeleton)
	{
		for (int i = 0; i < _skeletonMembers.Count; i++)
		{
			if (_skeletonMembers[i] == skeleton)
			{
				return i + 1;
			}
		}

		return 0;
	}

	private void EndAttackChain(AttackChain chain)
	{
		chain.IsActive = false;
		_activeAttackChains.Remove(chain);

		if (chain.Coroutine != null)
		{
			StopCoroutine(chain.Coroutine);
			chain.Coroutine = null;
		}

		chain.AttackQueue.Clear();
		chain.AttackQueue = null;
	}

	public override void ClearGroup()
	{
		foreach (var chain in _activeAttackChains.ToList())
		{
			EndAttackChain(chain);
		}

		_skeletonMembers.Clear();
		_activeAttackChains.Clear();
		_nextChainId = 1;

		base.ClearGroup();
	}

	protected override bool ShouldSkipGroupBehavior()
	{
		return false;
	}
}