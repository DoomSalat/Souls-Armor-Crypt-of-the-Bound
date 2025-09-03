# План реализации кадсцены игрока

## Обзор
Реализация кадсцены, которая запускается при клике по персонажу и проигрывает анимацию "Start Idle" с последующим переходом к геймплею.

## Статус выполнения
**ЭТАП 1: ПОЛНОСТЬЮ ЗАВЕРШЕН** ✅

Базовая функциональность кадсцены реализована:
- ✅ Запуск по клику на Scope коллайдер
- ✅ Анимация Start Idle → Start Idle End  
- ✅ Телепортация меча
- ✅ Отключение/включение геймплея
- ✅ Защита от повторных кликов

## Архитектура (реализована)
- **CutsceneState** - состояние кадсцены
- **PlayerStateMachine** - интеграция с системой состояний
- **PlayerKnightAnimator** - управление анимациями
- **PlayerKnightAnimatorEvents** - события анимации

## Файлы (созданы/изменены)
- `CutsceneState.cs` - новое состояние
- `PlayerStateMachine.cs` - добавлен CutsceneState
- `Player.cs` - логика перехода в кадсцену

---

# ЭТАП 2: Расширенная логика кадсцены с Scope и управлением камерой

## Следующий план: Scope логика и управление камерой

### 1. Добавить логику Scope для притягивания души
**Цель**: В `CutsceneState` реализовать логику Scope, аналогичную `AbsorptionState`, но без вызова инвентаря.

#### Что нужно сделать:
- **Активировать Scope** при входе в кадсцену
- **Притягивать душу** к игроку (как в `AbsorptionState.OnSoulTargeted()`)
- **Убить душу** после притягивания (без вызова инвентаря)
- **Завершить кадсцену** после убийства души

#### Логика работы:
```
CutsceneState.Enter() → Активировать Scope
↓
Душа найдена → Начать притягивание
↓
Притягивание завершено → Убить душу
↓
Завершить кадсцену
```

### 2. Управление камерой
**Цель**: Изменить положение камеры в начале кадсцены и плавно вернуть к дефолтному состоянию.

#### Что нужно сделать:
- **Приблизить камеру** к игроку в начале кадсцены
- **Плавно отдалить** камеру до дефолтного состояния во время анимации
- **Восстановить** исходное положение камеры после завершения

#### Логика работы:
```
CutsceneState.Enter() → Приблизить камеру к игроку
↓
Анимация Start Idle → Плавно отдалять камеру
↓
Анимация Start Idle End → Продолжать отдаление
↓
CutsceneState.Exit() → Восстановить исходное положение камеры
```

## Техническая оценка текущих скриптов

### ✅ Что уже готово:
1. **CutsceneState** - базовая структура и логика
2. **Scope система** - `AbsorptionScopeController` и `AbsorptionScope` готовы к использованию
3. **Управление душей** - `ISoul` интерфейс и методы притягивания
4. **Система событий** - события анимации и завершения кадсцены

### ⚠️ Что нужно доработать:

#### 1. CutsceneState.cs
```csharp
// Добавить:
private readonly AbsorptionScope _absorptionScope; // Ссылка на Scope
private ISoul _currentSoul; // Текущая душа для притягивания
private bool _isSoulAttractionActive = false; // Состояние притягивания

// Методы:
private void ActivateScope() // Активировать Scope
private void OnSoulFound(ISoul soul) // Обработка найденной души
private void OnSoulAttractionCompleted() // Притягивание завершено
private void KillSoul() // Убить душу
```

#### 2. PlayerStateMachine.cs
```csharp
// Обновить конструктор CutsceneState:
{ typeof(CutsceneState), new CutsceneState(playerKnightAnimator, inputReader, swordController, cutsceneSwordTarget, playerLimbs, absorptionScopeController, absorptionScope) }
```

#### 3. CameraController.cs (существующий скрипт)
**Статус: СУЩЕСТВУЕТ, ТРЕБУЕТ ДОРАБОТКИ** ⚠️

В `Assets/Content/Scripts systems/Camera/CameraController.cs` уже реализовано:
- ✅ Управление Cinemachine камерами
- ✅ Zoom эффекты для AbsorptionState
- ✅ Плавные переходы с DOTween
- ✅ Переключение между глобальной и игрок-моб камерами

**Что нужно добавить для кадсцены:**
```csharp
// Добавить в существующий CameraController:
[Title("Cutscene Settings")]
[SerializeField, Min(0.1f)] private float _cutsceneTargetOrthoSize = 2f; // Приближение к игроку
[SerializeField, Min(0.1f)] private float _cutsceneZoomDuration = 0.8f; // Длительность приближения
[SerializeField] private Ease _cutsceneZoomEase = Ease.InOutQuad; // Плавность

// Методы для кадсцены:
public void StartCutsceneZoom() // Приблизить камеру к игроку
public void SmoothTransitionToDefault() // Плавно отдалить до дефолтного состояния
public void RestoreDefaultPosition() // Восстановить исходное положение
```

**Преимущества использования существующего скрипта:**
- Не нужно создавать новый компонент
- Уже настроена интеграция с Cinemachine
- Есть готовые DOTween анимации
- Можно переиспользовать существующую логику приоритетов камер

#### 4. PlayerKnightAnimator.cs
```csharp
// Добавить события для управления камерой:
public event System.Action CutsceneCameraStart; // Начало приближения камеры
public event System.Action CutsceneCameraEnd; // Завершение отдаления камеры
```

## План реализации

### Фаза 1: Scope логика
1. **Добавить AbsorptionScope** в CutsceneState
2. **Реализовать активацию Scope** при входе в кадсцену
3. **Добавить обработку души** (находка, притягивание, убийство)
4. **Интегрировать с существующей логикой** завершения кадсцены

### Фаза 2: Управление камерой
1. **Добавить функции кадсцены** в существующий CameraController
2. **Добавить события камеры** в PlayerKnightAnimator
3. **Интегрировать управление камерой** в CutsceneState
4. **Настроить плавные переходы** камеры для кадсцены

### Фаза 3: Тестирование и отладка
1. **Проверить Scope логику** - притягивание и убийство души
2. **Проверить управление камерой** - приближение и отдаление
3. **Интеграционные тесты** - полный цикл кадсцены
4. **Оптимизация** - плавность анимаций и переходов
