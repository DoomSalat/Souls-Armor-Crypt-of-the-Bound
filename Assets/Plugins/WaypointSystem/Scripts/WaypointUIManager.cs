using UnityEngine;
using UnityEngine.Pool; // Required for efficient object pooling
using System.Collections.Generic;

namespace WrightAngle.Waypoint
{
	/// <summary>
	/// The core manager for the Waypoint System. Place one instance in your scene.
	/// Discovers active Waypoint Targets, manages a pool of Waypoint Marker UI elements,
	/// and orchestrates updates based on the assigned Camera and Waypoint Settings asset.
	/// Ensures efficient handling and display of waypoint markers.
	/// </summary>
	[AddComponentMenu("WrightAngle/Waypoint UI Manager")]
	[DisallowMultipleComponent] // Only one manager should exist per scene.
	public class WaypointUIManager : MonoBehaviour
	{
		[Header("Essential References")]
		[Tooltip("Assign the Waypoint Settings ScriptableObject asset here to configure the system.")]
		[SerializeField] private WaypointSettings settings;

		[Tooltip("Assign the primary Camera used in your gameplay scene.")]
		[SerializeField] private Camera waypointCamera;

		[Tooltip("Assign the UI Canvas's RectTransform that will serve as the parent for all instantiated waypoint markers.")]
		[SerializeField] private RectTransform markerParentCanvas;

		[Header("Custom Target Settings")]
		[SerializeField] private WaypointTarget[] customTargets = new WaypointTarget[0];

		// --- Internal State ---
		private ObjectPool<WaypointMarkerUI> markerPool; // Efficiently reuses marker UI GameObjects.
														 // Collections to manage active targets and their corresponding markers.
		private List<WaypointTarget> activeTargetList = new List<WaypointTarget>(); // Used for efficient iteration.
		private HashSet<WaypointTarget> activeTargetSet = new HashSet<WaypointTarget>(); // Used for fast checking of target existence.
		private Dictionary<WaypointTarget, WaypointMarkerUI> activeMarkers = new Dictionary<WaypointTarget, WaypointMarkerUI>(); // Maps a target to its active UI marker.
		private HashSet<WaypointTarget> customTargetSet = new HashSet<WaypointTarget>(); // Быстрый поиск кастомных целей

		// Platform optimization variables
		private int platformUpdateIndex = 0; // Индекс для распределения обновлений по кадрам
		private Vector3 cachedCameraPosition; // Кэшированная позиция камеры
		private float cachedCamPixelWidth;
		private float cachedCamPixelHeight;
		private bool cameraDataCached = false;
		private PlatformOptimizationData currentPlatformSettings; // Текущие настройки платформы

		private Camera _cachedWaypointCamera; // Cached camera reference for performance.
		private float lastUpdateTime = -1f;   // Used for update throttling based on UpdateFrequency.
		private bool isInitialized = false;   // Flag to prevent updates before successful initialization.

		// --- Unity Lifecycle ---

		private void Awake()
		{
			// Validate required references and setup.
			bool setupError = ValidateSetup();
			if (setupError)
			{
				enabled = false; // Disable component if setup fails.
				Debug.LogError($"<b>[{gameObject.name}] WaypointUIManager:</b> Component disabled due to setup errors. Check Inspector references.", this);
				return;
			}

			// Инициализируем платформенные настройки
			currentPlatformSettings = settings.GetCurrentPlatformSettings();

			// Автоматически включаем оптимизации если нужно
			if (settings.EnablePlatformOptimizations && currentPlatformSettings.enableOptimizations)
			{
				Debug.Log($"<b>[{gameObject.name}] WaypointUIManager:</b> Automatically enabling {currentPlatformSettings.platformName} optimizations for better performance.");
			}

			// Cache valid references.
			_cachedWaypointCamera = waypointCamera;
			// Set up the object pool for marker UI elements.
			InitializePool();
			InitializeCustomTargets();

			// Subscribe to events from WaypointTarget components for dynamic registration/unregistration.
			WaypointTarget.OnTargetEnabled += HandleTargetEnabled;
			WaypointTarget.OnTargetDisabled += HandleTargetDisabled;

			isInitialized = true; // Mark initialization successful.
								  //Debug.Log($"<b>[{gameObject.name}] WaypointUIManager:</b> Initialized for {currentPlatformSettings.platformName} platform{(currentPlatformSettings.enableOptimizations ? " with optimizations" : "")}.", this);
		}

		private void Start()
		{
			// Start runs after all Awakes, ensuring targets can be found reliably.
			if (!isInitialized) return; // Don't proceed if initialization failed.
										// Find and register any targets in the scene configured to activate automatically.
			FindAndRegisterInitialTargets();
		}

