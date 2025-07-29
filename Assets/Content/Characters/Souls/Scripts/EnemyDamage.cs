using UnityEngine;
using StatusSystem;
using System.Collections.Generic;

public class EnemyDamage : MonoBehaviour, IDamageable
{
	[SerializeField] private Transform _statusPoint;
	[SerializeField] private StatusMachine _statusMachine;

	protected KnockbackReceiver _knockbackReceiver;

	private bool _isDead = false;
	private HashSet<DamageType> _activeStatuses = new HashSet<DamageType>();

	public bool IsDead => _isDead;

	public void Initialize(KnockbackReceiver knockbackReceiver)
	{
		_knockbackReceiver = knockbackReceiver;
	}

	public void TakeDamage(DamageData damageData)
	{
		if (damageData.Amount <= 0)
		{
			ZeroDamage(damageData);
			return;
		}

		if (damageData.Type == DamageType.Poison)
		{
			ApplyStatus();
			return;
		}

		_isDead = true;
		_knockbackReceiver.ApplyKnockback(damageData);
		Death();
	}

	public void ApplyStatus()
	{
		if (_statusMachine != null && _statusPoint != null && !_activeStatuses.Contains(DamageType.Poison))
		{
			_activeStatuses.Add(DamageType.Poison);
			_statusMachine.ApplyStatus(DamageType.Poison, _statusPoint, this);
		}
	}

	public void ClearStatus()
	{
		if (_statusMachine != null)
		{
			_statusMachine.ClearAllStatuses(this);
			_activeStatuses.Clear();
		}
	}

	public void StatusEnd(DamageType statusType)
	{
		_activeStatuses.Remove(statusType);
	}

	private void ZeroDamage(DamageData damageData)
	{
		_knockbackReceiver.ApplyKnockback(damageData);
	}

	protected virtual void Death()
	{
		ClearStatus();
	}
}
