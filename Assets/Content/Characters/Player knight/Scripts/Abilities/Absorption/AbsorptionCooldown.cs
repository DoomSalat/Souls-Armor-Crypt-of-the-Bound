using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class AbsorptionCooldown : MonoBehaviour
{
	private const float FullCooldownProgress = 1f;

	[SerializeField, Required] private AbsorptionCooldownAnimator _absorptionCooldownAnimator;
	[SerializeField, MinValue(0)] private float _cooldownDuration = 2f;

	public event Action<float> CooldownProgressed;
	public event Action CooldownFinished;

	[ShowInInspector, ReadOnly] public bool IsOnCooldown { get; private set; }

	public void StartCooldown()
	{
		if (IsOnCooldown == false)
		{
			_absorptionCooldownAnimator.PlayAppear();
			StartCoroutine(CooldownCoroutine());
		}
	}

	private IEnumerator CooldownCoroutine()
	{
		IsOnCooldown = true;
		float elapsedTime = 0f;

		while (elapsedTime < _cooldownDuration)
		{
			elapsedTime += Time.deltaTime;
			CooldownProgressed?.Invoke(elapsedTime / _cooldownDuration);
			yield return null;
		}

		CooldownProgressed?.Invoke(FullCooldownProgress);
		IsOnCooldown = false;
		CooldownFinished?.Invoke();

		_absorptionCooldownAnimator.PlayDisappear();
	}
}