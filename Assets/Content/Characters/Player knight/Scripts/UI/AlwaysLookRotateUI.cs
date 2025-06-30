using UnityEngine;

[ExecuteAlways]
public class AlwaysLookRotateUI : MonoBehaviour
{
	[SerializeField] private bool _updateAlways = false;

	private RectTransform _rectTransform;
	private Quaternion _initialRotation;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_initialRotation = _rectTransform.rotation;
	}

	private void LateUpdate()
	{
		if (Application.isPlaying == false || _updateAlways)
		{
			UpdateRotation();
		}
	}

	public void UpdateRotation()
	{
		if (_rectTransform.rotation == _initialRotation)
			return;

		_rectTransform.rotation = _initialRotation;
	}
}
