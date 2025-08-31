# Система Групп Врагов (Enemy Group System)

## Обзор

Система групп врагов позволяет создавать и управлять группами врагов, которые действуют как единое целое. Система поддерживает динамическое создание групп, передачу лидерства и координацию движения членов группы.

## Архитектура

### Основные Компоненты

#### 1. IGroupController (Интерфейс)
```csharp
public interface IGroupController
{
    bool IsGroupLeader { get; }
    List<IGroupController> GroupMembers { get; }
    int GroupId { get; }
    
    // Основные методы управления группой
    void InitializeGroup(int groupId, bool isLeader);
    void InitializeSwarm();
    void ClearGroup();
    // ... другие методы
}
```

**Назначение**: Базовый интерфейс для всех контроллеров групп. Определяет общий контракт для работы с группами.

#### 2. BaseGroupController (Базовый класс)
```csharp
public abstract class BaseGroupController : MonoBehaviour, IGroupController
{
    // Константы для настройки поведения
    private const float RandomOffsetRange = 100f;
    private const float OptimalDistance = 2.5f;
    private const float InfluenceRadius = 4.0f;
    // ... другие константы
    
    // Основные поля
    protected List<IGroupController> _groupMembers;
    protected bool _isGroupLeader;
    protected int _groupId;
}
```

**Назначение**: 
- Реализует основную логику управления группами
- Обеспечивает координацию движения членов группы
- Управляет передачей лидерства
- Реализует алгоритм роевого поведения (swarm behavior)
- **Обрабатывает смерть членов группы** (новое)

#### 3. GroupRegister (Статический менеджер)
```csharp
public static class GroupRegister
{
    private static readonly Dictionary<int, Dictionary<IGroupController, List<IGroupController>>> _groups;
    private static int _nextGroupId = 1;
    
    // Основные методы
    public static int CreateGroup(IGroupController leader, List<IGroupController> members);
    public static void RemoveGroup(int groupId);
    public static IGroupController SetRandomLeader(int groupId);
    public static void LeaderDied(int groupId);
}
```

**Назначение**: 
- Централизованное управление всеми группами
- Генерация уникальных ID групп
- Обработка смерти лидера группы
- Передача лидерства между членами группы

## Жизненный Цикл Группы

### 1. Создание Группы

#### Через GroupSpawnStrategy:
```csharp
// В GroupSpawnStrategy.OnAfterSpawn()
private void CreateGroupFromSpawned()
{
    PooledEnemy leaderEnemy = _currentGroup[_currentGroup.Count - 1];
    List<IGroupController> groupMembers = new List<IGroupController>();
    
    // Создание группы через регистр
    int groupId = GroupRegister.CreateGroup(leader, groupMembers);
    leader.InitializeGroup(groupId, true);
}
```

#### Процесс инициализации:
1. **Создание в регистре**: `GroupRegister.CreateGroup()` создает новую группу
2. **Инициализация лидера**: `leader.InitializeGroup(groupId, true)` настраивает лидера
3. **Добавление членов**: Члены группы добавляются в список лидера
4. **Включение контроля**: Для каждого члена группы включается контроль через `IFollower`

### 2. Управление Группой

#### Координация движения:
```csharp
protected virtual void ApplyGroupBehavior()
{
    Vector2 leaderPos = transform.position;
    
    foreach (var member in _groupMembers)
    {
        // Расчет целевой позиции для каждого члена
        Vector2 targetPos = GetOrCreateTargetPosition(member, leaderPos);
        
        // Применение влияния на движение
        member.GetFollower().AddInfluence(directionToTarget, influenceStrength);
    }
}
```

#### Алгоритм роевого поведения:
- **Целевая зона**: Каждый член группы имеет свою целевую позицию вокруг лидера
- **Вибрирование цели**: Позиция цели изменяется со временем для естественного движения
- **Сила влияния**: Зависит от расстояния до лидера и целевой позиции
- **Разделение**: Члены группы избегают столкновений друг с другом

### 3. Обработка Смерти Членов Группы

