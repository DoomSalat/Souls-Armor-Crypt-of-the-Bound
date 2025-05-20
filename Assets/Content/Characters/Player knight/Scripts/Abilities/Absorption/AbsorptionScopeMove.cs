using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class AbsorptionScopeMove : MonoBehaviour
{
	private const float DefaultMoveFactor = 1f;

	[SerializeField, MinValue(0f)] private float _moveSpeed = 10f;
	[SerializeField, MinValue(0.01f)] private float _smoothingDistance = 0.5f;
	[SerializeField, Range(0f, 1f)] private float _minSpeedFactor = 0.1f;

	private Rigidbody2D _rigidbody;
	private Camera _mainCamera;
	private bool _isFollowing = false;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_mainCamera = Camera.main;
	}

	public void Move()
	{
		if (_isFollowing == false)
			return;

		Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
		Vector2 mouseWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPosition);

		Vector2 currentPosition = _rigidbody.position;
		Vector2 newPosition = Vector2.MoveTowards(currentPosition, mouseWorldPosition, _moveSpeed * Time.fixedDeltaTime);

		if (newPosition == mouseWorldPosition)
		{
			_rigidbody.linearVelocity = Vector2.zero;
		}
		else
		{
			float speedFactor = SmoothSpeedFactor(currentPosition, mouseWorldPosition);
			_rigidbody.linearVelocity = (newPosition - currentPosition) * speedFactor / Time.fixedDeltaTime;
		}
	}

	public void SetFollowing(bool follow)
	{
		_isFollowing = follow;

		if (follow == false)
		{
			_rigidbody.linearVelocity = Vector2.zero;
		}
	}

	private float SmoothSpeedFactor(Vector2 currentPosition, Vector2 targetPosition)
	{
		float distanceSqr = (targetPosition - currentPosition).sqrMagnitude;
		float smoothingDistanceSqr = _smoothingDistance * _smoothingDistance;

		if (distanceSqr >= smoothingDistanceSqr)
			return DefaultMoveFactor;

		float time = Mathf.Sqrt(distanceSqr / smoothingDistanceSqr);
		return Mathf.Lerp(_minSpeedFactor, DefaultMoveFactor, time);
	}
}
