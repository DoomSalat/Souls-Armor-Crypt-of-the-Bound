using UnityEngine;
using Sirenix.OdinInspector;
using System;

[RequireComponent(typeof(Animator))]
public class SkeletAnimator : MonoBehaviour
{
	[SerializeField, Required] private SkeletAnimatorEvent _animatorEvent;

	private Animator _animator;

	public event System.Action Throwed;
	public event System.Action ThrowEnded;
	public event System.Action SpawnSoul;
	public event System.Action DeathEnded;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_animatorEvent.Throwed += OnThrow;
		_animatorEvent.ThrowEnded += OnThrowEnd;
		_animatorEvent.SpawnSoul += OnSpawnSoul;
		_animatorEvent.DeathEnded += OnDeathEnd;
	}

	private void OnDisable()
	{
		_animatorEvent.Throwed -= OnThrow;
		_animatorEvent.ThrowEnded -= OnThrowEnd;
		_animatorEvent.SpawnSoul -= OnSpawnSoul;
		_animatorEvent.DeathEnded -= OnDeathEnd;
	}

	public void PlayThrow()
	{
		_animator.SetTrigger(SkeletAnimatorData.Params.Throw);
	}

	public void PlayWalk()
	{
		_animator.SetBool(SkeletAnimatorData.Params.Walk, true);
	}

	public void StopWalk()
	{
		_animator.SetBool(SkeletAnimatorData.Params.Walk, false);
	}

	public void PlayDeath()
	{
		_animator.SetTrigger(SkeletAnimatorData.Params.Death);
	}

	public void Reset()
	{
		_animator.SetBool(SkeletAnimatorData.Params.Walk, false);
		_animator.ResetTrigger(SkeletAnimatorData.Params.Throw);
		_animator.ResetTrigger(SkeletAnimatorData.Params.Death);
	}

	private void OnThrow()
	{
		Throwed?.Invoke();
	}

	private void OnThrowEnd()
	{
		ThrowEnded?.Invoke();
	}

	private void OnSpawnSoul()
	{
		SpawnSoul?.Invoke();
	}

	private void OnDeathEnd()
	{
		DeathEnded?.Invoke();
	}

	public void PlayIdle()
	{
		_animator.SetBool(SkeletAnimatorData.Params.Walk, false);
		_animator.Play(SkeletAnimatorData.Clips.Idle);
	}
}
