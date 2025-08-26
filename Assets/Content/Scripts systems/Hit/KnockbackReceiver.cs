using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
	[SerializeField, Min(0)] private float _knockbackMultiplier = 5f;
	[SerializeField, Min(0)] private float _maxKnockback = 100f;
	[SerializeField, Min(0)] private float _knockbackDuration = 0.2f;

	private Rigidbody2D _rigidbody;
	private Coroutine _knockbackCoroutine;

	public bool IsKnockedBack { get; private set; }

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	public void ApplyKnockback(DamageData damageData)
	{
		if (damageData.KnockbackForce <= 0)
			return;

		ApplyKnockback(damageData.KnockbackDirection, damageData.KnockbackForce);
	}

	public void ApplyKnockback(Vector2 direction, float force)
	{
		if (force <= 0)
		{
			return;
		}

		float knockback = Mathf.Min(force * _knockbackMultiplier, _maxKnockback);

		_rigidbody.linearVelocity = Vector2.zero;
		_rigidbody.AddForce(direction.normalized * knockback, ForceMode2D.Impulse);

		if (_knockbackCoroutine != null)
		{
			StopCoroutine(_knockbackCoroutine);
		}

		_knockbackCoroutine = StartCoroutine(KnockbackCoroutine());
	}

	public void ApplyKnockbackFromPosition(Vector3 fromPosition, float force)
	{
		Vector2 direction = (transform.position - fromPosition).normalized;
		ApplyKnockback(direction, force);
	}

	public void ApplyKnockbackToPosition(Vector3 toPosition, float force)
	{
		Vector2 direction = (toPosition - transform.position).normalized;
		ApplyKnockback(direction, force);
	}

	private IEnumerator KnockbackCoroutine()
	{
		IsKnockedBack = true;
		yield return new WaitForSeconds(_knockbackDuration);

		IsKnockedBack = false;
		_knockbackCoroutine = null;
	}

	public void StopKnockback()
	{
		if (_knockbackCoroutine != null)
		{
			StopCoroutine(_knockbackCoroutine);
			_knockbackCoroutine = null;
		}

		IsKnockedBack = false;
		_rigidbody.linearVelocity = Vector2.zero;
	}
}