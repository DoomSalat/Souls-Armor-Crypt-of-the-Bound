using UnityEngine;

public static class KnightAnimatorData
{
	public static class Params
	{
		public static readonly int Death = Animator.StringToHash(nameof(Death));
		public static readonly int Walk = Animator.StringToHash(nameof(Walk));
	}

	public static class Clips
	{
		public static readonly string Idle = "Idle";
	}
}
