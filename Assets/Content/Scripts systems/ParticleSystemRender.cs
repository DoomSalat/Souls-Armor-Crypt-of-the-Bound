using UnityEngine;

[System.Serializable]
public struct ParticleSystemRender
{
	[SerializeField] private ParticleSystem _particleSystem;
	[SerializeField] private bool _applyToTrail;

	public ParticleSystem ParticleSystem => _particleSystem;
	public bool ApplyToTrail => _applyToTrail;

	public ParticleSystemRender(ParticleSystem particleSystem, bool applyToTrail)
	{
		_particleSystem = particleSystem;
		_applyToTrail = applyToTrail;
	}
}