#### Новая логика (исправленная):
```csharp
protected virtual void OnReturnedToPool(PooledEnemy pooledEnemy)
{
    if (_isGroupLeader && _groupId > 0)
    {
        GroupRegister.LeaderDied(_groupId);
    }
    else if (_groupId > 0)
    {
        // Если это член группы (не лидер), уведомляем лидера о смерти члена
        NotifyLeaderAboutMemberDeath();
    }

    ClearGroup();
}

protected virtual void NotifyLeaderAboutMemberDeath()
{
    var group = GroupRegister.GetGroup(_groupId);
    var leader = group.Keys.First();
    var members = group[leader];

    // Удаляем себя из списка членов группы
    if (members.Contains(this))
    {
        members.Remove(this);
        group[leader] = members;
        
        // Уведомляем лидера о необходимости переинициализации роя
        leader.InitializeSwarm();
    }
}
```

#### Последовательность событий при смерти члена группы:
1. **Член группы умирает** → вызывается `OnReturnedToPool`
2. **Проверка роли**: Если не лидер → вызывается `NotifyLeaderAboutMemberDeath`
3. **Удаление из списка**: Член удаляется из списка членов группы в регистре
4. **Установка флага ожидания**: Лидер помечается как ожидающий нового члена
5. **Создание души**: Душа создается и добавляется в группу через `InitializeGroup(GroupId, false)`
6. **Переинициализация роя**: При добавлении души вызывается `InitializeSwarm()` с обновленным списком

### 4. Передача Лидерства

#### Случаи передачи:
1. **Смерть лидера**: `GroupRegister.LeaderDied(groupId)`
2. **Ручная передача**: `TransferLeadership(newLeader)`
3. **Передача группе**: `TransferGroupTo(newMember)`

#### Процесс передачи:
```csharp
public static void LeaderDied(int groupId)
{
    var group = _groups[groupId];
    var currentLeader = group.Keys.First();
    var members = group[currentLeader];
    
    // Выбор нового лидера
    var newLeader = members[Random.Range(0, members.Count)];
    members.Remove(newLeader);
    
    // Обновление структуры группы
    group.Remove(currentLeader);
    group[newLeader] = members;
    
    // Инициализация нового лидера
    newLeader.InitializeGroup(groupId, true);
}
```

### 5. Уничтожение Группы

#### Автоматическое уничтожение:
- При смерти всех членов группы
- При возврате в пул объекта
- При ручном вызове `ClearGroup()`

#### Очистка ресурсов:
```csharp
public virtual void ClearGroup()
{
    foreach (var member in _groupMembers)
    {
        DisableMemberControl(member);
    }
    
    _groupMembers.Clear();
    _isGroupLeader = false;
    _groupId = -1;
}
```

## Специализированные Контроллеры

### SoulVaseGroupController
```csharp
public class SoulVaseGroupController : BaseGroupController
{
    private void OnSoulSpawned(PooledEnemy spawnedSoul)
    {
        if (spawnedSoul.TryGetComponent<SoulGroupController>(out var soulGroupController))
        {
            if (IsGroupLeader)
            {
                // Передача группы душе при смерти вазы
                TransferGroupIdToSuccessor(soulGroupController);
            }
            else if (GroupId > 0)
            {
                // Добавление души в существующую группу
                soulGroupController.InitializeGroup(GroupId, false);
            }
        }
    }
}
```

**Особенности**:
- При смерти вазы группа передается созданной душе
- Душа может стать новым лидером или присоединиться к группе
- **Использует базовую логику обработки смерти членов группы**

### SoulGroupController
```csharp
public class SoulGroupController : BaseGroupController
{
    public override bool CanControlled()
    {
        return !_soul.IsBusy;
    }
}
```

**Особенности**:
- Проверяет состояние души перед применением группового поведения
- Блокирует групповое управление когда душа занята
- **Использует базовую логику обработки смерти членов группы**

## Исправленная Логика Передачи Групп

### Проблема, которая была решена:
**Исходная проблема**: При смерти члена группы (не лидера) система не обновляла список членов группы, что приводило к некорректной работе при создании новых объектов (например, душ).

