using Sirenix.OdinInspector;
using UnityEngine;

public class TestDamage : MonoBehaviour
{
	[SerializeField] private GameObject _damageObject;

	private IDamageable _damageable;

	private void OnValidate()
	{
		if (_damageObject != null)
		{
			if (_damageObject.TryGetComponent(out IDamageable damageable))
			{
				_damageable = damageable;
			}
			else
			{
				Debug.LogError($"Object {_damageObject.name} does not have an IDamageable component.");
				_damageObject = null;
			}
		}
	}

	[Button(nameof(Damage))]
	private void Damage()
	{
		_damageable.TakeDamage(new DamageData(10, DamageType.Physical, Vector2.zero, 0));
	}
}
