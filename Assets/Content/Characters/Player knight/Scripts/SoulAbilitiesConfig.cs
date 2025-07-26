using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public class SoulAbilitiesConfig
{
	[Header("Soul Abilities Configuration")]
	[SerializeField, Required] private List<SoulAbilitiesGroup> _soulGroups = new List<SoulAbilitiesGroup>();

	public List<SoulAbilitiesGroup> SoulGroups => _soulGroups;

	public List<SoulAbilityData> GetAllAbilities()
	{
		var allAbilities = new List<SoulAbilityData>();

		foreach (var group in _soulGroups)
		{
			allAbilities.AddRange(group.Abilities);
		}

		return allAbilities;
	}

	public List<SoulAbilityData> GetAbilitiesForSoulType(SoulType soulType)
	{
		foreach (var group in _soulGroups)
		{
			if (group.SoulType == soulType)
			{
				return group.Abilities;
			}
		}

		return new List<SoulAbilityData>();
	}
}