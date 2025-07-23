using UnityEngine;
using Sirenix.OdinInspector;

public class RedSoul : BaseSoul
{
	public override SoulType GetSoulType()
	{
		return SoulType.Red;
	}

	public virtual IAbility GetAbility()
	{
		return null;
	}
}
