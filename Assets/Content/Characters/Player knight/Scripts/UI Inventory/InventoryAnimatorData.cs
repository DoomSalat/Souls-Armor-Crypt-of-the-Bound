using UnityEngine;

public class InventoryAnimatorData : MonoBehaviour
{
	public static class Params
	{
		public static readonly int Activate = Animator.StringToHash(nameof(Activate));
		public static readonly int Deactivate = Animator.StringToHash(nameof(Deactivate));
	}
}
