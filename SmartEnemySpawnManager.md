# Умный Менеджер Спауна Врагов
## Принципы и Архитектура Интеллектуального Спауна

---

## 🎯 ОБЗОР СИСТЕМЫ

### Текущая Архитектура
- **8 секторов спауна** вокруг игрока (каждый 45°)
- **4 типа врагов**: Soul, SoulVase, Skelet, Knight
- **4 типа душ**: Blue, Green, Red, Yellow
- **4 уровня сложности**: 0, 1, 2, 3
- **Система токенов** для балансировки нагрузки по секторам

---

## 🧠 ПРИНЦИПЫ УМНОГО СПАУНА

### 1. **Адаптивная Сложность по Уровням**

#### **Уровень 0 (Очень Легкий)**
- **Soul**: 80% вероятность, 1-2 за спаун
- **SoulVase**: 20% вероятность, 1 за спаун
- **Skelet**: 0% (заблокирован)
- **Knight**: 0% (заблокирован)
- **Интервал спауна**: 8-12 секунд

#### **Уровень 1 (Легкий)**
- **Soul**: 70% вероятность, 1-3 за спаун
- **SoulVase**: 25% вероятность, 1-2 за спаун
- **Skelet**: 5% вероятность, 1 за спаун
- **Knight**: 0% (заблокирован)
- **Интервал спауна**: 6-10 секунд

#### **Уровень 2 (Средний)**
- **Soul**: 50% вероятность, 2-4 за спаун
- **SoulVase**: 30% вероятность, 1-2 за спаун
- **Skelet**: 15% вероятность, 1 за спаун
- **Knight**: 5% вероятность, 1 за спаун
- **Интервал спауна**: 4-8 секунд

#### **Уровень 3 (Сложный)**
- **Soul**: 40% вероятность, 2-5 за спаун
- **SoulVase**: 25% вероятность, 1-3 за спаун
- **Skelet**: 20% вероятность, 1-2 за спаун
- **Knight**: 15% вероятность, 1 за спаун
- **Интервал спауна**: 3-6 секунд

### 2. **Интеллектуальный Выбор Секторов**

#### **Алгоритм "Противоположная Сторона"**
```csharp
// Текущая логика уже реализована в SpawnerTokens
int oppositeSection = (lowestPowerSection + SectionCount / 2) % SectionCount;
int deviation = Random.Range(-_maxSectionDeviation, _maxSectionDeviation + 1);
int targetSection = (oppositeSection + deviation + SectionCount) % SectionCount;
```

#### **Улучшения для Умного Спауна**
- **Анализ позиции игрока**: Спаун в "слепых зонах"
- **Учет движения игрока**: Предсказание направления
- **Балансировка нагрузки**: Равномерное распределение по секторам

### 3. **Комплементарные Комбинации Врагов**

#### **Система Синергии**
```csharp
// Уже реализовано в SpawnerTokens.SuggestComplementaryKind()
if (knight > 0 && skelet == 0) return EnemyKind.Skelet;      // Рыцарь + Скелет
if (skelet > 0 && knight == 0) return EnemyKind.Knight;      // Скелет + Рыцарь  
if (soul + vase > 2) return EnemyKind.Skelet;                 // Много душ → Скелет
```

#### **Расширенные Комбинации**
- **Soul + SoulVase**: Базовая комбинация для всех уровней
- **Skelet + Soul**: Дальний бой + ближний бой
- **Knight + Skelet**: Два сильных врага (только уровни 2-3)
- **Soul + Soul + SoulVase**: Групповая атака

### 4. **Система Глюков (Накопительная)**

#### **Принцип Работы**
Система глюков работает по принципу **накопления шанса** с периодическим сбросом:

```csharp
// Настройки системы глюков
[SerializeField, Range(0, 1)] private float _glitchAccumulationRate = 0.02f;  // 2% за период
[SerializeField, MinValue(1)] private Vector2 _glitchAccumulationTime = new(5f, 10f);  // 5-10 секунд
[SerializeField, Range(0, 1)] private float _maxGlitchChance = 1f;  // Максимум 100%

// Логика накопления
private float _currentGlitchChance = 0f;
private float _nextAccumulationTime;

private void Update()
{
    if (_enableGlitchSystem && Time.time >= _nextAccumulationTime)
    {
        AccumulateGlitchChance();
    }
}

private void AccumulateGlitchChance()
{
    _currentGlitchChance += _glitchAccumulationRate;
    _currentGlitchChance = Mathf.Min(_currentGlitchChance, _maxGlitchChance);
    _nextAccumulationTime = Time.time + Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);
}
```

#### **Алгоритм Срабатывания**
```csharp
public SpawnSection SelectSection()
{
    // Проверка глюка перед обычной логикой
    if (_enableGlitchSystem && ShouldTriggerGlitch())
    {
        return (SpawnSection)Random.Range(0, SectionCount);  // Полностью случайный сектор
    }
    
    // Обычная AI логика выбора сектора
    return SelectNewTargetSection();
}

private bool ShouldTriggerGlitch()
{
    bool shouldGlitch = Random.value < _currentGlitchChance;
    
    if (shouldGlitch)
    {
        // ПОЛНЫЙ СБРОС при срабатывании
        _currentGlitchChance = 0f;
        _nextAccumulationTime = Time.time + Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);
    }
    
    return shouldGlitch;
}
```

#### **Пример Работы Системы**
```
Время: 0s   → Шанс: 0%    (начальное состояние)
Время: 7s   → Шанс: 2%    (первое накопление)
Время: 12s  → Шанс: 4%    (второе накопление)
Время: 18s  → Шанс: 6%    (третье накопление)
Время: 20s  → ГЛЮК!       (сработал, шанс сброшен в 0%)
Время: 25s  → Шанс: 2%    (накопление началось заново)
```

#### **Ключевые Особенности**
- **Накопительный принцип**: Шанс растет каждые 5-10 секунд
- **Полный сброс**: При срабатывании глюка шанс обнуляется
- **Случайные интервалы**: Время между накоплениями варьируется
- **Ограничение потолка**: Максимальный шанс настраивается
- **Полная случайность**: При глюке сектор выбирается полностью случайно

#### **Конфигурации Системы Глюков**

**Консервативная (Редкие Глюки):**
```csharp
_glitchAccumulationRate = 0.01f;      // 1% за период
_glitchAccumulationTime = new(10f, 15f);  // 10-15 секунд
_maxGlitchChance = 0.2f;              // Максимум 20%
```

**Агрессивная (Частые Глюки):**
```csharp
_glitchAccumulationRate = 0.05f;      // 5% за период
_glitchAccumulationTime = new(3f, 6f);    // 3-6 секунд
_maxGlitchChance = 1f;                 // Максимум 100%
```

**Сбалансированная (Средняя Частота):**
```csharp
_glitchAccumulationRate = 0.02f;      // 2% за период
_glitchAccumulationTime = new(5f, 10f);   // 5-10 секунд
_maxGlitchChance = 0.5f;               // Максимум 50%
```

#### **Настройки AI Компонента**

**Базовые AI Параметры:**
```csharp
[Header("AI Settings")]
[SerializeField, MinValue(0)] private float _desiredPowerPerSection = 5f;  // Желаемая мощность сектора
[SerializeField, MinValue(0)] private int _maxSectionDeviation = 2;        // Максимальное отклонение
```

**Умный Спаун:**
```csharp
[Header("Smart Spawning")]
[SerializeField] private bool _enableComplementaryAnalysis = true;    // Комплементарный анализ
[SerializeField] private bool _enableOppositeSectionLogic = true;   // Логика противоположной стороны
[SerializeField] private bool _enableGlitchSystem = true;           // Система глюков
```

**Отладка:**
```csharp
[Header("Debug")]
[SerializeField] private bool _showDebugInfo = false;  // Показывать debug информацию
```

#### **Отладочные Инструменты**
```csharp
// Публичные свойства для мониторинга
public float CurrentGlitchChance => _currentGlitchChance;
public float TimeToNextAccumulation => _nextAccumulationTime - Time.time;

// Кнопки отладки в Inspector
[Button("Force Glitch")]     // Установить 100% шанс
[Button("Show Current State")]  // Показать полную информацию
```

