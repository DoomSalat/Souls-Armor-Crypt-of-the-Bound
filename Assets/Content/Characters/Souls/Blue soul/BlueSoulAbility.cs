using UnityEngine;

public class BlueSoulAbility : MonoBehaviour, ISoul
{
	public Transform Transform => transform;

	public IAbility GetAbility()
	{
		return null;
	}

	public SoulType GetSoulType()
	{
		return SoulType.Blue;
	}
}
