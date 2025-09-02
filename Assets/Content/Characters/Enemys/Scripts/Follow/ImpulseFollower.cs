using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ImpulseFollower : MonoBehaviour, IFollower
{
	[SerializeField, ReadOnly] private Transform _target;

	[Header("Impulse Movement")]
	[SerializeField, MinValue(0)] private float _impulseForce = 10f;
	[SerializeField, MinValue(0)] private float _impulseInterval = 1f;
	[SerializeField, MinValue(0)] private float _maxSpeed = 15f;

	[Header("Spread Settings")]
	[SerializeField, MinValue(0), MaxValue(90)] private float _initialSpreadAngle = 90f;
	[SerializeField, MinValue(0)] private float _spreadReductionRate = 10f;
	[SerializeField, MinValue(0)] private float _minSpreadAngle = 5f;

	private Rigidbody2D _rigidbody;
	private float _currentSpreadAngle;
	private float _lastImpulseTime;
	private bool _isInitialImpulse;
	private bool _controlOverridden;

#pragma warning disable 0067
	public event System.Action TargetReached;
#pragma warning restore 0067

	public bool IsMovementEnabled => enabled;
	public Vector2 Direction => _rigidbody.linearVelocity.normalized;
	public Vector2 Velocity => _rigidbody.linearVelocity;
	public Transform Target => _target;
	public bool IsControlOverridden => _controlOverridden;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		ResetImpulseState();
	}

	public bool TryGetDistanceToTarget(out float distance)
	{
		if (_target == null)
		{
			distance = 0f;
			return false;
		}

		Vector2 diff = _rigidbody.position - (Vector2)_target.position;
		distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
		return true;
	}

	public void SetTarget(Transform target)
	{
		_target = target;
		ResetImpulseState();

		if (enabled == false)
			return;

		if (target == null)
		{
			StopMovement();
			return;
		}
	}

	public void PauseMovement()
	{
		enabled = false;
		StopMovement();
	}

	public void ResumeMovement()
	{
		enabled = true;
	}

	public void EnableMovement()
	{
		enabled = true;
		ResetImpulseState();
	}

	public void DisableMovement()
	{
		enabled = false;
		StopMovement();
		ResetImpulseState();
	}

	public void TryFollow()
	{
		if (_target == null || enabled == false)
			return;

		if (_controlOverridden == false)
		{
			if (Time.time - _lastImpulseTime >= _impulseInterval)
			{
				ApplyImpulse();
				_lastImpulseTime = Time.time;
			}

			LimitSpeed();
		}
	}

	public void AddInfluence(Vector2 influence, float strength)
	{
		if (_controlOverridden)
		{
			Vector2 force = influence.normalized * strength;
			_rigidbody.AddForce(force, ForceMode2D.Force);
		}
	}

	public void SetControlOverride(bool isOverridden)
	{
		_controlOverridden = isOverridden;

		if (isOverridden)
		{
			StopMovement();
		}
	}

	private void ApplyImpulse()
	{
		Vector2 directionToTarget = (_target.position - transform.position).normalized;
		Vector2 impulseDirection;

		if (_isInitialImpulse)
		{
			impulseDirection = directionToTarget;
			_isInitialImpulse = false;
		}
		else
		{
			float randomAngle = Random.Range(-_currentSpreadAngle, _currentSpreadAngle);
			impulseDirection = RotateVector(directionToTarget, randomAngle);

			_currentSpreadAngle = Mathf.Max(_minSpreadAngle, _currentSpreadAngle - _spreadReductionRate * Time.deltaTime);
		}

		Vector2 impulse = impulseDirection * _impulseForce;
		_rigidbody.AddForce(impulse, ForceMode2D.Impulse);
	}

	private Vector2 RotateVector(Vector2 vector, float angle)
	{
		float radians = angle * Mathf.Deg2Rad;
		float cos = Mathf.Cos(radians);
		float sin = Mathf.Sin(radians);

		return new Vector2(
			vector.x * cos - vector.y * sin,
			vector.x * sin + vector.y * cos
		);
	}

	private void LimitSpeed()
	{
		if (_rigidbody.linearVelocity.magnitude > _maxSpeed)
		{
			_rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _maxSpeed;
		}
	}

	private void ResetImpulseState()
	{
		_currentSpreadAngle = _initialSpreadAngle;
		_isInitialImpulse = true;
		_lastImpulseTime = 0f;
	}

	private void StopMovement()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}
}
