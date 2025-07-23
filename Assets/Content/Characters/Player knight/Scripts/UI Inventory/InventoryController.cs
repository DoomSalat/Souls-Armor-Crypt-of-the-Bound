using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UI.Inventory;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryController : MonoBehaviour
{
	[SerializeField, Required] private InventoryData _inventoryData;
	[SerializeField, Required] private InventoryAnimator _inventoryAnimator;
	[SerializeField, Required] private InventorySoul _inventorySoul;
	[SerializeField, Required] private LimbVisualization _limbVisualization;

	private CanvasGroup _canvasGroup;

	private RectTransform _soulRectTransform;

	public InventorySoul InventorySoul => _inventorySoul;

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
		_soulRectTransform = _inventorySoul.GetComponent<RectTransform>();
	}

	public void Start()
	{
		Disable();
	}

	private void OnEnable()
	{
		_inventoryAnimator.DeactivateAnimationEnded += Disable;
	}

	private void OnDisable()
	{
		_inventoryAnimator.DeactivateAnimationEnded -= Disable;
	}

	public void Activate(Dictionary<LimbType, LimbInfo> limbStates, SoulType soulType)
	{
		gameObject.SetActive(true);
		_canvasGroup.interactable = true;

		ResetSoulPosition();

		_inventoryData.UpdateSlotsState(limbStates);
		_inventorySoul.ApplySoul(soulType);

		_limbVisualization.UpdateLimbVisualization(limbStates);

		_inventoryAnimator.Activate();
	}

	public void Deactivate()
	{
		_inventoryAnimator.Deactivate();
	}

	public bool IsActive()
	{
		return gameObject.activeSelf;
	}

	private void Disable()
	{
		gameObject.SetActive(false);
		_canvasGroup.interactable = false;
	}

	private void ResetSoulPosition()
	{
		_soulRectTransform.anchoredPosition = Vector2.zero;
	}
}