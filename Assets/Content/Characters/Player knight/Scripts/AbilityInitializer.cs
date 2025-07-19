using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class AbilityInitializer : MonoBehaviour
{
	[Header("Soul Ability Configuration")]
	[SerializeField, Required] private List<SoulAbilityData> _soulAbilityData = new List<SoulAbilityData>();

	[Header("Spawn Configuration")]
	[SerializeField, Required] private Transform _abilitySpawnParent;

	[Header("Limb Visual Effect Spawn Points")]
	[SerializeField, Required] private Transform _headEffectsParent;
	[SerializeField, Required] private Transform _bodyEffectsParent;
	[SerializeField, Required] private Transform _leftArmEffectsParent;
	[SerializeField, Required] private Transform _rightArmEffectsParent;
	[SerializeField, Required] private Transform _leftLegEffectsParent;
	[SerializeField, Required] private Transform _rightLegEffectsParent;

	private List<IAbility> _abilities = new List<IAbility>();
	private Dictionary<SoulType, IAbility> _abilitiesBySoulType = new Dictionary<SoulType, IAbility>();
	private Dictionary<LimbType, Transform> _limbEffectsParents = new Dictionary<LimbType, Transform>();

	private void Awake()
	{
		InitializeLimbTransforms();
		InitializeAbilities();
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

	private void InitializeAbilities()
	{
		foreach (var soulData in _soulAbilityData)
		{
			if (soulData == null)
				continue;

			var effectsParent = GetEffectsParentForLimb(soulData.TargetLimbType);
			var ability = soulData.CreateAbility(_abilitySpawnParent, effectsParent);
			ability.Initialize();

			if (ability != null)
			{
				_abilities.Add(ability);
				_abilitiesBySoulType[soulData.SoulType] = ability;
			}
		}
	}

	public IAbility GetAbilitiesForLimbType(LimbType limbType)
	{
		IAbility ability = null;

		foreach (var soulData in _soulAbilityData)
		{
			if (soulData.TargetLimbType == limbType)
			{
				_abilitiesBySoulType.TryGetValue(soulData.SoulType, out ability);
				return ability;
			}
		}

		return null;
	}

	public Transform GetEffectsParentForLimb(LimbType limbType)
	{
		if (_limbEffectsParents.TryGetValue(limbType, out var effectsParent))
		{
			return effectsParent;
		}

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
		_abilitiesBySoulType.Clear();

		InitializeAbilities();
	}
}