using UnityEngine;
using System.Collections.Generic;

public class InventoryData : MonoBehaviour
{
	[SerializeField] private InventorySlot[] _inventorySlotsArray;

	private Dictionary<LimbType, InventorySlot> _inventorySlots = new Dictionary<LimbType, InventorySlot>();

	private void Awake()
	{
		foreach (var slot in _inventorySlotsArray)
		{
			if (slot != null)
			{
				_inventorySlots[slot.GetLimbType()] = slot;
			}
		}
	}

	public void UpdateSlotsState(Dictionary<LimbType, bool> limbStates)
	{
		foreach (var state in limbStates)
		{
			if (_inventorySlots.TryGetValue(state.Key, out InventorySlot slot))
			{
				if (state.Value)
				{
					slot.Activate();
				}
				else
				{
					slot.Deactivate();
				}
			}
		}
	}
}