using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class SwordAttackZoneScaler : MonoBehaviour
{
	[SerializeField, Required] private Vector2[] _attackZoneScales;

	private Transform _transform;

	private void Awake()
	{
		_transform = GetComponent<Transform>();
	}

	public void SetAttackZoneScale(int scaleIndex)
	{
		Vector2 targetScale = _attackZoneScales[scaleIndex];
		_transform.localScale = new Vector3(targetScale.x, targetScale.y, _transform.localScale.z);
	}

	public int GetScaleCount() => _attackZoneScales.Length;
}