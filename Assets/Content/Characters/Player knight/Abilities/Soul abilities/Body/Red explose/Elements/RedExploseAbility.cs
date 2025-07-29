using UnityEngine;
using Sirenix.OdinInspector;

public class RedExploseAbility : MonoBehaviour, IAbilityBody, IKnockbackProvider
{
	[SerializeField, Required] private RedExploseAnimator _redExplosePrefab;

	[Header("Explosion Settings")]
	[SerializeField] private float _explosionRadius = 5f;
	[SerializeField] private float _knockbackForce = 15f;
	[SerializeField] private LayerMask _enemyLayerMask = -1;

	private RedExploseAnimator _redExplose;
	private Collider2D[] _collidersBuffer = new Collider2D[20];

	public bool HasVisualEffects => true;

	public void Initialize()
	{

	}

	public void InitializeVisualEffects(Transform effectsParent)
	{
		_redExplose = Instantiate(_redExplosePrefab, effectsParent.position, effectsParent.rotation, effectsParent);
	}

	public void Activate()
	{
		_redExplose.PlayAnimation();
		ApplyExplosionKnockback();
	}

	private void ApplyExplosionKnockback()
	{
		Vector2 explosionCenter = transform.position;

#pragma warning disable 0618
		int colliderCount = Physics2D.OverlapCircleNonAlloc(explosionCenter, _explosionRadius, _collidersBuffer, _enemyLayerMask);
#pragma warning restore 0618

		for (int i = 0; i < colliderCount; i++)
		{
			Collider2D collider = _collidersBuffer[i];
			if (collider.TryGetComponent<HurtBox>(out var hurtBox) &&
				hurtBox.Faction != null &&
				hurtBox.Faction.IsTagged(Faction.Enemy))
			{
				Vector2 direction = (collider.transform.position - transform.position).normalized;
				var damageData = new DamageData(0, DamageType.Physical, direction, _knockbackForce);

				hurtBox.ApplyDamage(damageData);
			}
		}
	}

	public void CalculateKnockback(Collider2D hitCollider, Collider2D target, out Vector2 direction, out float force)
	{
		direction = (target.transform.position - transform.position).normalized;
		force = _knockbackForce;
	}

	public bool CanBlockDamage()
	{
		return false;
	}

	public void DamageBlocked() { }

	public void Deactivate() { }
}
