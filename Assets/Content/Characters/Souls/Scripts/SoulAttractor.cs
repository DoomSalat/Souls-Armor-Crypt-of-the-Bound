using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody2D))]
public class SoulAttractor : MonoBehaviour
{
	[SerializeField, MinValue(0)] private float _baseAcceleration = 5f;
	[SerializeField, MinValue(0)] private float _accelerationGrowth = 2f;
	[SerializeField, MinValue(0)] private float _maxSpeed = 20f;
	[SerializeField, MinValue(0)] private float _completionDistance = 0.3f;

	private Rigidbody2D _rigidbody;
	private Transform _target;
	private bool _isAttracting = false;
	private float _currentSpeed = 0f;
	private float _attractionTime = 0f;

	public event System.Action AttractionCompleted;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	public void StartAttraction(Transform target)
	{
		if (_isAttracting)
			return;

		_target = target;
		_isAttracting = true;
		_currentSpeed = 0f;
		_attractionTime = 0f;
	}

	public void StopAttraction()
	{
		_isAttracting = false;
		_currentSpeed = 0f;
		_attractionTime = 0f;
		_rigidbody.linearVelocity = Vector2.zero;
	}

	private void Update()
	{
		if (_isAttracting == false || _target == null)
			return;

		Vector2 direction = (_target.position - transform.position);
		float distance = direction.magnitude;

		if (distance <= _completionDistance)
		{
			_rigidbody.linearVelocity = Vector2.zero;
			transform.position = _target.position;
			_isAttracting = false;
			AttractionCompleted?.Invoke();
		}
	}

	private void FixedUpdate()
	{
		if (_isAttracting == false || _target == null)
			return;

		Vector2 direction = (_target.position - transform.position);
		float distance = direction.magnitude;

		if (distance <= _completionDistance)
		{
			_rigidbody.linearVelocity = Vector2.zero;
			return;
		}

		_attractionTime += Time.fixedDeltaTime;

		float currentAcceleration = _baseAcceleration + (_accelerationGrowth * _attractionTime);
		_currentSpeed += currentAcceleration * Time.fixedDeltaTime;

		_currentSpeed = Mathf.Min(_currentSpeed, _maxSpeed);

		float maxDistanceThisFrame = _currentSpeed * Time.fixedDeltaTime;

		if (maxDistanceThisFrame > distance)
		{
			_currentSpeed = distance / Time.fixedDeltaTime;
		}

		_rigidbody.linearVelocity = direction.normalized * _currentSpeed;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _completionDistance);
	}
}