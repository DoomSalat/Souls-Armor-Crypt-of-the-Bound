using UnityEngine;

[RequireComponent(typeof(TeleportAnimator))]
public class TeleportAnimatorEvent : MonoBehaviour
{
	public System.Action AnimationCompleted;

	public void OnAnimationCompleted()
	{
		AnimationCompleted?.Invoke();
	}
}
