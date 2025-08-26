using UnityEngine;

public static class FoundOverlapCircleUtilits
{
	public static Collider2D FindClosestEnemy(Vector2 center, float radius, LayerMask layerMask, Collider2D[] collidersBuffer)
	{
#pragma warning disable 0618
		int colliderCount = Physics2D.OverlapCircleNonAlloc(center, radius, collidersBuffer, layerMask);
#pragma warning restore 0618

		float closestSqrDistance = float.MaxValue;
		Collider2D closestEnemy = null;

		for (int i = 0; i < colliderCount; i++)
		{
			Collider2D collider = collidersBuffer[i];

			if (collider != null && IsEnemy(collider))
			{
				Vector2 toCollider = (Vector2)collider.transform.position - center;
				float sqrDistance = toCollider.sqrMagnitude;
				if (sqrDistance < closestSqrDistance)
				{
					closestSqrDistance = sqrDistance;
					closestEnemy = collider;
				}
			}
		}

		return closestEnemy;
	}

	public static Collider2D[] FindCircleEnemys(Vector2 center, float radius, LayerMask layerMask, Collider2D[] collidersBuffer, int maxKills)
	{
		Collider2D[] closestEnemys = new Collider2D[maxKills];

#pragma warning disable 0618
		int colliderCount = Physics2D.OverlapCircleNonAlloc(center, radius, collidersBuffer, layerMask);
#pragma warning restore 0618

		if (colliderCount <= 0)
		{
			return closestEnemys;
		}

		bool[] usedIndexFlags = new bool[colliderCount];

		int filled = 0;
		while (filled < maxKills)
		{
			float bestSqrDistance = float.MaxValue;
			int bestIndex = -1;

			for (int i = 0; i < colliderCount; i++)
			{
				if (usedIndexFlags[i])
				{
					continue;
				}

				Collider2D candidate = collidersBuffer[i];
				if (candidate == null || !IsEnemy(candidate))
				{
					continue;
				}

				Vector2 toCandidate = (Vector2)candidate.transform.position - center;
				float sqrDistance = toCandidate.sqrMagnitude;
				if (sqrDistance < bestSqrDistance)
				{
					bestSqrDistance = sqrDistance;
					bestIndex = i;
				}
			}

			if (bestIndex == -1)
			{
				break;
			}

			usedIndexFlags[bestIndex] = true;
			closestEnemys[filled] = collidersBuffer[bestIndex];
			filled++;
		}

		return closestEnemys;
	}

	private static bool IsEnemy(Collider2D collider)
	{
		return collider.TryGetComponent<HurtBox>(out var hurtBox) &&
			   hurtBox.Faction != null &&
			   hurtBox.Faction.IsTagged(Faction.Enemy);
	}
}
