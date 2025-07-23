using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class Sword : MonoBehaviour, IKnockbackProvider
{
	[SerializeField, Required] private Rigidbody2DLocalAxisLimiter _localAxisLimiter;
	[SerializeField, Required] private SwordFollow _followSystem;
	[SerializeField, Required] private SwordParticleController _particleController;
	[SerializeField, Required] private SwordSpeedTracker _speedTracker;
	[SerializeField, Required] private SwordKnockbackProvider _knockbackProvider;
	[SerializeField, Required] private SwordWallBounce _wallBounce;
	[SerializeField, Required] private SwordPlayerDetector _playerDetector;
	[SerializeField, Required] private SwordAttackZoneScaler _attackZoneScaler;

	[Header("Visualization")]
	[SerializeField, Required] private SmoothLook _eye;

	[Header("Soul Type")]
	[ShowInInspector, ReadOnly] private SoulType _currentSoulType = SoulType.None;

	private void Start()
	{
		DeactiveFollow();
	}

	private void OnEnable()
	{
		_playerDetector.EnteredRadius += OnEnteredRadius;
		_playerDetector.ExitedRadius += OnExitedRadius;
		_wallBounce.OnBounceEnded += OnBounceEnded;
	}

	private void OnDisable()
	{
		_playerDetector.EnteredRadius -= OnEnteredRadius;
		_playerDetector.ExitedRadius -= OnExitedRadius;
		_wallBounce.OnBounceEnded -= OnBounceEnded;
	}

	private void FixedUpdate()
	{
		if (_wallBounce.IsBouncing)
		{
			_followSystem.UpdatePocketOffset();
			return;
		}

		if (_followSystem.IsActive)
		{
			_speedTracker.UpdateSpeed();
			_localAxisLimiter.UpdateLimit();
		}
		else
		{
			_followSystem.UpdateFollowPosition();
		}

		ControllParticlesShow();
	}

	public void UpdateLook(Transform target)
	{
		if (_followSystem.IsActive)
			_eye.LookAt(target.position);
		else
			_eye.LookAt();
	}

	public void ActiveFollow()
	{
		_followSystem.Activate();
		_speedTracker.ResetSpeed();
	}

	public void DeactiveFollow()
	{
		_followSystem.Deactivate();
		_speedTracker.ResetSpeed();
		_followSystem.UpdatePocketOffset();
	}

	public void SetAttackZoneScale(int scaleIndex)
	{
		_attackZoneScaler.SetAttackZoneScale(scaleIndex);
	}

	public int GetAttackZoneScaleCount()
	{
		return _attackZoneScaler.GetScaleCount();
	}

	public void CalculateKnockback(Collider2D hitCollider, Collider2D other, out Vector2 direction, out float force)
	{
		_knockbackProvider.CalculateKnockback(hitCollider, other, out direction, out force);
	}

	public void SetSoulType(SoulType soulType)
	{
		if (_currentSoulType == soulType)
			return;

		_currentSoulType = soulType;

		_wallBounce.UpdateSoulType(soulType);
	}

	private void OnEnteredRadius()
	{
		_followSystem.SetFollowingState(true);
	}

	private void OnExitedRadius()
	{
		_followSystem.SetFollowingState(false);
	}

	private void OnBounceEnded(float recoveryTime, Ease recoveryEase)
	{
		_localAxisLimiter.SyncPosition();
		_speedTracker.ResetSpeed();
	}

	private void ControllParticlesShow()
	{
		if (_followSystem.IsActive || _followSystem.IsFollowing)
		{
			_particleController.EnableParticles();
		}
		else
		{
			_particleController.DisableParticles();
		}
	}
}
