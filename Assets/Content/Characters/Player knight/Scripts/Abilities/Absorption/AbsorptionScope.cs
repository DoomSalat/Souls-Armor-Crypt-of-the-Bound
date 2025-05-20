using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class AbsorptionScope : MonoBehaviour
{
	[SerializeField, MinValue(0f)] private float _moveSpeed = 10f;
	[SerializeField, MinValue(0f)] private float _maxForce = 50f;
	[Space]
	[SerializeField, Required] private AbsorptionAnimation _animation;
	[SerializeField, Required] private AbsorptionScopeCollider _collider;

	private Rigidbody2D _rigidbody;
	private bool _isFollowing = false;
	private Camera _camera;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_camera = Camera.main;
	}

	private void FixedUpdate()
	{
		if (_isFollowing == false)
			return;

		Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
		Vector2 mouseWorldPosition = _camera.ScreenToWorldPoint(mouseScreenPosition);

		Vector2 direction = (mouseWorldPosition - _rigidbody.position).normalized;
		Vector2 desiredVelocity = direction * _moveSpeed;

		Vector2 force = (desiredVelocity - _rigidbody.linearVelocity) * _rigidbody.mass;
		force = Vector2.ClampMagnitude(force, _maxForce);

		_rigidbody.AddForce(force, ForceMode2D.Force);

		_collider.UpdateCollider();
	}

	[ContextMenu(nameof(Activate))]
	public void Activate()
	{
		_isFollowing = !_isFollowing;

		if (_isFollowing == false)
		{
			_rigidbody.linearVelocity = Vector2.zero;
		}

		_animation.PlayAppear();
	}

	public void Hide(bool hide)
	{
		_isFollowing = false;
		_rigidbody.linearVelocity = Vector2.zero;

		_animation.PlayDissapear();
	}
}
