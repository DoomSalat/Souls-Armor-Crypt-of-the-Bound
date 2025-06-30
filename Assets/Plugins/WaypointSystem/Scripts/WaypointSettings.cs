using UnityEngine;

namespace WrightAngle.Waypoint
{
	/// <summary>
	/// Configure your waypoint system's appearance and behavior globally.
	/// Create instances via 'Assets -> Create -> WrightAngle -> Waypoint Settings'.
	/// This asset allows easy tweaking of performance, visuals, and core mechanics.
	/// </summary>
	[CreateAssetMenu(fileName = "WaypointSettings", menuName = "WrightAngle/Waypoint Settings", order = 1)]
	public class WaypointSettings : ScriptableObject
	{
		/// <summary> Specifies the camera projection type used in your scene. </summary>
		public enum ProjectionMode { Mode3D, Mode2D }

		/// <summary> Defines the unit system for distance display. </summary>
		public enum DistanceUnitSystem { Metric, Imperial }

		[Header("Core Functionality")]
		[Tooltip("How often (in seconds) the waypoint system updates. Lower values increase responsiveness but may impact performance.")]
		[Range(0.01f, 1.0f)]
		public float UpdateFrequency = 0.1f;

		[Tooltip("Minimum update frequency for UI smoothness. Set to 0 to disable. Recommended: 0.016 (60 FPS) or 0.033 (30 FPS).")]
		[Range(0f, 0.1f)]
		public float MinUIUpdateFrequency = 0.016f;

		[Tooltip("Enable high-frequency updates for custom markers in LateUpdate. Improves responsiveness but uses more performance.")]
		public bool EnableHighFrequencyCustomMarkers = true;

		[Header("WebGL Optimization")]
		[Tooltip("Enable WebGL optimizations. Reduces update frequency and disables expensive operations for better web performance.")]
		public bool EnableWebGLOptimizations = false;

		[Tooltip("Update frequency when WebGL optimizations are enabled. Higher values save performance.")]
		[Range(0.05f, 0.5f)]
		public float WebGLUpdateFrequency = 0.1f;

		[Tooltip("Maximum number of markers to update per frame when WebGL optimizations are enabled.")]
		[Range(1, 10)]
		public int MaxMarkersPerFrame = 3;

		[Header("Platform Optimization")]
		[Tooltip("Enable automatic platform-specific optimizations. Adjusts settings based on the current platform.")]
		public bool EnablePlatformOptimizations = true;

		[Tooltip("Enable mobile optimizations for Android/iOS. Reduces update frequency and marker count.")]
		public bool EnableMobileOptimizations = false;

		[Tooltip("Update frequency for mobile platforms.")]
		[Range(0.05f, 0.3f)]
		public float MobileUpdateFrequency = 0.08f;

		[Tooltip("Maximum markers per frame on mobile platforms.")]
		[Range(1, 8)]
		public int MobileMaxMarkersPerFrame = 4;

		[Tooltip("Enable console optimizations for PlayStation/Xbox/Switch. Balances performance and quality.")]
		public bool EnableConsoleOptimizations = false;

		[Tooltip("Update frequency for console platforms.")]
		[Range(0.02f, 0.1f)]
		public float ConsoleUpdateFrequency = 0.05f;

		[Tooltip("Maximum markers per frame on console platforms.")]
		[Range(3, 15)]
		public int ConsoleMaxMarkersPerFrame = 8;

		[Tooltip("Select Mode3D for perspective cameras or Mode2D for orthographic cameras to ensure correct calculations.")]
		public ProjectionMode GameMode = ProjectionMode.Mode3D;

		[Tooltip("Assign your custom waypoint marker prefab here. This UI element will represent your waypoints visually.")]
		public GameObject MarkerPrefab;

		[Tooltip("The maximum distance (in world units) from the camera at which a waypoint marker remains visible.")]
		public float MaxVisibleDistance = 1000f;

		[Tooltip("When using Mode2D, enable this to calculate the MaxVisibleDistance check using only X and Y axes, ignoring Z.")]
		public bool IgnoreZAxisForDistance2D = true;

		[Header("Off-Screen Indicator")]
		[Tooltip("Enable this to show markers clamped to the screen edges when their target is outside the camera view.")]
		public bool UseOffScreenIndicators = true;

		[Tooltip("Hide markers when the target object is visible on screen. Only show markers for off-screen targets.")]
		public bool HideMarkersWhenOnScreen = false;

		[Tooltip("Define the distance (in pixels) from the screen edges where off-screen indicators will be positioned.")]
		[Range(0f, 100f)]
		public float ScreenEdgeMargin = 50f;

		[Tooltip("Enable this to flip the off-screen marker's vertical orientation. Useful if your marker icon naturally points downwards.")]
		public bool FlipOffScreenMarkerY = false;

		[Header("Distance Scaling")]
		[Tooltip("Enable this to make waypoint markers scale based on their distance from the camera.")]
		public bool EnableDistanceScaling = false;

		[Tooltip("The distance (in world units) at which the marker will be at its Default Scale. Markers further than this will scale down towards Min Scale Factor.")]
		public float DistanceForDefaultScale = 50f;

