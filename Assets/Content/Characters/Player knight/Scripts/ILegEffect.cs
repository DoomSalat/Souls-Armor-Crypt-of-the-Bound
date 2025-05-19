using UnityEngine;

public interface ILegEffect
{
	void ApplyEffect(InputMove inputMove, Vector2 direction, float stepDuration);
}