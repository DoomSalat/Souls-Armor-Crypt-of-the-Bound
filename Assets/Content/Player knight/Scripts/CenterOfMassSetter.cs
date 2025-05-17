using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CenterOfMassSetter : MonoBehaviour
{
	[SerializeField] private Transform _centerOfMassTarget;

	private Rigidbody2D _rigidbody;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		UpdateCenterOfMass();
	}

	private void OnValidate()
	{
		if (_rigidbody == null)
			_rigidbody = GetComponent<Rigidbody2D>();

		UpdateCenterOfMass();
	}

	public void UpdateCenterOfMass()
	{
		if (_centerOfMassTarget == null)
			return;

		// Центр масс задаём в локальных координатах Rigidbody2D
		Vector2 localCenter = transform.InverseTransformPoint(_centerOfMassTarget.position);
		_rigidbody.centerOfMass = localCenter;
	}
}
