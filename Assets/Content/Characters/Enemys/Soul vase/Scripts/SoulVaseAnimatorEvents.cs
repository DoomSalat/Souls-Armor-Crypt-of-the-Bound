using UnityEngine;

public class SoulVaseAnimatorEvents : MonoBehaviour
{
	public event System.Action DeathEnded;

	public void DeathEnd()
	{
		DeathEnded?.Invoke();
	}
}
