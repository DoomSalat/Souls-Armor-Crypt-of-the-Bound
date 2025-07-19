using UnityEngine;

public static class ShieldAnimatorData
{
	public static class Params
	{
		public static readonly int Activate = Animator.StringToHash(nameof(Activate));
		public static readonly int Deactivate = Animator.StringToHash(nameof(Deactivate));
		public static readonly int Defend = Animator.StringToHash(nameof(Defend));
	}
}