		private void OnDestroy()
		{
			// --- Cleanup ---
			// Unsubscribe from events to prevent memory leaks when the manager is destroyed.
			WaypointTarget.OnTargetEnabled -= HandleTargetEnabled;
			WaypointTarget.OnTargetDisabled -= HandleTargetDisabled;

			// Clear and dispose of the object pool and internal tracking collections.
			markerPool?.Dispose();
			customTargetSet.Clear();
			activeTargetList.Clear();
			activeTargetSet.Clear();
			activeMarkers.Clear();
		}

		/// <summary> Validates that all essential Inspector references are assigned correctly. Returns true if an error is found. </summary>
		private bool ValidateSetup()
		{
			bool error = false;
			if (waypointCamera == null) { Debug.LogError("WaypointUIManager Error: Waypoint Camera not assigned!", this); error = true; }
			if (settings == null) { Debug.LogError("WaypointUIManager Error: WaypointSettings not assigned!", this); error = true; }
			else if (settings.GetMarkerPrefab() == null) { Debug.LogError($"WaypointUIManager Error: Marker Prefab missing in WaypointSettings '{settings.name}'!", this); error = true; } // Check prefab validity.
			if (markerParentCanvas == null) { Debug.LogError("WaypointUIManager Error: Marker Parent Canvas not assigned!", this); error = true; }
			else if (markerParentCanvas.GetComponentInParent<Canvas>() == null) { Debug.LogError("WaypointUIManager Error: Marker Parent Canvas must be a child of a UI Canvas!", this); error = true; } // Ensure it's part of a valid UI hierarchy.
			return error;
		}

		private void Update()
		{
			// Exit if not initialized or essential components are missing.
			if (!isInitialized) return;

			// Используем платформенные оптимизации
			if (currentPlatformSettings.enableOptimizations)
			{
				UpdatePlatformOptimized();
			}
			else
			{
				UpdateStandard();
			}
		}

		private void UpdatePlatformOptimized()
		{
			// Используем платформенную частоту обновления
			if (Time.time < lastUpdateTime + currentPlatformSettings.updateFrequency) return;
			lastUpdateTime = Time.time;

			// Кэшируем данные камеры только раз за кадр
			if (!cameraDataCached)
			{
				cachedCameraPosition = _cachedWaypointCamera.transform.position;
				cachedCamPixelWidth = _cachedWaypointCamera.pixelWidth;
				cachedCamPixelHeight = _cachedWaypointCamera.pixelHeight;
				cameraDataCached = true;
			}

			// Обновляем только ограниченное количество маркеров за кадр
			int markersToUpdate = Mathf.Min(currentPlatformSettings.maxMarkersPerFrame, activeTargetList.Count);

			for (int i = 0; i < markersToUpdate; i++)
			{
				// Циклически проходим по всем маркерам
				int targetIndex = (platformUpdateIndex + i) % activeTargetList.Count;
				if (targetIndex >= activeTargetList.Count) break;

				WaypointTarget target = activeTargetList[targetIndex];

				if (target == null || !target.gameObject.activeInHierarchy)
				{
					// Отложенная очистка для оптимизированных платформ
					RemoveTargetCompletely(target, targetIndex);
					continue;
				}

				UpdateSingleMarkerOptimized(target);
			}

			// Сдвигаем индекс для следующего кадра
			platformUpdateIndex = (platformUpdateIndex + markersToUpdate) % Mathf.Max(1, activeTargetList.Count);
		}

		private void UpdateStandard()
		{
			// Используем минимальную частоту обновления для UI, если она установлена
			float effectiveUpdateFrequency = settings.MinUIUpdateFrequency > 0 ?
				Mathf.Min(settings.UpdateFrequency, settings.MinUIUpdateFrequency) :
				settings.UpdateFrequency;

			// Throttle the update logic based on the frequency defined in settings.
			if (Time.time < lastUpdateTime + effectiveUpdateFrequency) return;
			lastUpdateTime = Time.time;

			UpdateAllMarkers();
		}

		private void LateUpdate()
		{
			// Сбрасываем кэш камеры в конце кадра
			cameraDataCached = false;

			// Обновляем маркеры в LateUpdate для лучшей синхронизации с движением камеры
			// Отключаем для оптимизированных платформ
			if (!isInitialized || !settings.EnableHighFrequencyCustomMarkers || currentPlatformSettings.enableOptimizations) return;

			// Для критически важных маркеров (например, кастомных) обновляем каждый кадр
			UpdateCriticalMarkers();
		}

