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

	private Dictionary<SpriteRenderer, Material> _originalSpriteMaterials;
	private Dictionary<ParticleSystem, Material> _originalParticleMaterials;
	private Dictionary<UIParticle, List<Material>> _originalUIParticleMaterials;
	private Dictionary<Image, Material> _originalImageMaterials;
	private Dictionary<RawImage, Material> _originalRawImageMaterials;

	private void Awake()
	{
		CacheOriginalMaterials();
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
		ResetParticleMaterials();
		ResetSpriteMaterials();
		ResetImageMaterials();
		ResetRawImageMaterials();
		ResetUIParticleMaterials();
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

		if (_particleSystems != null)
		{
			foreach (var psData in _particleSystems)
			{
				var ps = psData.ParticleSystem;
				if (ps == null)
					continue;

				var renderer = ps.GetComponent<ParticleSystemRenderer>();
				if (renderer != null)
				{
					_originalParticleMaterials[ps] = psData.ApplyToTrail ? renderer.trailMaterial : renderer.material;
				}
			}
		}

		if (_UIParticles != null)
		{
			foreach (var psData in _UIParticles)
			{
				var uiParticle = psData.UIParticle;
				if (uiParticle == null)
					continue;

				var materials = new List<Material>();
				uiParticle.GetMaterials(materials);
				_originalUIParticleMaterials[uiParticle] = new List<Material>(materials);
			}
		}

		if (_spriteRenderers != null)
		{
			foreach (var sr in _spriteRenderers)
			{
				if (sr == null)
					continue;

				_originalSpriteMaterials[sr] = sr.material;
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
	}

	private void ApplyToParticles(Material material)
	{
		if (_particleSystems == null)
			return;

		foreach (var psData in _particleSystems)
		{
			ApplyToParticleSystem(psData.ParticleSystem, material, psData.ApplyToTrail);
		}
	}

	private void ApplyToParticleSystem(ParticleSystem ps, Material material, bool applyToTrail)
	{
		if (ps == null || material == null)
			return;

		var renderer = ps.GetComponent<ParticleSystemRenderer>();
		if (renderer != null)
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

		foreach (var psData in _UIParticles)
		{
			ApplyToUIParticleSystem(psData.UIParticle, material, psData.ApplyToTrail);
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
			foreach (var ps in particleSystems)
			{
				if (ps != null && ps.isPlaying)
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

		foreach (var ps in particleSystems)
		{
			if (ps == null)
				continue;

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			if (renderer == null)
				continue;

			if (applyToTrail && ps.trails.enabled)
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

		foreach (var sr in _spriteRenderers)
		{
			if (sr != null)
			{
				sr.material = material;
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

		foreach (var sr in _maskSpriteRenderersMask)
		{
			if (sr != null && sr.sharedMaterial != null)
			{
				sr.sharedMaterial.SetFloat(ColorHueProperty, hueValue);
			}
		}
	}

	private void ResetParticleMaterials()
	{
		if (_originalParticleMaterials == null || _particleSystems == null)
			return;

		foreach (var psData in _particleSystems)
		{
			var ps = psData.ParticleSystem;
			if (ps == null || !_originalParticleMaterials.ContainsKey(ps))
				continue;

			var renderer = ps.GetComponent<ParticleSystemRenderer>();
			var originalMaterial = _originalParticleMaterials[ps];

			if (renderer != null && originalMaterial != null)
			{
				if (psData.ApplyToTrail)
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

		foreach (var psData in _UIParticles)
		{
			var uiParticle = psData.UIParticle;
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

				foreach (var ps in particleSystems)
				{
					if (ps == null)
						continue;

					var renderer = ps.GetComponent<ParticleSystemRenderer>();
					if (renderer == null)
						continue;

					if (psData.ApplyToTrail && ps.trails.enabled && materialIndex + 1 < originalMaterials.Count)
					{
						renderer.trailMaterial = originalMaterials[materialIndex + 1];
					}
					else if (!psData.ApplyToTrail && materialIndex < originalMaterials.Count)
					{
						renderer.material = originalMaterials[materialIndex];
					}

					materialIndex += ps.trails.enabled ? 2 : 1;
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

		foreach (var kvp in _originalSpriteMaterials)
		{
			if (kvp.Key != null && kvp.Value != null)
			{
				kvp.Key.material = kvp.Value;
			}
		}
	}

	private void ResetImageMaterials()
	{
		if (_originalImageMaterials == null)
			return;

		foreach (var kvp in _originalImageMaterials)
		{
			if (kvp.Key != null && kvp.Value != null)
			{
				kvp.Key.material = kvp.Value;
			}
		}
	}

	private void ResetRawImageMaterials()
	{
		if (_originalRawImageMaterials == null)
			return;

		foreach (var kvp in _originalRawImageMaterials)
		{
			if (kvp.Key != null && kvp.Value != null)
			{
				kvp.Key.material = kvp.Value;
			}
		}
	}

	private void ResetMasks()
	{
		if (_maskSpriteRenderersMask == null)
			return;

		foreach (var sr in _maskSpriteRenderersMask)
		{
			if (sr != null && sr.sharedMaterial != null)
			{
				sr.sharedMaterial.SetFloat(ColorHueProperty, 0f);
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

#if UNITY_EDITOR
	[Button("Testing Buttons (Editor Only)")]
	private void TestApplySoul(int index)
	{
		ApplySoulByIndex(index);
	}
#endif
}