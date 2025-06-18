using UnityEngine;

[RequireComponent(typeof(PlayerKnightAnimator))]
public class PlayerKnightAnimatorEvents : MonoBehaviour
{
	[SerializeField] private ParticleSystem[] _particlesAbsorptionBody;

	[Header("Absorption main")]
	[SerializeField] private ParticleSystemSwitcher _particleAbsorptionSwitcher;
	[SerializeField] private Transform _absorptionScopeTarget;

	[Header("Capture")]
	[SerializeField] private float _absorptionAttractorStrengthCapture = 2500f;
	[SerializeField] private float _absorptionAttractorReachDistanceCapture = 0.5f;
	[SerializeField] private float _absorptionAttractorFadeOutTimeCapture = 0.8f;
	[Header("Deactive")]
	[SerializeField] private float _absorptionAttractorStrengthDeactive = 100f;
	[SerializeField] private float _absorptionAttractorReachDistanceDeactive = 0.25f;
	[SerializeField] private float _absorptionAttractorFadeOutTimeDeactive = 0.3f;

	private ParticleAttractor _absorptionAttractor;

	private void Awake()
	{
		_absorptionAttractor = _particleAbsorptionSwitcher.GetComponent<ParticleAttractor>();
	}

	private void Start()
	{
		DeactivateAbsorptionAttractor();
	}

	public void PlayAbsorptionBody()
	{
		foreach (var particle in _particlesAbsorptionBody)
		{
			particle.Play();
		}
	}

	public void StopAbsorptionBody()
	{
		foreach (var particle in _particlesAbsorptionBody)
		{
			particle.Stop();
		}
	}

	public void PlayMainAbsorption()
	{
		DeactivateAbsorptionAttractor();
		_particleAbsorptionSwitcher.Play();
	}

	public void StopMainAbsorption()
	{
		_particleAbsorptionSwitcher.Stop();
		ActivateAbsorptionAttractor(false);
	}

	public void SwitchMainAbsorptionTo(int index)
	{
		_particleAbsorptionSwitcher.SwitchToTemplate(index);
	}

	public void ActivateAbsorptionAttractor(bool isCapured)
	{
		if (isCapured)
		{
			_absorptionAttractor.SetStrength(_absorptionAttractorStrengthCapture);
			_absorptionAttractor.SetReachDistance(_absorptionAttractorReachDistanceCapture);
			_absorptionAttractor.SetFadeOutTime(_absorptionAttractorFadeOutTimeCapture);
		}
		else
		{
			_absorptionAttractor.SetStrength(_absorptionAttractorStrengthDeactive);
			_absorptionAttractor.SetReachDistance(_absorptionAttractorReachDistanceDeactive);
			_absorptionAttractor.SetFadeOutTime(_absorptionAttractorFadeOutTimeDeactive);
		}

		_absorptionAttractor.SetTarget(isCapured ? _absorptionScopeTarget : _absorptionAttractor.transform);
	}

	public void DeactivateAbsorptionAttractor()
	{
		_absorptionAttractor.SetTarget(null);
	}
}
