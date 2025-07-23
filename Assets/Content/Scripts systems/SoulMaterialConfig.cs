using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "SoulMaterialConfig", menuName = "Configs/Soul Material Config", order = 1)]
public class SoulMaterialConfig : ScriptableObject
{
	private const string GameConfigName = "SoulMaterialConfig";
	private const string ParticleConfigName = "SoulMaterialConfigParticle";
	private const string CanvasConfigName = "SoulMaterialConfigCanvas";
	private const string UIParticleConfigName = "SoulMaterialConfigUIParticle";

	private static SoulMaterialConfig _instanceGame;
	private static SoulMaterialConfig _instanceParticle;
	private static SoulMaterialConfig _instanceCanvas;
	private static SoulMaterialConfig _instanceUIParticle;

	[Header("Soul Materials Configuration")]
	[SerializeField, Required] private SoulMaterialData[] _soulMaterials = new SoulMaterialData[5];

	[Header("Default Materials")]
	[SerializeField] private Material _defaultMaterial;
	[SerializeField] private Material _noneMaterial;

	private Dictionary<SoulType, Material> _materialLookup;

	public static SoulMaterialConfig InstanceGame
	{
		get
		{
			_instanceGame = Resources.Load<SoulMaterialConfig>(GameConfigName);

			if (_instanceGame == null)
			{
				Debug.LogError($"No Game {nameof(SoulMaterialConfig)} found in Resources folder! Please create one and place it in Resources: '{GameConfigName}'.");
			}

			return _instanceGame;
		}
	}

	public static SoulMaterialConfig InstanceParticle
	{
		get
		{
			_instanceParticle = Resources.Load<SoulMaterialConfig>(ParticleConfigName);

			if (_instanceParticle == null)
			{
				Debug.LogError($"No Particle {nameof(SoulMaterialConfig)} found in Resources folder! Please create one and place it in Resources: '{ParticleConfigName}'.");
			}

			return _instanceParticle;
		}
	}

	public static SoulMaterialConfig InstanceCanvas
	{
		get
		{
			_instanceCanvas = Resources.Load<SoulMaterialConfig>(CanvasConfigName);

			if (_instanceCanvas == null)
			{
				Debug.LogError($"No Canvas {nameof(SoulMaterialConfig)} found in Resources folder! Please create one and place it in Resources: '{CanvasConfigName}'.");
			}

			return _instanceCanvas;
		}
	}

	public static SoulMaterialConfig InstanceUIParticle
	{
		get
		{
			_instanceUIParticle = Resources.Load<SoulMaterialConfig>(UIParticleConfigName);

			if (_instanceUIParticle == null)
			{
				Debug.LogError($"No UIParticle {nameof(SoulMaterialConfig)} found in Resources folder! Please create one and place it in Resources: '{UIParticleConfigName}'.");
			}

			return _instanceUIParticle;
		}
	}

	public Material GetMaterial(SoulType soulType)
	{
		InitializeLookup();

		if (soulType == SoulType.None)
		{
			return _noneMaterial != null ? _noneMaterial : _defaultMaterial;
		}

		if (_materialLookup.TryGetValue(soulType, out Material material))
		{
			return material;
		}

		Debug.LogWarning($"[{name}] Material for soul type {soulType} not found! Using default material.");
		return _defaultMaterial;
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		var soulTypes = new HashSet<SoulType>();

		if (_soulMaterials != null)
		{
			foreach (var soulMaterial in _soulMaterials)
			{
				if (soulMaterial == null)
					continue;

				if (soulTypes.Add(soulMaterial.SoulType) == false)
				{
					Debug.LogWarning($"[{name}] Duplicate soul type {soulMaterial.SoulType} found!");
				}
			}
		}

		_materialLookup = null;
	}
#endif

	public bool HasMaterial(SoulType soulType)
	{
		InitializeLookup();
		return _materialLookup.ContainsKey(soulType);
	}

	public IEnumerable<SoulType> GetAvailableSoulTypes()
	{
		InitializeLookup();
		return _materialLookup.Keys;
	}

	private void InitializeLookup()
	{
		if (_materialLookup != null)
			return;

		_materialLookup = new Dictionary<SoulType, Material>();

		if (_soulMaterials == null)
			return;

		foreach (var soulMaterial in _soulMaterials)
		{
			if (soulMaterial == null)
				continue;

			if (_materialLookup.ContainsKey(soulMaterial.SoulType))
			{
				Debug.LogWarning($"[{name}] Duplicate soul type {soulMaterial.SoulType} in configuration!");
				continue;
			}

			_materialLookup[soulMaterial.SoulType] = soulMaterial.Material;
		}
	}
}