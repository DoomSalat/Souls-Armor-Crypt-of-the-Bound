using UnityEngine;

namespace SpawnerSystem
{
	public static class SpawnerSystemData
	{
		[System.Serializable]
		public struct SectionSpawnInfo
		{
			public float StartAngle;
			public float EndAngle;
			public float Radius;
			public Vector3 Center;
			public SpawnSection Section;

			public SectionSpawnInfo(float startAngle, float endAngle, float radius, Vector3 center, SpawnSection section)
			{
				StartAngle = startAngle;
				EndAngle = endAngle;
				Radius = radius;
				Center = center;
				Section = section;
			}
		}

		public enum SpawnSection
		{
			Section1 = 1,
			Section2 = 2,
			Section3 = 3,
			Section4 = 4,
			Section5 = 5,
			Section6 = 6,
			Section7 = 7,
			Section8 = 8,
			Section9 = 9,
			Section10 = 10,
			Section11 = 11,
			Section12 = 12
		}

		public const int SectionCount = 12;
		public const float SectionAngleDegrees = 360f / SectionCount;
		public const float SectionAngleRadians = Mathf.Deg2Rad * SectionAngleDegrees;
		public const float FullCircleDegrees = 360f;
		public const float FullCircleRadians = 2f * Mathf.PI;
		public static readonly int EnemyKindCount = System.Enum.GetValues(typeof(EnemyKind)).Length;

		public static float GetSectionAngle(int sectionIndex)
		{
			return Mathf.Clamp(sectionIndex, 0, SectionCount - 1) * SectionAngleDegrees;
		}

		public static float GetSectionAngleRadians(int sectionIndex)
		{
			return Mathf.Clamp(sectionIndex, 0, SectionCount - 1) * SectionAngleRadians;
		}

		public static int GetSectionIndex(float angleDegrees)
		{
			float normalizedAngle = angleDegrees % FullCircleDegrees;

			if (normalizedAngle < 0)
				normalizedAngle += FullCircleDegrees;

			return Mathf.FloorToInt(normalizedAngle / SectionAngleDegrees) % SectionCount;
		}

		public static int GetSectionIndexFromRadians(float angleRadians)
		{
			float angleDegrees = angleRadians * Mathf.Rad2Deg;

			return GetSectionIndex(angleDegrees);
		}
	}
}
