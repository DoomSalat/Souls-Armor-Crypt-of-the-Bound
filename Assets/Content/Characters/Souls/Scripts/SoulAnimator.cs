using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SoulAnimator : MonoBehaviour
{
	[SerializeField, Required] private AngularShake _angularShake;
	[SerializeField, Required] private SmoothRotate _smoothRotate;

	private bool _isAbsorptionActive = false;

	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
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
	}

	public void Reset()
	{
		_isAbsorptionActive = false;
		_angularShake.Stop();
		_smoothRotate.ResetRotation();
	}
}
