using UnityEngine;
using UnityEngine.EventSystems;

public class Dragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	private IDraggable _draggedObject;
	private RectTransform _rectTransform;
	private Canvas _canvas;
	private Vector2 _dragOffset;
	private bool _isDragging;

	private void Awake()
	{
		TryGetComponent<IDraggable>(out _draggedObject);

		_rectTransform = GetComponent<RectTransform>();
		_canvas = GetComponentInParent<Canvas>();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (enabled == false)
			return;

		if (_draggedObject != null)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_canvas.transform as RectTransform,
				eventData.position,
				_canvas.worldCamera,
				out localPoint
			);

			_dragOffset = (Vector2)_rectTransform.localPosition - localPoint;
			_isDragging = true;
			_draggedObject.OnDragStart();
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (enabled == false)
			return;

		if (_isDragging && _draggedObject != null)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_canvas.transform as RectTransform,
				eventData.position,
				_canvas.worldCamera,
				out localPoint
			);

			Vector2 newPosition = localPoint + _dragOffset;
			_rectTransform.localPosition = newPosition;

			_draggedObject.OnDrag(newPosition);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (enabled == false)
			return;

		if (_isDragging && _draggedObject != null)
		{
			_draggedObject.OnDragEnd();
		}

		_isDragging = false;
	}
}
