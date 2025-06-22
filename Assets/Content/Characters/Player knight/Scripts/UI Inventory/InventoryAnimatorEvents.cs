using Coffee.UIExtensions;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class InventoryAnimatorEvents : MonoBehaviour
{
	[SerializeField, Required] private Dragger _soulDragger;
	[Space]
	[SerializeField, Required] private InventorySoulAnimator _animatorSoul;
	[SerializeField, Required] private UIParticle _soulParticle;
	[SerializeField, Required] private DOTweenShakeEffect _inventoryShake;
	[Space]
	[SerializeField] private float _activationDelay = 0.5f;

	private Coroutine _draggerTimerRoutine;
	private WaitForSecondsRealtime _waitForCanDrag;

	private void Awake()
	{
		_waitForCanDrag = new WaitForSecondsRealtime(_activationDelay);
	}

	private void Start()
	{
		_soulDragger.enabled = false;
	}

	public void ActivateSoul()
	{
		_animatorSoul.ActivateSoul();
		_soulParticle.Play();
		_inventoryShake.PlayShake();

		if (_draggerTimerRoutine != null)
		{
			StopCoroutine(_draggerTimerRoutine);
		}

		_draggerTimerRoutine = StartCoroutine(ActivateDraggerWithDelay());
	}

	public void DeactivateSoul()
	{
		_animatorSoul.HideSoul();
		_soulParticle.Stop();

		_soulDragger.enabled = false;
	}

	private IEnumerator ActivateDraggerWithDelay()
	{
		yield return _waitForCanDrag;

		_soulDragger.enabled = true;
	}
}
