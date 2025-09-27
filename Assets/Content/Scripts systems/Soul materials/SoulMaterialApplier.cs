using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Coffee.UIExtensions;

public class SoulMaterialApplier : MonoBehaviour
{
	private const string ColorHueProperty = "_ColorHue";

	[Header("Target Components")]
	[SerializeField] private SpriteRenderer[] _spriteRenderers;
	[SerializeField] private SpriteRenderer[] _maskSpriteRenderersMask;
	[SerializeField] private ParticleSystemRender[] _particleSystems;
	[Space]
	[SerializeField] private Image[] _images;
	[SerializeField] private RawImage[] _rawImages;
	[SerializeField] private UIParticleSystemRender[] _UIParticles;

	[Header("Current State")]
	[SerializeField, ReadOnly] private SoulType _currentSoulType = SoulType.None;

	[Header("Reset Behavior")]
	[SerializeField] private bool _resetParticleMaterialsOnReset = false;

	private Dictionary<SpriteRenderer, Material> _originalSpriteMaterials;
	private Dictionary<ParticleSystem, Material> _originalParticleMaterials;
	private Dictionary<UIParticle, List<Material>> _originalUIParticleMaterials;
	private Dictionary<Image, Material> _originalImageMaterials;
	private Dictionary<RawImage, Material> _originalRawImageMaterials;
	private Dictionary<SpriteRenderer, Material> _duplicatedMaskMaterials;

	private Dictionary<ParticleSystem, ParticleSystemRenderer> _particleSystemRenderers;
	private Dictionary<ParticleSystem, ParticleSystemRenderer> _uiParticleSystemRenderers;

	private void Awake()
	{
		CacheOriginalMaterials();
	}

	private void OnDestroy()
	{
		CleanupDuplicatedMaterials();
	}

	public void ApplySoul(SoulType soulType)
	{
		var material = SoulMaterialConfig.InstanceGame.GetMaterial(soulType);
		var particleMaterial = SoulMaterialConfig.InstanceParticle.GetMaterial(soulType);
		var canvasMaterial = SoulMaterialConfig.InstanceCanvas.GetMaterial(soulType);
		var uiParticleMaterial = SoulMaterialConfig.InstanceUIParticle.GetMaterial(soulType);

		ApplyToSprites(material);
		ApplyToMasks(soulType);
		ApplyToParticles(particleMaterial);
		ApplyToImages(canvasMaterial);
		ApplyToRawImages(canvasMaterial);
		ApplyToUIParticles(uiParticleMaterial);

		_currentSoulType = soulType;
	}

	public void ResetToOriginalMaterials()
	{
		if (_resetParticleMaterialsOnReset)
		{
			ResetParticleMaterials();
			ResetUIParticleMaterials();
		}

		ResetSpriteMaterials();
		ResetImageMaterials();
		ResetRawImageMaterials();
		ResetMasks();

		_currentSoulType = SoulType.None;
	}

	public SoulType GetCurrentSoulType()
	{
		return _currentSoulType;
	}

	public SpriteRenderer[] GetMaskSpriteRenderers()
	{
		return _maskSpriteRenderersMask;
	}

	public bool HasMaskComponents()
	{
		return _maskSpriteRenderersMask != null && _maskSpriteRenderersMask.Length > 0;
	}

	public void ApplySoulByIndex(int materialIndex)
	{
		var soulType = GetSoulTypeByIndex(materialIndex);
		if (soulType != SoulType.None)
		{
			ApplySoul(soulType);
		}
	}

	public SoulType GetSoulTypeByIndex(int index)
	{
		var availableSoulTypes = SoulMaterialConfig.InstanceGame.GetAvailableSoulTypes();
		var soulTypesList = new System.Collections.Generic.List<SoulType>(availableSoulTypes);

		if (index >= 0 && index < soulTypesList.Count)
		{
			return soulTypesList[index];
		}

		return SoulType.None;
	}

