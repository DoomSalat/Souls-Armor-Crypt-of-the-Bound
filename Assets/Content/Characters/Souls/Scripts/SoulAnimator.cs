using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SoulAnimator : MonoBehaviour
{
	[SerializeField, Required] private AngularShake _angularShake;
	[SerializeField, Required] private SmoothRotate _smoothRotate;
	[SerializeField, Required] private SoulAnimatorEvent _soulAnimatorEvent;

	private bool _isAbsorptionActive = false;

	private Animator _animator;

	public event System.Action DeathExplosionStarted;
	public event System.Action DeathExplosionEnded;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_soulAnimatorEvent.DeathExplosionStarted += OnDeathExplosionStarted;
		_soulAnimatorEvent.DeathExplosionEnded += OnDeathExplosionEnded;
	}

	private void OnDisable()
	{
		_soulAnimatorEvent.DeathExplosionStarted -= OnDeathExplosionStarted;
		_soulAnimatorEvent.DeathExplosionEnded -= OnDeathExplosionEnded;
	}

	private void OnDeathExplosionStarted()
	{
		DeathExplosionStarted?.Invoke();
	}

	private void OnDeathExplosionEnded()
	{
		DeathExplosionEnded?.Invoke();
	}

	public void PlayDiscontinuity()
	{
		_animator.Play(SoulAnimatorData.Params.Discontinuity);
	}

	public void AbsorptionDirection(Vector3 movementDirection)
	{
		if (_isAbsorptionActive == false)
		{
			_angularShake.Play();
			PlayDiscontinuity();

			_isAbsorptionActive = true;
		}

		_smoothRotate.LookAt(movementDirection, 1, true);
	}

	public void PlayDeath()
	{
		_animator.Play(SoulAnimatorData.Params.Death);

		_angularShake.Play();
		_smoothRotate.ResetRotation();
	}

	public void Reset()
	{
		_isAbsorptionActive = false;
		_angularShake.Stop();
		_smoothRotate.ResetRotation();
	}
}