### 5. **Динамическая Адаптация**

#### **Система "Кризисного Режима"**
```csharp
public class CrisisManager
{
    private float _playerHealthPercentage;
    private int _consecutiveDeaths;
    private float _timeSinceLastKill;
    
    public bool IsInCrisis => _playerHealthPercentage < 0.3f || 
                              _consecutiveDeaths > 2 || 
                              _timeSinceLastKill > 30f;
    
    public void AdjustSpawnRates()
    {
        if (IsInCrisis)
        {
            // Уменьшить сложность на 50%
            // Увеличить шанс Soul на 30%
            // Заблокировать Knight временно
        }
    }
}
```

#### **Система "Мастерства"**
```csharp
public class MasteryTracker
{
    private float _killRate;           // Убийств в минуту
    private float _survivalTime;      // Время выживания
    private int _comboCount;          // Комбо убийств
    
    public DifficultyAdjustment GetAdjustment()
    {
        if (_killRate > 5f && _comboCount > 10)
        {
            return DifficultyAdjustment.Increase; // Увеличить сложность
        }
        return DifficultyAdjustment.Maintain;
    }
}
```

---

## 🎮 ВАРИАНТЫ УМНОГО СПАУНА

### **Вариант 1: Простой Адаптивный**
```csharp
public class SimpleAdaptiveSpawner
{
    private int _currentDifficulty = 0;
    private float _playerPerformance;
    
    public EnemyKind ChooseEnemyKind()
    {
        float[] weights = GetWeightsByDifficulty(_currentDifficulty);
        return WeightedRandomSelect(weights);
    }
    
    private float[] GetWeightsByDifficulty(int level)
    {
        return level switch
        {
            0 => new float[] { 0.8f, 0.2f, 0.0f, 0.0f }, // Soul, Vase, Skelet, Knight
            1 => new float[] { 0.7f, 0.25f, 0.05f, 0.0f },
            2 => new float[] { 0.5f, 0.3f, 0.15f, 0.05f },
            3 => new float[] { 0.4f, 0.25f, 0.2f, 0.15f },
            _ => new float[] { 0.4f, 0.25f, 0.2f, 0.15f }
        };
    }
}
```

### **Вариант 2: Машинное Обучение**
```csharp
public class MLBasedSpawner
{
    private NeuralNetwork _difficultyPredictor;
    private PlayerBehaviorAnalyzer _behaviorAnalyzer;
    
    public EnemyKind PredictOptimalEnemy()
    {
        float[] playerState = _behaviorAnalyzer.GetCurrentState();
        float[] prediction = _difficultyPredictor.Predict(playerState);
        
        return SelectEnemyFromPrediction(prediction);
    }
    
    private EnemyKind SelectEnemyFromPrediction(float[] prediction)
    {
        // Использует предсказание нейросети для выбора врага
        // Учитывает паттерны поведения игрока
    }
}
```

### **Вариант 3: Гибридный Интеллектуальный**
```csharp
public class HybridSmartSpawner
{
    private CrisisManager _crisisManager;
    private MasteryTracker _masteryTracker;
    private ComplementaryAnalyzer _complementaryAnalyzer;
    private PlayerPositionPredictor _positionPredictor;
    
    public SpawnDecision MakeSpawnDecision()
    {
        // 1. Анализ кризисной ситуации
        if (_crisisManager.IsInCrisis)
            return CreateCrisisSpawn();
            
        // 2. Анализ мастерства игрока
        var masteryAdjustment = _masteryTracker.GetAdjustment();
        if (masteryAdjustment == DifficultyAdjustment.Increase)
            return CreateChallengeSpawn();
            
        // 3. Комплементарный анализ
        var complementaryKind = _complementaryAnalyzer.SuggestComplementary();
        if (complementaryKind != EnemyKind.Soul)
            return CreateComplementarySpawn(complementaryKind);
            
        // 4. Позиционный анализ
        var optimalSection = _positionPredictor.PredictOptimalSection();
        return CreatePositionalSpawn(optimalSection);
    }
}
```

---

## 🔧 РЕАЛИЗАЦИЯ В СУЩЕСТВУЮЩЕЙ СИСТЕМЕ

### **SpawnerEnemysAI - Полная Реализация**

#### **Структура Компонента**
```csharp
[RequireComponent(typeof(SpawnerTokens))]
public class SpawnerEnemysAI : MonoBehaviour
{
    // Константы
    private const int SectionCount = 8;
    private const int EnemyKindCount = 4;
    private const float DefaultWeight = 1f;
    
    // AI настройки
    [Header("AI Settings")]
    [SerializeField, MinValue(0)] private float _desiredPowerPerSection = 5f;
    [SerializeField, MinValue(0)] private int _maxSectionDeviation = 2;
    
    // Система глюков
    [Header("Glitch Spawn System")]
    [SerializeField, Range(0, 1)] private float _glitchAccumulationRate = 0.02f;
    [SerializeField, MinValue(1)] private Vector2 _glitchAccumulationTime = new(5f, 10f);
    [SerializeField, Range(0, 1)] private float _maxGlitchChance = 1f;
    
    // Умный спаун
    [Header("Smart Spawning")]
    [SerializeField] private bool _enableComplementaryAnalysis = true;
    [SerializeField] private bool _enableOppositeSectionLogic = true;
    [SerializeField] private bool _enableGlitchSystem = true;
    
    // Отладка
    [Header("Debug")]
    [SerializeField] private bool _showDebugInfo = false;
}
```

#### **Публичные Свойства**
```csharp
// Состояние AI
public bool IsTargetingNewSection => _isTargetingNewSection;
public int CurrentTargetSection => _currentTargetSection;
public float CurrentSectionPower => _currentSectionPower;

// Система глюков
public float CurrentGlitchChance => _currentGlitchChance;
public float TimeToNextAccumulation => _nextAccumulationTime - Time.time;
```

#### **Основные Методы**

**1. Выбор Сектора (Главный Метод)**
```csharp
public SpawnSection SelectSection()
{
    // 1. Проверка глюка (приоритет)
    if (_enableGlitchSystem && ShouldTriggerGlitch())
    {
        var glitchSection = (SpawnSection)Random.Range(0, SectionCount);
        if (_showDebugInfo)
            Debug.Log($"[SpawnerEnemysAI] Glitch triggered! Random section: {glitchSection}");
        return glitchSection;
    }

    // 2. Обычная AI логика
    if (_isTargetingNewSection || _currentSectionPower >= _desiredPowerPerSection)
    {
        _currentTargetSection = SelectNewTargetSection();
        _currentSectionPower = 0f;
        _isTargetingNewSection = false;
    }

    return (SpawnSection)_currentTargetSection;
}
```

**2. Комплементарный Анализ**
```csharp
public EnemyKind SuggestComplementaryKind(SpawnSection section)
{
    if (!_enableComplementaryAnalysis)
    {
        // Случайный выбор если анализ отключен
        EnemyKind[] kinds = { EnemyKind.Soul, EnemyKind.SoulVase, EnemyKind.Skelet, EnemyKind.Knight };
        return kinds[Random.Range(0, kinds.Length)];
    }

    // Получаем данные о врагах в секторе
    int[] enemyCounts = _spawnerTokens.GetEnemyCountsBySection(section);
    int soul = enemyCounts[(int)EnemyKind.Soul];
    int vase = enemyCounts[(int)EnemyKind.SoulVase];
    int skelet = enemyCounts[(int)EnemyKind.Skelet];
    int knight = enemyCounts[(int)EnemyKind.Knight];

    // Комплементарная логика
    if (knight > 0 && skelet == 0) return EnemyKind.Skelet;
    else if (skelet > 0 && knight == 0) return EnemyKind.Knight;
    else if (soul + vase > 2) return EnemyKind.Skelet;
    else return EnemyKind.Soul;
}
```

