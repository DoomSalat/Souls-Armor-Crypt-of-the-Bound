using UnityEngine;

public static class PlayerKnightAnimatorData
{
	public static class Params
	{
		public static readonly int isMove = Animator.StringToHash(nameof(isMove));
		public static readonly int direction = Animator.StringToHash(nameof(direction));

		public static readonly int isCapture = Animator.StringToHash(nameof(isCapture));
		public static readonly int abdorptionActive = Animator.StringToHash(nameof(abdorptionActive));
		public static readonly int abdorptionDeactive = Animator.StringToHash(nameof(abdorptionDeactive));
		public static readonly int headed = Animator.StringToHash(nameof(headed));
	}

	public static class Clips
	{
		public static readonly string startIdle = "Start Idle";
		public static readonly string startIdleEnd = "Start out Idle cutscene";
	}
}
