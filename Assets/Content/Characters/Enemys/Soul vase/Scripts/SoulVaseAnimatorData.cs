using UnityEngine;

public static class SoulVaseAnimatorData
{
	public static class Params
	{
		public static readonly int Death = Animator.StringToHash(nameof(Death));
	}

	public static class Clips
	{
		public static readonly int Idle = Animator.StringToHash(nameof(Idle));
	}
}
