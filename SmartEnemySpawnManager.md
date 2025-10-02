# Умный Менеджер Спауна Врагов - Актуальная Реализация
## Архитектура AI Системы Спауна

---

## 🎯 ОБЗОР РЕАЛИЗОВАННОЙ СИСТЕМЫ

### Текущая Архитектура
- **12 секторов спауна** вокруг игрока (каждый 30°)
- **4 типа врагов**: Soul, SoulVase, Skelet, Knight
- **4 цвета душ**: Blue, Green, Red, Yellow
- **Система токенов (float)** для балансировки спауна
- **Система пресетов** с умным выбором секторов
- **Система глюков** для непредсказуемости
- **Мета-данные** через события
- **Прямой выбор секторов** через SpawnerEnemys

---

## 📊 СТРУКТУРА ДАННЫХ

### **1. DifficultyLevel**

```csharp
[System.Serializable]
public class DifficultyLevel
{
	private string _levelName;
	private int _defaultTokens = 3;
	
	private bool _enableWaves = true;
	private float _waveSpawnInterval = 2f;
	private int _waveThreshold = 3;
	private float _waveDuration = 15f;
}
```

### **2. EnemyData**

```csharp
public class EnemyData : ScriptableObject
{
	private int _tokenValue = 1;
	private float _spawnCooldown = 2f;
	private float _timerReduction = 0.5f;
	private float _spawnChance = 1f;
}
```

### **3. PresetData**

```csharp
public class PresetData : ScriptableObject
{
	private int _tokenCost = 3;
	private float _presetCooldown = 6f;
	private int _cooldownCycles = 2;  // Количество циклов кулдауна
	private EnemyPlacement[] _enemyPlacements;
	
	// Новый метод для получения весов по секторам
	public float[] GetSectionWeights()
	{
		float[] sectionWeights = new float[13]; // 0-12, где 0 всегда пустой
		foreach (var placement in _enemyPlacements)
		{
			if (placement.Section >= 1 && placement.Section <= 12)
			{
				sectionWeights[placement.Section] += placement.Count;
			}
		}
		
		return sectionWeights;
	}
}

public class EnemyPlacement
{
	private EnemyKind _enemyKind;
	private SoulType _soulType;  // Может быть Random для случайного выбора
	private int _section = 1;  // 1-12
	private int _count = 1;
}
```

---

## 🔄 СИСТЕМА СОБЫТИЙ

```
Враг умирает → ReturnToPool
→ SpawnerBase.EnemyReturnedToPool
→ SpawnerEnemys.EnemyReturnedToPoolEvent
→ SpawnerEnemysAI.OnEnemyReturnedToPool
→ Читает enemy.SpawnMeta
→ Возвращает токены + сокращает таймер
```

---

## 🎮 СИСТЕМА ПРЕСЕТОВ

```csharp
public PresetSpawnResult ProcessSpawn()
{
	// Выбор доступного пресета
	var selectedPreset = SelectAvailablePreset();
	
	if (selectedPreset == null)
	{
		// Все пресеты на кулдауне - пропускаем ход
		ReduceAllPresetCooldowns();
		return null;
	
	// Установка кулдауна для пресета
	SetPresetCooldown(selectedPreset.name, selectedPreset.CooldownCycles);
	
	// Спаун пресета
	var spawnedEnemies = SpawnPresetEnemies(
		selectedPreset.EnemyPlacements,
		selectedPreset.TokenCost,
		selectedPreset.PresetCooldown
	);
	
	return new PresetSpawnResult { SpawnedEnemies = spawnedEnemies };
}
```

---

## 🎭 УМНЫЙ ВЫБОР ПРЕСЕТОВ

```csharp
public PresetData SelectAvailablePreset()
{
	var availablePresets = GetAvailablePresets();
	var sectionWeights = GetCurrentSectionWeights();
	
	if (AllWeightsAreZero(sectionWeights))
	{
		// Все секторы пустые - выбираем самый дорогой пресет
		return SelectMostExpensivePreset(availablePresets);
	}
	
	// Выбираем пресет, который лучше всего балансирует секторы
	return SelectBalancingPreset(availablePresets, sectionWeights);
}

private PresetData SelectBalancingPreset(List<PresetData> presets, float[] weights)
{
	int weakestSection = FindWeakestSection(weights);
	PresetData bestPreset = null;
	float bestScore = float.MinValue;
	
	foreach (var preset in presets)
	{
		float score = CalculatePresetScoreForWeakSection(preset, weights, weakestSection);
		if (score > bestScore)
		{
			bestScore = score;
			bestPreset = preset;
		}
	}
	
	return bestPreset;
}
```

---

## 🎲 СИСТЕМА ГЛЮКОВ

```csharp
// Накопление каждые 5-10 сек
_glitchChance += 0.02f;  // +2%

// Проверка при выборе сектора
if (Random.value < _glitchChance)
{
	_glitchChance = 0f;  // Сброс!
	return RandomSection();
}
```

