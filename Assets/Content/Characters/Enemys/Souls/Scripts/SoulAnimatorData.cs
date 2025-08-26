using UnityEngine;

public static class SoulAnimatorData
{
	public static class Params
	{
		public static readonly int Discontinuity = Animator.StringToHash(nameof(Discontinuity));
		public static readonly int Death = Animator.StringToHash(nameof(Death));
	}
}