**3. Система Глюков**
```csharp
private void Update()
{
    if (_enableGlitchSystem && Time.time >= _nextAccumulationTime)
    {
        AccumulateGlitchChance();
    }
}

private void AccumulateGlitchChance()
{
    _currentGlitchChance += _glitchAccumulationRate;
    _currentGlitchChance = Mathf.Min(_currentGlitchChance, _maxGlitchChance);
    
    _lastGlitchAccumulationTime = Time.time;
    _nextAccumulationTime = Time.time + Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);
    
    if (_showDebugInfo)
    {
        Debug.Log($"[SpawnerEnemysAI] Glitch chance accumulated: {_currentGlitchChance:P1} " +
                  $"(next accumulation in {_nextAccumulationTime - Time.time:F1}s)");
    }
}

private bool ShouldTriggerGlitch()
{
    if (!_enableGlitchSystem) return false;
    
    bool shouldGlitch = Random.value < _currentGlitchChance;
    
    if (shouldGlitch)
    {
        _currentGlitchChance = 0f;
        _lastGlitchAccumulationTime = Time.time;
        _nextAccumulationTime = Time.time + Random.Range(_glitchAccumulationTime.x, _glitchAccumulationTime.y);
        
        if (_showDebugInfo)
        {
            Debug.Log($"[SpawnerEnemysAI] Glitch triggered! Chance was {_currentGlitchChance:P1}, reset to 0%");
        }
    }
    
    return shouldGlitch;
}
```

**4. Дополнительные Методы**
```csharp
// Управление секторами
public void ForceRetarget()
{
    _isTargetingNewSection = true;
    if (_showDebugInfo)
        Debug.Log("[SpawnerEnemysAI] Force retarget triggered");
}

public void CommitSectionPower(SpawnSection section, float powerAdded)
{
    int index = (int)section;
    if (index == _currentTargetSection)
    {
        _currentSectionPower += powerAdded;
        if (_showDebugInfo)
            Debug.Log($"[SpawnerEnemysAI] Section {section} power: {_currentSectionPower}/{_desiredPowerPerSection}");
    }
}

// Делегирование в SpawnerTokens
public float GetKindWeight(EnemyKind kind) => _spawnerTokens.GetKindWeight(kind);
public Vector3 GetSpawnPosition(SpawnSection section) => _spawnerTokens.GetSpawnPosition(section);
public SpawnSection GetSectionByDirection(SpawnDirection direction) => _spawnerTokens.GetSectionByDirection(direction);
```

**5. Отладочные Инструменты**
```csharp
[Button("Force Glitch")]
private void ForceGlitch()
{
    _currentGlitchChance = 1f; // 100% шанс
    Debug.Log("[SpawnerEnemysAI] Glitch chance set to 100%!");
}

[Button("Force Retarget")]
private void ForceRetargetDebug()
{
    ForceRetarget();
}

[Button("Show Current State")]
private void ShowCurrentState()
{
    float timeToNextAccumulation = _nextAccumulationTime - Time.time;
    Debug.Log($"[SpawnerEnemysAI] Current State:\n" +
        $"Target Section: {_currentTargetSection}\n" +
        $"Section Power: {_currentSectionPower}/{_desiredPowerPerSection}\n" +
        $"Is Targeting New: {_isTargetingNewSection}\n" +
        $"Glitch Chance: {_currentGlitchChance:P1}\n" +
        $"Next Accumulation: {timeToNextAccumulation:F1}s");
}
```

**6. Логика Выбора Секторов**
```csharp
private int SelectNewTargetSection()
{
    if (!_enableOppositeSectionLogic)
    {
        // Простой случайный выбор если логика отключена
        return Random.Range(0, SectionCount);
    }

    // Получаем веса секторов из SpawnerTokens
    float[] sectionWeights = _spawnerTokens.GetSectionWeights();
    
    // Находим сектор с наименьшим весом
    int lowestPowerSection = 0;
    float lowestPower = sectionWeights[0];
    
    for (int i = 1; i < sectionWeights.Length; i++)
    {
        if (sectionWeights[i] < lowestPower)
        {
            lowestPower = sectionWeights[i];
            lowestPowerSection = i;
        }
    }
    
    // Выбираем противоположный сектор с отклонением
    int oppositeSection = (lowestPowerSection + SectionCount / 2) % SectionCount;
    int deviation = Random.Range(-_maxSectionDeviation, _maxSectionDeviation + 1);
    int targetSection = (oppositeSection + deviation + SectionCount) % SectionCount;
    
    if (_showDebugInfo)
    {
        Debug.Log($"[SpawnerEnemysAI] Selected section {targetSection} " +
                  $"(opposite to {lowestPowerSection}, deviation: {deviation})");
    }
    
    return targetSection;
}
```

**7. Валидация Параметров**
```csharp
#if UNITY_EDITOR
private void OnValidate()
{
    // Валидация системы глюков
    if (_glitchAccumulationRate < 0f) _glitchAccumulationRate = 0f;
    if (_glitchAccumulationRate > 1f) _glitchAccumulationRate = 1f;
    if (_glitchAccumulationTime.x < 1f) _glitchAccumulationTime.x = 1f;
    if (_glitchAccumulationTime.y < _glitchAccumulationTime.x) _glitchAccumulationTime.y = _glitchAccumulationTime.x;
    if (_maxGlitchChance < 0f) _maxGlitchChance = 0f;
    if (_maxGlitchChance > 1f) _maxGlitchChance = 1f;
    
    // Валидация AI параметров
    if (_maxSectionDeviation < 0) _maxSectionDeviation = 0;
}
#endif
```

### **Расширение SpawnerTokens**
```csharp
public class SmartSpawnerTokens : SpawnerTokens
{
    [Header("Smart Spawning")]
    [SerializeField] private int _currentDifficultyLevel = 0;
    [SerializeField] private bool _enableCrisisMode = true;
    [SerializeField] private bool _enableMasteryTracking = true;
    
    private CrisisManager _crisisManager;
    private MasteryTracker _masteryTracker;
    
    public override EnemyKind SuggestComplementaryKind(SpawnSection section)
    {
        // Базовый комплементарный анализ
        var baseSuggestion = base.SuggestComplementaryKind(section);
        
        // Кризисный режим
        if (_crisisManager?.IsInCrisis == true)
        {
            return EnemyKind.Soul; // Только простые враги в кризисе
        }
        
        // Адаптация по мастерству
        var masteryAdjustment = _masteryTracker?.GetAdjustment();
        if (masteryAdjustment == DifficultyAdjustment.Increase)
        {
            return GetHarderEnemy(baseSuggestion);
        }
        
        return baseSuggestion;
    }
    
    public float GetKindWeight(EnemyKind kind, int difficultyLevel)
    {
        var baseWeight = GetKindWeight(kind);
        var difficultyMultiplier = GetDifficultyMultiplier(kind, difficultyLevel);
        
        return baseWeight * difficultyMultiplier;
    }
}
```

### **Новый Компонент: DifficultyManager**
```csharp
public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [SerializeField] private int _currentLevel = 0;
    [SerializeField] private float _levelTransitionTime = 30f;
    [SerializeField] private bool _autoAdjustDifficulty = true;
    
    [Header("Performance Tracking")]
    [SerializeField] private float _playerHealthThreshold = 0.3f;
    [SerializeField] private int _maxConsecutiveDeaths = 3;
    [SerializeField] private float _masteryKillRate = 5f;
    
    public int CurrentLevel => _currentLevel;
    public bool ShouldIncreaseDifficulty => CheckMasteryConditions();
    public bool ShouldDecreaseDifficulty => CheckCrisisConditions();
    
    private bool CheckMasteryConditions()
    {
        // Логика проверки мастерства игрока
        return _playerKillRate > _masteryKillRate && 
               _playerSurvivalTime > 60f;
    }
    
    private bool CheckCrisisConditions()
    {
        // Логика проверки кризисной ситуации
        return _playerHealth < _playerHealthThreshold || 
               _consecutiveDeaths >= _maxConsecutiveDeaths;
    }
}
```

---

## 📊 МЕТРИКИ И БАЛАНСИРОВКА

### **Ключевые Метрики**
- **Время выживания игрока** (цель: 2-5 минут на уровне)
- **Соотношение убийств/смертей** (цель: 3:1)
- **Покрытие секторов** (равномерное распределение)
- **Частота кризисных ситуаций** (не чаще 1 раза в 2 минуты)

