using UnityEngine;

[System.Serializable]
public struct LimbVisualData
{
	[SerializeField] private LimbType _limbType;
	[SerializeField] private Material _material;
	[SerializeField] private ParticleSystem[] _particles;

	public LimbType LimbType => _limbType;
	public Material Material => _material;
	public ParticleSystem[] Particles => _particles;
}