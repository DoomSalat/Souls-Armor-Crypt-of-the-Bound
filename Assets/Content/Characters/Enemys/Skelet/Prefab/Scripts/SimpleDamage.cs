using UnityEngine;
using Sirenix.OdinInspector;

public class SimpleDamage : MonoBehaviour, IDamageable
{
	[SerializeField, Required] private HitBox _hitBox;
	[SerializeField, Required] private HurtBox _hurtBox;

	public event System.Action<SimpleDamage, DamageData> DamageReceived;
	public event System.Action<SimpleDamage, DamageType> StatusApplied;

	public void TakeDamage(DamageData damageData)
	{
		DamageReceived?.Invoke(this, damageData);
	}

	public void StatusEnd(DamageType statusType)
	{
		StatusApplied?.Invoke(this, statusType);
	}

	public void EnableCollisions()
	{
		_hitBox.SetColliderEnabled(true);
		_hurtBox.SetColliderEnabled(true);
	}

	public void DisableCollisions()
	{
		_hitBox.SetColliderEnabled(false);
		_hurtBox.SetColliderEnabled(false);
	}
}
