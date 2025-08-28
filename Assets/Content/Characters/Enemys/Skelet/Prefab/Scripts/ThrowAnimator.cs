using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Animator))]
public class ThrowAnimator : MonoBehaviour
{
	[SerializeField, Required] private Animator _animator;
	[SerializeField, Required] private ThrowAnimatorEvent _animatorEvent;

	public event System.Action Ended;

	private void Awake()
	{
		if (_animator == null)
			_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_animatorEvent.Ended += OnEnded;
	}

	private void OnDisable()
	{
		_animatorEvent.Ended -= OnEnded;
	}

	public void Crack()
	{
		_animator.SetTrigger(ThrowAnimatorData.Params.Crack);
	}

	public void Reset()
	{
		_animator.Rebind();
		_animator.Play(ThrowAnimatorData.Clips.Idle);
	}

	private void OnEnded()
	{
		Ended?.Invoke();
	}
}
