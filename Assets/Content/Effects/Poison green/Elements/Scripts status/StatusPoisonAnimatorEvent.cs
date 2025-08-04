using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StatusPoisonAnimatorEvent : MonoBehaviour
{
	public event System.Action Activated;
	public event System.Action Ended;

	public void Activate()
	{
		Activated?.Invoke();
	}

	public void End()
	{
		Ended?.Invoke();
	}
}