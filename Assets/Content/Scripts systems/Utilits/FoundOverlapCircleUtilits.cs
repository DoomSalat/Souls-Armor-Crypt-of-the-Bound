using UnityEngine;

public static class FoundOverlapCircleUtilits
{
	public static Collider2D FindClosestEnemy(Vector2 center, float radius, LayerMask layerMask, Collider2D[] collidersBuffer)
	{
#pragma warning disable 0618
		int colliderCount = Physics2D.OverlapCircleNonAlloc(center, radius, collidersBuffer, layerMask);
#pragma warning restore 0618

		float closestDistance = float.MaxValue;
		Collider2D closestEnemy = null;

		for (int i = 0; i < colliderCount; i++)
		{
			Collider2D collider = collidersBuffer[i];

			if (collider != null && IsEnemy(collider))
			{
				float distance = Vector2.Distance(center, collider.transform.position);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestEnemy = collider;
				}
			}
		}

		return closestEnemy;
	}

	private static bool IsEnemy(Collider2D collider)
	{
		return collider.TryGetComponent<HurtBox>(out var hurtBox) &&
			   hurtBox.Faction != null &&
			   hurtBox.Faction.IsTagged(Faction.Enemy);
	}
}
