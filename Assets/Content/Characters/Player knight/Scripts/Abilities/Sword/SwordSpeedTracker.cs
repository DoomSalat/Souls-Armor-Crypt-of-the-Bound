using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SwordSpeedTracker : MonoBehaviour
{
	[ShowInInspector, ReadOnly] private float _currentSpeed;

	private Rigidbody2D _rigidbody;
	private Vector2 _previousPosition;

	public Rigidbody2D Rigidbody => _rigidbody;
	public float CurrentSpeed => _currentSpeed;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	public void UpdateSpeed()
	{
		Vector2 currentPosition = _rigidbody.position;
		_currentSpeed = (currentPosition - _previousPosition).magnitude / Time.fixedDeltaTime;
		_previousPosition = currentPosition;
	}

	public void ResetSpeed()
	{
		_currentSpeed = 0f;
		_previousPosition = _rigidbody.position;
	}
}