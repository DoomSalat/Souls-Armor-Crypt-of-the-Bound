using UnityEngine;

public static class AbsorptionCooldownAnimatorData
{
	public class Params
	{
		public static readonly int Appear = Animator.StringToHash(nameof(Appear));
		public static readonly int Disappear = Animator.StringToHash(nameof(Disappear));
	}
}