### **Автоматическая Балансировка**
```csharp
public class AutoBalancer
{
    public void AdjustSpawnRates()
    {
        if (_playerDeathRate > 0.5f)
        {
            // Уменьшить сложность
            DecreaseDifficulty();
        }
        else if (_playerKillRate > 8f)
        {
            // Увеличить сложность
            IncreaseDifficulty();
        }
    }
}
```

---

## 🎯 РЕКОМЕНДАЦИИ ПО ВНЕДРЕНИЮ

### **✅ Этап 0: Система Глюков (УЖЕ РЕАЛИЗОВАНА)**
1. ✅ **SpawnerEnemysAI** - накопительная система глюков
2. ✅ **Накопление шанса** - каждые 5-10 секунд +2%
3. ✅ **Полный сброс** - при срабатывании глюка
4. ✅ **Отладочные инструменты** - мониторинг и тестирование
5. ✅ **Настраиваемые параметры** - гибкая конфигурация

### **Этап 1: Базовая Адаптация**
1. Добавить `DifficultyManager` компонент
2. Расширить `SpawnerTokens` для поддержки уровней сложности
3. Реализовать простую систему весов по уровням

### **Этап 2: Интеллектуальные Функции**
1. Добавить `CrisisManager` для кризисных ситуаций
2. Реализовать `MasteryTracker` для отслеживания мастерства
3. Улучшить комплементарный анализ

### **Этап 3: Продвинутые Возможности**
1. Добавить предсказание позиции игрока
2. Реализовать машинное обучение (опционально)
3. Добавить A/B тестирование различных стратегий

---

## 🔍 ТЕСТИРОВАНИЕ И ОТЛАДКА

### **Инструменты Отладки**
```csharp
[System.Serializable]
public class SpawnDebugInfo
{
    public int currentDifficulty;
    public float[] sectionCosts;
    public int[] enemyCountsBySection;
    public bool isInCrisis;
    public float playerPerformance;
    public EnemyKind lastSpawnedKind;
    public SpawnSection lastSpawnedSection;
}
```

### **Визуализация в Scene View**
- Отображение секторов спауна
- Показ "стоимости" каждого сектора
- Визуализация предсказанных позиций игрока
- График производительности игрока

---

---

## 🎯 ПРОСТАЯ И ЭФФЕКТИВНАЯ СИСТЕМА AI

### **Основные Принципы**
- **8 секторов** вокруг игрока
- **Ограниченные токены** для AI (как ресурс для принятия решений)
- **Тактические сценарии** с откатом (как способности)
- **Двухфазная система**: Спокойный режим ↔ Режим волн
- **Случайность в выборе секторов** (слева/справа от выбранного)
- **Готовые пресеты тактик** для 4 типов врагов

---

## 🎮 СИСТЕМА ТОКЕНОВ AI

### **Принцип Работы**
```csharp
public class AITokenSystem
{
    [Header("AI Token Settings")]
    [SerializeField] private int _defaultTokens = 3;           // Дефолтные токены
    [SerializeField] private int _currentTokens = 3;           // Текущие токены
    [SerializeField] private int _returnedTokens = 0;          // Возвращенные токены
    [SerializeField] private int _maxTokens = 10;              // Максимум токенов
    
    [Header("Wave Settings")]
    [SerializeField] private int _waveThreshold = 5;           // Порог для волны
    [SerializeField] private int _wavePresetCost = 3;          // Стоимость пресетов в волне
    [SerializeField] private float _waveDuration = 15f;         // Длительность волны
    
    // Стоимость действий
    private const int BASIC_SPAWN_COST = 1;        // Обычный спаун
    private const int TACTICAL_SCENARIO_COST = 3;  // Тактический сценарий (обычный режим)
    
    public bool CanUsePresets => _returnedTokens > 0;
    public bool IsWaveMode => _returnedTokens >= _waveThreshold;
}
```

### **Использование Токенов**
- **Токены получаются** за убийство врагов (зависит от силы врага)
- **1 токен** - обычный спаун врага
- **3-6 токенов** - тактический сценарий (с откатом)
- **Токены возвращаются** после спауна (возврат = потрачено)
- **AI "афк"** пропорционально потраченным токенам

### **Система Врагов с Параметрами**
```csharp
[System.Serializable]
public class EnemyData
{
    [Header("Enemy Info")]
    public EnemyKind enemyKind;
    
    [Header("Token Settings")]
    public int tokenValue = 1;                   // Токены (стоимость и возврат одинаковые)
    
    [Header("Timer Settings")]
    public float spawnCooldown = 2f;             // "АФК" время после спауна
    public float timerReduction = 0.5f;          // Сокращение таймера за убийство
    
    [Header("Difficulty")]
    public int difficultyLevel = 0;              // Уровень сложности (0-3)
}

public class EnemyDataManager
{
    [SerializeField] private EnemyData[] _enemyData;
    
    public EnemyData GetEnemyData(EnemyKind enemyKind)
    {
        return _enemyData.FirstOrDefault(e => e.enemyKind == enemyKind);
    }
    
    public int GetTokenValue(EnemyKind enemyKind)
    {
        return GetEnemyData(enemyKind)?.tokenValue ?? 1;
    }
    
    public float GetSpawnCooldown(EnemyKind enemyKind)
    {
        return GetEnemyData(enemyKind)?.spawnCooldown ?? 2f;
    }
    
    public float GetTimerReduction(EnemyKind enemyKind)
    {
        return GetEnemyData(enemyKind)?.timerReduction ?? 0.5f;
    }
}
```

---

## 🎮 ПРИМЕР РАБОТЫ СИСТЕМЫ

### **Сценарий Игры**
```
🎮 НАЧАЛО ИГРЫ (Уровень 0):
- AI токены: 3 (дефолт для уровня 0)
- Возвращенные токены: 0
- Пресеты: недоступны (нужны возвращенные токены)
- Таймер: 3-8 секунд
- Максимум токенов: 3

⚔️ AI СПАУНИТ ДУШУ:
- Тратит: 1 токен
- Получает обратно: 1 токен
- "АФК": 2 секунды (по настройкам души)
- AI токены: 3 (не изменились)
- Возвращенные токены: 0

⚔️ ИГРОК УБИВАЕТ ДУШУ:
- Получено токенов: 1
- AI токены: 4 (но максимум 3, остается 3)
- Возвращенные токены: 1
- Таймер сократился на 0.5 секунды (по настройкам души)

⚔️ AI СПАУНИТ ЕЩЕ ДУШУ:
- Тратит: 1 токен
- Получает обратно: 1 токен
- "АФК": 2 секунды
- AI токены: 3
- Возвращенные токены: 1

⚔️ ИГРОК УБИВАЕТ ДУШУ:
- Получено токенов: 1
- AI токены: 3 (максимум)
- Возвращенные токены: 2
- Пресеты стали доступны! (требуется 2+ возвращенных)

🎯 AI ИСПОЛЬЗУЕТ ПРЕСЕТ "ПОДДЕРЖКА":
- Тратит: 2 токена
- Получает обратно: 2 токена
- "АФК": 4 секунды (по настройкам пресета)
- AI токены: 3
- Возвращенные токены: 2
- Спаунит: Soul + SoulVase

⚔️ ИГРОК УБИВАЕТ ВСЕХ ВРАГОВ ПРЕСЕТА:
- Получено токенов: 2
- AI токены: 3 (максимум)
- Возвращенные токены: 4
- Таймер сократился на 1.5 секунды (по настройкам пресета)

🌊 НАЧИНАЕТСЯ ВОЛНА (порог 3 для уровня 0):
- Таймер: 1 секунда (вместо 3-8)
- Пресеты: доступны по обычной стоимости
- Возвращенные токены считаются за заспауненных врагов

⚔️ AI АКТИВНО СПАУНИТ:
- Каждую секунду спаунит врага
- Получает возвращенные токены за каждого заспауненного
- Может использовать пресеты

⏰ ВОЛНА ЗАКАНЧИВАЕТСЯ (10 секунд для уровня 0):
- AI токены: сбрасываются до дефолта (3)
- Возвращенные токены: сбрасываются до 0
- Таймер: возвращается к 3-8 секундам
- Пресеты: снова недоступны

💡 ИГРОК АКТИВИРУЕТ ЛАМПУ:
- Уровень сложности: увеличивается до 1
- Дефолтные токены: увеличиваются до 4
- Максимум токенов: увеличивается до 5
- AI становится сильнее
```

