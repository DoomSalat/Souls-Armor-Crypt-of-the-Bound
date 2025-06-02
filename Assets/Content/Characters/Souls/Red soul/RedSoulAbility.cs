using UnityEngine;

public class RedSoulAbility : MonoBehaviour, ISoul
{
	public Transform Transform => transform;

	public IAbility GetAbility()
	{
		return null;
	}

	public SoulType GetSoulType()
	{
		return SoulType.Red;
	}
}
