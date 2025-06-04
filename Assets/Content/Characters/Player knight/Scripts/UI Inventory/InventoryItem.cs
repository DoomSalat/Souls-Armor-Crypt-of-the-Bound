using UnityEngine;

public class InventoryItem : MonoBehaviour
{
	[SerializeField] private SoulType _soulType;

	public void SetSoulType(SoulType soulType)
	{
		_soulType = soulType;
		//логика смены цвета
	}

	public SoulType GetSoulType()
	{
		return _soulType;
	}
}