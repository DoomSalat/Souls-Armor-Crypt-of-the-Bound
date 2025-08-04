using UnityEngine;
using UnityEngine.Pool;
using Sirenix.OdinInspector;

public class LightningSpawner : MonoBehaviour
{
	private const int DefaultCapacity = 10;
	private const int MaxSize = 20;

	[SerializeField, Required] private LightingPool _lightningPoolPrefab;

	private ObjectPool<LightingPool> _lightningPool;

	public LightingPool SpawnLightning(Vector3 targetPosition, bool foundEnemy)
	{
		LightingPool lightning = _lightningPool.Get();
		lightning.transform.position = transform.position;
		lightning.Play(targetPosition, foundEnemy);

		return lightning;
	}

	public void Initialize()
	{
		_lightningPool = new ObjectPool<LightingPool>(
			createFunc: CreateLightning,
			actionOnGet: OnGetLightning,
			actionOnRelease: OnReleaseLightning,
			actionOnDestroy: OnDestroyLightning,
			collectionCheck: true,
			defaultCapacity: DefaultCapacity,
			maxSize: MaxSize);
	}

	private LightingPool CreateLightning()
	{
		LightingPool lightning = Instantiate(_lightningPoolPrefab, transform.position, Quaternion.identity);
		lightning.SetPool(_lightningPool);

		return lightning;
	}

	private void OnGetLightning(LightingPool lightning)
	{
		lightning.gameObject.SetActive(true);
	}

	private void OnReleaseLightning(LightingPool lightning)
	{
		lightning.gameObject.SetActive(false);
	}

	private void OnDestroyLightning(LightingPool lightning)
	{
		Destroy(lightning.gameObject);
	}

	private void OnDestroy()
	{
		_lightningPool?.Dispose();
	}
}