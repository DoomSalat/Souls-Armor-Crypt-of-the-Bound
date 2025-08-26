using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Animator))]
public class SoulVaseAnimator : MonoBehaviour
{
	[SerializeField, Required] private GameObject _vaseGameObject;
	[SerializeField, Required] private SoulVaseAnimatorEvents _soulVaseAnimatorEvents;

	private Animator _animator;

	public event System.Action Ended;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_soulVaseAnimatorEvents.DeathEnded += OnDeathAnimationComplete;
	}

	private void OnDisable()
	{
		_soulVaseAnimatorEvents.DeathEnded -= OnDeathAnimationComplete;
	}

	public void PlayDeath()
	{
		_vaseGameObject.SetActive(false);

		_animator.SetTrigger(SoulVaseAnimatorData.Params.Death);
	}

	public void Reset()
	{
		_vaseGameObject.SetActive(true);
		_animator.ResetTrigger(SoulVaseAnimatorData.Params.Death);
		_animator.Play(SoulVaseAnimatorData.Clips.Idle);
	}

	private void OnDeathAnimationComplete()
	{
		Ended?.Invoke();
	}
}