	private void CacheOriginalMaterials()
	{
		_originalParticleMaterials = new Dictionary<ParticleSystem, Material>();
		_originalUIParticleMaterials = new Dictionary<UIParticle, List<Material>>();
		_originalSpriteMaterials = new Dictionary<SpriteRenderer, Material>();
		_originalImageMaterials = new Dictionary<Image, Material>();
		_originalRawImageMaterials = new Dictionary<RawImage, Material>();
		_duplicatedMaskMaterials = new Dictionary<SpriteRenderer, Material>();
		_particleSystemRenderers = new Dictionary<ParticleSystem, ParticleSystemRenderer>();
		_uiParticleSystemRenderers = new Dictionary<ParticleSystem, ParticleSystemRenderer>();

		if (_particleSystems != null)
		{
			foreach (var particleSystemData in _particleSystems)
			{
				var particleSystem = particleSystemData.ParticleSystem;
				if (particleSystem == null)
					continue;

				var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
				if (renderer != null)
				{
					_particleSystemRenderers[particleSystem] = renderer;
					_originalParticleMaterials[particleSystem] = particleSystemData.ApplyToTrail ? renderer.trailMaterial : renderer.material;
				}
			}
		}

		if (_UIParticles != null)
		{
			foreach (var uiParticleData in _UIParticles)
			{
				var uiParticle = uiParticleData.UIParticle;
				if (uiParticle == null)
					continue;

				var particleSystems = uiParticle.particles;
				if (particleSystems != null)
				{
					foreach (var particleSystem in particleSystems)
					{
						if (particleSystem == null)
							continue;

						var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
						if (renderer != null)
						{
							_uiParticleSystemRenderers[particleSystem] = renderer;
						}
					}
				}

				var materials = new List<Material>();
				uiParticle.GetMaterials(materials);
				_originalUIParticleMaterials[uiParticle] = new List<Material>(materials);
			}
		}

		if (_spriteRenderers != null)
		{
			foreach (var spriteRenderer in _spriteRenderers)
			{
				if (spriteRenderer == null)
					continue;

				_originalSpriteMaterials[spriteRenderer] = spriteRenderer.material;
			}
		}

		if (_images != null)
		{
			foreach (var image in _images)
			{
				if (image == null)
					continue;

				_originalImageMaterials[image] = image.material;
			}
		}

		if (_rawImages != null)
		{
			foreach (var rawImage in _rawImages)
			{
				if (rawImage == null)
					continue;

				_originalRawImageMaterials[rawImage] = rawImage.material;
			}
		}

		if (_maskSpriteRenderersMask != null)
		{
			foreach (var spriteRenderer in _maskSpriteRenderersMask)
			{
				if (spriteRenderer == null || spriteRenderer.sharedMaterial == null)
					continue;

				var duplicatedMaterial = new Material(spriteRenderer.sharedMaterial);
				_duplicatedMaskMaterials[spriteRenderer] = duplicatedMaterial;
			}
		}
	}

	private void ApplyToParticles(Material material)
	{
		if (_particleSystems == null)
			return;

		foreach (var particleSystemData in _particleSystems)
		{
			ApplyToParticleSystem(particleSystemData.ParticleSystem, material, particleSystemData.ApplyToTrail);
		}
	}

	private void ApplyToParticleSystem(ParticleSystem particleSystem, Material material, bool applyToTrail)
	{
		if (particleSystem == null || material == null)
			return;

		if (_particleSystemRenderers.TryGetValue(particleSystem, out var renderer))
		{
			if (applyToTrail)
				renderer.trailMaterial = material;
			else
				renderer.material = material;
		}
	}

	private void ApplyToUIParticles(Material material)
	{
		if (_UIParticles == null)
			return;

		foreach (var uiParticleData in _UIParticles)
		{
			ApplyToUIParticleSystem(uiParticleData.UIParticle, material, uiParticleData.ApplyToTrail);
		}
	}

	private void ApplyToUIParticleSystem(UIParticle uiParticle, Material material, bool applyToTrail)
	{
		if (uiParticle == null || material == null)
			return;

		bool wasPlaying = StopUIParticleForMaterialChange(uiParticle);
		ApplyMaterialToUIParticleCorrectly(uiParticle, material, applyToTrail);
		uiParticle.RefreshParticles();

		if (wasPlaying)
		{
			StartUIParticleAfterMaterialChange(uiParticle);
		}
	}

