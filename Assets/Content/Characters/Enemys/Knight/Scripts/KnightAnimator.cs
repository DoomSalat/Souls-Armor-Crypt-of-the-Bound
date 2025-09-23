using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KnightAnimator : MonoBehaviour
{
	[SerializeField, Required] private KnightAnimatorEvent _animatorEvent;

	private Animator _animator;

	public event System.Action DeathEnded;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_animatorEvent.DeathEnded += OnDeathEnded;
	}

	private void OnDisable()
	{
		_animatorEvent.DeathEnded -= OnDeathEnded;
	}

	private void OnDeathEnded()
	{
		DeathEnded?.Invoke();
	}

	public void PlayIdle()
	{
		_animator.Play(KnightAnimatorData.Clips.Idle);
	}

	public void PlayWalk()
	{
		_animator.SetBool(KnightAnimatorData.Params.Walk, true);
	}

	public void StopWalk()
	{
		_animator.SetBool(KnightAnimatorData.Params.Walk, false);
	}

	public void PlayDeath()
	{
		_animator.SetTrigger(KnightAnimatorData.Params.Death);
	}

	public void Reset()
	{
		_animator.ResetTrigger(KnightAnimatorData.Params.Death);
		_animator.SetBool(KnightAnimatorData.Params.Walk, false);
		_animator.Play(KnightAnimatorData.Clips.Idle);
		_animator.speed = 1;
	}
}
