using DG.Tweening;
using UnityEngine;

public class GameEndUIAnimation : MonoBehaviour
{
	[SerializeField] private float _moveDuration = 0.75f;
	[SerializeField] private Ease _ease = Ease.OutBack;
	[SerializeField] private float _offsetY = 800f;

	private RectTransform _animatedRect;

	private Vector2 _initialPos;

	private void Awake()
	{
		_animatedRect = GetComponent<RectTransform>();

		_initialPos = _animatedRect.anchoredPosition;
	}

	private void OnDestroy()
	{
		_animatedRect.DOKill();
	}

	public void PlayShow()
	{
		_animatedRect.anchoredPosition = _initialPos + Vector2.up * _offsetY;

		_animatedRect.DOKill();
		_animatedRect.DOAnchorPos(_initialPos, _moveDuration).SetEase(_ease).SetUpdate(true);
	}

	public void PlayHide()
	{
		Vector2 targetPos = _initialPos + Vector2.up * _offsetY;

		_animatedRect.DOKill();
		_animatedRect.DOAnchorPos(targetPos, _moveDuration).SetEase(_ease).SetUpdate(true);
	}
}
