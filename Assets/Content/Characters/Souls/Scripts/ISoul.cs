using UnityEngine;

public interface ISoul
{
	public Transform Transform { get; }

	public SoulType GetSoulType();
	public IAbility GetAbility();
}