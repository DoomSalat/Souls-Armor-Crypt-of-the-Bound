using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderIgnorer : MonoBehaviour
{
	[SerializeField, Required] private Collider2D[] _collidersToIgnore;

	private Collider2D _collider;

	private void Awake()
	{
		_collider = GetComponent<Collider2D>();
		OnEnable();
	}

	private void OnEnable()
	{
		if (_collider == null)
			return;

		IgnoreCollisions();
	}

	private void IgnoreCollisions()
	{
		if (_collidersToIgnore == null || _collidersToIgnore.Length == 0)
			return;

		foreach (var collider in _collidersToIgnore)
		{
			if (collider != null)
			{
				Physics2D.IgnoreCollision(_collider, collider, true);
			}
		}
	}

	private void OnValidate()
	{
		if (_collidersToIgnore != null)
		{
			for (int i = _collidersToIgnore.Length - 1; i >= 0; i--)
			{
				if (_collidersToIgnore[i] == null)
				{
					Debug.LogWarning($"Null коллайдер в {gameObject.name} был удалён из списка игнорируемых");
				}
			}
		}
	}
}