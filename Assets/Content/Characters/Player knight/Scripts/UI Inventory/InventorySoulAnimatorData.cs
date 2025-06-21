using UnityEngine;

public class InventorySoulAnimatorData : MonoBehaviour
{
	public static class Params
	{
		public static readonly int Activate = Animator.StringToHash(nameof(Activate));
		public static readonly int Hide = Animator.StringToHash(nameof(Hide));
	}
}