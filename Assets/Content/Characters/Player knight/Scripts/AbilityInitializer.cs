using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class AbilityInitializer : MonoBehaviour
{
	private const int LastIndex = 1;

	[Header("Soul Ability Configuration")]
	[SerializeField, Required] private SoulAbilitiesConfig _soulAbilitiesConfig;

	[Header("Spawn Configuration")]
	[SerializeField, Required] private Transform _abilitySpawnParent;

	[Header("Limb Visual Effect Spawn Points")]
	[SerializeField, Required] private LimbEffectsData _headEffectsParent;
	[SerializeField, Required] private LimbEffectsData _bodyEffectsParent;
	[SerializeField, Required] private LimbEffectsData _leftArmEffectsParent;
	[SerializeField, Required] private LimbEffectsData _rightArmEffectsParent;
	[SerializeField, Required] private LimbEffectsData _leftLegEffectsParent;
	[SerializeField, Required] private LimbEffectsData _rightLegEffectsParent;

	private PlayerLimbs _playerLimbs;
	private List<IAbility> _abilities = new List<IAbility>();
	private Dictionary<LimbType, Transform> _limbEffectsParents = new Dictionary<LimbType, Transform>();

	private readonly Dictionary<LimbType, IAbility> _currentAbilitiesCache = new Dictionary<LimbType, IAbility>();

	private void OnDestroy()
	{
		_playerLimbs.LimbStateChanged -= OnLimbStateChanged;
	}

	public void Initialize(PlayerLimbs playerLimbs)
	{
		_playerLimbs = playerLimbs;
		InitializeLimbTransforms();

		_playerLimbs.LimbStateChanged += OnLimbStateChanged;

		RefreshCurrentAbilitiesCache();
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

	public void ClearAbilityCache(LimbType limbType)
	{
		_currentAbilitiesCache[limbType] = null;
	}

	public IAbility GetAbilitiesForLimbType(LimbType limbType)
	{
		if (_playerLimbs.LimbStates.TryGetValue(limbType, out var limbInfo) == false || limbInfo.IsPresent == false)
			return null;

		if (limbInfo.SoulType == SoulType.None)
			return null;

		return _currentAbilitiesCache.TryGetValue(limbType, out var ability) ? ability : null;
	}

	public Transform GetEffectsParentForLimb(LimbType limbType)
	{
		if (_limbEffectsParents.TryGetValue(limbType, out var effectsParent))
			return effectsParent;

		return _abilitySpawnParent;
	}

	private void OnLimbStateChanged(LimbType limbType)
	{
		if (_playerLimbs.LimbStates.TryGetValue(limbType, out var limbInfo) == false)
		{
			UpdateCurrentAbilityCache(limbType);
			return;
		}

		var currentAbility = GetCurrentAbility(limbType);

		if (limbInfo.IsPresent == false)
		{
			if (currentAbility != null)
			{
				RemoveLimbAbility(limbType);
			}

			return;
		}

		if (limbInfo.SoulType != SoulType.None)
		{
			if (currentAbility != null)
			{
				RemoveLimbAbility(limbType);
			}

			CreateLimbAbility(limbType, limbInfo.SoulType);
		}
		else if (limbInfo.SoulType == SoulType.None && currentAbility != null)
		{
			RemoveLimbAbility(limbType);
		}
	}

	private void UpdateCurrentAbilityCache(LimbType limbType)
	{
		IAbility abilityForLimb = null;

		if (_playerLimbs.LimbStates.TryGetValue(limbType, out var limbInfo) && limbInfo.IsPresent && limbInfo.SoulType != SoulType.None)
		{
			for (int i = _abilities.Count - LastIndex; i >= 0; i--)
			{
				var ability = _abilities[i];
				if (ability is MonoBehaviour monoBehaviour)
				{
					if (monoBehaviour.name.Contains(limbType.ToString()))
					{
						abilityForLimb = ability;
						break;
					}
				}
			}
		}

		_currentAbilitiesCache[limbType] = abilityForLimb;
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
			_currentAbilitiesCache[limbType] = null;
		}
	}

	private void InitializeLimbTransforms()
	{
		_limbEffectsParents[LimbType.Head] = _headEffectsParent.transform;
		_limbEffectsParents[LimbType.Body] = _bodyEffectsParent.transform;
		_limbEffectsParents[LimbType.LeftArm] = _leftArmEffectsParent.transform;
		_limbEffectsParents[LimbType.RightArm] = _rightArmEffectsParent.transform;
		_limbEffectsParents[LimbType.LeftLeg] = _leftLegEffectsParent.transform;
		_limbEffectsParents[LimbType.RightLeg] = _rightLegEffectsParent.transform;
	}

	private List<LimbType> GetTargetLimbTypesForAbility(LimbType targetLimbType)
	{
		var limbTypes = new List<LimbType>();

		switch (targetLimbType)
		{
			case LimbType.LeftArm:
				limbTypes.Add(LimbType.LeftArm);
				limbTypes.Add(LimbType.RightArm);
				break;
			case LimbType.LeftLeg:
				limbTypes.Add(LimbType.LeftLeg);
				limbTypes.Add(LimbType.RightLeg);
				break;
			default:
				limbTypes.Add(targetLimbType);
				break;
		}

		return limbTypes;
	}

	private void RemoveLimbAbility(LimbType limbType)
	{
		if (_limbEffectsParents.TryGetValue(limbType, out var effectsParent))
		{
			for (int i = effectsParent.childCount - LastIndex; i >= 0; i--)
			{
				var child = effectsParent.GetChild(i);
				if (Application.isPlaying)
					Destroy(child.gameObject);
				else
					DestroyImmediate(child.gameObject);
			}
		}

		var currentAbility = GetCurrentAbility(limbType);
		if (currentAbility == null)
			return;

		currentAbility.Deactivate();

		if (currentAbility is MonoBehaviour monoBehaviour)
		{
			if (Application.isPlaying)
				Destroy(monoBehaviour.gameObject);
			else
				DestroyImmediate(monoBehaviour.gameObject);
		}

		_abilities.Remove(currentAbility);
		ClearAbilityCache(limbType);
	}

	private void CreateLimbAbility(LimbType limbType, SoulType soulType)
	{
		var soulData = FindSoulAbilityData(soulType, limbType);
		if (soulData == null)
			return;

		var effectsParent = GetEffectsParentForLimb(limbType);
		var limbTypeName = limbType.ToString();

		var ability = soulData.CreateAbility(_abilitySpawnParent, effectsParent, limbTypeName);
		ability.Initialize();

		if (ability != null)
		{
			_abilities.Add(ability);
			_currentAbilitiesCache[limbType] = ability;
		}
	}

	private SoulAbilityData FindSoulAbilityData(SoulType soulType, LimbType limbType)
	{
		var allAbilities = _soulAbilitiesConfig.GetAllAbilities();

		foreach (var soulData in allAbilities)
		{
			if (soulData == null)
				continue;

			if (soulData.SoulType == soulType)
			{
				var targetLimbTypes = GetTargetLimbTypesForAbility(soulData.TargetLimbType);
				if (targetLimbTypes.Contains(limbType))
					return soulData;
			}
		}

		return null;
	}
}