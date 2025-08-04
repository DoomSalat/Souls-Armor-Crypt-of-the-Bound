using UnityEngine;

public class RedFireAnimatorEvents : MonoBehaviour
{
	[SerializeField] private ParticleSystem _particleSystem;

	public event System.Action AnimationEnded;

	public void StopParticleSystem()
	{
		_particleSystem.Stop();
	}

	public void EndAnimation()
	{
		AnimationEnded?.Invoke();
	}
}
