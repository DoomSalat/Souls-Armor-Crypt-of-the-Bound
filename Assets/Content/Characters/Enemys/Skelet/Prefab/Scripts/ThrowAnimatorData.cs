using UnityEngine;

public static class ThrowAnimatorData
{
	public static class Params
	{
		public static readonly int Crack = Animator.StringToHash(nameof(Crack));
	}

	public static class Clips
	{
		public static readonly int Idle = Animator.StringToHash(nameof(Idle));
	}
}
