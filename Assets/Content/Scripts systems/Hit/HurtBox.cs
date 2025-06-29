using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HurtBox : MonoBehaviour
{
	[SerializeField, Required] private GameObject _owner;

	private IDamageable _damagable;
	private FactionTag _factionTag;

	public FactionTag Faction => _factionTag;

	private void Awake()
	{
		_damagable = _owner.GetComponent<IDamageable>();
		TryGetComponent<FactionTag>(out _factionTag);
	}

	private void OnValidate()
	{
		if (_owner != null)
		{
			if (_owner.TryGetComponent<IDamageable>(out _) == false)
			{
				Debug.LogError($"Owner {_owner.name} does not implement {nameof(IDamageable)} in {nameof(HurtBox)} on {gameObject.name}");
				_owner = null;
			}
		}
	}

	public void ApplyDamage(DamageData damageData)
	{
		if (_damagable != null)
		{
			_damagable.TakeDamage(damageData);
		}
	}
}
