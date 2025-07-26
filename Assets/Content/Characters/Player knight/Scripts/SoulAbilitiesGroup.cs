using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public class SoulAbilitiesGroup
{
	[SerializeField, Required] private SoulType _soulType;
	[SerializeField, Required] private List<SoulAbilityData> _abilities = new List<SoulAbilityData>();

	public SoulType SoulType => _soulType;
	public List<SoulAbilityData> Abilities => _abilities;
}