using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class SwordChargeEffect : MonoBehaviour
{
	[SerializeField, Required] private Animator _animator;
	[SerializeField, Required] private ParticleSystem _particleSystem;
	[SerializeField, MinValue(0)] private float _chargeAnimationDuration = 0.3f;

	private Coroutine _chargeAnimationCoroutine;
	private WaitForSeconds _chargeAnimationDurationWait;
	private bool _isCharged = false;

	private void Awake()
	{
		_chargeAnimationDurationWait = new WaitForSeconds(_chargeAnimationDuration);
	}

	public void PlayCharged()
	{
		if (_isCharged)
			return;

		Stop();

		_chargeAnimationCoroutine = StartCoroutine(ChargeAnimationCoroutine());
	}

	public void Stop()
	{
		if (_chargeAnimationCoroutine != null)
		{
			StopCoroutine(_chargeAnimationCoroutine);
			_chargeAnimationCoroutine = null;
		}

		_isCharged = false;
		_particleSystem.Stop();
		_animator.ResetTrigger(SwordChargeEffectData.Params.Charge);
	}

	private IEnumerator ChargeAnimationCoroutine()
	{
		_animator.SetTrigger(SwordChargeEffectData.Params.Charge);
		yield return _chargeAnimationDurationWait;

		_particleSystem.Play();
		_isCharged = true;
		_chargeAnimationCoroutine = null;
	}
}
