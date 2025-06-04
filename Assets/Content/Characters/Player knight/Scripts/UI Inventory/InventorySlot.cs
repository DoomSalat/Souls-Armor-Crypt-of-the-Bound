using UnityEngine;

public class InventorySlot : MonoBehaviour
{
	[SerializeField] private LimbType _limbType;
	[SerializeField] private InventoryItem _currentItem;

	public bool IsFilled { get; private set; } = true;

	public LimbType GetLimbType()
	{
		return _limbType;
	}

	public void SetItem(SoulType soulType)
	{
		_currentItem.SetSoulType(soulType);
	}

	public void Activate()
	{
		IsFilled = true;
		_currentItem.gameObject.SetActive(true);
	}

	public void Deactivate()
	{
		IsFilled = false;
		_currentItem.gameObject.SetActive(false);
	}

	public SoulType GetItemType()
	{
		return _currentItem.GetSoulType();
	}
}