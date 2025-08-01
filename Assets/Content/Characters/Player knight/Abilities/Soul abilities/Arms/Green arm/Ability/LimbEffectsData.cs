using UnityEngine;

public class LimbEffectsData : MonoBehaviour
{
	[SerializeField] private Transform _swordTarget;

	public Transform SwordTarget => _swordTarget;
}