### **Преимущества Пресетов**
- **Обычный спаун**: 1 токен → 1 токен обратно → "афк" 2 секунды
- **Пресет "Окружение"**: 3 токена → 3 токена обратно → "афк" 6 секунд
- **Но пресет дает 4 врага вместо 1!** (в 4 раза эффективнее)

### **Детальный Пример**
```
🎮 НАЧАЛО ИГРЫ:
- AI токены: 0
- Доступные пресеты: все (никогда не использовались)

⚔️ ИГРОК УБИВАЕТ 3 ДУШИ:
- Получено токенов: 3 (1+1+1)
- AI токены: 3
- Доступные пресеты: "Простая Волна" (1 токен), "Поддержка" (2 токена)

🎯 AI ВЫБИРАЕТ "ПОДДЕРЖКА":
- Тратит: 2 токена
- Получает обратно: 2 токена
- "АФК": 4 секунды (2 токена × 2 секунды)
- Спаунит: Soul + SoulVase
- AI токены: 3 (не изменились)

⚔️ ИГРОК УБИВАЕТ СКЕЛЕТА:
- Получено токенов: 5
- AI токены: 8
- Доступные пресеты: все кроме "Поддержка" (откат 30 сек)

🎯 AI ВЫБИРАЕТ "ОКРУЖЕНИЕ":
- Тратит: 4 токена
- Получает обратно: 4 токена
- "АФК": 8 секунд (4 токена × 2 секунды)
- Спаунит: 4 врага по углам
- AI токены: 8 (не изменились)

⚔️ ИГРОК УБИВАЕТ РЫЦАРЯ:
- Получено токенов: 10
- AI токены: 18
- Доступные пресеты: все кроме "Окружение" (откат 30 сек)

🎯 AI ВЫБИРАЕТ "ДВОЙНОЙ УДАР":
- Тратит: 6 токенов
- Получает обратно: 6 токенов
- "АФК": 12 секунд (6 токенов × 2 секунды)
- Спаунит: 2 Knight'а (спереди и сзади)
- AI токены: 18 (не изменились)
```

### **Ключевые Особенности**
1. **Токены не тратятся** - они возвращаются полностью
2. **"АФК" настраивается** - каждый враг/пресет имеет свои параметры
3. **Пресеты выгодны** - больше врагов за те же "афк" время
4. **Уровни сложности** - 4 уровня с разными параметрами
5. **Ограничение токенов** - AI никогда не получит больше максимума
6. **Случайный выбор пресетов** - последний пресет имеет меньший шанс
7. **Умный выбор секторов** - анализ весов для балансировки
8. **Прогрессия через лампу** - игрок может влиять на сложность

---

## 📈 СИСТЕМА УРОВНЕЙ СЛОЖНОСТИ

### **Параметры Сложности**
```csharp
[System.Serializable]
public class DifficultyLevel
{
    [Header("Level Info")]
    public int level = 0;
    public string levelName;
    
    [Header("Token Settings")]
    public int tokens = 3;                      // Токены AI (максимум и дефолт одинаковые)
    
    [Header("Wave Settings")]
    public float waveDuration = 15f;            // Длительность волны
    public int waveThreshold = 5;               // Порог для активации волны
    
    [Header("Preset Settings")]
    public int presetCostMultiplier = 1;        // Множитель стоимости пресетов
    public int presetTokenMultiplier = 1;       // Множитель возвращаемых токенов
}

public class DifficultyManager
{
    [SerializeField] private DifficultyLevel[] _difficultyLevels;
    [SerializeField] private int _currentDifficulty = 0;
    
    public DifficultyLevel GetCurrentDifficulty()
    {
        return _difficultyLevels[_currentDifficulty];
    }
    
    public void IncreaseDifficulty()
    {
        if (_currentDifficulty < _difficultyLevels.Length - 1)
        {
            _currentDifficulty++;
            Debug.Log($"[Difficulty] Increased to level {_currentDifficulty}");
        }
    }
    
    public void DecreaseDifficulty()
    {
        if (_currentDifficulty > 0)
        {
            _currentDifficulty--;
            Debug.Log($"[Difficulty] Decreased to level {_currentDifficulty}");
        }
    }
    
    public void SetDifficulty(int level)
    {
        _currentDifficulty = Mathf.Clamp(level, 0, _difficultyLevels.Length - 1);
        Debug.Log($"[Difficulty] Set to level {_currentDifficulty}");
    }
}
```

### **Настройки Уровней**
```csharp
// Уровень 0 (Очень Легкий)
new DifficultyLevel
{
    level = 0,
    levelName = "Very Easy",
    tokens = 3,
    waveDuration = 10f,
    waveThreshold = 3,
    presetCostMultiplier = 1,
    presetTokenMultiplier = 1
}

// Уровень 1 (Легкий)
new DifficultyLevel
{
    level = 1,
    levelName = "Easy",
    tokens = 4,
    waveDuration = 15f,
    waveThreshold = 4,
    presetCostMultiplier = 1,
    presetTokenMultiplier = 1
}

// Уровень 2 (Средний)
new DifficultyLevel
{
    level = 2,
    levelName = "Medium",
    tokens = 5,
    waveDuration = 20f,
    waveThreshold = 5,
    presetCostMultiplier = 1,
    presetTokenMultiplier = 1
}

// Уровень 3 (Сложный)
new DifficultyLevel
{
    level = 3,
    levelName = "Hard",
    tokens = 6,
    waveDuration = 25f,
    waveThreshold = 6,
    presetCostMultiplier = 1,
    presetTokenMultiplier = 1
}
```

---

## ⏰ СИСТЕМА СЛУЧАЙНЫХ ТАЙМЕРОВ

### **Принцип Работы**
```csharp
public class RandomTimerSystem
{
    [Header("Timer Settings")]
    [SerializeField] private Vector2 _spawnIntervalRange = new(3f, 8f);  // 3-8 секунд
    [SerializeField] private float _currentTimer = 0f;
    [SerializeField] private float _nextSpawnTime = 0f;
    [SerializeField] private float _timerReductionPerKill = 0.5f;       // Сокращение за убийство
    
    [Header("Wave Settings")]
    [SerializeField] private float _waveSpawnInterval = 1f;              // 1 секунда в волне
    
    // Таймер меняется после каждого убийства врага
    public void OnEnemyKilled()
    {
        // Сокращаем текущий таймер
        _nextSpawnTime -= _timerReductionPerKill;
        
        // Устанавливаем новый таймер
        _nextSpawnTime = Time.time + Random.Range(_spawnIntervalRange.x, _spawnIntervalRange.y);
    }
    
    public bool ShouldSpawn()
    {
        return Time.time >= _nextSpawnTime;
    }
    
    public void SetWaveMode(bool isWave)
    {
        if (isWave)
        {
            _nextSpawnTime = Time.time + _waveSpawnInterval;
        }
        else
        {
            _nextSpawnTime = Time.time + Random.Range(_spawnIntervalRange.x, _spawnIntervalRange.y);
        }
    }
}
```

### **Система "АФК" AI**
```csharp
public class AIAfkSystem
{
    private float _afkDuration = 0f;
    private float _afkStartTime = 0f;
    private bool _isAfk = false;
    
    public void StartAfk(int tokensSpent)
    {
        // AI "афк" пропорционально потраченным токенам
        _afkDuration = tokensSpent * 2f;  // 2 секунды за токен
        _afkStartTime = Time.time;
        _isAfk = true;
    }
    
    public bool IsAfk()
    {
        if (_isAfk && Time.time - _afkStartTime >= _afkDuration)
        {
            _isAfk = false;
        }
        return _isAfk;
    }
}
```

---

## 🎭 ТАКТИЧЕСКИЕ СЦЕНАРИИ

