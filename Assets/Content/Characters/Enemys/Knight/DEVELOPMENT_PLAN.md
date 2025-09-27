# Knight Enemy - План дальнейшего развития

## 🎯 Текущее состояние
Рыцарь-враг с механикой блокировки меча игрока. Базовая функциональность реализована и работает.

## 🚀 Планируемые улучшения

### 1. 💀 Система смерти с душами

#### 1.1 Система цветовых эффектов
- **При создании врага** он уже знает на какие души распадется
- **3 точки разрушения:** меч, голова, тело
- **Меч и голова** имеют цветовые эффекты, соответствующие душам
- **Цвет меча** отражает душу, которая появится после его исчезновения

#### 1.2 Анимационная последовательность смерти
- **Первая анимация смерти** - базовая анимация смерти рыцаря
- **Вторая анимация смерти** - дополнительная анимация разрушения/исчезновения тела
- **Меч останавливается на месте** после первой анимации смерти

#### 1.3 Поведение меча при смерти
- **При смерти рыцаря меч останавливается на месте**
- **Через 2-3 секунды меч исчезает (fade out эффект)**
- **На месте исчезновения меча появляется душа с предопределенным цветом**

#### 1.4 Души из тела рыцаря
- **Через небольшое время после второй анимации смерти (1-2 секунды)**
- **Из головы рыцаря выходит душа с предопределенным цветом**
- **Из тела рыцаря выходит душа с предопределенным цветом**
- **Души появляются в соответствующих точках (голова, тело)**

#### 1.5 Система предопределенных душ
```csharp
// Планируемая реализация
public class KnightSoulConfiguration : MonoBehaviour
{
    [Header("Soul Configuration")]
    [SerializeField] private SoulType swordSoulType;
    [SerializeField] private SoulType headSoulType;
    [SerializeField] private SoulType bodySoulType;
    
    [Header("Visual Components")]
    [SerializeField, Required] private KnightSwordVisual _knightSwordVisual;
    [SerializeField, Required] private KnightHeadVisual _knightHeadVisual;
    
    [Header("Available Soul Types")]
    [SerializeField] private SoulType[] availableSoulTypes = { 
        SoulType.Red, SoulType.Blue, SoulType.Green, SoulType.Yellow 
    };
    
    public SoulType SwordSoulType => swordSoulType;
    public SoulType HeadSoulType => headSoulType;
    public SoulType BodySoulType => bodySoulType;
    
    private void Start()
    {
        // При создании врага определяем типы душ
        ConfigureSoulTypes();
        ApplyVisualEffects();
    }
    
    private void ConfigureSoulTypes()
    {
        // Случайно выбираем типы душ из доступных
        swordSoulType = GetRandomSoulType();
        headSoulType = GetRandomSoulType();
        bodySoulType = GetRandomSoulType();
    }
    
    private void ApplyVisualEffects()
    {
        // Применяем SoulMaterialApplier к мечу и голове
        _knightSwordVisual.SetSoulType(swordSoulType);
        _knightHeadVisual.SetSoulType(headSoulType);
    }
    
    private SoulType GetRandomSoulType()
    {
        if (availableSoulTypes == null || availableSoulTypes.Length == 0)
            return SoulType.Red; // Fallback
            
        return availableSoulTypes[Random.Range(0, availableSoulTypes.Length)];
    }
}
```

### 2. 🎨 Визуальные эффекты

