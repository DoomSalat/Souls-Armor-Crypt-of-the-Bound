using UnityEngine;

public static class AbsorptionAnimationData
{
	public static class Params
	{
		public static readonly int IsTarget = Animator.StringToHash(nameof(IsTarget));
		public static readonly int Appear = Animator.StringToHash(nameof(Appear));
		public static readonly int Dissapear = Animator.StringToHash(nameof(Dissapear));
	}
}