### **Система Пресетов с Билдером**
```csharp
[System.Serializable]
public class PresetData
{
    [Header("Preset Info")]
    public string presetName;
    public string description;
    
    [Header("Token Settings")]
    public int tokenValue = 3;                  // Токены (стоимость и возврат одинаковые)
    
    [Header("Timer Settings")]
    public float spawnCooldown = 6f;            // "АФК" время после спауна
    public float timerReduction = 1.5f;         // Сокращение таймера за убийство всех врагов
    
    [Header("Difficulty Settings")]
    public int difficultyLevel = 0;              // Уровень сложности (0-3)
    public int requiredTokens = 5;              // Требуемые токены для активации
    
    [Header("Enemy Placement")]
    public EnemyPlacement[] enemyPlacements;    // Размещение врагов
}

[System.Serializable]
public class EnemyPlacement
{
    public EnemyKind enemyKind;                 // Тип врага
    public int relativeSection;                // Относительный сектор (0-11)
    public int count = 1;                      // Количество врагов
}

/// <summary>
/// Билдер для создания пресетов с возможностью поворота.
/// Позволяет задать врагов относительно главного врага.
/// </summary>
public class PresetBuilder
{
    private List<EnemyPlacement> _enemyPlacements = new List<EnemyPlacement>();
    private int _mainEnemySection = 0;          // Сектор главного врага
    
    /// <summary>
    /// Добавляет врага в пресет.
    /// </summary>
    /// <param name="enemyKind">Тип врага</param>
    /// <param name="relativeSection">Относительный сектор (0-11)</param>
    /// <param name="count">Количество врагов</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public PresetBuilder AddEnemy(EnemyKind enemyKind, int relativeSection, int count = 1)
    {
        _enemyPlacements.Add(new EnemyPlacement
        {
            enemyKind = enemyKind,
            relativeSection = relativeSection,
            count = count
        });
        return this;
    }
    
    /// <summary>
    /// Устанавливает главного врага (центрального).
    /// </summary>
    /// <param name="enemyKind">Тип главного врага</param>
    /// <param name="count">Количество главных врагов</param>
    /// <returns>Текущий билдер для цепочки вызовов</returns>
    public PresetBuilder SetMainEnemy(EnemyKind enemyKind, int count = 1)
    {
        _enemyPlacements.Add(new EnemyPlacement
        {
            enemyKind = enemyKind,
            relativeSection = 0,  // Главный враг всегда в секторе 0
            count = count
        });
        return this;
    }
    
    /// <summary>
    /// Создает пресет с заданными параметрами.
    /// </summary>
    /// <param name="presetName">Название пресета</param>
    /// <param name="description">Описание</param>
    /// <param name="tokenValue">Стоимость в токенах</param>
    /// <param name="spawnCooldown">Время "афк"</param>
    /// <param name="timerReduction">Сокращение таймера</param>
    /// <param name="difficultyLevel">Уровень сложности</param>
    /// <param name="requiredTokens">Требуемые токены</param>
    /// <returns>Готовый пресет</returns>
    public PresetData Build(string presetName, string description, int tokenValue, 
        float spawnCooldown, float timerReduction, int difficultyLevel, int requiredTokens)
    {
        return new PresetData
        {
            presetName = presetName,
            description = description,
            tokenValue = tokenValue,
            spawnCooldown = spawnCooldown,
            timerReduction = timerReduction,
            difficultyLevel = difficultyLevel,
            requiredTokens = requiredTokens,
            enemyPlacements = _enemyPlacements.ToArray()
        };
    }
    
    /// <summary>
    /// Поворачивает пресет на указанное количество секторов.
    /// </summary>
    /// <param name="rotation">Количество секторов для поворота (0-11)</param>
    /// <returns>Новый билдер с повернутым пресетом</returns>
    public PresetBuilder Rotate(int rotation)
    {
        var rotatedBuilder = new PresetBuilder();
        
        foreach (var placement in _enemyPlacements)
        {
            int newSection = (placement.relativeSection + rotation) % 12;
            rotatedBuilder.AddEnemy(placement.enemyKind, newSection, placement.count);
        }
        
        return rotatedBuilder;
    }
}
```

public class PresetManager
{
    [SerializeField] private PresetData[] _presets;
    private PresetData _lastUsedPreset;
    
    public PresetData GetRandomAvailablePreset(int currentDifficulty, int availableTokens)
    {
        var availablePresets = _presets.Where(p => 
            p.difficultyLevel <= currentDifficulty &&
            p.requiredTokens <= availableTokens &&
            p != _lastUsedPreset).ToArray();
            
        if (availablePresets.Length == 0) return null;
        
        // Случайный выбор
        var selectedPreset = availablePresets[Random.Range(0, availablePresets.Length)];
        _lastUsedPreset = selectedPreset;
        return selectedPreset;
    }
    
    public bool CanUsePreset(int currentDifficulty, int availableTokens)
    {
        return _presets.Any(p => 
            p.difficultyLevel <= currentDifficulty &&
            p.requiredTokens <= availableTokens);
    }
}
```

### **Примеры Использования Билдера Пресетов**

#### **1. "Окружение" (Surround)**
```csharp
var surroundPreset = new PresetBuilder()
    .SetMainEnemy(EnemyKind.Soul, 1)           // Главный враг в секторе 0
    .AddEnemy(EnemyKind.Soul, 3, 1)            // Душа в секторе 3 (справа)
    .AddEnemy(EnemyKind.Soul, 9, 1)            // Душа в секторе 9 (слева)
    .AddEnemy(EnemyKind.Soul, 6, 1)            // Душа в секторе 6 (сзади)
    .Build("Surround", "Окружение игрока душами", 3, 6f, 1.5f, 0, 3);
```

#### **2. "Клещи" (Pincer)**
```csharp
public class PincerTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            // Атака с двух сторон
            Left = EnemyKind.Skelet,      // Дальний бой слева
            Right = EnemyKind.Knight,     // Ближний бой справа
            Cost = 4,
            Cooldown = 35f
        };
    }
}
```

#### **3. "Засада" (Ambush)**
```csharp
public class AmbushTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            // Атака сзади и сбоку
            Back = EnemyKind.Knight,      // Сильный сзади
            Left = EnemyKind.Soul,        // Быстрый слева
            Right = EnemyKind.Soul,       // Быстрый справа
            Cost = 4,
            Cooldown = 30f
        };
    }
}
```

#### **4. "Волна Слабости" (Weak Wave)**
```csharp
public class WeakWaveTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            // Много слабых врагов
            Front = EnemyKind.Soul,
            FrontLeft = EnemyKind.Soul,
            FrontRight = EnemyKind.Soul,
            Left = EnemyKind.Soul,
            Right = EnemyKind.Soul,
            Cost = 2,
            Cooldown = 20f
        };
    }
}
```

#### **5. "Элитная Группа" (Elite Squad)**
```csharp
public class EliteSquadTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            // Мало сильных врагов
            Front = EnemyKind.Knight,
            Back = EnemyKind.Skelet,
            Cost = 5,
            Cooldown = 45f
        };
    }
}
```

#### **6. "Босс + Миньоны" (Boss Minions)**
```csharp
public class BossMinionsTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            // 1 сильный + поддержка
            Front = EnemyKind.Knight,         // Босс
            FrontLeft = EnemyKind.Soul,       // Миньон
            FrontRight = EnemyKind.Soul,       // Миньон
            Back = EnemyKind.SoulVase,        // Поддержка
            Cost = 4,
            Cooldown = 40f
        };
    }
}
```

---

## 🎯 СИСТЕМА ВЫБОРА СЕКТОРОВ

### **Выбор Сектора с Наименьшим Весом (12 Секторов)**
```csharp
/// <summary>
/// Умный выбор сектора для спауна врагов.
/// Анализирует позиции всех врагов и выбирает сектор с наименьшим весом.
/// Применяет случайность для непредсказуемости поведения.
/// </summary>
public class SmartSectionSelection
{
    private Transform _playerTransform;
    private float _spawnRadius = 10f;
    private const int SECTOR_COUNT = 12;        // Количество секторов
    
    /// <summary>
    /// Выбирает оптимальный сектор для спауна.
    /// </summary>
    /// <returns>Сектор с наименьшим весом (с учетом случайности)</returns>
    public int SelectOptimalSection()
    {
        // Получаем позиции всех врагов
        var enemyPositions = GetAllEnemyPositions();
        
        // Вычисляем веса для каждого сектора
        float[] sectionWeights = CalculateSectionWeights(enemyPositions);
        
        // Находим сектор с наименьшим весом
        int lowestWeightSection = FindLowestWeightSection(sectionWeights);
        
        // Применяем случайность (30% шанс соседнего сектора)
        return ApplyRandomness(lowestWeightSection);
    }
    
