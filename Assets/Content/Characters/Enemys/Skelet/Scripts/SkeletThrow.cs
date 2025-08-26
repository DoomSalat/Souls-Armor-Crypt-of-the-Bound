using UnityEngine;
using Sirenix.OdinInspector;

public class SkeletThrow : MonoBehaviour
{
	[Header("Throw Settings")]
	[SerializeField, Required] private Transform _throwPoint;
	[SerializeField, Required] private ThrowSpawner _throwSpawner;

	public void Initialize(ThrowSpawner throwSpawner)
	{
		_throwSpawner = throwSpawner;
	}

	public void Attack(Vector3 target)
	{
		if (_throwSpawner == null)
		{
			Debug.LogWarning($"[{nameof(SkeletThrow)}] ThrowSpawner not initialized!");
			return;
		}

		Vector3 throwDirection = (target - _throwPoint.position).normalized;
		_throwSpawner.SpawnThrow(_throwPoint.position, throwDirection, _throwPoint.rotation);
	}

	[Button("Test Throw")]
	private void TestThrow()
	{
		Vector3 target = _throwPoint.position + Vector3.right * 5f;
		Attack(target);
	}
}