#### 2.1 Цветовые эффекты меча и головы
```csharp
// Используем существующий SoulMaterialApplier
public class KnightSwordVisual : MonoBehaviour
{
    [SerializeField, Required] private SoulMaterialApplier _soulMaterialApplier;
    [SerializeField] private ParticleSystem swordGlowEffect;
    
    private SoulType _currentSoulType;
    
    public void SetSoulType(SoulType soulType)
    {
        _currentSoulType = soulType;
        
        // Применяем материал души к спрайтам и частицам
        _soulMaterialApplier.ApplySoul(soulType);
        
        // Дополнительно настраиваем эффект свечения
        if (swordGlowEffect != null)
        {
            var main = swordGlowEffect.main;
            var soulColor = GetSoulColor(soulType);
            main.startColor = soulColor;
        }
    }
    
    public void ResetToOriginalMaterials()
    {
        _soulMaterialApplier.ResetToOriginalMaterials();
    }
    
    public SoulType GetCurrentSoulType()
    {
        return _currentSoulType;
    }
    
    private Color GetSoulColor(SoulType soulType)
    {
        // Получаем цвет из конфигурации душ
        return SoulMaterialConfig.InstanceGame.GetMaterial(soulType).color;
    }
}

public class KnightHeadVisual : MonoBehaviour
{
    [SerializeField, Required] private SoulMaterialApplier _soulMaterialApplier;
    [SerializeField] private ParticleSystem headGlowEffect;
    
    private SoulType _currentSoulType;
    
    public void SetSoulType(SoulType soulType)
    {
        _currentSoulType = soulType;
        
        // Применяем материал души к спрайтам и частицам
        _soulMaterialApplier.ApplySoul(soulType);
        
        // Дополнительно настраиваем эффект свечения
        if (headGlowEffect != null)
        {
            var main = headGlowEffect.main;
            var soulColor = GetSoulColor(soulType);
            main.startColor = soulColor;
        }
    }
    
    public void ResetToOriginalMaterials()
    {
        _soulMaterialApplier.ResetToOriginalMaterials();
    }
    
    public SoulType GetCurrentSoulType()
    {
        return _currentSoulType;
    }
    
    private Color GetSoulColor(SoulType soulType)
    {
        // Получаем цвет из конфигурации душ
        return SoulMaterialConfig.InstanceGame.GetMaterial(soulType).color;
    }
}
```

#### 2.2 Эффект исчезновения меча
```csharp
// Планируемая реализация
public class SwordDeathEffect : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private ParticleSystem swordDisappearEffect;
    
    public void StartSwordDisappear(Vector3 swordPosition, Color soulColor)
    {
        // Fade out меча с сохранением цвета души
        // Запуск эффекта частиц с цветом души
        // Создание души на месте меча с предопределенным цветом
    }
}
```

#### 2.3 Эффект появления душ
```csharp
// Планируемая реализация
public class SoulSpawnEffect : MonoBehaviour
{
    [SerializeField] private float soulSpawnDelay = 1.5f;
    [SerializeField] private float spawnRadius = 1f;
    
    [Header("Soul Prefabs")]
    [SerializeField] private GameObject soulPrefab; // Базовый префаб души
    
    public void SpawnSoulFromHead(Vector3 headPosition, SoulType soulType)
    {
        // Создание души из головы с предопределенным цветом
        GameObject soulInstance = Instantiate(soulPrefab, headPosition, Quaternion.identity);
        
        // Применяем SoulMaterialApplier к созданной душе
        if (soulInstance.TryGetComponent<SoulMaterialApplier>(out var soulApplier))
        {
            soulApplier.ApplySoul(soulType);
        }
        
        // Запускаем эффекты появления
        StartSpawnEffects(soulInstance, soulType);
    }
    
    public void SpawnSoulFromBody(Vector3 bodyPosition, SoulType soulType)
    {
        // Создание души из тела с предопределенным цветом
        GameObject soulInstance = Instantiate(soulPrefab, bodyPosition, Quaternion.identity);
        
        // Применяем SoulMaterialApplier к созданной душе
        if (soulInstance.TryGetComponent<SoulMaterialApplier>(out var soulApplier))
        {
            soulApplier.ApplySoul(soulType);
        }
        
        // Запускаем эффекты появления
        StartSpawnEffects(soulInstance, soulType);
    }
    
    public void SpawnSoulFromSword(Vector3 swordPosition, SoulType soulType)
    {
        // Создание души из меча с предопределенным цветом
        GameObject soulInstance = Instantiate(soulPrefab, swordPosition, Quaternion.identity);
        
        // Применяем SoulMaterialApplier к созданной душе
        if (soulInstance.TryGetComponent<SoulMaterialApplier>(out var soulApplier))
        {
            soulApplier.ApplySoul(soulType);
        }
        
        // Запускаем эффекты появления
        StartSpawnEffects(soulInstance, soulType);
    }
    
    private void StartSpawnEffects(GameObject soulInstance, SoulType soulType)
    {
        // Запускаем анимацию появления души
        // Эффекты частиц с соответствующим цветом
        // Звуковые эффекты
    }
}
```

### 3. 🔧 Техническая реализация

#### 3.1 Модификация Knight.cs
```csharp
// Добавить в OnDeathRequested
private void OnDeathRequested(DamageData damageData)
{
    _animator.PlayDeath();
    _follower.DisableMovement();
    _damage.DisableCollisions();
    
    // НОВОЕ: Остановить меч на месте
    _knightSword.StopOnDeath();
    
    // НОВОЕ: Запустить последовательность смерти с двумя анимациями
    _deathSequence.StartDeathSequence();
}
```