		private void UpdateAllMarkers()
		{
			// Cache camera position for use within the loop.
			Vector3 cameraPosition = _cachedWaypointCamera.transform.position;
			float camPixelWidth = _cachedWaypointCamera.pixelWidth;
			float camPixelHeight = _cachedWaypointCamera.pixelHeight;

			// Iterate backwards through the list of active targets for safe removal during iteration.
			for (int i = activeTargetList.Count - 1; i >= 0; i--)
			{
				WaypointTarget target = activeTargetList[i];

				// --- Target Validity & Cleanup ---
				// Handle cases where the target might have been destroyed or deactivated unexpectedly.
				if (target == null || !target.gameObject.activeInHierarchy)
				{
					RemoveTargetCompletely(target, i); // Clean up tracking data.
					continue; // Move to the next target.
				}

				// --- Check for Custom Target ---
				bool isCustomTarget = customTargetSet.Contains(target);

				// Кастомные маркеры обновляются в LateUpdate, пропускаем их здесь
				if (isCustomTarget) continue;

				UpdateSingleMarker(target, cameraPosition, camPixelWidth, camPixelHeight);
			}
		}

		private void UpdateCriticalMarkers()
		{
			// Обновляем только кастомные маркеры каждый кадр для лучшей отзывчивости
			Vector3 cameraPosition = _cachedWaypointCamera.transform.position;
			float camPixelWidth = _cachedWaypointCamera.pixelWidth;
			float camPixelHeight = _cachedWaypointCamera.pixelHeight;

			foreach (WaypointTarget target in customTargetSet)
			{
				if (target != null && target.gameObject.activeInHierarchy && activeTargetSet.Contains(target))
				{
					UpdateSingleMarker(target, cameraPosition, camPixelWidth, camPixelHeight);
				}
			}
		}

		private void UpdateSingleMarker(WaypointTarget target, Vector3 cameraPosition, float camPixelWidth, float camPixelHeight)
		{
			// --- Core Waypoint Logic ---
			Transform targetTransform = target.transform;
			Vector3 targetWorldPos = targetTransform.position;

			// Calculate distance for visibility checks and scaling.
			float distanceToTarget = CalculateDistance(cameraPosition, targetWorldPos, settings);

			// Hide marker and skip further processing if beyond the maximum visible distance.
			if (distanceToTarget > settings.MaxVisibleDistance)
			{
				TryReleaseMarker(target); // Release marker back to the pool if it was active.
				return;
			}

			// Project the target's world position to screen space.
			Vector3 screenPos = _cachedWaypointCamera.WorldToScreenPoint(targetWorldPos);
			bool isBehindCamera = screenPos.z <= 0; // Check if target is behind the camera's near plane.
													// Check if the projected position is within the screen bounds (and not behind).
			bool isOnScreen = !isBehindCamera && screenPos.x > 0 && screenPos.x < camPixelWidth && screenPos.y > 0 && screenPos.y < camPixelHeight;

			// Determine if a marker should be displayed based on screen status and settings.
			bool shouldShowMarker;
			if (settings.HideMarkersWhenOnScreen)
			{
				// Показывать маркер только если объект НЕ на экране и включены off-screen индикаторы
				shouldShowMarker = !isOnScreen && settings.UseOffScreenIndicators;
			}
			else
			{
				// Стандартная логика: показывать маркер если объект на экране ИЛИ если включены off-screen индикаторы
				shouldShowMarker = isOnScreen || (settings.UseOffScreenIndicators && !isOnScreen);
			}

			if (shouldShowMarker)
			{
				// --- Get or Activate Marker ---
				// Try to get an existing marker; if none exists, retrieve one from the pool.
				if (!activeMarkers.TryGetValue(target, out WaypointMarkerUI markerInstance))
				{
					markerInstance = markerPool.Get(); // Get from pool (activates the GameObject).
					activeMarkers.Add(target, markerInstance); // Associate the new marker with the target.
				}
				// Ensure the marker's GameObject is active (could be inactive if just retrieved from pool).
				if (!markerInstance.gameObject.activeSelf) markerInstance.gameObject.SetActive(true);

				// --- Update Marker Visuals ---
				// Call the marker's UpdateDisplay method to set its position, rotation, and scale.
				markerInstance.UpdateDisplay(screenPos, isOnScreen, isBehindCamera, _cachedWaypointCamera, settings, distanceToTarget);
			}
			else // Marker should not be shown (e.g., off-screen and indicators disabled).
			{
				TryReleaseMarker(target); // Release marker back to the pool if it was active.
			}
		}

