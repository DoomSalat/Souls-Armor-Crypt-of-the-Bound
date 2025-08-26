using UnityEngine;

public class SimpleDamage : MonoBehaviour, IDamageable
{
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
}
