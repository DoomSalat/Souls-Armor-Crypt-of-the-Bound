using UnityEngine;
using Sirenix.OdinInspector;

public class SkeletThrowRadial : BaseSkeletThrow
{
	private const float FullAngle = 360f;

	[Header("Radial Throw Settings")]
	[SerializeField, MinValue(2)] private int _directionCount = 8;
	[SerializeField] private bool _includePlayerDirection = true;

	protected override void Attack(Vector3 target, float speed, float endDistance)
	{
		if (_throwSpawner == null)
		{
			Debug.LogWarning($"[{nameof(SkeletThrowRadial)}] ThrowSpawner not initialized!");
			return;
		}

		Vector3 throwCenter = transform.position;
		float angleStep = FullAngle / _directionCount;

		for (int i = 0; i < _directionCount; i++)
		{
			float angle = i * angleStep;
			Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;

			_throwSpawner.SpawnThrow(_throwPoint.position, direction, _throwPoint.rotation, speed, endDistance);
		}

		if (_includePlayerDirection)
		{
			Vector3 playerDirection = (target - throwCenter).normalized;
			_throwSpawner.SpawnThrow(_throwPoint.position, playerDirection, _throwPoint.rotation, speed, endDistance);
		}
	}
}
