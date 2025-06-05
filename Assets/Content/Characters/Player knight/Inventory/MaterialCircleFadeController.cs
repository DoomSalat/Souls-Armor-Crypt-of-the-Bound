using UnityEngine;

[ExecuteAlways]
public class MaterialCircleFadeController : MonoBehaviour
{
	[SerializeField, Range(-0.1f, 1f)] private float _radius = 0.5f;
	[SerializeField, Range(0f, 1f)] private float _smoothRadius = 0.5f;

	[Header("Serialize")]
	[SerializeField] private Material _material;
	[SerializeField] private string _radiusPropertyName = "_Radius";
	[SerializeField] private string _smoothRadiusPropertyName = "_SmoothRadius";

	private float _lastRadius;
	private float _lastSmoothRadius;

	private void OnValidate()
	{
		UpdateMaterialRadius();
	}

	private void Update()
	{
		if (Mathf.Approximately(_lastRadius, _radius) == false || Mathf.Approximately(_lastSmoothRadius, _smoothRadius) == false)
		{
			UpdateMaterialRadius();
			_lastRadius = _radius;
			_lastSmoothRadius = _smoothRadius;
		}
	}

	private void UpdateMaterialRadius()
	{
		_material.SetFloat(_radiusPropertyName, _radius);
		_material.SetFloat(_smoothRadiusPropertyName, _smoothRadius);
	}

	public void SetRadius(float radius)
	{
		_radius = radius;
		UpdateMaterialRadius();
	}
}