    private float[] CalculateSectionWeights(Vector3[] enemyPositions)
    {
        float[] weights = new float[SECTOR_COUNT];
        
        for (int i = 0; i < SECTOR_COUNT; i++)
        {
            Vector3 sectionPosition = GetSectionPosition(i);
            
            float totalWeight = 0f;
            foreach (Vector3 enemyPos in enemyPositions)
            {
                float distance = Vector3.Distance(sectionPosition, enemyPos);
                float weight = 1f / (distance + 1f); // Ближе = больше вес
                totalWeight += weight;
            }
            
            weights[i] = totalWeight;
        }
        
        return weights;
    }
    
    private int FindLowestWeightSection(float[] weights)
    {
        int lowestIndex = 0;
        float lowestWeight = weights[0];
        
        for (int i = 1; i < weights.Length; i++)
        {
            if (weights[i] < lowestWeight)
            {
                lowestWeight = weights[i];
                lowestIndex = i;
            }
        }
        
        return lowestIndex;
    }
    
    private int ApplyRandomness(int targetSection)
    {
        // 70% шанс выбрать точный сектор
        // 30% шанс выбрать соседний (слева или справа)
        
        if (Random.value < 0.7f)
        {
            return targetSection; // Точный выбор
        }
        else
        {
            // Случайный соседний сектор
            int deviation = Random.Range(0, 2) == 0 ? -1 : 1; // -1 или +1
            int randomIndex = (targetSection + deviation + SECTOR_COUNT) % SECTOR_COUNT;
            return randomIndex;
        }
    }
}
```

### **Влияние на Веса Секторов**
- **Умный выбор** - AI выбирает сектор с наименьшим весом
- **Случайность** - 30% шанс соседнего сектора для непредсказуемости
- **Динамичность** - веса пересчитываются в реальном времени
- **Балансировка** - равномерное распределение врагов по секторам

---

## 🏗️ АРХИТЕКТУРА СИСТЕМЫ

### **Основной Класс AI**
```csharp
public class SimpleSpawnerAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private int _defaultTokens = 3;
    [SerializeField] private int _maxTokens = 10;
    
    [Header("Timer Settings")]
    [SerializeField] private Vector2 _spawnIntervalRange = new(3f, 8f);
    [SerializeField] private float _timerReductionPerKill = 0.5f;
    
    [Header("Wave Settings")]
    [SerializeField] private int _waveThreshold = 5;
    [SerializeField] private float _waveDuration = 15f;
    [SerializeField] private int _wavePresetCost = 3;
    
    [Header("Tactical Scenarios")]
    [SerializeField] private TacticalScenario[] _scenarios;
    
    // Компоненты
    private AITokenSystem _tokenSystem;
    private RandomTimerSystem _timerSystem;
    private AIAfkSystem _afkSystem;
    private TacticalScenario _scenarioSystem;
    private SmartSectionSelection _sectionSelection;
    
    // Состояние волны
    private bool _isWaveMode = false;
    private float _waveStartTime = 0f;
    
    private void Update()
    {
        // Проверяем, не "афк" ли AI
        if (_afkSystem.IsAfk()) return;
        
        // Проверяем режим волны
        CheckWaveMode();
        
        // Проверяем, пора ли спаунить
        if (_timerSystem.ShouldSpawn())
        {
            DecideSpawnAction();
        }
    }
    
    // Вызывается при убийстве врага
    public void OnEnemyKilled(EnemyKind enemyKind)
    {
        int tokensGained = EnemyTokenValue.GetTokenValue(enemyKind);
        _tokenSystem.AddTokens(tokensGained);
        _timerSystem.OnEnemyKilled();
    }
    
    // Вызывается при заспауне врага (для волны)
    public void OnEnemySpawned(EnemyKind enemyKind)
    {
        if (_isWaveMode)
        {
            int tokensGained = EnemyTokenValue.GetTokenValue(enemyKind);
            _tokenSystem.AddReturnedTokens(tokensGained);
        }
    }
    
    // Активация лампы игроком
    public void OnLampActivated()
    {
        _tokenSystem.IncreaseDefaultTokens(2); // Увеличиваем дефолтные токены
    }
}
```

### **Логика Принятия Решений**
```csharp
private void DecideSpawnAction()
{
    if (_isWaveMode)
    {
        DecideWaveAction();
    }
    else
    {
        DecideNormalAction();
    }
}

private void DecideNormalAction()
{
    // 1. Проверить доступные тактические сценарии (только если есть возвращенные токены)
    if (_tokenSystem.CanUsePresets)
    {
        var availableScenario = _scenarioSystem.GetRandomAvailableScenario();
        if (availableScenario != null && _tokenSystem.CanAfford(availableScenario.Cost))
        {
            ExecuteTacticalScenario(availableScenario);
            return;
        }
    }
    
    // 2. Обычный спаун
    if (_tokenSystem.CanAfford(1))
    {
        SpawnBasicEnemy();
    }
}

private void DecideWaveAction()
{
    // В режиме волны пресеты дешевле
    if (_tokenSystem.CanUsePresets)
    {
        var availableScenario = _scenarioSystem.GetRandomAvailableScenario();
        if (availableScenario != null && _tokenSystem.CanAfford(_wavePresetCost))
        {
            ExecuteWaveScenario(availableScenario);
            return;
        }
    }
    
    // Обычный спаун в волне
    if (_tokenSystem.CanAfford(1))
    {
        SpawnBasicEnemy();
    }
}

private void CheckWaveMode()
{
    if (!_isWaveMode && _tokenSystem.IsWaveMode)
    {
        StartWaveMode();
    }
    else if (_isWaveMode && Time.time - _waveStartTime >= _waveDuration)
    {
        EndWaveMode();
    }
}

private void StartWaveMode()
{
    _isWaveMode = true;
    _waveStartTime = Time.time;
    _timerSystem.SetWaveMode(true);
    Debug.Log("[AI] Wave mode started!");
}

private void EndWaveMode()
{
    _isWaveMode = false;
    _tokenSystem.ResetToDefault();
    _timerSystem.SetWaveMode(false);
    Debug.Log("[AI] Wave mode ended!");
}

private void ExecuteTacticalScenario(TacticalScenario.Scenario scenario)
{
    // Спауним сценарий
    SpawnScenario(scenario);
    
    // Тратим токены
    int tokensSpent = scenario.Cost;
    _tokenSystem.SpendTokens(tokensSpent);
    
    // Возвращаем токены (полный возврат)
    _tokenSystem.ReturnTokens(tokensSpent);
    
    // AI уходит в "афк" пропорционально потраченным токенам
    _afkSystem.StartAfk(tokensSpent);
    
    // Устанавливаем откат сценария
    scenario.lastUsedTime = Time.time;
}

private void SpawnBasicEnemy()
{
    // Обычный спаун 1 врага
    int tokensSpent = 1;
    _tokenSystem.SpendTokens(tokensSpent);
    _tokenSystem.ReturnTokens(tokensSpent);
    _afkSystem.StartAfk(tokensSpent);
    
    // Спауним случайного врага в случайном секторе
    var randomEnemy = GetRandomEnemyKind();
    var randomSection = GetRandomSection();
    SpawnEnemy(randomEnemy, randomSection);
}
```

---

## 📊 БАЛАНСИРОВКА

### **Настройки Сложности**
```csharp
[System.Serializable]
public class SimpleDifficultySettings
{
    [Header("Token Settings")]
    public int maxTokens = 10;
    public int tokensPerKill = 1;
    
    [Header("Timer Settings")]
    public Vector2 spawnIntervalRange = new(3f, 8f);
    
    [Header("Afk Settings")]
    public float afkSecondsPerToken = 2f;
    