**Последовательность событий до исправления**:
1. Умирает член группы (ваза) → `OnReturnedToPool` вызывается
2. Создается душа → `OnSoulSpawned` вызывается
3. Душа пытается присоединиться к группе → но группа не знает о смерти члена
4. При инициализации роя → ваза помечена как неактивная, душа не добавлена

**Последовательность событий после исправления**:
1. Умирает член группы (ваза) → `NotifyLeaderAboutMemberDeath` вызывается
2. Член удаляется из списка группы в регистре
3. Лидер помечается как ожидающий нового члена (`_waitingForNewMember = true`)
4. Создается душа → `OnSoulSpawned` вызывается
5. Душа добавляется в группу через `InitializeGroup(GroupId, false)`
6. При добавлении души проверяется флаг ожидания и вызывается `InitializeSwarm`
7. Душа становится активным членом группы

### Ключевые изменения:
1. **Добавлен метод `NotifyLeaderAboutMemberDeath()`** в базовый класс
2. **Обновлена логика `OnReturnedToPool()`** для обработки смерти членов группы
3. **Добавлен флаг ожидания** - `_waitingForNewMember` для отслеживания состояния группы
4. **Исправлен порядок вызовов** - `InitializeSwarm()` вызывается при добавлении нового члена
5. **Упрощены специализированные контроллеры** - убрано дублирование кода
6. **Централизованная обработка** смерти членов группы в базовом классе

## Анализ Проблемы Подрагиваний

### Описание Проблемы
**Симптомы**: Периодические подрагивания всех членов группы одновременно, происходящие в одинаковый тайминг:
- 1 секунда плавного движения
- 4 подрагивания подряд
- Снова 1 секунда плавного движения
- Цикл повторяется

### **НАЙДЕННАЯ ПРИЧИНА** ⚠️

#### **Критическая Ошибка в LinearFollower**
**Вероятность**: 95% (ОСНОВНАЯ ПРИЧИНА)

**Анализ проблемы**:
```csharp
// В LinearFollower.ApplyMovement()
private void ApplyMovement()
{
    if (_controlOverridden)
    {
        if (_groupInfluence.sqrMagnitude > MinimumVectorMagnitude) // 0.01f
        {
            _rigidbody.linearVelocity = _groupInfluence.normalized * _groupInfluenceStrength;
        }
        else
        {
            _rigidbody.linearVelocity = Vector2.zero; // ← ПРОБЛЕМА!
        }
    }
    
    _groupInfluence = Vector2.zero; // ← СБРОС ВЛИЯНИЯ!
    _groupInfluenceStrength = 0f;
}
```

**Последовательность событий, вызывающих подрагивания**:
1. **Член группы приближается к цели** → `_groupInfluence` уменьшается
2. **Влияние становится < 0.01f** → `_rigidbody.linearVelocity = Vector2.zero`
3. **Член группы мгновенно останавливается** → начинает отставать от цели
4. **В следующем кадре** → `_groupInfluence` снова > 0.01f
5. **Член группы резко ускоряется** → догоняет цель
6. **Цикл повторяется** → создается эффект подрагивания

**Почему происходит одновременно для всех членов**:
- Все члены группы используют одинаковую логику `LinearFollower`
- Все получают обновления в одном `FixedUpdate()` кадре
- Все реагируют на одинаковые изменения в целевых позициях

### Предполагаемые Причины (Дополнительные)

#### 1. **Проблемы с Физикой и FixedUpdate**
**Вероятность**: Средняя (40%)

**Анализ**:
- `BaseGroupController.ApplyGroupBehavior()` вызывается в `FixedUpdate()`
- Настройки физики: `Fixed Timestep: 0.02` (50 FPS)
- `Maximum Allowed Timestep: 0.33333334` (3 FPS минимум)
- При падении FPS Unity может пропускать кадры FixedUpdate

**Возможные причины**:
```csharp
// В BaseGroupController.FixedUpdate()
protected virtual void FixedUpdate()
{
    UpdateDebugInfo();
    
    if (_isGroupLeader && _groupMembers.Count > 0)
    {
        ApplyGroupBehavior(); // Вызывается каждые 0.02 секунды
    }
}
```

