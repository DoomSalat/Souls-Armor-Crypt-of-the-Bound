using UnityEngine;
using System;

public class PlayerDamage : MonoBehaviour, IDamageable
{
	private PlayerLimbs _limbsState;
	private PlayerKnightAnimator _playerKnightAnimator;
	private AbilityInitializer _abilityInitializer;

	public event Action Dead;
	public event Action BodyLost;
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

		_isInitialized = true;
		OnEnable();
	}

	public void TakeDamage(DamageData damageData)
	{
		if (_limbsState.LimbStates[LimbType.Body].IsPresent == false)
		{
			_limbsState.TakeDamage();
			return;
		}

		IAbility ability = _abilityInitializer.GetAbilitiesForLimbType(LimbType.Body);

		if (ability is IAbilityBody abilityBody && abilityBody.CanBlockDamage())
		{
			abilityBody.DamageBlocked();
			return;
		}

		_limbsState.TakeDamage();

		if (ability != null && _limbsState.LimbStates[LimbType.Body].IsPresent)
			ability.Activate();
	}

	public void StatusEnd(DamageType statusType)
	{

	}

	private void OnDead()
	{
		Dead?.Invoke();
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