    [Header("Scenario Settings")]
    public float scenarioCooldownMultiplier = 1f;
    public int scenarioCostMultiplier = 1;
}
```

### **Простая Адаптация**
```csharp
public class SimpleAdaptiveDifficulty
{
    private SimpleDifficultySettings _settings;
    private float _playerKillRate; // Убийств в минуту
    
    public void AdjustDifficulty()
    {
        if (_playerKillRate > 5f) // Игрок убивает много
        {
            _settings.spawnIntervalRange.x *= 0.8f;  // Быстрее спаун
            _settings.spawnIntervalRange.y *= 0.8f;
        }
        else if (_playerKillRate < 2f) // Игрок убивает мало
        {
            _settings.spawnIntervalRange.x *= 1.2f;  // Медленнее спаун
            _settings.spawnIntervalRange.y *= 1.2f;
        }
    }
}

---

## 🎯 ГОТОВЫЕ ПРЕСЕТЫ ТАКТИК

### **Для 4 Типов Врагов: Soul, SoulVase, Skelet, Knight**

#### **Базовые Тактики (Низкая Сложность)**
```csharp
// 1. "Простая Волна" - только Soul
public class BasicWaveTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Soul,
            Left = EnemyKind.Soul,
            Right = EnemyKind.Soul,
            Cost = 1,
            Cooldown = 15f
        };
    }
}

// 2. "Поддержка" - Soul + SoulVase
public class SupportTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Soul,
            Back = EnemyKind.SoulVase,
            Cost = 2,
            Cooldown = 20f
        };
    }
}
```

#### **Средние Тактики (Средняя Сложность)**
```csharp
// 3. "Дальний Бой" - Skelet + поддержка
public class RangedTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Skelet,        // Дальний бой
            Back = EnemyKind.SoulVase,       // Поддержка
            Cost = 3,
            Cooldown = 25f
        };
    }
}

// 4. "Ближний Бой" - Knight + Soul
public class MeleeTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Knight,       // Сильный ближний бой
            Left = EnemyKind.Soul,           // Быстрый поддержка
            Cost = 3,
            Cooldown = 30f
        };
    }
}
```

#### **Продвинутые Тактики (Высокая Сложность)**
```csharp
// 5. "Смешанная Атака" - все типы
public class MixedAssaultTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Knight,       // Сильный фронт
            Back = EnemyKind.Skelet,         // Дальний бой сзади
            Left = EnemyKind.Soul,           // Быстрый слева
            Right = EnemyKind.SoulVase,      // Поддержка справа
            Cost = 5,
            Cooldown = 40f
        };
    }
}

// 6. "Окружение" - 4 врага по углам
public class SurroundTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            FrontLeft = EnemyKind.Soul,
            FrontRight = EnemyKind.Soul,
            BackLeft = EnemyKind.SoulVase,
            BackRight = EnemyKind.SoulVase,
            Cost = 4,
            Cooldown = 35f
        };
    }
}
```

#### **Элитные Тактики (Максимальная Сложность)**
```csharp
// 7. "Двойной Удар" - 2 сильных врага
public class DoubleStrikeTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Knight,       // Сильный спереди
            Back = EnemyKind.Knight,        // Сильный сзади
            Cost = 6,
            Cooldown = 50f
        };
    }
}

// 8. "Артиллерия" - 2 дальних бойца
public class ArtilleryTactic
{
    public EnemyComposition GetComposition()
    {
        return new EnemyComposition
        {
            Front = EnemyKind.Skelet,       // Дальний бой спереди
            Back = EnemyKind.Skelet,        // Дальний бой сзади
            Left = EnemyKind.SoulVase,       // Поддержка слева
            Right = EnemyKind.SoulVase,     // Поддержка справа
            Cost = 6,
            Cooldown = 45f
        };
    }
}
```

### **Система Билдера Тактик**
```csharp
public class TacticBuilder
{
    public static TacticalScenario CreateCustomTactic(string name, 
        EnemyKind[] enemies, 
        SpawnSection[] positions,
        int cost, 
        float cooldown)
    {
        var composition = new EnemyComposition();
        
        for (int i = 0; i < Mathf.Min(enemies.Length, positions.Length); i++)
        {
            composition.SetEnemyAtPosition(positions[i], enemies[i]);
        }
        
        return new TacticalScenario
        {
            Name = name,
            Composition = composition,
            Cost = cost,
            Cooldown = cooldown
        };
    }
}

// Пример использования билдера
var customTactic = TacticBuilder.CreateCustomTactic(
    "Моя Тактика",
    new EnemyKind[] { EnemyKind.Knight, EnemyKind.Soul, EnemyKind.Soul },
    new SpawnSection[] { SpawnSection.Front, SpawnSection.Left, SpawnSection.Right },
    4, 30f
);
```

### **Настройка Тактик в Inspector**
```csharp
[System.Serializable]
public class TacticPreset
{
    [Header("Tactic Info")]
    public string tacticName;
    public string description;
    public DifficultyLevel difficulty;
    
    [Header("Enemy Placement")]
    public EnemyPlacement[] enemyPlacements;
    
    [Header("Cost & Cooldown")]
    public int tokenCost;
    public float cooldownTime;
    
    [Header("Conditions")]
    public bool requiresCalmMode = true;
    public bool requiresWaveMode = false;
    public float minPlayerHealth = 0f;
    public float maxPlayerHealth = 1f;
}

[System.Serializable]
public class EnemyPlacement
{
    public SpawnSection section;
    public EnemyKind enemyType;
    public int count = 1;
}
```

---

## 🎮 ИТОГОВАЯ ПРОСТАЯ СИСТЕМА

### **Основные Компоненты**
1. **Система Токенов** - получаются за убийства, тратятся на спаун
2. **Случайные Таймеры** - меняются после каждого убийства
3. **Тактические Сценарии** - с откатом и стоимостью в токенах
4. **Система "АФК"** - AI неактивен пропорционально потраченным токенам
5. **Случайность Секторов** - 30% шанс соседнего сектора
6. **Готовые Пресеты** - 8 тактик для 4 типов врагов

---

## 📈 ЗАКЛЮЧЕНИЕ

### **✅ Система Глюков (Готова к Использованию)**
Накопительная система глюков **полностью реализована** и обеспечивает:
- **Предсказуемость** - игрок чувствует накопление глюков
- **Случайность** - полная непредсказуемость при срабатывании
- **Контролируемость** - точная настройка частоты и интенсивности
- **Отладку** - полная видимость состояния системы

### **🎯 Простая Система Умного Спауна**
Предложенная простая система обеспечивает:

1. **Простота реализации** - все компоненты понятны и легко кодируются
2. **Тактическое разнообразие** - 8 разных сценариев боя
3. **Ресурсное управление** - AI должен думать, как тратить токены
4. **Ритмичность** - чередование активности и "афк" AI
5. **Случайность** - непредсказуемость в выборе секторов
6. **Адаптивность** - можно легко настраивать сложность
7. **🎲 Система Глюков** - накопительная случайность для непредсказуемости
8. **🎭 Тактические Сценарии** - разнообразные боевые ситуации
9. **⏰ Случайные Таймеры** - непредсказуемое время спауна
10. **🎮 Система "АФК"** - AI неактивен после больших трат

### **🚀 Статус Реализации**

#### **✅ ПОЛНОСТЬЮ РЕАЛИЗОВАНО:**
- **SpawnerEnemysAI** - умный AI компонент
- **Система Глюков** - накопительная случайность
- **Комплементарный Анализ** - умные комбинации врагов
- **Логика Противоположной Стороны** - спаун напротив наименее загруженного сектора
- **Отладочные Инструменты** - полная видимость состояния
- **Валидация Параметров** - автоматическая проверка настроек

#### **🔄 ТРЕБУЕТ РЕАЛИЗАЦИИ:**
- **Система Токенов** - получение за убийства, трата на спаун
- **Случайные Таймеры** - изменение после каждого убийства
- **Тактические Сценарии** - 8 готовых пресетов с откатом
- **Система "АФК"** - неактивность AI пропорционально тратам
- **Случайность Секторов** - 30% шанс соседнего сектора
- **Готовые Пресеты** - настройка тактик в Inspector

Система интегрируется с существующей архитектурой и может быть внедрена поэтапно, начиная с **готовой системы глюков** и постепенно добавляя простые компоненты.
