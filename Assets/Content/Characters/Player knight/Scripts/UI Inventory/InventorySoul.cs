using UnityEngine;
using System;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Dragger))]
public class InventorySoul : MonoBehaviour, IDraggable
{
	[SerializeField, Required] private InventorySlot[] _allSlots;
	[Space]
	[SerializeField] private SoulType _soulType;

	private RectTransform _rectTransform;
	private RectTransform[] _slotsRectTransform;

	public event Action<SoulType, LimbType> OnSoulPlaced;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_slotsRectTransform = new RectTransform[_allSlots.Length];

		for (int i = 0; i < _allSlots.Length; i++)
		{
			_slotsRectTransform[i] = _allSlots[i].GetComponent<RectTransform>();
		}
	}

	public void OnDragStart()
	{
		// Можно добавить визуальные эффекты при начале перетаскивания
	}

	public void OnDrag(Vector3 position)
	{

	}

	public void OnDragEnd()
	{
		InventorySlot bestSlot = FindBestOverlappingSlot();

		if (bestSlot != null)
		{
			PlaceSoul(bestSlot.GetLimbType());
		}
	}

	private InventorySlot FindBestOverlappingSlot()
	{
		Rect soulRect = GetWorldRect(_rectTransform);
		InventorySlot bestSlot = null;
		float maxOverlapArea = 0f;

		for (int i = 0; i < _allSlots.Length; i++)
		{
			Rect slotWorldRect = GetWorldRect(_slotsRectTransform[i]);

			if (soulRect.Overlaps(slotWorldRect))
			{
				float overlapArea = GetOverlapArea(soulRect, slotWorldRect);

				if (overlapArea > maxOverlapArea)
				{
					maxOverlapArea = overlapArea;
					bestSlot = _allSlots[i];
				}
			}
		}

		return bestSlot;
	}

	private Rect GetWorldRect(RectTransform rectTransform)
	{
		Vector3[] corners = new Vector3[4];
		rectTransform.GetWorldCorners(corners);

		float minX = corners[0].x;
		float minY = corners[0].y;
		float maxX = corners[2].x;
		float maxY = corners[2].y;

		return new Rect(minX, minY, maxX - minX, maxY - minY);
	}

	private float GetOverlapArea(Rect rect1, Rect rect2)
	{
		float left = Mathf.Max(rect1.xMin, rect2.xMin);
		float right = Mathf.Min(rect1.xMax, rect2.xMax);
		float bottom = Mathf.Max(rect1.yMin, rect2.yMin);
		float top = Mathf.Min(rect1.yMax, rect2.yMax);

		if (left < right && bottom < top)
		{
			return (right - left) * (top - bottom);
		}

		return 0f;
	}

	private void PlaceSoul(LimbType limbType)
	{
		OnSoulPlaced?.Invoke(_soulType, limbType);
		Debug.Log($"Soul {_soulType} placed on {limbType}");
	}
}