using UnityEngine;
using Sirenix.OdinInspector;

public class BlueArmAbility : MonoBehaviour, IAbilityArm
{
	[Header("Blue Arm Settings")]
	[SerializeField, MinValue(0)] private float _swordSpeed = 2f;

	public float SwordSpeed => _swordSpeed;
	public bool HasVisualEffects => false;

	public void Initialize() { }
	public void Activate() { }
	public void Deactivate() { }
	public void InitializeVisualEffects(Transform effectsParent) { }

}