using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Soul : MonoBehaviour, IDamageable
{
	[SerializeField] private Transform _target;
	[Space]
	[SerializeField, Required] private TargetFollower _targetFollower;
	[SerializeField, Required] private SoulAttractor _soulAttractor;
	[SerializeField, Required] private SoulAnimator _soulAnimator;
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private HurtBox _hurtBox;

	[Header("Knockback")]
	[SerializeField, Required] private KnockbackReceiver _knockbackReceiver;
	[SerializeField, MinValue(0)] private float _attackKnockbackForce = 10f;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;

	private bool _isDead = false;
	private bool _isAttracted = false;
	private bool _isEndAttraction = false;
	private event System.Action _attractionCompleted;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();
	}

	private void OnEnable()
	{
		_soulAttractor.AttractionCompleted += OnAttractionCompletedInternal;

		_soulAnimator.DeathExplosionStarted += OnStartDeathExplosion;
		_soulAnimator.DeathExplosionEnded += OnEndDeathExplosion;

		_hitBox.TargetHitted += OnHitTarget;
	}

	private void OnDisable()
	{
		_soulAttractor.AttractionCompleted -= OnAttractionCompletedInternal;

		_soulAnimator.DeathExplosionStarted -= OnStartDeathExplosion;
		_soulAnimator.DeathExplosionEnded -= OnEndDeathExplosion;

		_hitBox.TargetHitted -= OnHitTarget;
	}

	private void OnStartDeathExplosion()
	{
		_rigidbody.linearVelocity = Vector2.zero;
	}

	private void OnEndDeathExplosion()
	{
		Dead();
	}

	private void Update()
	{
		if (_isAttracted)
		{
			if (_isEndAttraction == false)
			{
				_soulAnimator.AbsorptionDirection(_rigidbody.linearVelocity);
			}
			else
			{
				_soulAnimator.AbsorptionDirection(Vector2.up);
			}
		}
	}

	private void FixedUpdate()
	{
		if (_isDead == false)
			_targetFollower.TryFollow(_target);
	}

	public void TakeDamage(DamageData damageData)
	{
		_isDead = true;
		_hitBox.gameObject.SetActive(false);
		_hurtBox.gameObject.SetActive(false);

		_knockbackReceiver.ApplyKnockback(damageData);
		DieAnimation();
	}

	public void StartAttraction(Transform target, Action AttractionCompleted)
	{
		_attractionCompleted = AttractionCompleted;

		DisableCollisions();
		_targetFollower.enabled = false;

		_isAttracted = true;
		_soulAnimator.AbsorptionDirection(_rigidbody.linearVelocity);

		_soulAttractor.StartAttraction(target);
	}

	public void OnAbsorptionCompleted()
	{
		_soulAnimator.PlayDeath();
	}

	private void OnAttractionCompletedInternal()
	{
		_isEndAttraction = true;

		_attractionCompleted?.Invoke();
		_attractionCompleted = null;
	}

	private void OnHitTarget(Collider2D targetCollider, DamageData damageData)
	{
		if (_isDead || _isAttracted)
			return;

		Vector3 targetPosition = targetCollider.transform.position;
		_knockbackReceiver.ApplyKnockbackFromPosition(targetPosition, _attackKnockbackForce);

		StartCoroutine(DisableTargetFollowerTemporarily());
	}

	private IEnumerator DisableTargetFollowerTemporarily()
	{
		_targetFollower.enabled = false;

		while (_knockbackReceiver.IsKnockedBack)
		{
			yield return null;
		}

		if (_isDead == false && _isAttracted == false)
		{
			_targetFollower.enabled = true;
		}
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

	private void DieAnimation()
	{
		DisableCollisions();
		_targetFollower.enabled = false;

		_soulAnimator.PlayDeath();
	}

	private void Dead()
	{
		_rigidbody.linearVelocity = Vector2.zero;
		_soulAnimator.Reset();
		gameObject.SetActive(false);
	}
}
