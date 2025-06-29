using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Rigidbody2DLocalAxisLimiter : MonoBehaviour
{
	private const float DefaultSpeedFactor = 1;

	[Range(0f, 1f), SerializeField] private float _localXSpeedFactor = 1f; // 1 = полная свобода, 0 = нет движения
	[Range(0f, 1f), SerializeField] private float _localYSpeedFactor = 1f;

	private Rigidbody2D _rigidbody;
	private Vector2 _lastPosition;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_lastPosition = _rigidbody.position;
	}

	public void UpdateLimit()
	{
		Vector2 delta = _rigidbody.position - _lastPosition;
		delta = ApplyAxisSpeedFactors(delta);
		Vector2 correctedPosition = _lastPosition + delta;

		_rigidbody.MovePosition(correctedPosition);
		_lastPosition = correctedPosition;
	}

	public void SyncPosition()
	{
		_lastPosition = _rigidbody.position;
	}

	private Vector2 ApplyAxisSpeedFactors(Vector2 delta)
	{
		if (_localXSpeedFactor < DefaultSpeedFactor)
			delta = ScaleComponentAlongAxis(delta, transform.right, _localXSpeedFactor);
		if (_localYSpeedFactor < DefaultSpeedFactor)
			delta = ScaleComponentAlongAxis(delta, transform.up, _localYSpeedFactor);

		return delta;
	}

	private Vector2 ScaleComponentAlongAxis(Vector2 vector, Vector2 axis, float speedFactor)
	{
		float component = Vector2.Dot(vector, axis);
		component *= speedFactor;

		return vector - axis * Vector2.Dot(vector, axis) + axis * component;
	}
}
