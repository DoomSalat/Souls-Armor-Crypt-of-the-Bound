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

## Стабильность Системы

### ✅ **Система Работает Стабильно**

Система групп врагов была успешно оптимизирована и теперь работает без подрагиваний. Все критические проблемы были устранены, и движение членов группы стало плавным и естественным.

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
- Минимальная сила влияния установлена в константу `MinInfluenceStrength = 0.1f`
- Заменен прямой вызов `AddInfluence()` на контролируемый метод

**Результат**: Дополнительная защита от резких изменений в движении

### 🎯 **ТЕКУЩЕЕ СОСТОЯНИЕ**
Система групп врагов:
- ✅ **Работает без подрагиваний**
- ✅ **Движение плавное и естественное**
- ✅ **Члены группы двигаются по инерции при малом влиянии**
- ✅ **Нет резких остановок и ускорений**
- ✅ **Стабильная и предсказуемая работа**

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
