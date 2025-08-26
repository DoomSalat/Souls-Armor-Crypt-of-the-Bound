using UnityEngine;

public interface IFollower
{
	void SetTarget(Transform target);

	void TryFollow();
	void PauseMovement();
	void ResumeMovement();
	void EnableMovement();
	void DisableMovement();

	bool TryGetDistanceToTarget(out float distance);

	bool IsMovementEnabled { get; }
	Vector2 Direction { get; }
	Transform Target { get; }

	event System.Action TargetReached;
}
