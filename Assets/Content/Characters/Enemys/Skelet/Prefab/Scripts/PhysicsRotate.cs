using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsRotate : MonoBehaviour
{
	[SerializeField, Required] private Rigidbody2D _rigidbody;
	[SerializeField] private float _rotationSpeed = 360f;
	[SerializeField] private AnimationCurve _decelerationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	private float _currentMultiplier = 1f;
	private bool _isActive = true;

	private void FixedUpdate()
	{
		if (!_isActive)
			return;

		float targetAngularVelocity = _rotationSpeed * _currentMultiplier;
		_rigidbody.angularVelocity = targetAngularVelocity;
	}

	public void SetRotationProgress(float progress)
	{
		float clampedProgress = Mathf.Clamp01(progress);
		_currentMultiplier = _decelerationCurve.Evaluate(1f - clampedProgress);
	}

	public void StopRotation()
	{
		_isActive = false;
		_rigidbody.angularVelocity = 0f;
	}

	public void StartRotation()
	{
		_isActive = true;
		_currentMultiplier = 1f;
		_rigidbody.angularVelocity = _rotationSpeed;
	}
}
