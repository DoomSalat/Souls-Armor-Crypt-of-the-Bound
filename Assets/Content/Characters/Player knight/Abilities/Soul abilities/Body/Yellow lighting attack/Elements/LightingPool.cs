using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Pool;

public class LightingPool : MonoBehaviour
{
	[SerializeField, Required] private LightningAnimation _animation;

	private IObjectPool<LightingPool> _pool;

	private void OnEnable()
	{
		_animation.AnimationEnded += ReturnToPool;
	}

	private void OnDisable()
	{
		_animation.AnimationEnded -= ReturnToPool;
	}

	public void SetPool(IObjectPool<LightingPool> pool)
	{
		_pool = pool;
	}

	public void Play(Vector3 targetPosition, bool foundEnemy)
	{
		_animation.PlayAnimation(targetPosition, foundEnemy);
	}

	private void ReturnToPool()
	{
		_animation.Reset();
		_pool?.Release(this);
	}
}
