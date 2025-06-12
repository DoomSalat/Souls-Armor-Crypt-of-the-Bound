using UnityEngine;

[ExecuteAlways]
public class LookAtTarget : MonoBehaviour
{
	private const string TempTargetName = "TempTarget";

	[SerializeField] private Transform _target;

	[Header("Settings")]
	[SerializeField] private float _rotationSpeed = 90f;
	[SerializeField] private bool _smoothRotation = true;
	[SerializeField] private bool _useSlerp = false;
	[SerializeField] private bool _continuous = true;
	[Space]
	[SerializeField] private float _angleOffset = 0f;
	[SerializeField] private bool _flipY = false;
	[Space]
	[SerializeField] private float _slerpSpeed = 2f;

	private void Update()
	{
		if (_target == null || !_continuous)
			return;

		RotateToTarget();
	}

	public void RotateToTarget()
	{
		if (_target == null)
			return;

		Vector3 direction = _target.position - transform.position;

		if (_flipY)
			direction.y = -direction.y;

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		angle += _angleOffset;

		Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

		if (_smoothRotation)
		{
			if (_useSlerp)
			{
				transform.rotation = Quaternion.Slerp(
					transform.rotation,
					targetRotation,
					_slerpSpeed * Time.deltaTime
				);
			}
			else
			{
				transform.rotation = Quaternion.RotateTowards(
					transform.rotation,
					targetRotation,
					_rotationSpeed * Time.deltaTime
				);
			}
		}
		else
		{
			transform.rotation = targetRotation;
		}
	}

	public void SetTargetPosition(Vector3 targetPosition)
	{
		if (_target == null)
		{
			GameObject tempTarget = new GameObject(TempTargetName);
			_target = tempTarget.transform;
		}

		_target.position = targetPosition;
	}

	public void LookAtTargetOnce()
	{
		RotateToTarget();
	}

	public float GetAngleToTarget()
	{
		if (_target == null)
			return 0f;

		Vector3 direction = _target.position - transform.position;
		if (_flipY) direction.y = -direction.y;

		return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + _angleOffset;
	}

	public bool IsLookingAtTarget(float tolerance = 5f)
	{
		if (_target == null)
			return false;

		float targetAngle = GetAngleToTarget();
		float currentAngle = transform.rotation.eulerAngles.z;

		float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
		return angleDiff <= tolerance;
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		if (_target == null)
			return;

		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(transform.position, _target.position);

		Vector3 direction = _target.position - transform.position;
		if (_flipY) direction.y = -direction.y;

		Gizmos.color = Color.red;
		Gizmos.DrawRay(transform.position, direction.normalized * 2f);

		Vector3 forward = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z) * Vector3.right;
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, forward * 1.5f);
	}
#endif
}