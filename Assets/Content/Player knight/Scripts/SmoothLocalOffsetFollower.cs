using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SmoothLook : MonoBehaviour
{
	[Title("Target")]
	[Required, SerializeField] private Transform _target;

	[Title("Initial Position")]
	[SerializeField] private Vector2 _initialLocalPosition = Vector2.zero;

	[Title("Offsets")]
	[SerializeField] private float _offsetDistanceX = 1f;
	[SerializeField] private float _offsetDistanceY = 1f;

	[Title("Smoothing")]
	[SerializeField, Min(0f)] private float _smoothTime = 0.2f;

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
		Vector2 targetPos = _isFollowing ? CalculateDirectionLocalTarget() : _initialLocalPosition;
		_transform.localPosition = Vector2.SmoothDamp(_transform.localPosition, targetPos, ref _velocity, _smoothTime);
	}

	public void SetFollowing(bool isFollowing)
	{
		_isFollowing = isFollowing;
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