using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemSwitcher : MonoBehaviour
{
	[SerializeField] private ParticleSystem[] _particleSystemTemplates;
	[SerializeField, ReadOnly] private int _currentTemplateIndex = -1;

	[Header("Debug")]
	[SerializeField, ReadOnly] private bool _isInitialized = false;

	private ParticleSystem _particleSystem;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		Initialize();
	}

	public bool SwitchToTemplate(int templateIndex)
	{
		Initialize();

		if (ValidateTemplateIndex(templateIndex) == false)
			return false;

		var templateSystem = _particleSystemTemplates[templateIndex];

		if (templateSystem == null)
		{
			Debug.LogError($"[ParticleSystemSwitcher] Template with index {templateIndex} is null!");
			return false;
		}

		bool wasPlaying = _particleSystem.isPlaying;

		CopyParticleSystemSettings(templateSystem, _particleSystem);

		if (wasPlaying && _particleSystem.isPlaying == false)
		{
			_particleSystem.Play();
		}

		_currentTemplateIndex = templateIndex;

		return true;
	}

	public bool SwitchToNextTemplate()
	{
		if (_particleSystemTemplates.Length == 0)
			return false;

		int nextIndex = (_currentTemplateIndex + 1) % _particleSystemTemplates.Length;
		return SwitchToTemplate(nextIndex);
	}

	public bool SwitchToPreviousTemplate()
	{
		if (_particleSystemTemplates.Length == 0)
			return false;

		int previousIndex = _currentTemplateIndex <= 0 ? _particleSystemTemplates.Length - 1 : _currentTemplateIndex - 1;
		return SwitchToTemplate(previousIndex);
	}

	public int GetCurrentTemplateIndex()
	{
		return _currentTemplateIndex;
	}

	public int GetTemplateCount()
	{
		return _particleSystemTemplates?.Length ?? 0;
	}

	public void Play()
	{
		_particleSystem.Play();
	}

	public void Stop()
	{
		_particleSystem.Stop();
	}

	private void Initialize()
	{
		if (_isInitialized)
			return;

		if (_particleSystem == null)
		{
			Debug.LogError($"[ParticleSystemSwitcher] ParticleSystem not found on {gameObject.name}!");
			return;
		}

		if (_particleSystemTemplates == null || _particleSystemTemplates.Length == 0)
		{
			Debug.LogWarning($"[ParticleSystemSwitcher] Templates array is empty on {gameObject.name}!");
			return;
		}

		_isInitialized = true;
	}

	private bool ValidateTemplateIndex(int templateIndex)
	{
		if (_isInitialized == false)
		{
			Debug.LogError($"[ParticleSystemSwitcher] Component not initialized on {gameObject.name}!");
			return false;
		}

		if (templateIndex < 0 || templateIndex >= _particleSystemTemplates.Length)
		{
			Debug.LogError($"[ParticleSystemSwitcher] Index {templateIndex} out of range [0, {_particleSystemTemplates.Length - 1}]!");
			return false;
		}

		return true;
	}

	private void CopyParticleSystemSettings(ParticleSystem source, ParticleSystem target)
	{
		var sourceMain = source.main;
		var targetMain = target.main;

		targetMain.startLifetime = sourceMain.startLifetime;
		targetMain.startSpeed = sourceMain.startSpeed;
		targetMain.startSize = sourceMain.startSize;
		targetMain.startRotation = sourceMain.startRotation;
		targetMain.startColor = sourceMain.startColor;
		targetMain.gravityModifier = sourceMain.gravityModifier;
		targetMain.simulationSpace = sourceMain.simulationSpace;
		targetMain.maxParticles = sourceMain.maxParticles;

		var sourceEmission = source.emission;
		var targetEmission = target.emission;

		targetEmission.enabled = sourceEmission.enabled;
		targetEmission.rateOverTime = sourceEmission.rateOverTime;
		targetEmission.rateOverDistance = sourceEmission.rateOverDistance;

		var sourceShape = source.shape;
		var targetShape = target.shape;

		targetShape.enabled = sourceShape.enabled;
		targetShape.shapeType = sourceShape.shapeType;
		targetShape.angle = sourceShape.angle;
		targetShape.radius = sourceShape.radius;
		targetShape.radiusThickness = sourceShape.radiusThickness;
		targetShape.arc = sourceShape.arc;
		targetShape.arcMode = sourceShape.arcMode;
		targetShape.arcSpread = sourceShape.arcSpread;

		var sourceVelocity = source.velocityOverLifetime;
		var targetVelocity = target.velocityOverLifetime;

		targetVelocity.enabled = sourceVelocity.enabled;
		targetVelocity.space = sourceVelocity.space;
		targetVelocity.x = sourceVelocity.x;
		targetVelocity.y = sourceVelocity.y;
		targetVelocity.z = sourceVelocity.z;

		var sourceColorOverLifetime = source.colorOverLifetime;
		var targetColorOverLifetime = target.colorOverLifetime;

		targetColorOverLifetime.enabled = sourceColorOverLifetime.enabled;
		targetColorOverLifetime.color = sourceColorOverLifetime.color;

		var sourceSizeOverLifetime = source.sizeOverLifetime;
		var targetSizeOverLifetime = target.sizeOverLifetime;

		targetSizeOverLifetime.enabled = sourceSizeOverLifetime.enabled;
		targetSizeOverLifetime.size = sourceSizeOverLifetime.size;

		var sourceRotationOverLifetime = source.rotationOverLifetime;
		var targetRotationOverLifetime = target.rotationOverLifetime;

		targetRotationOverLifetime.enabled = sourceRotationOverLifetime.enabled;
		targetRotationOverLifetime.x = sourceRotationOverLifetime.x;
		targetRotationOverLifetime.y = sourceRotationOverLifetime.y;
		targetRotationOverLifetime.z = sourceRotationOverLifetime.z;

		var sourceForceOverLifetime = source.forceOverLifetime;
		var targetForceOverLifetime = target.forceOverLifetime;

		targetForceOverLifetime.enabled = sourceForceOverLifetime.enabled;
		targetForceOverLifetime.space = sourceForceOverLifetime.space;
		targetForceOverLifetime.x = sourceForceOverLifetime.x;
		targetForceOverLifetime.y = sourceForceOverLifetime.y;
		targetForceOverLifetime.z = sourceForceOverLifetime.z;

		var sourceExternalForces = source.externalForces;
		var targetExternalForces = target.externalForces;

		targetExternalForces.enabled = sourceExternalForces.enabled;
		targetExternalForces.multiplier = sourceExternalForces.multiplier;

		var sourceNoise = source.noise;
		var targetNoise = target.noise;

		targetNoise.enabled = sourceNoise.enabled;
		targetNoise.strength = sourceNoise.strength;
		targetNoise.frequency = sourceNoise.frequency;
		targetNoise.scrollSpeed = sourceNoise.scrollSpeed;
		targetNoise.damping = sourceNoise.damping;
		targetNoise.octaveCount = sourceNoise.octaveCount;
		targetNoise.octaveMultiplier = sourceNoise.octaveMultiplier;
		targetNoise.octaveScale = sourceNoise.octaveScale;

		var sourceCollision = source.collision;
		var targetCollision = target.collision;

		targetCollision.enabled = sourceCollision.enabled;
		targetCollision.type = sourceCollision.type;
		targetCollision.mode = sourceCollision.mode;
		targetCollision.dampen = sourceCollision.dampen;
		targetCollision.bounce = sourceCollision.bounce;
		targetCollision.lifetimeLoss = sourceCollision.lifetimeLoss;
		targetCollision.minKillSpeed = sourceCollision.minKillSpeed;
		targetCollision.maxKillSpeed = sourceCollision.maxKillSpeed;

		var sourceTriggers = source.trigger;
		var targetTriggers = target.trigger;

		targetTriggers.enabled = sourceTriggers.enabled;
		targetTriggers.inside = sourceTriggers.inside;
		targetTriggers.outside = sourceTriggers.outside;
		targetTriggers.enter = sourceTriggers.enter;
		targetTriggers.exit = sourceTriggers.exit;

		var sourceSubEmitters = source.subEmitters;
		var targetSubEmitters = target.subEmitters;

		targetSubEmitters.enabled = sourceSubEmitters.enabled;

		var sourceTextureSheetAnimation = source.textureSheetAnimation;
		var targetTextureSheetAnimation = target.textureSheetAnimation;

		targetTextureSheetAnimation.enabled = sourceTextureSheetAnimation.enabled;
		targetTextureSheetAnimation.mode = sourceTextureSheetAnimation.mode;
		targetTextureSheetAnimation.numTilesX = sourceTextureSheetAnimation.numTilesX;
		targetTextureSheetAnimation.numTilesY = sourceTextureSheetAnimation.numTilesY;
		targetTextureSheetAnimation.animation = sourceTextureSheetAnimation.animation;
		targetTextureSheetAnimation.frameOverTime = sourceTextureSheetAnimation.frameOverTime;
		targetTextureSheetAnimation.startFrame = sourceTextureSheetAnimation.startFrame;
		targetTextureSheetAnimation.cycleCount = sourceTextureSheetAnimation.cycleCount;

		var sourceLights = source.lights;
		var targetLights = target.lights;

		targetLights.enabled = sourceLights.enabled;
		targetLights.light = sourceLights.light;
		targetLights.ratio = sourceLights.ratio;
		targetLights.useRandomDistribution = sourceLights.useRandomDistribution;
		targetLights.useParticleColor = sourceLights.useParticleColor;
		targetLights.sizeAffectsRange = sourceLights.sizeAffectsRange;
		targetLights.alphaAffectsIntensity = sourceLights.alphaAffectsIntensity;
		targetLights.range = sourceLights.range;
		targetLights.rangeMultiplier = sourceLights.rangeMultiplier;
		targetLights.intensity = sourceLights.intensity;
		targetLights.intensityMultiplier = sourceLights.intensityMultiplier;
		targetLights.maxLights = sourceLights.maxLights;

		var sourceTrails = source.trails;
		var targetTrails = target.trails;

		targetTrails.enabled = sourceTrails.enabled;
		targetTrails.mode = sourceTrails.mode;
		targetTrails.ratio = sourceTrails.ratio;
		targetTrails.lifetime = sourceTrails.lifetime;
		targetTrails.lifetimeMultiplier = sourceTrails.lifetimeMultiplier;
		targetTrails.minVertexDistance = sourceTrails.minVertexDistance;
		targetTrails.textureMode = sourceTrails.textureMode;
		targetTrails.sizeAffectsWidth = sourceTrails.sizeAffectsWidth;
		targetTrails.sizeAffectsLifetime = sourceTrails.sizeAffectsLifetime;
		targetTrails.inheritParticleColor = sourceTrails.inheritParticleColor;
		targetTrails.colorOverLifetime = sourceTrails.colorOverLifetime;
		targetTrails.widthOverTrail = sourceTrails.widthOverTrail;
		targetTrails.colorOverTrail = sourceTrails.colorOverTrail;

		var sourceCustomData = source.customData;
		var targetCustomData = target.customData;

		targetCustomData.enabled = sourceCustomData.enabled;

		var sourceRenderer = source.GetComponent<ParticleSystemRenderer>();
		var targetRenderer = target.GetComponent<ParticleSystemRenderer>();

		if (sourceRenderer != null && targetRenderer != null)
		{
			targetRenderer.material = sourceRenderer.material;
			targetRenderer.trailMaterial = sourceRenderer.trailMaterial;
			targetRenderer.renderMode = sourceRenderer.renderMode;
			targetRenderer.alignment = sourceRenderer.alignment;
			targetRenderer.sortMode = sourceRenderer.sortMode;
			targetRenderer.sortingOrder = sourceRenderer.sortingOrder;
			targetRenderer.sortingLayerName = sourceRenderer.sortingLayerName;
			targetRenderer.normalDirection = sourceRenderer.normalDirection;
			targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
			targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
			targetRenderer.motionVectorGenerationMode = sourceRenderer.motionVectorGenerationMode;
		}
	}

#if UNITY_EDITOR
	[ContextMenu(nameof(ShowDebugInfo))]
	private void ShowDebugInfo()
	{
		Debug.Log($"[ParticleSystemSwitcher] Debug info for {gameObject.name}:");
		Debug.Log($"- Initialized: {_isInitialized}");
		Debug.Log($"- Current index: {_currentTemplateIndex}");
		Debug.Log($"- Template count: {GetTemplateCount()}");
		Debug.Log($"- ParticleSystem: {(_particleSystem != null ? _particleSystem.name : "null")}");
	}

	[ContextMenu(nameof(ForceInitialize))]
	private void ForceInitialize()
	{
		_isInitialized = false;
		Initialize();
	}
#endif
}