	private bool StopUIParticleForMaterialChange(UIParticle uiParticle)
	{
		bool wasPlaying = false;
		var particleSystems = uiParticle.particles;

		if (particleSystems != null)
		{
			foreach (var particleSystem in particleSystems)
			{
				if (particleSystem != null && particleSystem.isPlaying)
				{
					wasPlaying = true;
					break;
				}
			}
		}

		if (wasPlaying)
		{
			uiParticle.Stop();
			uiParticle.Clear();
		}

		return wasPlaying;
	}

	private void StartUIParticleAfterMaterialChange(UIParticle uiParticle)
	{
		uiParticle.RefreshParticles();
		uiParticle.Play();
	}

	private bool ApplyMaterialToUIParticleCorrectly(UIParticle uiParticle, Material material, bool applyToTrail)
	{
		var particleSystems = uiParticle.particles;
		if (particleSystems == null || particleSystems.Count == 0)
			return false;

		bool materialApplied = false;

		foreach (var particleSystem in particleSystems)
		{
			if (particleSystem == null)
				continue;

			if (!_uiParticleSystemRenderers.TryGetValue(particleSystem, out var renderer))
				continue;

			if (applyToTrail && particleSystem.trails.enabled)
			{
				renderer.trailMaterial = material;
				materialApplied = true;
			}
			else if (!applyToTrail)
			{
				renderer.material = material;
				materialApplied = true;
			}
		}

		if (materialApplied)
		{
			var savedParticleSystems = new List<ParticleSystem>(particleSystems);
			uiParticle.RefreshParticles(savedParticleSystems);
		}

		return materialApplied;
	}

	private void ApplyToSprites(Material material)
	{
		if (_spriteRenderers == null || material == null)
			return;

		foreach (var spriteRenderer in _spriteRenderers)
		{
			if (spriteRenderer != null)
			{
				spriteRenderer.material = material;
			}
		}
	}

	private void ApplyToImages(Material material)
	{
		if (_images == null || material == null)
			return;

		foreach (var image in _images)
		{
			if (image != null)
			{
				image.material = material;
			}
		}
	}

	private void ApplyToRawImages(Material material)
	{
		if (_rawImages == null || material == null)
			return;

		foreach (var rawImage in _rawImages)
		{
			if (rawImage != null)
			{
				rawImage.material = material;
			}
		}
	}

	private void ApplyToMasks(SoulType soulType)
	{
		if (_maskSpriteRenderersMask == null)
			return;

		float hueValue = GetColorHueForSoulType(soulType);

		foreach (var spriteRenderer in _maskSpriteRenderersMask)
		{
			if (spriteRenderer == null)
				continue;

			if (_duplicatedMaskMaterials.TryGetValue(spriteRenderer, out var duplicatedMaterial))
			{
				duplicatedMaterial.SetFloat(ColorHueProperty, hueValue);
				spriteRenderer.material = duplicatedMaterial;
			}
		}
	}

	private void ResetParticleMaterials()
	{
		if (_originalParticleMaterials == null || _particleSystems == null)
			return;

		foreach (var particleSystemData in _particleSystems)
		{
			var particleSystem = particleSystemData.ParticleSystem;
			if (particleSystem == null || !_originalParticleMaterials.ContainsKey(particleSystem))
				continue;

			var originalMaterial = _originalParticleMaterials[particleSystem];
			if (originalMaterial == null)
				continue;

			if (_particleSystemRenderers.TryGetValue(particleSystem, out var renderer))
			{
				if (particleSystemData.ApplyToTrail)
					renderer.trailMaterial = originalMaterial;
				else
					renderer.material = originalMaterial;
			}
		}
	}

