using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Image), typeof(AbsorptionCooldownUIAnimation))]
public class AbsorptionCooldownUI : MonoBehaviour
{
	private const float DefaultFillAmount = 1f;

	[SerializeField, Required] private AbsorptionCooldown _absorptionCooldown;

	private Image _cooldownImage;
	private AbsorptionCooldownUIAnimation _cooldownAnimation;

	private void Awake()
	{
		_cooldownImage = GetComponent<Image>();
		_cooldownAnimation = GetComponent<AbsorptionCooldownUIAnimation>();
		_cooldownImage.fillAmount = DefaultFillAmount;
	}

	private void Start()
	{
		_cooldownImage.fillAmount = DefaultFillAmount;
		_cooldownAnimation.ResetToDefault();
	}

	private void OnEnable()
	{
		_absorptionCooldown.CooldownProgressed += OnCooldownProgressed;
		_absorptionCooldown.CooldownFinished += OnCooldownFinished;
	}

	private void OnDisable()
	{
		_absorptionCooldown.CooldownProgressed -= OnCooldownProgressed;
		_absorptionCooldown.CooldownFinished -= OnCooldownFinished;
	}

	private void OnCooldownProgressed(float progress)
	{
		_cooldownImage.fillAmount = progress;
		_cooldownAnimation.SetProgress(progress);
	}

	private void OnCooldownFinished()
	{
		_cooldownImage.fillAmount = DefaultFillAmount;
		_cooldownAnimation.ResetToDefault();
	}
}