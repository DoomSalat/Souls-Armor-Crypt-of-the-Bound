using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class AbilityInitializer : MonoBehaviour
{
	[Header("Soul Ability Configuration")]
	[SerializeField, Required] private SoulAbilitiesConfig _soulAbilitiesConfig;

	[Header("Spawn Configuration")]
	[SerializeField, Required] private Transform _abilitySpawnParent;

	[Header("Limb Visual Effect Spawn Points")]
	[SerializeField, Required] private Transform _headEffectsParent;
	[SerializeField, Required] private Transform _bodyEffectsParent;
	[SerializeField, Required] private Transform _leftArmEffectsParent;
	[SerializeField, Required] private Transform _rightArmEffectsParent;
	[SerializeField, Required] private Transform _leftLegEffectsParent;
	[SerializeField, Required] private Transform _rightLegEffectsParent;

	private PlayerLimbs _playerLimbs;
	private List<IAbility> _abilities = new List<IAbility>();
	private Dictionary<(SoulType, LimbType), IAbility> _abilitiesByTypeAndLimb = new Dictionary<(SoulType, LimbType), IAbility>();
	private Dictionary<LimbType, Transform> _limbEffectsParents = new Dictionary<LimbType, Transform>();

	private readonly Dictionary<LimbType, LimbType> _genericLimbMap = new Dictionary<LimbType, LimbType>();
	private readonly Dictionary<LimbType, IAbility> _currentAbilitiesCache = new Dictionary<LimbType, IAbility>();

	public void Initialize(PlayerLimbs playerLimbs)
	{
		_playerLimbs = playerLimbs;
		InitializeLimbTransforms();
		InitializeLimbTypeMapping();
		InitializeAbilities();

		_playerLimbs.LimbStateChanged += OnLimbStateChanged;

		RefreshCurrentAbilitiesCache();
	}

	private void OnDestroy()
	{
		_playerLimbs.LimbStateChanged -= OnLimbStateChanged;
	}

	private void OnLimbStateChanged(LimbType limbType)
	{
		UpdateCurrentAbilityCache(limbType);
	}

	private void UpdateCurrentAbilityCache(LimbType limbType)
	{
		_currentAbilitiesCache[limbType] = GetAbilitiesForLimbType(limbType);
	}

	private void RefreshCurrentAbilitiesCache()
	{
		var limbTypes = new[]
		{
			LimbType.LeftLeg, LimbType.RightLeg,
			LimbType.LeftArm, LimbType.RightArm,
			LimbType.Head, LimbType.Body
		};

		foreach (var limbType in limbTypes)
		{
			_currentAbilitiesCache[limbType] = GetAbilitiesForLimbType(limbType);
		}
	}

	public IAbility GetCurrentAbility(LimbType limbType)
	{
		return _currentAbilitiesCache.TryGetValue(limbType, out var ability) ? ability : null;
	}

	public IAbility GetCurrentArmAbility(LimbType armType)
	{
		return armType switch
		{
			LimbType.LeftArm or LimbType.RightArm => GetCurrentAbility(armType),
			_ => null
		};
	}

	public IAbility GetCurrentLegAbility(LimbType legType)
	{
		return legType switch
		{
			LimbType.LeftLeg or LimbType.RightLeg => GetCurrentAbility(legType),
			_ => null
		};
	}

	public bool HasCurrentAbility(LimbType limbType)
	{
		return _currentAbilitiesCache.TryGetValue(limbType, out var ability) && ability != null;
	}

	public ReadOnlyDictionary<LimbType, IAbility> GetAllCurrentAbilities()
	{
		return new ReadOnlyDictionary<LimbType, IAbility>(_currentAbilitiesCache);
	}

	public void ClearAbilityCache(LimbType limbType)
	{
		_currentAbilitiesCache[limbType] = null;
	}

	public void ClearAllAbilitiesCache()
	{
		_currentAbilitiesCache.Clear();
	}

	private void InitializeLimbTransforms()
	{
		_limbEffectsParents[LimbType.Head] = _headEffectsParent;
		_limbEffectsParents[LimbType.Body] = _bodyEffectsParent;
		_limbEffectsParents[LimbType.LeftArm] = _leftArmEffectsParent;
		_limbEffectsParents[LimbType.RightArm] = _rightArmEffectsParent;
		_limbEffectsParents[LimbType.LeftLeg] = _leftLegEffectsParent;
		_limbEffectsParents[LimbType.RightLeg] = _rightLegEffectsParent;
	}

	private void InitializeLimbTypeMapping()
	{
		foreach (LimbType limbType in System.Enum.GetValues(typeof(LimbType)))
		{
			_genericLimbMap[limbType] = ConvertToGenericLimbType(limbType);
		}
	}

	private void InitializeAbilities()
	{
		var allAbilities = _soulAbilitiesConfig.GetAllAbilities();

		foreach (var soulData in allAbilities)
		{
			if (soulData == null)
				continue;

			var effectsParent = GetEffectsParentForLimb(soulData.TargetLimbType);
			var ability = soulData.CreateAbility(_abilitySpawnParent, effectsParent);
			ability.Initialize();

			if (ability != null)
			{
				_abilities.Add(ability);
				_abilitiesByTypeAndLimb[(soulData.SoulType, soulData.TargetLimbType)] = ability;
			}
		}
	}

	public IAbility GetAbilitiesForLimbType(LimbType limbType)
	{
		if (_playerLimbs.LimbStates.TryGetValue(limbType, out var limbInfo) == false || limbInfo.IsPresent == false)
			return null;

		if (limbInfo.SoulType == SoulType.None)
			return null;

		var genericLimbType = _genericLimbMap[limbType];

		return _abilitiesByTypeAndLimb.TryGetValue((limbInfo.SoulType, genericLimbType), out var ability) ? ability : null;
	}

	public IAbility GetAbilityBySoulType(SoulType soulType)
	{
		foreach (var abilityEntry in _abilitiesByTypeAndLimb)
		{
			if (abilityEntry.Key.Item1 == soulType)
			{
				return abilityEntry.Value;
			}
		}

		return null;
	}

	public bool HasAbilityForLimbType(LimbType limbType)
	{
		if (_playerLimbs.LimbStates.TryGetValue(limbType, out var limbInfo) == false || limbInfo.IsPresent == false)
			return false;

		if (limbInfo.SoulType == SoulType.None)
			return false;

		var genericLimbType = _genericLimbMap[limbType];
		return _abilitiesByTypeAndLimb.ContainsKey((limbInfo.SoulType, genericLimbType));
	}

	private LimbType ConvertToGenericLimbType(LimbType specificLimbType)
	{
		return specificLimbType switch
		{
			LimbType.LeftArm or LimbType.RightArm => LimbType.LeftArm,
			LimbType.LeftLeg or LimbType.RightLeg => LimbType.LeftLeg,
			_ => specificLimbType
		};
	}

	public Transform GetEffectsParentForLimb(LimbType limbType)
	{
		if (_limbEffectsParents.TryGetValue(limbType, out var effectsParent))
			return effectsParent;

		return _abilitySpawnParent;
	}

	[ContextMenu(nameof(ReinitializeAbilities))]
	private void ReinitializeAbilities()
	{
		foreach (var ability in _abilities)
		{
			if (ability is MonoBehaviour monoBehaviour)
			{
				if (Application.isPlaying)
					Destroy(monoBehaviour.gameObject);
				else
					DestroyImmediate(monoBehaviour.gameObject);
			}
		}

		_abilities.Clear();
		_abilitiesByTypeAndLimb.Clear();

		InitializeAbilities();
		RefreshCurrentAbilitiesCache();
	}
}