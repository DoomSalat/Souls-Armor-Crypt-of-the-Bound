using UnityEngine;
using StatusSystem;

public class StatusPoison : MonoBehaviour, IStatus
{
	[SerializeField] private StatusPoisonAnimator _animator;
	[SerializeField] private StatusPoisonAnimatorEvent _animatorEvent;

	private float _damage = 1;
	private IDamageable _damageable;

	public System.Action<StatusPoison, IDamageable> OnStatusEndedCallback { get; set; }

	private void OnEnable()
	{
		_animatorEvent.Activated += OnActivated;
		_animatorEvent.Ended += OnEnd;
	}

	private void OnDisable()
	{
		_animatorEvent.Activated -= OnActivated;
		_animatorEvent.Ended -= OnEnd;
	}

	public void Initialize(IDamageable damageable)
	{
		_damageable = damageable;
		_animator.PlayPoisonAnimation();
	}

	public void HurtActive(HurtBox hurtBox)
	{
		_animator.PlayPoisonAnimation();
	}

	private void OnActivated()
	{
		if (_damageable != null)
		{
			_damageable.TakeDamage(new DamageData(_damage, DamageType.Physical, Vector2.zero, 0));
		}
	}

	private void OnEnd()
	{
		OnStatusEnded();
	}

	public void OnStatusEnded()
	{
		OnStatusEndedCallback?.Invoke(this, _damageable);
	}
}
