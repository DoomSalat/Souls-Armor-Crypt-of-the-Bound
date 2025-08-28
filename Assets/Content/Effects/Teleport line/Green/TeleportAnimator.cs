using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Animator))]
public class TeleportAnimator : MonoBehaviour
{
	[SerializeField, Required] private Animator _animator;
	[SerializeField, Required] private TeleportAnimatorEvent _teleportAnimatorEvent;

	public event System.Action Ended;

	private void Awake()
	{
		if (_animator == null)
			_animator = GetComponent<Animator>();
	}

	private void OnEnable()
	{
		_teleportAnimatorEvent.AnimationCompleted += OnEnded;
	}

	private void OnDisable()
	{
		_teleportAnimatorEvent.AnimationCompleted -= OnEnded;
	}

	public void Play()
	{
		_animator.Play(TeleportAnimatorData.Clips.Play);
	}

	private void OnEnded()
	{
		Ended?.Invoke();
	}
}
