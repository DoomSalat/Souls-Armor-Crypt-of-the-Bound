using Sirenix.OdinInspector;
using UnityEngine;

namespace SpawnerSystem
{

	[RequireComponent(typeof(SpawnerEnemys))]
	public class SpawnerManualUI : MonoBehaviour
	{
		[Header("Manual Spawner")]
		[SerializeField, Required] private SpawnerEnemys _spawnerEnemys;

		[Header("Spawn Direction")]
		[SerializeField] private SpawnerSystemData.SpawnSection _selectedDirection = SpawnerSystemData.SpawnSection.Section1;

		[HorizontalGroup("SpawnTable", 0.25f)]

		[VerticalGroup("SpawnTable/Blue")]
		[Title("Blue Souls")]
		[GUIColor(0.7f, 0.8f, 1f)]
		[VerticalGroup("SpawnTable/Blue/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnBlueSoul() => SpawnEnemyUnified(SoulType.Blue, EnemyKind.Soul);
		[VerticalGroup("SpawnTable/Blue/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnBlueSoulVase() => SpawnEnemyUnified(SoulType.Blue, EnemyKind.SoulVase);
		[VerticalGroup("SpawnTable/Blue/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnBlueSkelet() => SpawnEnemyUnified(SoulType.Blue, EnemyKind.Skelet);

		[VerticalGroup("SpawnTable/Green")]
		[Title("Green Souls")]
		[GUIColor(0.8f, 1f, 0.8f)]
		[VerticalGroup("SpawnTable/Green/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnGreenSoul() => SpawnEnemyUnified(SoulType.Green, EnemyKind.Soul);
		[VerticalGroup("SpawnTable/Green/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnGreenSoulVase() => SpawnEnemyUnified(SoulType.Green, EnemyKind.SoulVase);
		[VerticalGroup("SpawnTable/Green/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnGreenSkelet() => SpawnEnemyUnified(SoulType.Green, EnemyKind.Skelet);

		[VerticalGroup("SpawnTable/Red")]
		[Title("Red Souls")]
		[GUIColor(1f, 0.8f, 0.8f)]
		[VerticalGroup("SpawnTable/Red/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnRedSoul() => SpawnEnemyUnified(SoulType.Red, EnemyKind.Soul);
		[VerticalGroup("SpawnTable/Red/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnRedSoulVase() => SpawnEnemyUnified(SoulType.Red, EnemyKind.SoulVase);
		[VerticalGroup("SpawnTable/Red/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnRedSkelet() => SpawnEnemyUnified(SoulType.Red, EnemyKind.Skelet);

		[VerticalGroup("SpawnTable/Yellow")]
		[Title("Yellow Souls")]
		[GUIColor(1f, 1f, 0.8f)]
		[VerticalGroup("SpawnTable/Yellow/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnYellowSoul() => SpawnEnemyUnified(SoulType.Yellow, EnemyKind.Soul);
		[VerticalGroup("SpawnTable/Yellow/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnYellowSoulVase() => SpawnEnemyUnified(SoulType.Yellow, EnemyKind.SoulVase);
		[VerticalGroup("SpawnTable/Yellow/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnYellowSkelet() => SpawnEnemyUnified(SoulType.Yellow, EnemyKind.Skelet);

		[Button("Spawn Knight", ButtonSizes.Medium)]
		public void SpawnKnight() => SpawnEnemyUnified(SoulType.None, EnemyKind.Knight);

		private void SpawnEnemyUnified(SoulType soulType, EnemyKind enemyKind)
		{
			_spawnerEnemys.SpawnEnemy(soulType, enemyKind, _selectedDirection);
		}

	}
}
