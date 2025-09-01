using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;

[RequireComponent(typeof(SoulVase))]
public class SoulVaseGroupController : BaseGroupController
{
	[SerializeField, Required] private SoulVase _soulVase;
	[SerializeField, Required] private SoulSpawnerRequested _soulSpawner;

	protected override void Awake()
	{
		base.Awake();
		_soulVase = GetComponent<SoulVase>();
		_soulSpawner = GetComponent<SoulSpawnerRequested>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_soulSpawner != null)
		{
			_soulSpawner.SpawnedSoul += OnSoulSpawned;
		}
	}

	protected override void OnDisable()
	{
		if (_soulSpawner != null)
		{
			_soulSpawner.SpawnedSoul -= OnSoulSpawned;
		}

		base.OnDisable();
	}

	public override bool CanControlled()
	{
		if (_soulVase == null)
			return false;

		return !_soulVase.IsBusy;
	}

	protected override bool ShouldSkipGroupBehavior()
	{
		return !CanControlled();
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
}
