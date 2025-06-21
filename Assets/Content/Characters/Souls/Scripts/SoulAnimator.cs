using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SoulAnimator : MonoBehaviour
{
	[SerializeField, Required] private AngularShake _angularShake;
	[SerializeField, Required] private SmoothRotate _smoothRotate;

	private bool _isAbsorptionActive = false;

	public void AbsorptionDirection(Vector3 movementDirection)
	{
		if (_isAbsorptionActive == false)
		{
			_angularShake.Play();
			_isAbsorptionActive = true;
		}

		_smoothRotate.LookAt(movementDirection, 1, true);
	}

	public void AbsorptionEnded()
	{
		_isAbsorptionActive = false;
		_angularShake.Stop();
		_smoothRotate.ResetRotation();
	}
}
