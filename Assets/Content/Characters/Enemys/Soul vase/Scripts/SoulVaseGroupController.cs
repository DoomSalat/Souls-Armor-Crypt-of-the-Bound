using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(SoulVase))]
public class SoulVaseGroupController : BaseGroupController
{
	[SerializeField, Required] private SoulVase _soulVase;

	protected override void Awake()
	{
		base.Awake();
		_soulVase = GetComponent<SoulVase>();
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
}