	private void ResetUIParticleMaterials()
	{
		if (_originalUIParticleMaterials == null || _UIParticles == null)
			return;

		foreach (var uiParticleData in _UIParticles)
		{
			var uiParticle = uiParticleData.UIParticle;
			if (uiParticle == null || !_originalUIParticleMaterials.ContainsKey(uiParticle))
				continue;

			var originalMaterials = _originalUIParticleMaterials[uiParticle];
			if (originalMaterials == null || originalMaterials.Count == 0)
				continue;

			bool wasPlaying = StopUIParticleForMaterialChange(uiParticle);

			var particleSystems = uiParticle.particles;
			if (particleSystems != null && particleSystems.Count > 0)
			{
				int materialIndex = 0;

				foreach (var particleSystem in particleSystems)
				{
					if (particleSystem == null)
						continue;

					if (!_uiParticleSystemRenderers.TryGetValue(particleSystem, out var renderer))
						continue;

					if (uiParticleData.ApplyToTrail && particleSystem.trails.enabled && materialIndex + 1 < originalMaterials.Count)
					{
						renderer.trailMaterial = originalMaterials[materialIndex + 1];
					}
					else if (!uiParticleData.ApplyToTrail && materialIndex < originalMaterials.Count)
					{
						renderer.material = originalMaterials[materialIndex];
					}

					materialIndex += particleSystem.trails.enabled ? 2 : 1;
				}

				var savedParticleSystems = new List<ParticleSystem>(particleSystems);
				uiParticle.RefreshParticles(savedParticleSystems);
			}

			uiParticle.RefreshParticles();

			if (wasPlaying)
			{
				StartUIParticleAfterMaterialChange(uiParticle);
			}
		}
	}

	private void ResetSpriteMaterials()
	{
		if (_originalSpriteMaterials == null)
			return;

		foreach (var spriteMaterialPair in _originalSpriteMaterials)
		{
			if (spriteMaterialPair.Key != null && spriteMaterialPair.Value != null)
			{
				spriteMaterialPair.Key.material = spriteMaterialPair.Value;
			}
		}
	}

	private void ResetImageMaterials()
	{
		if (_originalImageMaterials == null)
			return;

		foreach (var imageMaterialPair in _originalImageMaterials)
		{
			if (imageMaterialPair.Key != null && imageMaterialPair.Value != null)
			{
				imageMaterialPair.Key.material = imageMaterialPair.Value;
			}
		}
	}

	private void ResetRawImageMaterials()
	{
		if (_originalRawImageMaterials == null)
			return;

		foreach (var rawImageMaterialPair in _originalRawImageMaterials)
		{
			if (rawImageMaterialPair.Key != null && rawImageMaterialPair.Value != null)
			{
				rawImageMaterialPair.Key.material = rawImageMaterialPair.Value;
			}
		}
	}

	private void ResetMasks()
	{
		if (_maskSpriteRenderersMask == null)
			return;

		foreach (var spriteRenderer in _maskSpriteRenderersMask)
		{
			if (spriteRenderer == null)
				continue;

			if (_duplicatedMaskMaterials.TryGetValue(spriteRenderer, out var duplicatedMaterial))
			{
				duplicatedMaterial.SetFloat(ColorHueProperty, 0f);
				spriteRenderer.material = duplicatedMaterial;
			}
		}
	}

	private float GetColorHueForSoulType(SoulType soulType)
	{
		var material = SoulMaterialConfig.InstanceGame.GetMaterial(soulType);
		if (material != null && material.HasProperty(ColorHueProperty))
		{
			return material.GetFloat(ColorHueProperty);
		}

		return 0f;
	}

	private void CleanupDuplicatedMaterials()
	{
		if (_duplicatedMaskMaterials == null)
			return;

		foreach (var duplicatedMaterial in _duplicatedMaskMaterials.Values)
		{
			if (duplicatedMaterial != null)
			{
				DestroyImmediate(duplicatedMaterial);
			}
		}

		_duplicatedMaskMaterials.Clear();
	}

#if UNITY_EDITOR
	[Button("Testing Buttons (Editor Only)")]
	private void TestApplySoul(int index)
	{
		if (_originalSpriteMaterials == null)
			CacheOriginalMaterials();

		ApplySoulByIndex(index);
	}
#endif
}