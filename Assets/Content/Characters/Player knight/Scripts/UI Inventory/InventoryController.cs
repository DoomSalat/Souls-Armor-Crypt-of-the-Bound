using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryController : MonoBehaviour
{
	[SerializeField, Required] private InventoryData _inventoryData;
	[SerializeField, Required] private InventoryAnimator _inventoryAnimator;
	[SerializeField, Required] private InventorySoul _inventorySoul;

	private CanvasGroup _canvasGroup;

	public InventorySoul InventorySoul => _inventorySoul;

	private void Awake()
	{
		_canvasGroup = GetComponent<CanvasGroup>();
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

	public void Activate(Dictionary<LimbType, bool> limbStates)
	{
		gameObject.SetActive(true);
		_canvasGroup.interactable = true;
		_inventoryData.UpdateSlotsState(limbStates);
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
}