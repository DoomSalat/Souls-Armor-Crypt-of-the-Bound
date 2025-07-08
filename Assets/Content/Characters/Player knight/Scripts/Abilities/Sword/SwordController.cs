using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpringJoint2D))]
public class SwordController : MonoBehaviour
{
	private const float MinSpringFrequency = 0.01f;

	[SerializeField, Required] private SpringJoint2D _springJoint;
	[SerializeField, Required] private Sword _sword;
	[SerializeField, Required] private SwordWallBounce _swordWallBounce;

	private float _originalSpringFrequency;
	private string _springRecoveryTweenId;

	private bool _isControlled;

	private void Awake()
	{
		_originalSpringFrequency = _springJoint.frequency;
		_springRecoveryTweenId = "SpringRecovery_" + GetInstanceID();
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

	public void Activate()
	{
		if (_isControlled)
			return;

		_springJoint.enabled = true;
		_isControlled = true;
		_sword.ActiveFollow();
	}

	public void Deactivate()
	{
		if (_isControlled == false)
			return;

		_sword.DeactiveFollow();
		_isControlled = false;
		_springJoint.enabled = false;
	}

	private void DisableSpring()
	{
		DOTween.Kill(_springRecoveryTweenId);

		_springJoint.enabled = false;
		_springJoint.frequency = MinSpringFrequency;
	}

	private void EnableSpringWithDelay(float recoveryTime, Ease recoveryEase)
	{
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