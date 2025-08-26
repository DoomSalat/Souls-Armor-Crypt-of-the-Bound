using UnityEngine;

public static class SkeletAnimatorData
{
	public static class Params
	{
		public static readonly int Walk = Animator.StringToHash(nameof(Walk));
		public static readonly int Death = Animator.StringToHash(nameof(Death));
		public static readonly int Throw = Animator.StringToHash(nameof(Throw));
	}

	public static class Clips
	{
		public static readonly int Idle = Animator.StringToHash(nameof(Idle));
	}
}
