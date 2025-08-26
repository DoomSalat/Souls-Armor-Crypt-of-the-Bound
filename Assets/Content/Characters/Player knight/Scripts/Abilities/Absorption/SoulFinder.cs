using Sirenix.OdinInspector;
using UnityEngine;

public class SoulFinder : MonoBehaviour
{
	[SerializeField, MinValue(0f)] private float _searchRadius = 5f;
	[SerializeField] private LayerMask _soulLayerMask;
	[SerializeField, MinValue(1)] private int _maxBuffer = 10;

	private readonly Collider2D[] _colliderBuffer;

	public SoulFinder()
	{
		_colliderBuffer = new Collider2D[_maxBuffer];
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.orange;
		Gizmos.DrawWireSphere(transform.position, _searchRadius);
	}

	public bool TryFindSoul(out ISoul soul)
	{
		soul = null;

		System.Array.Clear(_colliderBuffer, 0, _colliderBuffer.Length);
#pragma warning disable CS0618 // Type or member is obsolete
		var hitsCount = Physics2D.OverlapCircleNonAlloc(transform.position, _searchRadius, _colliderBuffer, _soulLayerMask);
#pragma warning restore CS0618 // Type or member is obsolete

		if (hitsCount == 0)
			return false;

		soul = FindNearest();

		if (soul != null)
		{
			return true;
		}

		return false;
	}

	private ISoul FindNearest()
	{
		ISoul nearestSoul = null;
		float minDistanceSqr = _searchRadius * _searchRadius;

		for (int i = 0; i < _colliderBuffer.Length; i++)
		{
			if (_colliderBuffer[i] == null)
				continue;

			//Debug.Log(_colliderBuffer[i].gameObject.name);

			if (_colliderBuffer[i].TryGetComponent(out ISoul soul) && _colliderBuffer[i].gameObject.activeInHierarchy)
			{
				Vector2 targetPosition2D = _colliderBuffer[i].transform.position;
				Vector2 currentPosition2D = transform.position;
				float distanceSqr = (targetPosition2D - currentPosition2D).sqrMagnitude;

				if (distanceSqr < minDistanceSqr)
				{
					minDistanceSqr = distanceSqr;
					nearestSoul = soul;
				}
			}
		}

		return nearestSoul;
	}
}
