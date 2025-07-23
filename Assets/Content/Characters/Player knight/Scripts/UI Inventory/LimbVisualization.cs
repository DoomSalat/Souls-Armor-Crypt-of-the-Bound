using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace UI.Inventory
{
	public class LimbVisualization : MonoBehaviour
	{
		[Header("Limb Render Data")]
		[SerializeField, Required] private LimbRenderData[] _limbRenderData;

		[Header("Current State")]
		[SerializeField, ReadOnly] private Dictionary<LimbType, LimbInfo> _currentLimbStates = new Dictionary<LimbType, LimbInfo>();

		private Dictionary<LimbType, LimbRenderData> _limbRenderDictionary;

		private void Awake()
		{
			InitializeDictionary();
			InitializeLimbStates();
		}

		public void UpdateLimbVisualization(Dictionary<LimbType, LimbInfo> limbStates)
		{
			foreach (var limbState in limbStates)
			{
				_currentLimbStates[limbState.Key] = limbState.Value;
				UpdateSingleLimbVisualization(limbState.Key, limbState.Value);
			}
		}

		public void ResetAllLimbsToOriginal()
		{
			foreach (var renderData in _limbRenderData)
			{
				if (renderData.SoulMaterialApplier != null)
				{
					renderData.SoulMaterialApplier.ResetToOriginalMaterials();
				}
			}
		}

		private void UpdateSingleLimbVisualization(LimbType limbType, LimbInfo limbInfo)
		{
			if (!_limbRenderDictionary.TryGetValue(limbType, out LimbRenderData renderData))
			{
				Debug.LogWarning($"LimbVisualization: LimbRenderData is null for limb type {limbType}");
				return;
			}

			renderData.SoulMaterialApplier.gameObject.SetActive(limbInfo.IsPresent);

			if (limbInfo.IsPresent && renderData.SoulMaterialApplier != null && limbInfo.SoulType != SoulType.None)
			{
				renderData.SoulMaterialApplier.ApplySoul(limbInfo.SoulType);
			}
			else if (renderData.SoulMaterialApplier != null)
			{
				renderData.SoulMaterialApplier.ResetToOriginalMaterials();
			}
		}

		private void InitializeDictionary()
		{
			_limbRenderDictionary = new Dictionary<LimbType, LimbRenderData>();

			foreach (var renderData in _limbRenderData)
			{
				if (renderData.LimbType != LimbType.None)
				{
					_limbRenderDictionary[renderData.LimbType] = renderData;
				}
			}
		}

		private void InitializeLimbStates()
		{
			foreach (LimbType limbType in System.Enum.GetValues(typeof(LimbType)))
			{
				if (limbType != LimbType.None)
				{
					_currentLimbStates[limbType] = new LimbInfo(false, SoulType.None);
				}
			}
		}
	}
}