#### 3.2 Модификация KnightSword.cs
```csharp
// Добавить новые методы
public void StopOnDeath()
{
    _isBlockingEnabled = false;
    _target = null;
    _rigidbody.linearVelocity = Vector2.zero;
    _rigidbody.angularVelocity = 0f;
    
    // Запустить эффект исчезновения
    _swordDeathEffect.StartDisappear();
}

public void StartDisappearEffect()
{
    // Fade out анимация
    // Эффект частиц
    // Создание души на месте
}
```

### 4. 🎮 Система душ

#### 4.1 KnightDeathSequence.cs
```csharp
[RequireComponent(typeof(Knight))]
public class KnightDeathSequence : MonoBehaviour
{
    [Header("Death Animation Settings")]
    [SerializeField] private float secondDeathDelay = 1f;
    [SerializeField] private float headSoulDelay = 1.5f;
    [SerializeField] private float bodySoulDelay = 1.8f;
    [SerializeField] private float swordSoulDelay = 2f;
    
    [Header("Spawn Points")]
    [SerializeField] private Transform headSpawnPoint;
    [SerializeField] private Transform bodySpawnPoint;
    
    private Knight _knight;
    private KnightAnimator _animator;
    private KnightSword _knightSword;
    private KnightSoulConfiguration _soulConfig;
    private SoulSpawnEffect _soulSpawnEffect;
    
    public void StartDeathSequence()
    {
        StartCoroutine(DeathSequenceCoroutine());
    }
    
    private IEnumerator DeathSequenceCoroutine()
    {
        // 1. Первая анимация смерти уже запущена в Knight.cs
        
        // 2. Ждем и запускаем вторую анимацию смерти
        yield return new WaitForSeconds(secondDeathDelay);
        _animator.PlaySecondDeath();
        
        // 3. Ждем и создаем душу из головы
        yield return new WaitForSeconds(headSoulDelay);
        _soulSpawnEffect.SpawnSoulFromHead(
            headSpawnPoint.position, 
            _soulConfig.HeadSoulType
        );
        
        // 4. Ждем и создаем душу из тела
        yield return new WaitForSeconds(bodySoulDelay);
        _soulSpawnEffect.SpawnSoulFromBody(
            bodySpawnPoint.position, 
            _soulConfig.BodySoulType
        );
        
        // 5. Ждем исчезновения меча и создаем душу меча
        yield return new WaitForSeconds(swordSoulDelay);
        _soulSpawnEffect.SpawnSoulFromSword(
            _knightSword.transform.position, 
            _soulConfig.SwordSoulType
        );
    }
}
```

### 5. 📋 Правильный порядок реализации

#### 🎯 **ЭТАП 1: Вторая анимация смерти (1 час)**
- [ ] **Создать вторую анимацию смерти** в Animator Controller
- [ ] **Добавить параметр** `SecondDeath` в KnightAnimatorData
- [ ] **Реализовать метод** `PlaySecondDeath()` в KnightAnimator
- [ ] **Протестировать** анимацию в игре

#### 🎯 **ЭТАП 2: Порядок логики смерти (1.5 часа)**
- [ ] **Создать KnightSoulConfiguration** - определение типов душ при создании
- [ ] **Создать KnightDeathSequence** - управление последовательностью смерти
- [ ] **Модифицировать Knight.cs** - запуск системы душ при смерти
- [ ] **Создать KnightSwordVisual и KnightHeadVisual** - цветовые эффекты
- [ ] **Протестировать** полную последовательность смерти

#### 🎯 **ЭТАП 3: Визуальные эффекты (1 час)**
- [ ] **Настроить SoulMaterialApplier** на мече и голове рыцаря
- [ ] **Создать эффекты исчезновения меча** с fade out
- [ ] **Создать эффекты появления душ** в нужных точках
- [ ] **Протестировать** визуальные эффекты

#### 🎯 **ЭТАП 4: Интеграция в спавнеры (30 мин)**
- [ ] **Добавить префаб рыцаря** в спавнеры (EnemyPrefabByKind)
- [ ] **Раскомментировать код** в SpawnerBase.ChooseKindWeightedByTokens()
- [ ] **Настроить веса** для EnemyKind.Knight в SpawnerTokens
- [ ] **Финальное тестирование** спавна рыцаря в игре

### 6. 🎨 Настройки эффектов

