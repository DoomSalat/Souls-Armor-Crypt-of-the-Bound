using Sirenix.OdinInspector;
using UnityEngine;

namespace SpawnerSystem
{
	public enum SpawnDirection
	{
		Right,      // 0° = 0 * 45° = 0°
		TopRight,   // 45° = 1 * 45° = 45°
		Top,        // 90° = 2 * 45° = 90°
		TopLeft,    // 135° = 3 * 45° = 135°
		Left,       // 180° = 4 * 45° = 180°
		BottomLeft, // 225° = 5 * 45° = 225°
		Bottom,     // 270° = 6 * 45° = 270°
		BottomRight // 315° = 7 * 45° = 315°
	}

	[RequireComponent(typeof(SpawnerEnemys))]
	public class SpawnerManualUI : MonoBehaviour
	{
		[Header("Manual Spawner")]
		[SerializeField, Required] private SpawnerEnemys _spawnerEnemys;

		[Header("Spawn Direction")]
		[SerializeField] private SpawnDirection _selectedDirection = SpawnDirection.Right;

		[HorizontalGroup("SpawnTable", 0.25f)]

		[VerticalGroup("SpawnTable/Blue")]
		[Title("Blue Souls")]
		[GUIColor(0.7f, 0.8f, 1f)]
		[VerticalGroup("SpawnTable/Blue/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnBlueSoul() => _spawnerEnemys.SpawnEnemyManually(SoulType.Blue, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Blue/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnBlueSoulVase() => _spawnerEnemys.SpawnEnemyManually(SoulType.Blue, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Blue/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnBlueSkelet() => _spawnerEnemys.SpawnEnemyManually(SoulType.Blue, EnemyKind.Skelet, _selectedDirection);

		[VerticalGroup("SpawnTable/Green")]
		[Title("Green Souls")]
		[GUIColor(0.8f, 1f, 0.8f)]
		[VerticalGroup("SpawnTable/Green/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnGreenSoul() => _spawnerEnemys.SpawnEnemyManually(SoulType.Green, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Green/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnGreenSoulVase() => _spawnerEnemys.SpawnEnemyManually(SoulType.Green, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Green/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnGreenSkelet() => _spawnerEnemys.SpawnEnemyManually(SoulType.Green, EnemyKind.Skelet, _selectedDirection);

		[VerticalGroup("SpawnTable/Red")]
		[Title("Red Souls")]
		[GUIColor(1f, 0.8f, 0.8f)]
		[VerticalGroup("SpawnTable/Red/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnRedSoul() => _spawnerEnemys.SpawnEnemyManually(SoulType.Red, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Red/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnRedSoulVase() => _spawnerEnemys.SpawnEnemyManually(SoulType.Red, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Red/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnRedSkelet() => _spawnerEnemys.SpawnEnemyManually(SoulType.Red, EnemyKind.Skelet, _selectedDirection);

		[VerticalGroup("SpawnTable/Yellow")]
		[Title("Yellow Souls")]
		[GUIColor(1f, 1f, 0.8f)]
		[VerticalGroup("SpawnTable/Yellow/Soul")]
		[Button("Soul", ButtonSizes.Medium)]
		public void SpawnYellowSoul() => _spawnerEnemys.SpawnEnemyManually(SoulType.Yellow, EnemyKind.Soul, _selectedDirection);
		[VerticalGroup("SpawnTable/Yellow/Vase")]
		[Button("Soul Vase", ButtonSizes.Medium)]
		public void SpawnYellowSoulVase() => _spawnerEnemys.SpawnEnemyManually(SoulType.Yellow, EnemyKind.SoulVase, _selectedDirection);
		[VerticalGroup("SpawnTable/Yellow/Skelet")]
		[Button("Skelet", ButtonSizes.Medium)]
		public void SpawnYellowSkelet() => _spawnerEnemys.SpawnEnemyManually(SoulType.Yellow, EnemyKind.Skelet, _selectedDirection);

		[Button("Spawn Knight", ButtonSizes.Medium)]
		public void SpawnKnight() => _spawnerEnemys.SpawnEnemyManually(SoulType.None, EnemyKind.Knight, _selectedDirection);
	}
}
