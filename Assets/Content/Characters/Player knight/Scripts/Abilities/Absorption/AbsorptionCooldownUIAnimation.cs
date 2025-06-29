using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AbsorptionCooldownUIAnimation : MonoBehaviour
{
	private Color _whiteClearColor = new Color(1f, 1f, 1f, 0f);

	private Image _image;
	private Color _originalColor;

	private void Awake()
	{
		_image = GetComponent<Image>();
		_originalColor = _image.color;
	}

	public void SetProgress(float progress)
	{
		Color startColor = _whiteClearColor;
		_image.color = Color.Lerp(startColor, _originalColor, progress);
	}

	public void ResetToDefault()
	{
		_image.color = _originalColor;
	}
}