#### 6.1 Параметры таймингов
```csharp
[Header("Death Animation Timing")]
[SerializeField] private float secondDeathDelay = 1f;      // Задержка второй анимации смерти
[SerializeField] private float headSoulDelay = 1.5f;       // Задержка души из головы
[SerializeField] private float bodySoulDelay = 1.8f;       // Задержка души из тела
[SerializeField] private float swordSoulDelay = 2f;        // Задержка души меча
[SerializeField] private float swordFadeDuration = 1f;     // Длительность исчезновения меча
```

#### 6.2 Параметры душ
```csharp
[Header("Soul Settings")]
[SerializeField] private float spawnRadius = 1f;           // Радиус появления душ
[SerializeField] private SoulType[] availableSoulTypes;    // Доступные типы душ
[SerializeField] private Color[] soulColors;               // Цвета для каждого типа души
[SerializeField] private GameObject[] soulPrefabs;         // Префабы душ по типам
```

#### 6.3 Цветовые эффекты
```csharp
[Header("Visual Effects")]
[SerializeField] private float glowIntensity = 1.5f;       // Интенсивность свечения
[SerializeField] private float glowPulseSpeed = 2f;        // Скорость пульсации свечения
[SerializeField] private Color glowColor = Color.white;    // Цвет свечения
[SerializeField] private Material swordGlowMaterial;       // Материал свечения меча
[SerializeField] private Material headGlowMaterial;        // Материал свечения головы
```

### 7. 🔄 Интеграция с существующими системами

#### 7.1 Система пула объектов
- Интегрировать с EnemyPool для переиспользования душ
- Оптимизировать создание/уничтожение объектов

#### 7.2 Система событий
- Использовать существующие события DeathRequested/DeathCompleted
- Добавить новые события для синхронизации эффектов

### 8. 🎯 Ожидаемый результат

После реализации:
1. **При создании врага** → меч и голова получают цветовые эффекты душ
2. **Рыцарь умирает** → первая анимация смерти
3. **Меч останавливается** → остается на месте с цветовым эффектом
4. **Через 1 сек** → вторая анимация смерти (разрушение тела)
5. **Через 1.5 сек** → из головы выходит душа с предопределенным цветом
6. **Через 1.8 сек** → из тела выходит душа с предопределенным цветом
7. **Через 2 сек** → меч исчезает, на его месте появляется душа с цветом меча
8. **Визуально красивая** сцена смерти с цветовыми эффектами и предсказуемыми душами

### 9. 📝 Заметки для быстрой реализации

#### ✅ **УЖЕ ГОТОВО:**
- EnemyKind.Knight существует в системе
- SpawnerBase уже инициализирует Knight через InitializeComponents()
- SoulType enum и SoulMaterialApplier готовы к использованию
- Префаб рыцаря существует: Knight (game).prefab

#### ⚡ **БЫСТРАЯ ИНТЕГРАЦИЯ:**
1. **Добавить префаб в спавнеры** - просто добавить Knight (game).prefab в EnemyPrefabByKind с EnemyKind.Knight
2. **Раскомментировать код** в SpawnerBase.ChooseKindWeightedByTokens() для включения Knight
3. **Настроить веса** в SpawnerTokens для EnemyKind.Knight

#### 🎯 **ПРАВИЛЬНЫЙ ПОРЯДОК ЗАДАЧ:**

**1️⃣ СНАЧАЛА - Вторая анимация смерти:**
- Создать анимацию в Animator Controller
- Добавить параметр SecondDeath
- Реализовать метод PlaySecondDeath()
- Протестировать анимацию

**2️⃣ ПОТОМ - Порядок логики смерти:**
- KnightSoulConfiguration (определение душ при создании)
- KnightDeathSequence (управление последовательностью)
- Модификация Knight.cs (запуск системы душ)
- KnightSwordVisual и KnightHeadVisual (цветовые эффекты)

**3️⃣ ЗАТЕМ - Визуальные эффекты:**
- Настройка SoulMaterialApplier
- Эффекты исчезновения меча
- Эффекты появления душ

**4️⃣ В КОНЦЕ - Интеграция в спавнеры:**
- Добавление префаба в спавнеры
- Настройка весов
- Финальное тестирование

#### 🔧 **ТЕХНИЧЕСКИЕ ДЕТАЛИ:**
- Использовать существующие системы без создания новых
- Максимально переиспользовать код
- Следовать паттернам существующих врагов
- Обеспечить совместимость с EnemyPool

---

**Статус:** 📋 В разработке  
**Приоритет:** 🔥 Высокий  
**Оценка времени:** 4 часа (благодаря использованию существующих систем и готовой интеграции)
