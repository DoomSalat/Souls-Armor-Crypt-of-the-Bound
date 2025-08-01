using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Transform))]
public class SwordAttackZoneScaler : MonoBehaviour
{
	[SerializeField, Required] private Vector2[] _attackZoneScales;
	[SerializeField] private float _scaleDuration = 0.1f;
	[SerializeField] private Ease _scaleEase = Ease.OutQuad;

	private Transform _transform;
	private Tween _scaleTween;

	private void Awake()
	{
		_transform = GetComponent<Transform>();
	}

	public void SetAttackZoneScale(int scaleIndex)
	{
		if (scaleIndex < 0 || scaleIndex >= _attackZoneScales.Length)
		{
			Debug.LogWarning($"Scale index {scaleIndex} is out of range. Available scales: {_attackZoneScales.Length}");
			return;
		}

		Vector2 targetScale = _attackZoneScales[scaleIndex];
		Vector3 targetScale3D = new Vector3(targetScale.x, targetScale.y, _transform.localScale.z);

		_scaleTween.Kill();
		_scaleTween = _transform.DOScale(targetScale3D, _scaleDuration)
			.SetEase(_scaleEase);
	}

	public void SetAttackZoneScaleImmediate(int scaleIndex)
	{
		if (scaleIndex < 0 || scaleIndex >= _attackZoneScales.Length)
		{
			Debug.LogWarning($"Scale index {scaleIndex} is out of range. Available scales: {_attackZoneScales.Length}");
			return;
		}

		_scaleTween.Kill();
		Vector2 targetScale = _attackZoneScales[scaleIndex];
		_transform.localScale = new Vector3(targetScale.x, targetScale.y, _transform.localScale.z);
	}

	public int GetScaleCount() => _attackZoneScales.Length;

	private void OnDestroy()
	{
		_scaleTween.Kill();
	}
}