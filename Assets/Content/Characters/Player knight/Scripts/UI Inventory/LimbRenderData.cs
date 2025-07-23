using UnityEngine;
using Sirenix.OdinInspector;

namespace UI.Inventory
{
	[System.Serializable]
	public struct LimbRenderData
	{
		[SerializeField] private LimbType _limbType;
		[SerializeField, Required] private SoulMaterialApplier _soulMaterialApplier;

		public LimbType LimbType => _limbType;
		public SoulMaterialApplier SoulMaterialApplier => _soulMaterialApplier;

		public LimbRenderData(LimbType limbType, SoulMaterialApplier soulMaterialApplier)
		{
			_limbType = limbType;
			_soulMaterialApplier = soulMaterialApplier;
		}
	}
}