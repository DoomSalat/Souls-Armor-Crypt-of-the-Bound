using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Soul : MonoBehaviour, IDamagable
{
	[SerializeField] private Transform _target;
	[Space]
	[SerializeField, Required] private TargetFollower _targetFollower;
	[SerializeField, Required] private SmoothLook _eye;
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private HurtBox _hurtBox;
	[SerializeField, MinValue(0)] private float _knockbackMultiplier = 5f;
	[SerializeField, MinValue(0)] private float _maxKnockback = 100f;
	[SerializeField, MinValue(0)] private float _stopThreshold = 0.01f;

	private bool _isDead = false;

	private Rigidbody2D _rigidbody;
	private WaitUntil _waitKnockStop;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();

		_waitKnockStop = new WaitUntil(() => _rigidbody.linearVelocity.sqrMagnitude <= _stopThreshold); ;
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

		_rigidbody.linearVelocity = Vector2.zero;
		_eye.SetFollowing(false);
	}

	private IEnumerator WaitForStop()
	{
		if (_knockbackMultiplier <= 0)
		{
			Dead();
			yield break;
		}

		yield return new WaitForFixedUpdate();
		yield return _waitKnockStop;

		Dead();
	}
}
