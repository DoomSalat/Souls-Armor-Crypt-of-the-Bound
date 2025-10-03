using UnityEngine;
using StatusSystem;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpawnerSystem;

public class EnemyDamage : MonoBehaviour, IDamageable
{
	[SerializeField, Required] private Transform _statusPoint;

	[SerializeField, ReadOnly] private StatusMachine _statusMachine;

	private PooledEnemy _pooledEnemy;

	private Collider2D _collider;
	private HitBox _hitBox;
	private HurtBox _hurtBox;

	private bool _isDead = false;
	private HashSet<DamageType> _activeStatuses = new HashSet<DamageType>();

	public bool IsDead => _isDead;

	public event System.Action<DamageData> DeathRequested;

	public void InitializeCreate(StatusMachine statusMachine)
	{
		_statusMachine = statusMachine;
	}

	public virtual void Initialize(Collider2D collider, HitBox hitBox, HurtBox hurtBox)
	{
		_collider = collider;
		_hitBox = hitBox;
		_hurtBox = hurtBox;
		_pooledEnemy = GetComponent<PooledEnemy>();
	}

	public virtual void TakeDamage(DamageData damageData)
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
		DamageEffects(damageData);
		Death(damageData);
	}

	public void DisableCollisions()
	{
		if (_hitBox != null)
			_hitBox.SetColliderEnabled(false);

		if (_hurtBox != null)
			_hurtBox.SetColliderEnabled(false);

		if (_collider != null)
			_collider.enabled = false;
	}

	public void EnableCollisions()
	{
		if (_hitBox != null)
			_hitBox.SetColliderEnabled(true);

		if (_hurtBox != null)
			_hurtBox.SetColliderEnabled(true);

		if (_collider != null)
			_collider.enabled = true;
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

	private void ZeroDamage(DamageData damageData) { }

	protected virtual void DamageEffects(DamageData damageData) { }

	protected virtual void Death(DamageData damageData)
	{
		ClearStatus();

		DeathRequested?.Invoke(damageData);
	}

	public void CompleteDeath()
	{
		if (_pooledEnemy != null)
		{
			_pooledEnemy.ReturnToPool();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void ResetDeathState()
	{
		_isDead = false;
	}

	public void ForceDeath()
	{
		_isDead = true;
		DisableCollisions();
		Death(new DamageData(0, DamageType.Physical, Vector2.zero, 0));
	}
}