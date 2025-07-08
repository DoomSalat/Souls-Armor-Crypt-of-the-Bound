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
	[SerializeField, Required] private SoulAnimatorEvent _soulAnimatorEvent;
	[SerializeField, Required] private SmoothLook _eye;
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private HurtBox _hurtBox;
	[SerializeField, MinValue(0)] private float _knockbackMultiplier = 5f;
	[SerializeField, MinValue(0)] private float _maxKnockback = 100f;
	[SerializeField, MinValue(0)] private float _stopThreshold = 0.01f;
	[SerializeField, MinValue(0.01f)] private float _eyeKnockbackSpeedMultiplier = 3;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;

	private WaitUntil _waitKnockStop;
	private WaitForFixedUpdate _waitFixedKnockStop;

	private bool _isDead = false;
	private bool _isDying = false;
	private bool _isAttracted = false;
	private bool _isEndAttraction = false;
	private Vector3 _lockDeadEyePos;
	private event System.Action _attractionCompleted;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
		_collider = GetComponent<Collider2D>();

		_waitKnockStop = new WaitUntil(() => _rigidbody.linearVelocity.sqrMagnitude <= _stopThreshold);
		_waitFixedKnockStop = new WaitForFixedUpdate();
	}

	private void OnEnable()
	{
		_soulAttractor.AttractionCompleted += OnAttractionCompletedInternal;
		_soulAnimatorEvent.EndDeathExplosion += OnEndDeathExplosion;
	}

	private void OnDisable()
	{
		_soulAttractor.AttractionCompleted -= OnAttractionCompletedInternal;
		_soulAnimatorEvent.EndDeathExplosion -= OnEndDeathExplosion;
	}

	private void OnEndDeathExplosion()
	{
		Dead();
	}

	private void Update()
	{
		if (_isDying == false)
		{
			if (_isDead == false)
				_eye.LookAt(_rigidbody.linearVelocity);
			else
			{
				_eye.LookAt(_lockDeadEyePos, _eyeKnockbackSpeedMultiplier);
			}
		}

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
		Debug.Log($"Take damage: {gameObject.name}");

		_isDead = true;
		_hitBox.gameObject.SetActive(false);
		_hurtBox.gameObject.SetActive(false);

		ApplyKnockback(damageData);
		StartCoroutine(WaitForStop());
	}

	public void StartAttraction(Transform target, Action AttractionCompleted)
	{
		_attractionCompleted = AttractionCompleted;

		_hitBox.gameObject.SetActive(false);
		_hurtBox.gameObject.SetActive(false);
		_collider.enabled = false;
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

	private void ApplyKnockback(DamageData damageData)
	{
		_rigidbody.linearVelocity = Vector2.zero;

		if (damageData.KnockbackForce > 0)
		{
			float knockback = Mathf.Min(damageData.KnockbackForce * _knockbackMultiplier, _maxKnockback);

			_rigidbody.AddForce(damageData.KnockbackDirection * damageData.KnockbackForce * _knockbackMultiplier, ForceMode2D.Impulse);
		}
	}

	private void Dead()
	{
		Debug.Log($"{gameObject.name} is Dead.");

		_isDying = true;
		_rigidbody.linearVelocity = Vector2.zero;
		gameObject.SetActive(false);
	}

	private IEnumerator WaitForStop()
	{
		if (_knockbackMultiplier <= 0)
		{
			Dead();
			yield break;
		}

		yield return _waitFixedKnockStop;
		_lockDeadEyePos = _rigidbody.linearVelocity;
		yield return _waitKnockStop;

		Dead();
	}
}
