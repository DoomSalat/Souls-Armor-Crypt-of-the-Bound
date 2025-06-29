using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwordFollow : MonoBehaviour
{
	[SerializeField, MinValue(0)] private float _deactiveDamping = 1f;
	[SerializeField, MinValue(0)] private float _followRadius = 2f;
	[SerializeField, MinValue(0)] private float _springForce = 6f;
	[SerializeField, MinValue(0)] private float _dampingForce = 8f;

	private Rigidbody2D _rigidbody;
	private Transform _parentPocket;
	[ShowInInspector, ReadOnly] private Vector2 _pocketOffset;
	private float _rigidbodySaveDampingLinear;

	private bool _isActive;
	private bool _isInRadius;
	private bool _hasValidOffset;

	public bool IsActive => _isActive;
	public bool IsInRadius => _isInRadius;
	public Vector2 PocketOffset => _pocketOffset;

	public event System.Action EnteredRadius;
	public event System.Action ExitedRadius;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();

		_parentPocket = transform.parent;
		_rigidbodySaveDampingLinear = _rigidbody.linearDamping;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _followRadius);
	}

	public void UpdateFollowPosition()
	{
		if (_isActive)
			return;

		float sqrDistanceToPocket = (transform.position - _parentPocket.position).sqrMagnitude;
		bool wasInRadius = _isInRadius;
		_isInRadius = sqrDistanceToPocket <= _followRadius * _followRadius;

		if (_isInRadius && wasInRadius == false)
		{
			UpdatePocketOffset();
			EnteredRadius?.Invoke();
		}
		else if (_isInRadius == false && wasInRadius == true)
		{
			ExitedRadius?.Invoke();
		}

		if (_isInRadius && _hasValidOffset)
		{
			Vector2 targetPosition = _parentPocket.TransformPoint(_pocketOffset);
			Vector2 currentPosition = _rigidbody.position;
			Vector2 direction = (targetPosition - currentPosition).normalized;
			float distance = Vector2.Distance(currentPosition, targetPosition);

			Vector2 springForce = direction * distance * _springForce;
			Vector2 dampingForce = -_rigidbody.linearVelocity * _dampingForce;

			_rigidbody.AddForce(springForce + dampingForce, ForceMode2D.Force);
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

		if (_isInRadius)
		{
			UpdatePocketOffset();
		}
		else
		{
			_hasValidOffset = false;
		}
	}

	public void UpdatePocketOffset()
	{
		_pocketOffset = _parentPocket.InverseTransformPoint(transform.position);
		_hasValidOffset = true;
	}
}