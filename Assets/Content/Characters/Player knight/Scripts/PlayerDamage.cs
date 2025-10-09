using UnityEngine;
using System;
using System.Collections;

public class PlayerDamage : MonoBehaviour, IDamageable
{
	[SerializeField] private float _deadDelay = 2f;

	private PlayerLimbs _limbsState;
	private PlayerKnightAnimator _playerKnightAnimator;
	private AbilityInitializer _abilityInitializer;
	private WaitForSeconds _deadDelayWait;

	private bool _isDamageLocked = false;

	public event Action<DamageData> DamageTaken;
	public event Action Died;
	public event Action DiedEnd;
	public event Action BodyLost;
	public event Action Damaged;
	public event Action LegsLost;
	public event Action LegsRestored;

	private bool _isInitialized = false;

	private void OnEnable()
	{
		if (_isInitialized == false)
			return;

		_limbsState.Dead += OnDead;
		_limbsState.BodyLosted += OnBodyLost;
		_limbsState.LegsLosted += OnLegsLost;
		_limbsState.LegsRestored += OnLegsRestored;
	}

	private void OnDisable()
	{
		_limbsState.Dead -= OnDead;
		_limbsState.BodyLosted -= OnBodyLost;
		_limbsState.LegsLosted -= OnLegsLost;
		_limbsState.LegsRestored -= OnLegsRestored;
	}

	public void Initialize(PlayerLimbs limbsState, PlayerKnightAnimator playerKnightAnimator, AbilityInitializer abilityInitializer)
	{
		_limbsState = limbsState;
		_playerKnightAnimator = playerKnightAnimator;
		_abilityInitializer = abilityInitializer;
		_deadDelayWait = new WaitForSeconds(_deadDelay);

		_isInitialized = true;
		OnEnable();
	}

	public void TakeDamage(DamageData damageData)
	{
		DamageTaken?.Invoke(damageData);
	}

	public void ApplyDamage(DamageData damageData)
	{
		if (_isDamageLocked)
			return;

		if (_limbsState.LimbStates[LimbType.Body].IsPresent == false)
		{
			_limbsState.TakeDamage();
			Damaged?.Invoke();
			return;
		}

		IAbility ability = _abilityInitializer.GetAbilitiesForLimbType(LimbType.Body);

		if (ability is IAbilityBody abilityBody && abilityBody.CanBlockDamage())
		{
			abilityBody.DamageBlocked();
			return;
		}

		_limbsState.TakeDamage();
		Damaged?.Invoke();

		if (ability != null && _limbsState.LimbStates[LimbType.Body].IsPresent)
			ability.Activate();
	}

	public void SetDamageLock(bool isLocked)
	{
		_isDamageLocked = isLocked;
	}

	public void StatusEnd(DamageType statusType)
	{

	}

	private void OnDead()
	{
		Died?.Invoke();
		StartCoroutine(DeadCoroutine());
	}

	private IEnumerator DeadCoroutine()
	{
		yield return _deadDelayWait;
		DiedEnd?.Invoke();
	}

	private void OnBodyLost()
	{
		BodyLost?.Invoke();
	}

	private void OnLegsLost()
	{
		_playerKnightAnimator.FallLegs();
		LegsLost?.Invoke();
	}

	private void OnLegsRestored()
	{
		_playerKnightAnimator.GetUpLegs();
		LegsRestored?.Invoke();
	}
}
