using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SmoothRotate : MonoBehaviour
{
	private const float MinMultiplierSpeed = 0.01f;

	[SerializeField, MinValue(0f)] private float _smoothTime = 0.2f;
	[SerializeField] private float _initialRotationZ = 0f;

	private float _velocity;

	public void LookAt(Vector3 targetPosition, float speedMultiplier = 1)
	{
		speedMultiplier = Mathf.Max(speedMultiplier, MinMultiplierSpeed);

		Vector3 direction = targetPosition - transform.position;

		float targetAngle = -(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);

		float currentAngle = transform.eulerAngles.z;
		float newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref _velocity, _smoothTime / speedMultiplier);

		transform.rotation = Quaternion.Euler(0, 0, newAngle);
	}

	public void LookAt(Vector3 targetDirection, float speedMultiplier = 1, bool useAsDirection = true)
	{
		if (useAsDirection)
		{
			speedMultiplier = Mathf.Max(speedMultiplier, MinMultiplierSpeed);
			float targetAngle = -(Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg - 90f);

			float currentAngle = transform.eulerAngles.z;
			float newAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref _velocity, _smoothTime / speedMultiplier);

			transform.rotation = Quaternion.Euler(0, 0, newAngle);
		}
		else
		{
			LookAt(targetDirection, speedMultiplier);
		}
	}

	public void ResetRotation(float speedMultiplier = 1)
	{
		speedMultiplier = Mathf.Max(speedMultiplier, MinMultiplierSpeed);

		float currentAngle = transform.eulerAngles.z;
		float newAngle = Mathf.SmoothDampAngle(currentAngle, _initialRotationZ, ref _velocity, _smoothTime / speedMultiplier);

		transform.rotation = Quaternion.Euler(0, 0, newAngle);
	}
}