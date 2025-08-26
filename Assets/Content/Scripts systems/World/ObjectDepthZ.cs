using UnityEngine;

[ExecuteInEditMode]
public class ObjectDepthZ : MonoBehaviour
{
	[Header("Depth Settings")]
	[SerializeField] private float _depthStepZ = 0.01f;
	[SerializeField] private float _sensitivityY = 0.01f;
	[SerializeField] private bool _useLocalPosition = false;

	private void LateUpdate()
	{
		UpdateDepthPosition();
	}

	private void OnValidate()
	{
		UpdateDepthPosition();
	}

	private void UpdateDepthPosition()
	{
		Vector3 currentPosition = _useLocalPosition ? transform.localPosition : transform.position;
		float currentY = currentPosition.y;

		float targetZ = CalculateDepthZ(currentY);

		Vector3 newPosition = currentPosition;
		newPosition.z = targetZ;

		if (_useLocalPosition)
		{
			transform.localPosition = newPosition;
		}
		else
		{
			transform.position = newPosition;
		}
	}

	private float CalculateDepthZ(float yPosition)
	{
		float steps = yPosition / _sensitivityY;
		return Mathf.Floor(steps) * _depthStepZ;
	}
}
