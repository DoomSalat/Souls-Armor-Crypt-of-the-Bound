using Sirenix.OdinInspector;
using UnityEngine;

public class SwordParticleController : MonoBehaviour
{
	[SerializeField, Required] private ParticleSystem _controlledParticles;

	public bool IsPlaying => _controlledParticles.isPlaying;

	public void EnableParticles()
	{
		_controlledParticles.Play();
	}

	public void DisableParticles()
	{
		_controlledParticles.Stop();
	}
}