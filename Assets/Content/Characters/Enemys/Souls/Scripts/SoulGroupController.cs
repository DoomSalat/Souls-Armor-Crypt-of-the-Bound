using UnityEngine;
using Sirenix.OdinInspector;
using SpawnerSystem;

[RequireComponent(typeof(Soul))]
public class SoulGroupController : BaseGroupController
{
	[SerializeField, Required] private Soul _soul;

	protected override void Awake()
	{
		base.Awake();
		_soul = GetComponent<Soul>();
	}

	public override bool CanControlled()
	{
		if (_soul == null)
			return false;
		return !_soul.IsBusy;
	}

	protected override bool ShouldSkipGroupBehavior()
	{
		return !CanControlled();
	}
}