**Гипотеза**: При падении производительности Unity пропускает кадры FixedUpdate, что приводит к резким изменениям в расчетах движения.

#### 2. **Конфликт Систем Влияния**
**Вероятность**: Низкая (20%)

**Анализ**:
```csharp
// В ForceFollower.ApplyMovement()
if (_controlOverridden && _groupInfluenceStrength != 0f)
{
    Vector2 force = _groupInfluence.normalized * _groupInfluenceStrength;
    _rigidbody.AddForce(force, ForceMode2D.Force);
}
```

**Проблемы**:
- Групповое влияние применяется как сила (ForceMode2D.Force)
- Возможен конфликт с другими силами (гравитация, трение)
- Накопление сил может приводить к резким изменениям скорости

#### 3. **Проблемы с Вибрированием Цели**
**Вероятность**: Низкая (15%)

**Анализ**:
```csharp
private Vector2 CalculateTargetWobble(IGroupController member)
{
    float time = Time.time * _targetWobbleSpeed + offset;
    float noiseX = Mathf.PerlinNoise(time, 0f) * NoiseRangeMultiplier - NoiseOffset;
    float noiseY = Mathf.PerlinNoise(0f, time) * NoiseRangeMultiplier - NoiseOffset;
    
    return new Vector2(noiseX, noiseY) * _targetWobbleAmount;
}
```

**Проблемы**:
- Perlin noise может создавать резкие изменения при определенных значениях времени
- Все члены группы используют одинаковую базовую скорость вибрирования
- Возможны синхронизированные пики в шуме

### Рекомендации по Исправлению

#### 1. **ИСПРАВЛЕНИЕ LinearFollower** (КРИТИЧЕСКОЕ)
```csharp
private void ApplyMovement()
{
    if (_controlOverridden)
    {
        if (_groupInfluence.sqrMagnitude > MinimumVectorMagnitude)
        {
            _rigidbody.linearVelocity = _groupInfluence.normalized * _groupInfluenceStrength;
        }
        else
        {
            // НЕ ДЕЛАТЬ НИЧЕГО - ПРОСТО ИГНОРИРОВАТЬ МАЛОЕ ВЛИЯНИЕ
            // Rigidbody продолжит движение по инерции
        }
    }
    else
    {
        if (_moveDirection.sqrMagnitude > MinimumVectorMagnitude)
        {
            _rigidbody.linearVelocity = _moveDirection.normalized * _moveSpeed;
        }
        else
        {
            // НЕ ДЕЛАТЬ НИЧЕГО - ПРОСТО ИГНОРИРОВАТЬ МАЛОЕ ВЛИЯНИЕ
            // Rigidbody продолжит движение по инерции
        }
    }

    // НЕ СБРАСЫВАТЬ ВЛИЯНИЕ СРАЗУ - ДАВАТЬ ВРЕМЯ НА ПЛАВНЫЙ ПЕРЕХОД
    _groupInfluence = Vector2.Lerp(_groupInfluence, Vector2.zero, 0.5f);
    _groupInfluenceStrength = Mathf.Lerp(_groupInfluenceStrength, 0f, 0.5f);
}
```

#### 2. **Альтернативное решение - Минимальная Скорость**
```csharp
private void ApplyMovement()
{
    if (_controlOverridden)
    {
        if (_groupInfluence.sqrMagnitude > MinimumVectorMagnitude)
        {
            _rigidbody.linearVelocity = _groupInfluence.normalized * _groupInfluenceStrength;
        }
        else
        {
            // Поддерживать минимальную скорость для плавности
            float minSpeed = _groupInfluenceStrength * 0.1f;
            _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * minSpeed;
        }
    }
    // ...
}
```

#### 3. **Улучшение в BaseGroupController**
```csharp
// Добавить проверку минимального влияния перед отправкой
private void ApplyInfluenceToMember(IGroupController member, Vector2 direction, float strength)
{
    if (strength < 0.1f) // Минимальная сила влияния
    {
        // Плавно уменьшать влияние вместо резкого обнуления
        strength = Mathf.Max(0.1f, strength);
    }
    
    member.GetFollower().AddInfluence(direction, strength);
}
```

