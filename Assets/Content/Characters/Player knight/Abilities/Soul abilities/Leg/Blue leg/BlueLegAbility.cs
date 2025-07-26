using UnityEngine;
using Sirenix.OdinInspector;

public class BlueLegAbility : MonoBehaviour, IAbilityLeg
{
	[Header("Blue Leg Settings")]
	[SerializeField, MinValue(0)] private float _speed = 5f;

	public float Speed => _speed;
	public bool HasVisualEffects => false;

	public void Initialize() { }
	public void Activate() { }
	public void Deactivate() { }
	public void InitializeVisualEffects(Transform effectsParent) { }

}