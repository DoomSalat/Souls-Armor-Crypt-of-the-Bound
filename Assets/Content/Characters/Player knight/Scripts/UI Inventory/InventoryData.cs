using UnityEngine;
using System.Collections.Generic;

public class InventoryData : MonoBehaviour
{
	[SerializeField] private InventorySlot[] _inventorySlotsArray;

	private Dictionary<LimbType, List<InventorySlot>> _inventorySlots = new Dictionary<LimbType, List<InventorySlot>>();

	private void Awake()
	{
		foreach (var slot in _inventorySlotsArray)
		{
			LimbType limbType = slot.GetLimbType();

			if (!_inventorySlots.ContainsKey(limbType))
			{
				_inventorySlots[limbType] = new List<InventorySlot>();
			}

			_inventorySlots[limbType].Add(slot);
		}
	}

	public void UpdateSlotsState(Dictionary<LimbType, LimbInfo> limbStates)
	{
		foreach (var state in limbStates)
		{
			if (_inventorySlots.TryGetValue(state.Key, out List<InventorySlot> slots))
			{
				foreach (var slot in slots)
				{
					if (state.Value.IsPresent)
					{
						slot.Activate();
						slot.SetItem(state.Value.SoulType);
					}
					else
					{
						slot.Deactivate();
					}
				}
			}
		}
	}
}