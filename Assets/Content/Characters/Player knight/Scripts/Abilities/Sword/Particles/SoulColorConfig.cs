using UnityEngine;

[System.Serializable]
public class SoulColorConfig
{
	[Header("Soul Colors Configuration")]
	[SerializeField] private Color _noneColor = Color.white;
	[SerializeField] private Color _blueColor = Color.blue;
	[SerializeField] private Color _redColor = Color.red;
	[SerializeField] private Color _greenColor = Color.green;
	[SerializeField] private Color _purpleColor = Color.magenta;

	public Color GetColor(SoulType soulType)
	{
		return soulType switch
		{
			SoulType.None => _noneColor,
			SoulType.Blue => _blueColor,
			SoulType.Red => _redColor,
			SoulType.Green => _greenColor,
			SoulType.Purple => _purpleColor,
			_ => _noneColor
		};
	}
}