using UnityEngine;

public class FactionTag : MonoBehaviour
{
	[SerializeField] private Faction faction = Faction.Enemy;

	public Faction Faction => faction;

	private void OnValidate()
	{
		if (faction != Faction.Player && faction != Faction.Enemy)
		{
			Debug.LogWarning($"Faction on {gameObject.name} is not explicitly set. Defaulting to Enemy.");
			faction = Faction.Enemy;
		}
	}

	public bool IsTagged(Faction factionToCheck)
	{
		return faction == factionToCheck;
	}
}