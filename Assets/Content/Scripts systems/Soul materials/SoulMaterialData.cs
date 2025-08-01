using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "SoulMaterialData", menuName = "Souls/Soul Material Data", order = 2)]
public class SoulMaterialData : ScriptableObject
{
	[Header("Soul Material Configuration")]
	[SerializeField, Required] private SoulType _soulType;
	[SerializeField, Required] private Material _material;

	public SoulType SoulType => _soulType;
	public Material Material => _material;

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (_soulType == SoulType.None)
		{
			Debug.LogWarning($"[{name}] SoulType should not be None for a valid material configuration!");
		}

		if (_material == null)
		{
			Debug.LogWarning($"[{name}] Material not assigned for soul type {_soulType}!");
		}
	}
#endif
}