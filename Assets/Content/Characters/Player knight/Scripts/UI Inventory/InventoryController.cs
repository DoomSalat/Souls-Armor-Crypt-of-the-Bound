using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class InventoryController : MonoBehaviour
{
	[SerializeField, Required] private InventoryData _inventoryData;

	public void Start()
	{
		Deactivate();
	}

	public void Activate(Dictionary<LimbType, bool> limbStates)
	{
		gameObject.SetActive(true);
		_inventoryData.UpdateSlotsState(limbStates);
	}

	public void Deactivate()
	{
		gameObject.SetActive(false);
	}

	public bool IsActive()
	{
		return gameObject.activeSelf;
	}
}