		[Tooltip("The distance (in world units) beyond which the marker will be at its Min Scale Factor. Scaling occurs between Distance For Default Scale and this value.")]
		public float MaxScalingDistance = 200f;

		[Tooltip("The minimum scale factor for the marker when it is at or beyond Max Scaling Distance. 0 will make it invisible.")]
		[Range(0f, 1f)]
		public float MinScaleFactor = 0.5f;

		[Tooltip("The default scale factor for the marker when it is at or closer than Distance For Default Scale.")]
		[Range(0.1f, 5f)]
		public float DefaultScaleFactor = 1.0f;

		[Header("Distance Text (TMPro)")]
		[Tooltip("Enable this to display the distance to the waypoint as text.")]
		public bool DisplayDistanceText = false;

		[Tooltip("Choose the unit system for displaying distances (Metric: m/km, Imperial: ft/mi).")]
		public DistanceUnitSystem UnitSystem = DistanceUnitSystem.Metric;

		[Tooltip("Number of decimal places for distance values (e.g., 1 for 123.4m).")]
		[Range(0, 3)]
		public int DistanceDecimalPlaces = 0;

		[Tooltip("Suffix for distances displayed in meters.")]
		public string SuffixMeters = "m";
		[Tooltip("Suffix for distances displayed in kilometers.")]
		public string SuffixKilometers = "km";
		[Tooltip("Suffix for distances displayed in feet.")]
		public string SuffixFeet = "ft";
		[Tooltip("Suffix for distances displayed in miles.")]
		public string SuffixMiles = "mi";

		// Conversion constants
		public const float METERS_PER_KILOMETER = 1000f;
		public const float FEET_PER_METER = 3.28084f;
		public const float FEET_PER_MILE = 5280f;


		// --- Helper Methods ---

		/// <summary>
		/// Retrieves the assigned marker prefab GameObject.
		/// Ensures a prefab is assigned before use.
		/// </summary>
		/// <returns>The assigned marker prefab, or null if none is set.</returns>
		public GameObject GetMarkerPrefab()
		{
			if (MarkerPrefab == null)
			{
				Debug.LogError("WaypointSettings: Marker Prefab is not assigned! Please assign a prefab in the Waypoint Settings asset.", this);
			}
			return MarkerPrefab;
		}

		// --- Platform Optimization Methods ---

		/// <summary>
		/// Определяет текущую платформу и возвращает соответствующие настройки оптимизации
		/// </summary>
		public PlatformOptimizationData GetCurrentPlatformSettings()
		{
			if (!EnablePlatformOptimizations)
			{
				return new PlatformOptimizationData
				{
					updateFrequency = UpdateFrequency,
					maxMarkersPerFrame = int.MaxValue,
					enableOptimizations = false,
					platformName = "Standard"
				};
			}

#if UNITY_WEBGL && !UNITY_EDITOR
				return new PlatformOptimizationData
				{
					updateFrequency = WebGLUpdateFrequency,
					maxMarkersPerFrame = MaxMarkersPerFrame,
					enableOptimizations = true,
					platformName = "WebGL"
				};
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				return new PlatformOptimizationData
				{
					updateFrequency = MobileUpdateFrequency,
					maxMarkersPerFrame = MobileMaxMarkersPerFrame,
					enableOptimizations = true,
					platformName = "Mobile"
				};
#elif (UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE || UNITY_SWITCH) && !UNITY_EDITOR
				return new PlatformOptimizationData
				{
					updateFrequency = ConsoleUpdateFrequency,
					maxMarkersPerFrame = ConsoleMaxMarkersPerFrame,
					enableOptimizations = true,
					platformName = "Console"
				};
#else
			// PC/Mac/Linux или Editor
			return new PlatformOptimizationData
			{
				updateFrequency = Mathf.Min(UpdateFrequency, MinUIUpdateFrequency > 0 ? MinUIUpdateFrequency : UpdateFrequency),
				maxMarkersPerFrame = int.MaxValue,
				enableOptimizations = false,
				platformName = "Desktop"
			};
#endif
		}

		/// <summary>
		/// Проверяет, нужны ли оптимизации для текущей платформы
		/// </summary>
		public bool ShouldUseOptimizations()
		{
#if UNITY_WEBGL && !UNITY_EDITOR
				return EnableWebGLOptimizations || EnablePlatformOptimizations;
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				return EnableMobileOptimizations || EnablePlatformOptimizations;
#elif (UNITY_PS4 || UNITY_PS5 || UNITY_XBOXONE || UNITY_GAMECORE || UNITY_SWITCH) && !UNITY_EDITOR
				return EnableConsoleOptimizations || EnablePlatformOptimizations;
#else
			return false; // Desktop не нуждается в оптимизациях
#endif
		}
	}

	/// <summary>
	/// Данные оптимизации для конкретной платформы
	/// </summary>
	[System.Serializable]
	public struct PlatformOptimizationData
	{
		public float updateFrequency;
		public int maxMarkersPerFrame;
		public bool enableOptimizations;
		public string platformName;
	}
} // End Namespace