using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SmoothLook : MonoBehaviour
{
	private const float MinMultiplierSpeed = 0.01f;

	[SerializeField] private Vector2 _initialLocalPosition = Vector2.zero;
	[SerializeField, MinValue(0f)] private float _smoothTime = 0.2f;

	[Title("Offsets")]
	[SerializeField, MinValue(0f)] private float _offsetDistanceX = 1f;
	[SerializeField, MinValue(0f)] private float _offsetDistanceY = 1f;

	private Vector2 _velocity;

	public void LookAt(Vector3 targetPosition, float speedMultiplier = 1)
	{
		speedMultiplier = Mathf.Max(speedMultiplier, MinMultiplierSpeed);

		float initialPosZ = transform.localPosition.z;

		Vector2 targetPos = CalculateOffsetPosition(targetPosition);
		transform.localPosition = Vector2.SmoothDamp(transform.localPosition, targetPos, ref _velocity, _smoothTime / speedMultiplier);

		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, initialPosZ);
	}

	public void LookAt(float speedMultiplier = 1)
	{
		speedMultiplier = Mathf.Max(speedMultiplier, MinMultiplierSpeed);

		float initialPosZ = transform.localPosition.z;

		transform.localPosition = Vector2.SmoothDamp(transform.localPosition, _initialLocalPosition, ref _velocity, _smoothTime / speedMultiplier);

		transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, initialPosZ);
	}

	private Vector2 CalculateOffsetPosition(Vector3 targetPosition)
	{
		Vector2 localTarget = transform.parent != null ? transform.parent.InverseTransformPoint(targetPosition) : targetPosition;
		Vector2 delta = localTarget - _initialLocalPosition;

		if (delta == Vector2.zero)
		{
			return _initialLocalPosition;
		}

		Vector2 direction = delta.normalized;
		Vector2 offset = new Vector2(direction.x * _offsetDistanceX, direction.y * _offsetDistanceY);

		return _initialLocalPosition + offset;
	}
}