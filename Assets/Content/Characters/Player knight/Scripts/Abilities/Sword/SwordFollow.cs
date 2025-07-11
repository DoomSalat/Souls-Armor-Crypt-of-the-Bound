using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwordFollow : MonoBehaviour
{
	[SerializeField, MinValue(0)] private float _deactiveDamping = 1f;
	[SerializeField, MinValue(0)] private float _springForce = 6f;
	[SerializeField, MinValue(0)] private float _dampingForce = 8f;

	private Rigidbody2D _rigidbody;
	private Transform _parentPocket;
	[ShowInInspector, ReadOnly] private Vector2 _pocketOffset;
	private float _rigidbodySaveDampingLinear;

	private bool _isActive;
	private bool _isFollowing;
	private bool _hasValidOffset;

	public bool IsActive => _isActive;
	public bool IsFollowing => _isFollowing;
	public Vector2 PocketOffset => _pocketOffset;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();

		_parentPocket = transform.parent;
		_rigidbodySaveDampingLinear = _rigidbody.linearDamping;
	}

	public void UpdateFollowPosition()
	{
		if (_isActive || _isFollowing == false)
			return;

		if (_hasValidOffset == false)
		{
			UpdatePocketOffset();
		}

		Vector2 targetPosition = _parentPocket.TransformPoint(_pocketOffset);
		Vector2 currentPosition = _rigidbody.position;
		Vector2 displacement = targetPosition - currentPosition;
		float distance = displacement.magnitude;

		Vector2 springForce = displacement.normalized * distance * _springForce;
		Vector2 dampingForce = -_rigidbody.linearVelocity * _dampingForce;

		_rigidbody.AddForce(springForce + dampingForce, ForceMode2D.Force);
	}

	public void SetFollowingState(bool isFollowing)
	{
		_isFollowing = isFollowing;

		if (_isFollowing)
		{
			UpdatePocketOffset();
		}
	}

	public void Activate()
	{
		_isActive = true;
		_rigidbody.linearDamping = _rigidbodySaveDampingLinear;
		_rigidbody.linearVelocity = Vector2.zero;

		transform.SetParent(null);
		_hasValidOffset = false;
	}

	public void Deactivate()
	{
		_isActive = false;
		_rigidbody.linearDamping = _deactiveDamping;
		_rigidbody.linearVelocity = Vector2.zero;

		transform.SetParent(_parentPocket);
	}

	public void UpdatePocketOffset()
	{
		_pocketOffset = _parentPocket.InverseTransformPoint(transform.position);
		_hasValidOffset = true;
	}
}