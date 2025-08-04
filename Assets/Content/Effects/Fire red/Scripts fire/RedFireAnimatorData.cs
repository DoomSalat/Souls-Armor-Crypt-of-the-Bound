using UnityEngine;

public static class RedFireAnimatorData
{
	public static class Params
	{
		public static readonly int Stop = Animator.StringToHash(nameof(Stop));
	}

	public static class Clips
	{
		public static readonly int Start = Animator.StringToHash(nameof(Start));
	}
}
