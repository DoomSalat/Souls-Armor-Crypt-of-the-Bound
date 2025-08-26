using UnityEngine;

public class SkeletAnimatorEvent : MonoBehaviour
{
	public event System.Action Throwed;
	public event System.Action ThrowEnded;
	public event System.Action SpawnSoul;
	public event System.Action DeathEnded;

	public void Throw()
	{
		Throwed?.Invoke();
	}

	public void ThrowEnd()
	{
		ThrowEnded?.Invoke();
	}

	public void RequestSpawnSoul()
	{
		SpawnSoul?.Invoke();
	}

	public void DeathEnd()
	{
		DeathEnded?.Invoke();
	}
}
