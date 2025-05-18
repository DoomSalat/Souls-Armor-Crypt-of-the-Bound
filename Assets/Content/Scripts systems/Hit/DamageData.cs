using UnityEngine;

public struct DamageData
{
	public float Amount { get; }
	public DamageType Type { get; }
	public Vector2 KnockbackDirection { get; }
	public float KnockbackForce { get; }

	public DamageData(float amount, DamageType type, Vector2 knockbackDirection, float knockbackForce)
	{
		if (amount < 0)
		{
			Debug.LogError($"Invalid damage amount: {amount}.");
			amount = 0;
		}

		Amount = amount;
		Type = type;
		KnockbackDirection = knockbackDirection.normalized;
		KnockbackForce = Mathf.Max(0, knockbackForce);
	}
}