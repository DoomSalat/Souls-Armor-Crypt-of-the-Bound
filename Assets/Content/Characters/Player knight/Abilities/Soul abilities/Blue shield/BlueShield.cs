using UnityEngine;

public class BlueShield : MonoBehaviour
{
	[SerializeField] private ShieldAnimator _shieldAnimator;

	public void Activate()
	{
		_shieldAnimator.Activate();
	}

	public void Deactivate()
	{
		_shieldAnimator.Deactivate();
	}

	public void Defend()
	{
		_shieldAnimator.Defend();
	}
}
