using UnityEngine;

public class CreatureFlip : MonoBehaviour
{
	private const float ScaleMultiplier = -1f;

	public void FlipLeft()
	{
		Vector3 scale = transform.localScale;

		if (scale.x <= 0)
		{
			return;
		}

		scale.x *= ScaleMultiplier;
		transform.localScale = scale;
	}

	public void FlipRight()
	{
		Vector3 scale = transform.localScale;

		if (scale.x >= 0)
		{
			return;
		}

		scale.x *= ScaleMultiplier;
		transform.localScale = scale;
	}
}