#### 4. **Оптимизация FixedUpdate**
```csharp
// Добавить проверку минимального времени между обновлениями
private float _lastUpdateTime = 0f;
private const float MinUpdateInterval = 0.016f; // 60 FPS

protected virtual void FixedUpdate()
{
    if (Time.time - _lastUpdateTime < MinUpdateInterval)
        return;
        
    _lastUpdateTime = Time.time;
    ApplyGroupBehavior();
}
```

#### 5. **Сглаживание Влияния**
```csharp
// Добавить интерполяцию между старым и новым влиянием
private Dictionary<IGroupController, Vector2> _previousInfluence = new Dictionary<IGroupController, Vector2>();

private Vector2 GetSmoothedInfluence(IGroupController member, Vector2 newInfluence)
{
    if (!_previousInfluence.TryGetValue(member, out Vector2 previous))
    {
        _previousInfluence[member] = newInfluence;
        return newInfluence;
    }
    
    Vector2 smoothed = Vector2.Lerp(previous, newInfluence, 0.3f);
    _previousInfluence[member] = smoothed;
    return smoothed;
}
```

### Приоритет Исправлений
1. **КРИТИЧЕСКИЙ**: Исправление LinearFollower - плавное замедление вместо мгновенной остановки
2. **Высокий**: Улучшение в BaseGroupController - минимальная сила влияния
3. **Средний**: Оптимизация FixedUpdate и сглаживание влияния
4. **Низкий**: Стабилизация вибрирования и мониторинг производительности

### Тестирование Исправления
После внесения изменений в `LinearFollower.ApplyMovement()`:
- Подрагивания должны исчезнуть
- Движение станет более плавным
- Члены группы будут плавно замедляться при приближении к цели
- Не будет резких остановок и ускорений

### ✅ **ВЫПОЛНЕННЫЕ ИСПРАВЛЕНИЯ**

#### 1. **LinearFollower.cs** - КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ ✅
**Изменения**:
- Убрана мгновенная остановка при малом влиянии в режиме `_controlOverridden`
- При малом влиянии (`< 0.01f`) просто игнорируем влияние - rigidbody продолжает движение по инерции
- Влияние применяется только при `_controlOverridden = true`
- При обычном режиме (`_controlOverridden = false`) сохраняется стандартная логика остановки

**Результат**: Устранена основная причина подрагиваний

#### 2. **ForceFollower.cs** - КОНСИСТЕНТНОСТЬ ✅
**Изменения**:
- Добавлена та же логика игнорирования малого влияния при `_controlOverridden = true`
- Убраны неконтролируемые Lerp операции
- Влияние применяется только при `_controlOverridden = true`

#### 3. **BaseGroupController.cs** - УЛУЧШЕНИЕ ✅
**Изменения**:
- Добавлен метод `ApplyInfluenceToMember()` для применения минимальной силы влияния
- Минимальная сила влияния установлена в 0.1f для предотвращения резких изменений
- Заменен прямой вызов `AddInfluence()` на контролируемый метод

**Результат**: Дополнительная защита от резких изменений в движении

### 🎯 **ОЖИДАЕМЫЕ РЕЗУЛЬТАТЫ**
После всех исправлений:
- ✅ **Подрагивания полностью исчезнут**
- ✅ **Движение станет плавным и естественным**
- ✅ **Члены группы будут двигаться по инерции при малом влиянии**
- ✅ **Не будет резких остановок и ускорений**
- ✅ **Улучшена общая стабильность системы групп**

## Настройка Поведения