		private void UpdateSingleMarkerOptimized(WaypointTarget target)
		{
			// Используем кэшированные данные камеры для WebGL оптимизации
			Transform targetTransform = target.transform;
			Vector3 targetWorldPos = targetTransform.position;

			// Упрощенная проверка дистанции (без корня для экономии)
			Vector3 deltaPos = targetWorldPos - cachedCameraPosition;
			float sqrDistance = deltaPos.sqrMagnitude;
			float maxDistanceSqr = settings.MaxVisibleDistance * settings.MaxVisibleDistance;

			if (sqrDistance > maxDistanceSqr)
			{
				TryReleaseMarker(target);
				return;
			}

			// Project the target's world position to screen space.
			Vector3 screenPos = _cachedWaypointCamera.WorldToScreenPoint(targetWorldPos);
			bool isBehindCamera = screenPos.z <= 0;
			bool isOnScreen = !isBehindCamera && screenPos.x > 0 && screenPos.x < cachedCamPixelWidth && screenPos.y > 0 && screenPos.y < cachedCamPixelHeight;

			// Упрощенная логика видимости для WebGL
			bool shouldShowMarker = settings.HideMarkersWhenOnScreen ?
				(!isOnScreen && settings.UseOffScreenIndicators) :
				(isOnScreen || (settings.UseOffScreenIndicators && !isOnScreen));

			if (shouldShowMarker)
			{
				if (!activeMarkers.TryGetValue(target, out WaypointMarkerUI markerInstance))
				{
					markerInstance = markerPool.Get();
					activeMarkers.Add(target, markerInstance);
				}

				if (!markerInstance.gameObject.activeSelf)
					markerInstance.gameObject.SetActive(true);

				// Вычисляем реальную дистанцию только при необходимости
				float realDistance = Mathf.Sqrt(sqrDistance);
				markerInstance.UpdateDisplay(screenPos, isOnScreen, isBehindCamera, _cachedWaypointCamera, settings, realDistance);
			}
			else
			{
				TryReleaseMarker(target);
			}
		}

		// --- Calculation Helper ---

		/// <summary> Calculates distance between camera and target, optionally ignoring Z-axis in 2D mode. Used for MaxVisibleDistance check. </summary>
		private float CalculateDistance(Vector3 camPos, Vector3 targetPos, WaypointSettings currentSettings)
		{
			if (currentSettings.GameMode == WaypointSettings.ProjectionMode.Mode2D && currentSettings.IgnoreZAxisForDistance2D)
			{
				// Calculate distance using only X and Y components.
				return Vector2.Distance(new Vector2(camPos.x, camPos.y), new Vector2(targetPos.x, targetPos.y));
			}
			else
			{
				// Calculate standard 3D distance.
				return Vector3.Distance(camPos, targetPos);
			}
		}

		// --- Target Management ---

		/// <summary> Scans the scene at startup for WaypointTargets configured to 'ActivateOnStart'. </summary>
		private void FindAndRegisterInitialTargets()
		{
			// Find all WaypointTarget components in the scene, including inactive ones initially.
			// Use FindObjectsByType for modern Unity versions and better performance options.
			WaypointTarget[] allTargets = FindObjectsByType<WaypointTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			int activationCount = 0;
			foreach (WaypointTarget target in allTargets)
			{
				// Register only if ActivateOnStart is true AND the target's GameObject is currently active in the hierarchy.
				if (target.ActivateOnStart && target.gameObject.activeInHierarchy)
				{
					RegisterTarget(target);
					activationCount++;
				}
				else
				{
					// To help users understand why auto-activation didn't occur
					Debug.Log($"<b>[{gameObject.name}] WaypointUIManager:</b> Target '{target.gameObject.name}' has ActivateOnStart=true but is inactive in the hierarchy. It will not be auto-activated.", target.gameObject);
				}
			}
			//Debug.Log($"<b>[{gameObject.name}] WaypointUIManager:</b> Found {allTargets.Length} potential targets, activated {activationCount} marked 'ActivateOnStart'.");
		}

		/// <summary> Adds a target to the internal tracking collections if it's not already tracked. </summary>
		private void RegisterTarget(WaypointTarget target)
		{
			// Use HashSet.Add for an efficient way to add only if the target isn't already present.
			if (target != null && activeTargetSet.Add(target))
			{
				// If successfully added to the set, also add to the list used for iteration.
				activeTargetList.Add(target);
				// Note: The UI marker itself is only fetched from the pool when needed during the Update loop.
			}
		}

