using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(FactionTag))]
public class HitBox : MonoBehaviour
{
	[SerializeField] private float _damageAmount = 10f;
	[SerializeField] private DamageType _damageType = DamageType.Physical;
	[Space]
	[SerializeField] private GameObject _ownerKnockback;

	private Collider2D _collider;
	private FactionTag _ownerFaction;
	private IKnockbackProvider _knockbackProvider;

	private void Awake()
	{
		_collider = GetComponent<Collider2D>();
		_ownerFaction = GetComponent<FactionTag>();

		if (_ownerKnockback != null)
			_knockbackProvider = _ownerKnockback.GetComponent<IKnockbackProvider>();
	}

	private void OnValidate()
	{
		if (_ownerKnockback != null)
		{
			if (_ownerKnockback.TryGetComponent<IKnockbackProvider>(out _) == false)
			{
				Debug.LogError($"Owner {_ownerKnockback.name} does not implement {nameof(IKnockbackProvider)} in {nameof(HitBox)} on {gameObject.name}");
				_ownerKnockback = null;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent<HurtBox>(out var hurtBox) && CanDamage(hurtBox.Faction))
		{
			Vector2 direction = Vector2.zero;
			float force = 0f;

			if (_knockbackProvider != null)
			{
				_knockbackProvider.CalculateKnockback(_collider, other, out direction, out force);
			}
			else
			{
				direction = (other.transform.position - transform.position).normalized;
			}

			var damageData = new DamageData(
				_damageAmount,
				_damageType,
				direction,
				force
			);

			hurtBox.ApplyDamage(damageData);
		}
	}

	private bool CanDamage(FactionTag faction)
	{
		return _ownerFaction.Faction != faction.Faction;
	}
}
