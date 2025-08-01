using System;

[Serializable]
public struct LimbInfo
{
	public bool IsPresent;
	public SoulType SoulType;

	public LimbInfo(bool isPresent, SoulType soulType)
	{
		IsPresent = isPresent;
		SoulType = soulType;
	}
}