		/// <summary> Attempts to find the marker associated with a target and releases it back to the pool. </summary>
		private void TryReleaseMarker(WaypointTarget target)
		{
			// Check if the target is valid and if there's an active marker mapped to it.
			if (target != null && activeMarkers.TryGetValue(target, out WaypointMarkerUI markerToRelease))
			{
				markerPool.Release(markerToRelease); // Return the marker to the pool (deactivates the GameObject).
				activeMarkers.Remove(target);        // Remove the association from the dictionary.
			}
		}

		/// <summary> Removes a target completely from all tracking lists and ensures its marker is released. </summary>
		private void RemoveTargetCompletely(WaypointTarget target, int listIndex = -1)
		{
			// Ensure the marker is released back to the pool first.
			TryReleaseMarker(target);

			// Remove from the fast lookup set.
			if (target != null) activeTargetSet.Remove(target);

			// Efficiently remove from the list if the index is known and valid.
			if (listIndex >= 0 && listIndex < activeTargetList.Count && activeTargetList[listIndex] == target)
			{
				activeTargetList.RemoveAt(listIndex);
			}
			// Fallback: Search and remove from the list if index is unknown or invalid (less efficient).
			else if (target != null)
			{
				activeTargetList.Remove(target);
			}
			// Handle potential null entries that might occur if objects are destroyed improperly.
			else
			{
				activeTargetList.RemoveAll(item => item == null);
			}
		}

		// --- Pool Management Callbacks ---

		/// <summary> Sets up the Object Pool for creating and reusing WaypointMarkerUI instances. </summary>
		private void InitializePool()
		{
			// Get the prefab configured in settings (already validated in Awake).
			GameObject prefab = settings.GetMarkerPrefab();
			if (prefab == null) return; // Safety check.

			markerPool = new ObjectPool<WaypointMarkerUI>(
				createFunc: () =>
				{ // Defines how to create a new marker instance when the pool is empty.
					GameObject go = Instantiate(prefab, markerParentCanvas);
					WaypointMarkerUI ui = go.GetComponent<WaypointMarkerUI>();
					// Add the script if missing on the prefab (for robustness).
					if (ui == null)
					{
						ui = go.AddComponent<WaypointMarkerUI>();
						Debug.LogWarning($"WaypointUIManager: Added missing WaypointMarkerUI script to '{prefab.name}' instance.", go);
					}
					go.SetActive(false); // Ensure new instances start inactive.
					return ui;
				},
				actionOnGet: (marker) => marker.gameObject.SetActive(true),    // Action performed when an item is taken from the pool.
				actionOnRelease: (marker) => marker.gameObject.SetActive(false), // Action performed when an item is returned to the pool.
				actionOnDestroy: (marker) => { if (marker != null) Destroy(marker.gameObject); }, // Action performed when the pool destroys an item.
				collectionCheck: true, // Adds extra checks in editor builds to detect pool corruption issues.
				defaultCapacity: 10,   // Initial number of items the pool can hold.
				maxSize: 100         // Maximum number of items the pool will store.
			);
		}

		// --- Target Event Handlers ---

		/// <summary> Responds to the OnTargetEnabled event, registering the target. </summary>
		private void HandleTargetEnabled(WaypointTarget target) => RegisterTarget(target);

		/// <summary> Responds to the OnTargetDisabled event, removing the target completely. </summary>
		private void HandleTargetDisabled(WaypointTarget target)
		{
			// Find the target's index for potentially faster removal from the list.
			int index = activeTargetList.IndexOf(target);
			RemoveTargetCompletely(target, index);
		}

		// --- Custom Target Management ---

		/// <summary> Initializes custom targets. </summary>
		private void InitializeCustomTargets()
		{
			// Добавляем все кастомные цели в HashSet для быстрого поиска
			foreach (WaypointTarget target in customTargets)
			{
				if (target != null)
				{
					customTargetSet.Add(target);
				}
			}
		}

		// --- Public API ---

		/// <summary>
		/// Проверяет, является ли указанная цель кастомной
		/// </summary>
		/// <param name="target">Цель для проверки</param>
		/// <returns>True, если цель находится в списке кастомных целей</returns>
		public bool IsCustomTarget(WaypointTarget target)
		{
			return customTargetSet.Contains(target);
		}

		/// <summary>
		/// Добавляет цель в список кастомных целей во время выполнения
		/// </summary>
		/// <param name="target">Цель для добавления</param>
		public void AddCustomTarget(WaypointTarget target)
		{
			if (target != null)
			{
				customTargetSet.Add(target);
			}
		}

		/// <summary>
		/// Удаляет цель из списка кастомных целей во время выполнения
		/// </summary>
		/// <param name="target">Цель для удаления</param>
		public void RemoveCustomTarget(WaypointTarget target)
		{
			if (target != null)
			{
				customTargetSet.Remove(target);
			}
		}

	}
}