**Важно:** Глюк влияет **только** на выбор главного сектора!

---

## 🎯 СИСТЕМА СЕКТОРОВ (12 СЕКТОРОВ)

### Расположение секторов:
- **Сектор 1**: 0° (12 часов) - прямо вверх
- **Сектор 2**: 30° (1 час) - вправо-вверх  
- **Сектор 3**: 60° (2 часа) - вправо-вверх
- **Сектор 4**: 90° (3 часа) - вправо
- **Сектор 5**: 120° (4 часа) - вправо-вниз
- **Сектор 6**: 150° (5 часов) - вправо-вниз
- **Сектор 7**: 180° (6 часов) - вниз
- **Сектор 8**: 210° (7 часов) - влево-вниз
- **Сектор 9**: 240° (8 часов) - влево-вниз
- **Сектор 10**: 270° (9 часов) - влево
- **Сектор 11**: 300° (10 часов) - влево-вверх
- **Сектор 12**: 330° (11 часов) - влево-вверх

---

## 💡 КЛЮЧЕВЫЕ ПРАВИЛА

1. **Токены (float)**
   - Максимум = DifficultyLevel.DefaultTokens
   - Делятся между врагами пресета
   - Возвращаются через мета-файл

2. **Таймеры**
   - Пресет: PresetCooldown
   - Сокращаются на TimerReduction
   - Сохраняется предыдущая длительность при нуле токенов

3. **Пресеты**
   - Умный выбор на основе весов секторов
   - Система кулдаунов с циклами
   - Балансировка секторов через "равномерность"

4. **Секторы**
   - 12 секторов по 30° каждый
   - Сектор 1 = 0° (12 часов)
   - Прямой выбор через SpawnerEnemys
   - Индексация 1-12 (сектор 0 не используется)

---

## 🔧 ПРИМЕР РАБОТЫ

### Умный Выбор Пресета
```
Текущие веса секторов: [0, 2, 1, 3, 0, 1, 2, 0, 1, 2, 0, 1]
Самый слабый сектор: 1 (вес = 0)

Доступные пресеты:
- Пресет A: [1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0] (2 врага, 6 токенов)
- Пресет B: [0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0] (2 врага, 4 токена)

Выбирается Пресет A (помогает слабому сектору 1)
```

### Спаун Пресета
```
Пресет A: 2 врага, 6 токенов, 8 секунд
Каждый враг вернет: 3.0 токена (6/2)
Каждый враг сократит: 4.0 сек (8/2)

Результат после спауна:
Новые веса: [0, 2, 2, 3, 0, 1, 2, 0, 1, 2, 0, 1]
Сектор 1 получил +1 врага!
```

### Система Кулдаунов
```
Пресет A: кулдаун 2 цикла
Пресет B: кулдаун 1 цикл

Ход 1: Спаун Пресета A (кулдаун A = 2, B = 1)
Ход 2: Спаун Пресета B (кулдаун A = 1, B = 1)  
Ход 3: Пропуск (все на кулдауне, уменьшаем: A = 0, B = 0)
Ход 4: Спаун Пресета A (кулдаун A = 2, B = 0)
```

### Случайный Выбор Цвета Души
```
Пресет с SoulType.Random:
- Доступные спаунеры: Blue, Green, Red, Yellow
- Случайный выбор: Green
- Результат: Green Soul в секторе 1

Следующий спаун того же пресета:
- Случайный выбор: Red  
- Результат: Red Soul в секторе 1

Fallback при отсутствии спаунеров:
- Доступные спаунеры: []
- Fallback: Blue
- Результат: Blue Soul в секторе 1
```

---

## 🆕 НОВЫЕ ВОЗМОЖНОСТИ

### **Прямой выбор секторов**
- `SpawnerEnemys` напрямую конвертирует `SpawnDirection` → `SpawnSection`
- Нет зависимости от `SpawnerTokens.GetSectionByDirection()`
- Полный контроль над позиционированием

### **Умная балансировка секторов**
- Анализ весов всех 12 секторов
- Поиск самого слабого сектора
- Выбор пресета, который лучше всего помогает слабому сектору
- Расчет "равномерности" для оптимизации распределения

### **Система кулдаунов пресетов**
- Каждый пресет имеет свой кулдаун (в циклах)
- Если все пресеты на кулдауне - пропуск хода
- Автоматическое уменьшение кулдаунов
- Сброс кулдаунов при смене сложности

### **12-секторная система**
- Более точное позиционирование (30° вместо 45°)
- Сектор 1 = 12 часов (интуитивно)
- Правильные углы от +Y оси
- Совместимость с существующим кодом

### **Случайный выбор цвета души**
- `SoulType.Random` в пресетах
- Автоматический выбор из доступных цветов
- Динамическое определение доступных спаунеров
- Fallback на Blue при отсутствии доступных цветов

---

**Система полностью реализована и обновлена!** 🎯

