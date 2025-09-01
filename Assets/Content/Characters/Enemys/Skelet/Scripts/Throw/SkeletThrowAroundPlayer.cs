using UnityEngine;
using Sirenix.OdinInspector;

public class SkeletThrowAroundPlayer : BaseSkeletThrow
{
	private const float FullAngle = 360f;
	private const float Half = 0.5f;

	[SerializeField, Required] private TeleportAnimator _teleportEffect;

	[Header("Around Player Settings")]
	[SerializeField, MinValue(0.1f)] private float _minDistanceFromPlayer = 2f;
	[SerializeField, MinValue(0.1f)] private float _maxDistanceFromPlayer = 5f;

	private SpriteRenderer _teleportSprite;

	private void Awake()
	{
		_teleportSprite = _teleportEffect.GetComponent<SpriteRenderer>();
		_teleportEffect.Ended += OnTeleportAnimationCompleted;

		OnTeleportAnimationCompleted();
	}

	private void OnDestroy()
	{
		_teleportEffect.Ended -= OnTeleportAnimationCompleted;
		OnTeleportAnimationCompleted();
	}

	protected override void Attack(Vector3 target, float speed, float endDistance)
	{
		if (_throwSpawner == null)
		{
			Debug.LogWarning($"[{nameof(SkeletThrowAroundPlayer)}] ThrowSpawner not initialized!");
			return;
		}

		float randomAngle = Random.Range(0f, FullAngle);
		float randomDistance = Random.Range(_minDistanceFromPlayer, _maxDistanceFromPlayer);

		Vector3 spawnDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.right;
		Vector3 spawnPosition = target + spawnDirection * randomDistance;

		Vector3 throwDirection = (target - spawnPosition).normalized;
		Vector3 throwPointScale = _throwPoint.lossyScale;

		PlayTeleportEffect(spawnPosition);
		_throwSpawner.SpawnThrow(spawnPosition, throwDirection, Quaternion.identity, speed, endDistance, throwPointScale);
	}

	private void PlayTeleportEffect(Vector3 spawnPosition)
	{
		Vector3 centerPosition = (_throwPoint.position + spawnPosition) * Half;
		centerPosition.z = _teleportEffect.transform.position.z;
		_teleportEffect.transform.position = centerPosition;

		Vector3 direction = spawnPosition - _throwPoint.position;
		float distance = direction.magnitude;

		float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		_teleportEffect.transform.rotation = Quaternion.Euler(0, 0, angle);

		_teleportSprite.size = new Vector2(distance, _teleportSprite.size.y);

		_teleportEffect.gameObject.SetActive(true);
		_teleportEffect.transform.SetParent(null);
		_teleportEffect.Play();
	}

	private void OnTeleportAnimationCompleted()
	{
		_teleportEffect.gameObject.SetActive(false);
		_teleportEffect.transform.SetParent(gameObject.transform);
	}
}
