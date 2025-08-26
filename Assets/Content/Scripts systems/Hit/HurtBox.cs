using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HurtBox : MonoBehaviour
{
	[SerializeField, Required] private GameObject _owner;

	private IDamageable _damagable;
	private FactionTag _factionTag;
	private bool _isProcessingDamage;
	private Coroutine _resetRoutine;
	private WaitForFixedUpdate _waitForFixedUpdate;

	private Collider2D _collider;

	public FactionTag Faction => _factionTag;

	private void Awake()
	{
		_damagable = _owner.GetComponent<IDamageable>();
		TryGetComponent<FactionTag>(out _factionTag);
		_collider = GetComponent<Collider2D>();

		_waitForFixedUpdate = new WaitForFixedUpdate();
	}

	private void OnEnable()
	{
		_isProcessingDamage = false;
		_resetRoutine = null;
	}

	private void OnDisable()
	{
		if (_resetRoutine != null)
		{
			StopCoroutine(_resetRoutine);
			_resetRoutine = null;
		}
		_isProcessingDamage = false;
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
		if (_damagable != null && _isProcessingDamage == false)
		{
			_isProcessingDamage = true;
			_damagable.TakeDamage(damageData);

			if (_resetRoutine != null)
				StopCoroutine(_resetRoutine);

			_resetRoutine = StartCoroutine(ResetDamageProcessing());
		}
	}

	public void SetColliderEnabled(bool enabled)
	{
		if (_collider != null)
			_collider.enabled = enabled;
	}

	private IEnumerator ResetDamageProcessing()
	{
		yield return _waitForFixedUpdate;
		_isProcessingDamage = false;
		_resetRoutine = null;
	}
}
