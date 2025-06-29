using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class PlayerLimbsVisual : MonoBehaviour
{
	private const string WhiteAmountParam = "_WhiteAmount";
	private const string FadeParam = "_Fade";

	private const float DefaultWhiteAmount = 0f;
	private const float DefaultFade = 1f;

	private const float MaxWhiteAmount = 1f;
	private const float MinFade = 0f;
	private const float MaxFade = 1f;

	[SerializeField, Required] private LimbVisualData[] _limbsData;

	[Header("Animation Settings")]
	[SerializeField, MinValue(0)] private float _whiteEffectDuration = 0.3f;
	[SerializeField, MinValue(0)] private float _fadeDuration = 0.5f;

	private Dictionary<LimbType, LimbVisualData> _limbsMap;

	private void Awake()
	{
		InitializeMap();
	}

	private void OnDestroy()
	{
		Reset();
	}

	private void InitializeMap()
	{
		_limbsMap = new Dictionary<LimbType, LimbVisualData>();

		foreach (var limbData in _limbsData)
		{
			_limbsMap[limbData.LimbType] = limbData;
		}
	}

	public void PlayLose(LimbType limbType)
	{
		if (_limbsMap.TryGetValue(limbType, out LimbVisualData limbData) == false)
		{
			Debug.LogWarning($"Limb visual data not found for {limbType}");
			return;
		}

		StopParticles(limbData);
		LoseAnimation(limbData);

		Debug.Log($"Playing lose animation for {limbType}");
	}

	public void PlayRestore(LimbType limbType)
	{
		if (_limbsMap.TryGetValue(limbType, out LimbVisualData limbData) == false)
		{
			Debug.LogWarning($"Limb visual data not found for {limbType}");
			return;
		}

		RestoreAnimation(limbData);
		PlayParticles(limbData);
	}

	private void StopParticles(LimbVisualData limbData)
	{
		if (limbData.Particles != null)
		{
			foreach (var particle in limbData.Particles)
			{
				particle.gameObject.SetActive(false);
			}
		}
	}

	private void PlayParticles(LimbVisualData limbData)
	{
		if (limbData.Particles != null)
		{
			foreach (var particle in limbData.Particles)
			{
				particle.gameObject.SetActive(true);
				particle.Play();
			}
		}
	}

	private void LoseAnimation(LimbVisualData limbData)
	{
		Sequence sequence = DOTween.Sequence();

		sequence.Append(
			DOTween.To(() => limbData.Material.GetFloat(WhiteAmountParam),
					  x => limbData.Material.SetFloat(WhiteAmountParam, x),
					  MaxWhiteAmount, _whiteEffectDuration)
		);

		sequence.Append(
			DOTween.To(() => limbData.Material.GetFloat(FadeParam),
					  x => limbData.Material.SetFloat(FadeParam, x),
					  MinFade, _fadeDuration)
		);

		sequence.SetEase(Ease.InOutQuart);
		sequence.Play();
	}

	private void RestoreAnimation(LimbVisualData limbData)
	{
		Sequence sequence = DOTween.Sequence();

		sequence.Append(
			DOTween.To(() => limbData.Material.GetFloat(FadeParam),
					  x => limbData.Material.SetFloat(FadeParam, x),
					  MaxFade, _fadeDuration)
		);

		sequence.Append(
			DOTween.To(() => limbData.Material.GetFloat(WhiteAmountParam),
					  x => limbData.Material.SetFloat(WhiteAmountParam, x),
					  DefaultWhiteAmount, _whiteEffectDuration)
		);

		sequence.OnComplete(() => PlayParticles(limbData));
		sequence.SetEase(Ease.InOutQuart);
		sequence.Play();
	}

	private void Reset()
	{
		foreach (var limbData in _limbsData)
		{
			limbData.Material.SetFloat(WhiteAmountParam, DefaultWhiteAmount);
			limbData.Material.SetFloat(FadeParam, DefaultFade);
		}
	}
}