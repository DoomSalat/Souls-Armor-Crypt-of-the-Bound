using Sirenix.OdinInspector;
using UnityEngine;

public class SwordParticleController : MonoBehaviour
{
	[SerializeField, Required] private ParticleSystem _controlledParticles;

	public bool IsPlaying => _controlledParticles.isPlaying;

	public void EnableParticles()
	{
		if (IsPlaying)
			return;

		_controlledParticles.Play();
	}

	public void DisableParticles()
	{
		if (IsPlaying == false)
			return;

		_controlledParticles.Stop();
	}
}