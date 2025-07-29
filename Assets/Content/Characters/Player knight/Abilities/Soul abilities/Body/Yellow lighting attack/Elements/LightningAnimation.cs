using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class LightningAnimation : MonoBehaviour
{
	private const float RandomAngleRange = 360f;

	[Header("Lightning Animation")]
	[SerializeField] private SpriteRenderer _lightningSprite;
	[SerializeField] private float _lightningDuration = 0.2f;
	[SerializeField] private float _fadeDuration = 1.5f;

	private Color _clearWhiteColor = new Color(1f, 1f, 1f, 0f);

	private IObjectPool<LightningAnimation> _pool;

	public void SetPool(IObjectPool<LightningAnimation> pool)
	{
		_pool = pool;
	}

	public void PlayAnimation(Vector3 targetPosition, bool foundEnemy)
	{
		if (foundEnemy)
		{
			Vector2 direction = (targetPosition - transform.position).normalized;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
		else
		{
			float randomAngle = Random.Range(0f, RandomAngleRange);
			transform.rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);
		}

		_lightningSprite.size = new Vector2(0f, _lightningSprite.size.y);
		_lightningSprite.color = Color.white;

		float distance = Vector2.Distance(transform.position, targetPosition);

		DOTween.To(() => _lightningSprite.size.x, x => _lightningSprite.size = new Vector2(x, _lightningSprite.size.y), distance, _lightningDuration)
			.SetEase(Ease.OutQuad)
			.OnComplete(() =>
			{
				_lightningSprite.DOFade(0f, _fadeDuration)
					.SetEase(Ease.InQuad)
					.OnComplete(() =>
					{
						ReturnToPool();
					});
			});
	}

	private void ReturnToPool()
	{
		_lightningSprite.size = new Vector2(0f, _lightningSprite.size.y);
		_lightningSprite.color = _clearWhiteColor;
		transform.rotation = Quaternion.identity;

		_pool?.Release(this);
	}
}