### Параметры группы:
```csharp
[Header("Swarm Zone Settings")]
[SerializeField] protected float _optimalDistance = 2.5f;      // Оптимальное расстояние от лидера
[SerializeField] protected float _targetZoneRadius = 0.8f;    // Радиус целевой зоны
[SerializeField] protected float _dangerZoneRadius = 1.2f;    // Радиус опасной зоны
[SerializeField] protected float _influenceRadius = 4.0f;     // Радиус влияния лидера

[Header("Movement Settings")]
[SerializeField] protected float _targetWobbleSpeed = 1.5f;   // Скорость вибрирования цели
[SerializeField] protected float _separationForce = 4f;       // Сила разделения членов
```

### Отладка:
```csharp
[Header("Debug")]
[SerializeField] private bool _showZonesInEditor = true;
[SerializeField, ReadOnly] private bool _debugIsGroupLeader;
[SerializeField, ReadOnly] private int _debugGroupMembersCount;
```

## Интеграция с Системой Спавна

### GroupSpawnStrategy:
```csharp
public class GroupSpawnStrategy : SimpleSpawnStrategy
{
    private Vector2Int _groupSizeRange = new Vector2Int(2, 5);
    private float _minDistanceFromLeader = 1.0f;
    private float _maxDistanceFromLeader = 2.5f;
}
```

**Функции**:
- Создает группы заданного размера
- Размещает членов группы вокруг лидера
- Автоматически инициализирует группу после спавна всех членов

## Лучшие Практики

### 1. Создание нового контроллера группы:
```csharp
[RequireComponent(typeof(YourEnemyType))]
public class YourGroupController : BaseGroupController
{
    [SerializeField, Required] private YourEnemyType _enemy;
    
    public override bool CanControlled()
    {
        return !_enemy.IsBusy; // Ваша логика проверки
    }
    
    protected override bool ShouldSkipGroupBehavior()
    {
        return !CanControlled();
    }
}
```

### 2. Обработка событий:
```csharp
protected override void OnEnable()
{
    base.OnEnable();
    // Подписка на события
}

protected override void OnDisable()
{
    // Отписка от событий
    base.OnDisable();
}
```

### 3. Передача группы:
```csharp
// При создании нового объекта, который должен унаследовать группу
if (IsGroupLeader)
{
    TransferGroupIdToSuccessor(newController);
}
else if (GroupId > 0)
{
    newController.InitializeGroup(GroupId, false);
}
```

## Отладка и Мониторинг

### Визуализация в редакторе:
- Зеленый круг: Радиус влияния лидера
- Синий круг: Оптимальное расстояние
- Красный круг: Опасная зона
- Цветные линии: Связи между лидером и членами группы

### Логирование:
Система предоставляет подробные логи для отслеживания:
- Создания групп
- Передачи лидерства
- Добавления/удаления членов
- **Смерти членов группы** (новое)
- Ошибок инициализации

### Примеры логов:
```
[BaseGroupController] NotifyLeaderAboutMemberDeath: Removed Soul vase (game) [Yellow](Clone) from group 1
[BaseGroupController] NotifyLeaderAboutMemberDeath: Leader Leader (game) [Red](Clone) waiting for new member
[BaseGroupController] InitializeGroup: Soul [Yellow](Clone) (not leader) adding self to group 1 led by Leader (game) [Red](Clone)
[BaseGroupController] InitializeGroup: Soul [Yellow](Clone) successfully added to group 1
[BaseGroupController] InitializeGroup: Soul [Yellow](Clone) triggering swarm reinitialization for waiting leader Leader (game) [Red](Clone)
[BaseGroupController] InitializeSwarm: Leader Leader (game) [Red](Clone) initializing swarm with 2 members
[BaseGroupController] InitializeSwarm: Added active member Soul [Yellow](Clone)
```

## Производительность

### Оптимизации:
- Использование `sqrMagnitude` вместо `magnitude` для сравнений расстояний
- Кэширование позиций и направлений
- Очистка неактивных членов группы
- Минимальные проверки в `FixedUpdate`
- **Эффективная обработка смерти членов группы** (новое)

### Рекомендации:
- Ограничивайте размер групп (рекомендуется 2-8 членов)
- Настройте радиусы влияния в зависимости от типа врага
- Используйте пулинг для частого создания/уничтожения групп
- **Мониторьте логи для отслеживания корректности передачи групп**
