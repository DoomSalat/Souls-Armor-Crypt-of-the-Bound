using System.Collections.Generic;
using UnityEngine;

public class PlayerSoulMaterial : MonoBehaviour
{
	[Header("Soul Material Appliers")]
	[SerializeField] private PlayerSoulMaterialApplierData[] _limbAppliers = new PlayerSoulMaterialApplierData[6];

	private Dictionary<LimbType, SoulMaterialApplier> _applierLookup;
	private Dictionary<LimbType, SoulType> _currentLimbSouls;

	public event System.Action<LimbType, SoulType> SoulMaterialApplied;
	public event System.Action<LimbType> SoulMaterialReset;

	private void Awake()
	{
		InitializeApplierLookup();
		_currentLimbSouls = new Dictionary<LimbType, SoulType>();
	}

	private void Start()
	{
		foreach (LimbType limbType in System.Enum.GetValues(typeof(LimbType)))
		{
			if (limbType != LimbType.None)
			{
				_currentLimbSouls[limbType] = SoulType.None;
			}
		}
	}

	public void Apply(LimbType limbType, SoulType soulType)
	{
		_applierLookup[limbType].ApplySoul(soulType);
		_currentLimbSouls[limbType] = soulType;

		SoulMaterialApplied?.Invoke(limbType, soulType);
	}

	public void ResetLimb(LimbType limbType)
	{
		var applier = _applierLookup[limbType];

		if (applier != null)
		{
			applier.ResetToOriginalMaterials();
			_currentLimbSouls[limbType] = SoulType.None;

			SoulMaterialReset?.Invoke(limbType);
		}
	}

	public SoulType GetLimbSoulType(LimbType limbType)
	{
		if (_currentLimbSouls.TryGetValue(limbType, out SoulType soulType))
		{
			return soulType;
		}

		return SoulType.None;
	}

	public List<LimbType> GetLimbsWithSoulType(SoulType soulType)
	{
		var result = new List<LimbType>();

		foreach (var kvp in _currentLimbSouls)
		{
			if (kvp.Value == soulType)
			{
				result.Add(kvp.Key);
			}
		}

		return result;
	}

	public void AddApplierForLimb(LimbType limbType, SoulMaterialApplier applier)
	{
		if (applier == null)
			return;

		_applierLookup[limbType] = applier;

		if (_currentLimbSouls.TryGetValue(limbType, out SoulType currentSoul) &&
			currentSoul != SoulType.None)
		{
			applier.ApplySoul(currentSoul);
		}
	}

	private void InitializeApplierLookup()
	{
		_applierLookup = new Dictionary<LimbType, SoulMaterialApplier>();

		if (_limbAppliers == null)
			return;

		foreach (var applierData in _limbAppliers)
		{
			if (applierData.Applier == null || applierData.LimbType == LimbType.None)
				continue;

			if (_applierLookup.ContainsKey(applierData.LimbType))
			{
				Debug.LogWarning($"[{name}] Duplicate applier for limb {applierData.LimbType}!");
				continue;
			}

			_applierLookup[applierData.LimbType] = applierData.Applier;
		}
	}

#if UNITY_EDITOR
	[ContextMenu(nameof(ApplyAllCurrentSouls))]
	private void ApplyAllCurrentSouls()
	{
		foreach (var kvp in _currentLimbSouls)
		{
			if (kvp.Value != SoulType.None)
			{
				Apply(kvp.Key, kvp.Value);
			}
		}
	}

	[ContextMenu(nameof(ResetAllSoulMaterials))]
	private void ResetAllSoulMaterials()
	{
		foreach (var limbType in _currentLimbSouls.Keys)
		{
			ResetLimb(limbType);
		}
	}

	private void OnValidate()
	{
		if (_limbAppliers != null)
		{
			var limbTypes = new HashSet<LimbType>();
			foreach (var applierData in _limbAppliers)
			{
				if (applierData.LimbType == LimbType.None)
					continue;

				if (limbTypes.Add(applierData.LimbType) == false)
				{
					Debug.LogWarning($"[{name}] Duplicate limb type {applierData.LimbType} found in appliers!");
				}
			}
		}
	}
#endif
}