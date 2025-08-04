using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RedFireAnimator : MonoBehaviour
{
	[SerializeField] private RedFireAnimatorEvents _redFireAnimatorEvents;

	private Animator _animator;

	public event System.Action AnimationEnded;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_redFireAnimatorEvents.AnimationEnded += OnAnimationEnded;
	}

	private void OnDisable()
	{
		_redFireAnimatorEvents.AnimationEnded -= OnAnimationEnded;
	}

	public void Play()
	{
		_animator.Play(RedFireAnimatorData.Clips.Start);
	}

	public void Stop()
	{
		_animator.SetTrigger(RedFireAnimatorData.Params.Stop);
	}

	private void OnAnimationEnded()
	{
		AnimationEnded?.Invoke();
	}
}
