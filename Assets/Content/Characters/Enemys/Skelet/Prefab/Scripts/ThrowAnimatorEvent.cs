using UnityEngine;

[RequireComponent(typeof(ThrowAnimator))]
public class ThrowAnimatorEvent : MonoBehaviour
{
	public event System.Action Ended;

	public void End()
	{
		Ended?.Invoke();
	}
}
