using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SmoothLook : MonoBehaviour
{
	[Title("Target")]
	[SerializeField] private Transform _target;

	[Title("Initial Position")]
	[SerializeField] private Vector2 _initialLocalPosition = Vector2.zero;

	[Title("Offsets")]
	[SerializeField, MinValue(0f)] private float _offsetDistanceX = 1f;
	[SerializeField, MinValue(0f)] private float _offsetDistanceY = 1f;

	[Title("Smoothing")]
	[SerializeField, MinValue(0f)] private float _smoothTime = 0.2f;

	[Title("Control")]
	[SerializeField] private bool _isFollowing = false;

	private Vector2 _velocity;
	private Transform _transform;

	private void Awake()
	{
		_transform = transform;
	}

	private void Update()
	{
		float initalPosZ = transform.localPosition.z;

		Vector2 targetPos = (_target != null && _isFollowing) ? CalculateDirectionLocalTarget() : _initialLocalPosition;
		_transform.localPosition = Vector2.SmoothDamp(_transform.localPosition, targetPos, ref _velocity, _smoothTime);

		_transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, initalPosZ);
	}

	public void SetTarget(Transform newTarget)
	{
		_target = newTarget;
		_isFollowing = _target != null;
	}

	public void SetFollowing(bool isFollowing)
	{
		_isFollowing = isFollowing && _target != null;
	}

	private Vector2 CalculateDirectionLocalTarget()
	{
		Vector2 localTarget = _transform.parent.InverseTransformPoint(_target.position);
		Vector2 delta = localTarget - _initialLocalPosition;

		if (delta == Vector2.zero)
			return _initialLocalPosition;

		Vector2 direction = delta.normalized;
		Vector2 offset = new Vector2(direction.x * _offsetDistanceX, direction.y * _offsetDistanceY);

		return _initialLocalPosition + offset;
	}
}