using UnityEngine;

public class SoulDamage : EnemyDamage
{
	private Rigidbody2D _rigidbody;
	private Collider2D _collider;

	private HitBox _hitBox;
	private HurtBox _hurtBox;
	private SoulAnimator _soulAnimator;
	private TargetFollower _targetFollower;

	private void OnEnable()
	{
		if (_soulAnimator == null)
			return;

		_soulAnimator.DeathExplosionStarted += OnStartDeathExplosion;
		_soulAnimator.DeathExplosionEnded += OnEndDeathExplosion;
	}

	private void OnDisable()
	{
		_soulAnimator.DeathExplosionStarted -= OnStartDeathExplosion;
		_soulAnimator.DeathExplosionEnded -= OnEndDeathExplosion;
	}

	public void Initialize(Rigidbody2D rigidbody, Collider2D collider, HitBox hitBox, HurtBox hurtBox, SoulAnimator soulAnimator, TargetFollower targetFollower, KnockbackReceiver knockbackReceiver)
	{
		_rigidbody = rigidbody;
		_collider = collider;

		_soulAnimator = soulAnimator;
		_targetFollower = targetFollower;

		_hitBox = hitBox;
		_hurtBox = hurtBox;
		_knockbackReceiver = knockbackReceiver;

		OnEnable();
	}

	private void OnStartDeathExplosion()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}

	private void OnEndDeathExplosion()
	{
		Dead();
	}

	protected override void Death()
	{
		DisableCollisions();
		_targetFollower.enabled = false;

		_soulAnimator.PlayDeath();
	}

	private void DisableCollisions()
	{
		_hitBox.gameObject.SetActive(false);
		_hurtBox.gameObject.SetActive(false);
		_collider.enabled = false;
	}

	private void EnableCollisions()
	{
		_hitBox.gameObject.SetActive(true);
		_hurtBox.gameObject.SetActive(true);
		_collider.enabled = true;
	}

	private void Dead()
	{
		_rigidbody.linearVelocity = Vector2.zero;
		_soulAnimator.Reset();
		gameObject.SetActive(false);
	}
}