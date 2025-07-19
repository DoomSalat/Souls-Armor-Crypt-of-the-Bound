using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "SoulAbilityData", menuName = "Souls/Soul Ability Data", order = 1)]
public class SoulAbilityData : ScriptableObject
{
	[Header("Soul Configuration")]
	[SerializeField, Required] private SoulType _soulType;
	[SerializeField, Required] private LimbType _targetLimbType;

	[Header("Ability Prefab")]
	[SerializeField, Required] private GameObject _abilityPrefab;
	[SerializeField, TextArea(5, 10)] private string _abilityDescription;

	public SoulType SoulType => _soulType;
	public LimbType TargetLimbType => _targetLimbType;
	public GameObject AbilityPrefab => _abilityPrefab;
	public string AbilityDescription => _abilityDescription;

	public IAbility CreateAbility(Transform abilityParent, Transform effectsParent)
	{
		GameObject abilityObject = Instantiate(_abilityPrefab, abilityParent);
		var abilityComponent = abilityObject.GetComponent<IAbility>();

		if (abilityComponent.HasVisualEffects)
		{
			abilityComponent.InitializeVisualEffects(effectsParent);
		}

		return abilityComponent;
	}
}