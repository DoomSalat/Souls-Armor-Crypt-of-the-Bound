using UnityEngine;
using UnityEngine.Pool;
using Sirenix.OdinInspector;

public class LightningSpawner : MonoBehaviour
{
	[Header("Lightning Pool Settings")]
	[SerializeField, Required] private LightningAnimation _lightningPrefab;
	[SerializeField] private int _defaultCapacity = 10;
	[SerializeField] private int _maxSize = 20;

	private ObjectPool<LightningAnimation> _lightningPool;

	public void Initialize()
	{
		_lightningPool = new ObjectPool<LightningAnimation>(
			createFunc: CreateLightning,
			actionOnGet: OnGetLightning,
			actionOnRelease: OnReleaseLightning,
			actionOnDestroy: OnDestroyLightning,
			collectionCheck: true,
			defaultCapacity: _defaultCapacity,
			maxSize: _maxSize);
	}

	private LightningAnimation CreateLightning()
	{
		LightningAnimation lightning = Instantiate(_lightningPrefab, transform.position, Quaternion.identity);
		lightning.SetPool(_lightningPool);
		return lightning;
	}

	private void OnGetLightning(LightningAnimation lightning)
	{
		lightning.gameObject.SetActive(true);
	}

	private void OnReleaseLightning(LightningAnimation lightning)
	{
		lightning.gameObject.SetActive(false);
	}

	private void OnDestroyLightning(LightningAnimation lightning)
	{
		Destroy(lightning.gameObject);
	}

	public LightningAnimation SpawnLightning(Vector3 targetPosition, bool foundEnemy)
	{
		LightningAnimation lightning = _lightningPool.Get();
		lightning.transform.position = transform.position;
		lightning.PlayAnimation(targetPosition, foundEnemy);
		return lightning;
	}

	private void OnDestroy()
	{
		_lightningPool?.Dispose();
	}
}