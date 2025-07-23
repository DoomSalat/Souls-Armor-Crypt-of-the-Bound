using UnityEngine;

[System.Serializable]
public struct PlayerSoulMaterialApplierData
{
	[SerializeField] private LimbType _limbType;
	[SerializeField] private SoulMaterialApplier _applier;

	public LimbType LimbType => _limbType;
	public SoulMaterialApplier Applier => _applier;

	public PlayerSoulMaterialApplierData(LimbType limbType, SoulMaterialApplier applier)
	{
		_limbType = limbType;
		_applier = applier;
	}
}