using UnityEngine;
using Sirenix.OdinInspector;

public abstract class BaseSkeletThrow : MonoBehaviour
{
	[Header("Throw Settings")]
	[SerializeField, Required] protected Transform _throwPoint;
	[SerializeField] protected ThrowSpawner _throwSpawner;

	[Header("Throw Parameters")]
	[SerializeField] protected float _speedBone = 10f;
	[SerializeField] protected float _endDistanceBone = 10f;

	public void Initialize(ThrowSpawner throwSpawner)
	{
		_throwSpawner = throwSpawner;
	}

	public void Attack(Vector3 target)
	{
		Attack(target, _speedBone, _endDistanceBone);
	}

	protected virtual void Attack(Vector3 target, float speed, float endDistance)
	{
		if (_throwSpawner == null)
		{
			Debug.LogWarning($"[{nameof(BaseSkeletThrow)}] ThrowSpawner not initialized!");
			return;
		}

		Vector3 throwDirection = (target - _throwPoint.position).normalized;
		Vector3 throwPointScale = _throwPoint.lossyScale;
		_throwSpawner.SpawnThrow(_throwPoint.position, throwDirection, _throwPoint.rotation, speed, endDistance, throwPointScale);
	}

	[Button("Test Throw")]
	protected virtual void TestThrow()
	{
		Vector3 target = _throwPoint.position + Vector3.right * 5f;
		Attack(target);
	}
}
