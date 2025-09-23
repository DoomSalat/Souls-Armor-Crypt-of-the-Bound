using UnityEngine;

[RequireComponent(typeof(KnightAnimator))]
public class KnightAnimatorEvent : MonoBehaviour
{
	public event System.Action DeathEnded;

	public void DeathEnd()
	{
		DeathEnded?.Invoke();
	}
}
