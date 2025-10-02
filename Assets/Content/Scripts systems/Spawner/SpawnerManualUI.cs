using Sirenix.OdinInspector;
using UnityEngine;

namespace SpawnerSystem
{
	public enum SpawnDirection
	{
		Section1,   // 0° = 12 часов
		Section2,   // 30° = 1 час
		Section3,   // 60° = 2 часа
		Section4,   // 90° = 3 часа
		Section5,   // 120° = 4 часа
		Section6,   // 150° = 5 часов
		Section7,   // 180° = 6 часов
		Section8,   // 210° = 7 часов
		Section9,   // 240° = 8 часов
		Section10,  // 270° = 9 часов
		Section11,  // 300° = 10 часов
		Section12   // 330° = 11 часов
	}

	[RequireComponent(typeof(SpawnerEnemys))]
	public class SpawnerManualUI : MonoBehaviour
	{
		[Header("Manual Spawner")]
		[SerializeField, Required] private SpawnerEnemys _spawnerEnemys;

		[Header("Spawn Direction")]
		[SerializeField] private SpawnDirection _selectedDirection = SpawnDirection.Section1;

		[HorizontalGroup("SpawnTable", 0.25f)]

		[VerticalGroup("SpawnTable/Blue")]
		[Title("Blue Souls")]
		[GUIColor(0.7f, 0.8f, 1f)]
		[VerticalGroup("SpawnTable/Blue/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnBlueSoul() => _spawnerEnemys.SpawnEnemy(SoulType.Blue, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Blue/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnBlueSoulVase() => _spawnerEnemys.SpawnEnemy(SoulType.Blue, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Blue/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnBlueSkelet() => _spawnerEnemys.SpawnEnemy(SoulType.Blue, EnemyKind.Skelet, _selectedDirection);

		[VerticalGroup("SpawnTable/Green")]
		[Title("Green Souls")]
		[GUIColor(0.8f, 1f, 0.8f)]
		[VerticalGroup("SpawnTable/Green/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnGreenSoul() => _spawnerEnemys.SpawnEnemy(SoulType.Green, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Green/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnGreenSoulVase() => _spawnerEnemys.SpawnEnemy(SoulType.Green, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Green/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnGreenSkelet() => _spawnerEnemys.SpawnEnemy(SoulType.Green, EnemyKind.Skelet, _selectedDirection);

		[VerticalGroup("SpawnTable/Red")]
		[Title("Red Souls")]
		[GUIColor(1f, 0.8f, 0.8f)]
		[VerticalGroup("SpawnTable/Red/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnRedSoul() => _spawnerEnemys.SpawnEnemy(SoulType.Red, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Red/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnRedSoulVase() => _spawnerEnemys.SpawnEnemy(SoulType.Red, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Red/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnRedSkelet() => _spawnerEnemys.SpawnEnemy(SoulType.Red, EnemyKind.Skelet, _selectedDirection);

		[VerticalGroup("SpawnTable/Yellow")]
		[Title("Yellow Souls")]
		[GUIColor(1f, 1f, 0.8f)]
		[VerticalGroup("SpawnTable/Yellow/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnYellowSoul() => _spawnerEnemys.SpawnEnemy(SoulType.Yellow, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Yellow/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnYellowSoulVase() => _spawnerEnemys.SpawnEnemy(SoulType.Yellow, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Yellow/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnYellowSkelet() => _spawnerEnemys.SpawnEnemy(SoulType.Yellow, EnemyKind.Skelet, _selectedDirection);

		[Button("Spawn Knight", ButtonSizes.Medium)]
		public void SpawnKnight() => _spawnerEnemys.SpawnEnemy(SoulType.None, EnemyKind.Knight, _selectedDirection);
	}
}
