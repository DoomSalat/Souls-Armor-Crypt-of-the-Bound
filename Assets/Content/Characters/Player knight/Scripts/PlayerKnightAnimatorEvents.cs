using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(PlayerKnightAnimator))]
public class PlayerKnightAnimatorEvents : MonoBehaviour
{
	[SerializeField] private ParticleSystem[] _particlesAbsorptionBody;
	[SerializeField] private ParticleSystem[] _particlesHeadState;

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

	[Header("Fall legs")]
	[SerializeField] private float _fallLegsDuration = 0.5f;
	[SerializeField] private float _fallLegsHeight = 0.3f;

	private float _currentHeight;
	private Tween _fallLegsTween;

	private ParticleAttractor _absorptionAttractor;

	private void Awake()
	{
		_absorptionAttractor = _particleAbsorptionSwitcher.GetComponent<ParticleAttractor>();

		_currentHeight = transform.localPosition.y;
	}

	private void Start()
	{
		DeactivateAbsorptionAttractor();
		StopHeadStateParticles();
	}

	private void OnDestroy()
	{
		_fallLegsTween?.Kill();
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

	public void PlayFallLegs()
	{
		_fallLegsTween?.Kill();
		_fallLegsTween = transform.DOLocalMoveY(_fallLegsHeight, _fallLegsDuration)
			.SetEase(Ease.OutBounce);
	}

	public void PlayGetUpLegs()
	{
		_fallLegsTween?.Kill();
		transform.localPosition = new Vector3(transform.localPosition.x, _currentHeight, transform.localPosition.z);
	}

	public void PlayHeadStateParticles()
	{
		foreach (var particle in _particlesHeadState)
		{
			particle.gameObject.SetActive(true);
		}
	}

	public void StopHeadStateParticles()
	{
		foreach (var particle in _particlesHeadState)
		{
			particle.gameObject.SetActive(false);
		}
	}
}
