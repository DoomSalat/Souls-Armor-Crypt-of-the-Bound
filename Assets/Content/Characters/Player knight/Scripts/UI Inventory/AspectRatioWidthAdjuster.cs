using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class AspectRatioWidthAdjuster : MonoBehaviour
{
	[SerializeField, Required] private Image image;
	[SerializeField, Required] private RectTransform rectTransform;

	private void Awake()
	{
		image = GetComponent<Image>();
		rectTransform = GetComponent<RectTransform>();
	}

	private void Start()
	{
		AdjustWidthToAspect();
	}

	[ContextMenu(nameof(AdjustWidthToAspect))]
	public void AdjustWidthToAspect()
	{
		if (image.sprite == null)
		{
			Debug.LogWarning("Sprite not assigned for Image component.");
			return;
		}

		float aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
		float currentHeight = rectTransform.rect.height;
		float desiredWidth = currentHeight * aspectRatio;
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, desiredWidth);
	}

	private void OnRectTransformDimensionsChange()
	{
		if (image != null && rectTransform != null)
		{
			AdjustWidthToAspect();
		}
	}
}