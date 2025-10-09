using UnityEngine;
using DG.Tweening;

public class LightningAnimation : MonoBehaviour
{
	private const float RandomAngleRange = 360f;
	private const float MinRandomDistance = 2f;
	private const float MaxRandomDistance = 5f;

	[Header("Lightning Animation")]
	[SerializeField] private SpriteRenderer _lightningSprite;
	[SerializeField] private float _lightningDuration = 0.2f;
	[SerializeField] private float _fadeDuration = 1.5f;

	private Color _clearWhiteColor = new Color(1f, 1f, 1f, 0f);

	public event System.Action AnimationEnded;

	public void PlayAnimation(Vector3 targetPosition, bool foundEnemy)
	{
		float distance;

		if (foundEnemy)
		{
			Vector2 direction = (targetPosition - transform.position).normalized;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

			Vector2 diff = targetPosition - transform.position;
			distance = Mathf.Sqrt(diff.x * diff.x + diff.y * diff.y);
		}
		else
		{
			float randomAngle = Random.Range(0f, RandomAngleRange);
			transform.rotation = Quaternion.AngleAxis(randomAngle, Vector3.forward);

			distance = Random.Range(MinRandomDistance, MaxRandomDistance);
		}

		_lightningSprite.size = new Vector2(0f, _lightningSprite.size.y);
		_lightningSprite.color = Color.white;

		DOTween.To(() => _lightningSprite.size.x, x => _lightningSprite.size = new Vector2(x, _lightningSprite.size.y), distance, _lightningDuration)
			.SetEase(Ease.OutQuad)
			.OnComplete(() =>
			{
				_lightningSprite.DOFade(0f, _fadeDuration)
					.SetEase(Ease.InQuad)
					.OnComplete(() =>
					{
						AnimationEnded?.Invoke();
					});
			});
	}

	public void Reset()
	{
		_lightningSprite.size = new Vector2(0f, _lightningSprite.size.y);
		_lightningSprite.color = _clearWhiteColor;
		transform.rotation = Quaternion.identity;
	}
}