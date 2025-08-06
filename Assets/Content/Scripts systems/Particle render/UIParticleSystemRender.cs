using UnityEngine;
using Coffee.UIExtensions;

[System.Serializable]
public struct UIParticleSystemRender
{
	[SerializeField] private UIParticle _uiParticle;
	[SerializeField] private bool _applyToTrail;

	public UIParticle UIParticle => _uiParticle;
	public bool ApplyToTrail => _applyToTrail;

	public UIParticleSystemRender(UIParticle uiParticle, bool applyToTrail)
	{
		_uiParticle = uiParticle;
		_applyToTrail = applyToTrail;
	}
}