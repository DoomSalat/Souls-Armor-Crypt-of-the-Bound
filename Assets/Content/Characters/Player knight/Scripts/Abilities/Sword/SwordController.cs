using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpringJoint2D))]
public class SwordController : MonoBehaviour
{
	private const float MinSpringFrequency = 0.01f;
	private const string SpringRecoveryTweenId = "SpringRecovery_";

	private const float TargetGizmoRadius = 0.1f;
	private const float AnchorGizmoRadius = 0.05f;

	private const float RotationTorqueForce = 2f;
	private const float RandomHalf = 0.5f;

	private const float JoystickDeadzone = 0.1f;
	private const float JoystickSensitivityPower = 2f;

	private float _lastRotationTime;

	[Header("Characteristics")]
	[SerializeField, Required] private MouseFollower _mouseFollower;
	[SerializeField, Required] private SpringJoint2D _springJoint;
	[SerializeField] private float _speed = 1;

	[Header("Components")]
	[SerializeField, Required] private Sword _sword;
	[SerializeField, Required] private SwordWallBounce _swordWallBounce;

	private float _originalSpringFrequency;
	private string _springRecoveryTweenId;

	private bool _isControlled;
	private bool _isMouseControlled;

	public bool IsControlled => _isControlled;
	public Transform SwordTransform => _sword.transform;

	private void Awake()
	{
		_originalSpringFrequency = _springJoint.frequency;
		_springRecoveryTweenId = SpringRecoveryTweenId + GetInstanceID();
	}

	private void OnEnable()
	{
		_swordWallBounce.OnBounceStarted += DisableSpring;
		_swordWallBounce.OnBounceEnded += EnableSpringWithDelay;
	}

	private void OnDisable()
	{
		_swordWallBounce.OnBounceStarted -= DisableSpring;
		_swordWallBounce.OnBounceEnded -= EnableSpringWithDelay;
	}

	private void Update()
	{
		_sword.UpdateLook(transform);
	}

	private void OnDestroy()
	{
		DOTween.Kill(_springRecoveryTweenId);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, TargetGizmoRadius);

		Gizmos.color = Color.yellow;
		Vector3 anchorPos = GetCenterTargetPosition();
		Gizmos.DrawWireSphere(anchorPos, AnchorGizmoRadius);
		Gizmos.DrawLine(anchorPos, transform.position);
	}

	public void Activate(Vector2 direction = default)
	{
		if (_isControlled)
			return;

		_springJoint.enabled = true;
		_isControlled = true;
		_sword.ActiveFollow();
		transform.position = GetCenterTargetPosition();

		if (_isMouseControlled)
		{
			_mouseFollower.enabled = true;
		}
		else
		{
			MoveTarget(direction);
		}
	}

	public void Deactivate()
	{
		if (_isControlled == false)
			return;

		_sword.DeactiveFollow();
		_isControlled = false;
		_springJoint.enabled = false;
		_mouseFollower.enabled = false;
	}

	public void SetMouseControlled(bool isMouseControlled)
	{
		_isMouseControlled = isMouseControlled;
	}

	public void SetIndexAttackZoneScale(int scaleIndex)
	{
		_sword.SetAttackZoneScale(scaleIndex);
	}

	public void MoveTarget(Vector2 direction)
	{
		Vector3 centerTargetPosition = GetCenterTargetPosition();

		float joystickMagnitude = direction.magnitude;
		if (joystickMagnitude < JoystickDeadzone)
			return;

		Vector3 targetDirection = new Vector3(direction.x, direction.y, 0).normalized;
		float sensitivityCurve = Mathf.Pow(joystickMagnitude, JoystickSensitivityPower);

		Vector3 targetPosition = centerTargetPosition + targetDirection * _speed * sensitivityCurve;
		transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

		RandomRotateImpulse();
	}

	public void SetSwordSpeed(float speed)
	{
		_speed = speed;
	}

	public void SetSpeedThreshold(float threshold)
	{
		_sword.SetSpeedThreshold(threshold);
	}

	private Vector3 GetCenterTargetPosition()
	{
		return _sword.transform.TransformPoint(_springJoint.connectedAnchor);
	}

	private void RandomRotateImpulse()
	{
		float currentSecond = Mathf.Floor(Time.time);
		if (currentSecond == _lastRotationTime)
			return;

		_lastRotationTime = currentSecond;

		float force = Random.value > RandomHalf ? RotationTorqueForce : -RotationTorqueForce;
		_sword.RotateImpulse(force);
	}

	public void ShowSword()
	{
		_sword.gameObject.SetActive(true);
	}

	public void HideSword()
	{
		_sword.gameObject.SetActive(false);
	}

	public void SetUnscaledTime()
	{
		_sword.SetUnscaledTime();
	}

	public void SetNormalTime()
	{
		_sword.SetNormalTime();
	}

	private void DisableSpring()
	{
		DOTween.Kill(_springRecoveryTweenId);

		_springJoint.enabled = false;
		_springJoint.frequency = MinSpringFrequency;
	}

	private void EnableSpringWithDelay(float recoveryTime, Ease recoveryEase)
	{
		transform.position = GetCenterTargetPosition();

		DOTween.Kill(_springRecoveryTweenId);
		StartSpringRecovery(recoveryTime, recoveryEase);
	}

	private void StartSpringRecovery(float duration, Ease ease)
	{
		if (_isControlled == true)
		{
			_springJoint.frequency = MinSpringFrequency;
			_springJoint.enabled = true;
		}

		DOTween.To(() => _springJoint.frequency,
			x => _springJoint.frequency = x,
			_originalSpringFrequency,
			duration)
			.SetEase(ease)
			.SetId(_springRecoveryTweenId);
	}
}