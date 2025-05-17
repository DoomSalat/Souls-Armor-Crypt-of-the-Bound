using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class StrongAngularVelocityLimiter : MonoBehaviour
{
	[SerializeField] private float _maxAngularVelocity = 180f; // град/с
	[SerializeField] private float _angularBrakeTorque = 500f; // сила торможения

	private Rigidbody2D _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void FixedUpdate()
	{
		float angVel = _rigidbody.angularVelocity;

		// Ограничение по скорости вращения
		if (Mathf.Abs(angVel) > _maxAngularVelocity)
		{
			_rigidbody.angularVelocity = Mathf.Sign(angVel) * _maxAngularVelocity;
		}

		// Сильное торможение угловой скорости, чтобы перебить пружину
		if (Mathf.Abs(angVel) > 0.01f)
		{
			float brakeTorque = -Mathf.Sign(angVel) * _angularBrakeTorque;
			_rigidbody.AddTorque(brakeTorque);
		}
	}
}
