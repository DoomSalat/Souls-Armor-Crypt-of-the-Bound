using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class BlueShieldAbility : MonoBehaviour, IAbilityBody
{
	[SerializeField, Required] private BlueShield _blueShield;
	[SerializeField] private float _shieldDuration = 5f;

	private bool _isActive = false;
	private Coroutine _timerRoutine;
	private WaitForSeconds _waitForDeactive;

	public bool HasVisualEffects => true;

	public void Initialize()
	{
		_waitForDeactive = new WaitForSeconds(_shieldDuration);
	}

	public void Activate()
	{
		if (_isActive)
			return;

		_isActive = true;
		_blueShield.Activate();

		DeactiveTimer();
		_timerRoutine = StartCoroutine(TimerActive());
	}

	public void Deactivate()
	{
		if (_isActive == false)
			return;

		_isActive = false;
		_blueShield.Deactivate();

		DeactiveTimer();
	}

	public bool CanBlockDamage()
	{
		return _isActive;
	}

	public void DamageBlocked()
	{
		_blueShield.Defend();

		DeactiveTimer();
		_isActive = false;
	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_blueShield = Instantiate(_blueShield, effectsParent.position, effectsParent.rotation, effectsParent);
	}

	private IEnumerator TimerActive()
	{
		yield return _waitForDeactive;
		Deactivate();
	}

	private void DeactiveTimer()
	{
		if (_timerRoutine != null)
		{
			StopCoroutine(_timerRoutine);
			_timerRoutine = null;
		}
	}
}