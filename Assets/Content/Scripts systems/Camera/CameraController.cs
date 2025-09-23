using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
	private const int PriorityHigh = 10;
	private const int PriorityLow = 0;
	private const string AbsorptionZoom = "AbsorptionZoom";
	private const string AbsorptionZoomReturn = "AbsorptionZoomReturn";
	private const string CutsceneZoomReturn = "CutsceneZoomReturn";

	[SerializeField, Required] private CinemachineCamera _mainCamera;
	[SerializeField, Required] private CinemachineCamera _playerMobCamera;
	[SerializeField, Required] private PlayerStateMachine _playerStateMachine;

	[Title("Absorption Settings")]
	[SerializeField, Min(0.1f)] private float _absorptionTargetOrthoSize = 3f;
	[SerializeField, Min(0.1f)] private float _absorptionZoomDuration = 0.5f;
	[SerializeField] private Ease _absorptionZoomEase = Ease.InQuad;

	[Title("Return Settings")]
	[SerializeField, Min(0.1f)] private float _returnZoomDuration = 0.35f;
	[SerializeField] private Ease _returnZoomEase = Ease.OutQuad;

	[Title("Cutscene Settings")]
	[SerializeField, Min(0.1f)] private float _cutsceneStartOrthoSize = 2f;
	[SerializeField, Min(0.1f)] private float _cutsceneTransitionDuration = 1.5f;
	[SerializeField] private Ease _cutsceneReturnEase = Ease.OutQuad;

	private float _originalOrthoSize;
	private Tween _currentZoomTween;
	private AbsorptionState _absorptionState;
	private CutsceneState _cutsceneState;

	private void Awake()
	{
		_mainCamera.Priority = PriorityHigh;
		_playerMobCamera.Priority = PriorityLow;
		_originalOrthoSize = _mainCamera.Lens.OrthographicSize;
	}

	private void OnEnable()
	{
		if (_playerStateMachine.IsStatesInitialized)
		{
			OnStatesInitialized();
		}
		else
		{
			_playerStateMachine.StatesInitialized += OnStatesInitialized;
		}
	}

	private void OnDisable()
	{
		_playerStateMachine.StatesInitialized -= OnStatesInitialized;

		UnsubscribeFromAbsorptionEvents();
		UnsubscribeFromCutsceneEvents();
	}

	private void OnDestroy()
	{
		_currentZoomTween?.Kill();
	}

	private void OnStatesInitialized()
	{
		_absorptionState = _playerStateMachine.GetState<AbsorptionState>();
		_cutsceneState = _playerStateMachine.GetState<CutsceneState>();

		SubscribeToAbsorptionEvents();
		SubscribeToCutsceneEvents();
	}

	private void SubscribeToAbsorptionEvents()
	{
		if (_absorptionState == null)
			return;

		_absorptionState.AbsorptionStarted += StartAbsorptionZoom;
		_absorptionState.InventoryClosed += EndAbsorptionZoom;
	}

	private void UnsubscribeFromAbsorptionEvents()
	{
		if (_absorptionState == null)
			return;

		_absorptionState.AbsorptionStarted -= StartAbsorptionZoom;
		_absorptionState.InventoryClosed -= EndAbsorptionZoom;
	}

	private void SubscribeToCutsceneEvents()
	{
		if (_cutsceneState == null)
			return;

		_cutsceneState.CutsceneStarted += OnCutsceneStarted;
		_cutsceneState.CutsceneOuted += OnCutsceneOuted;
	}

	private void UnsubscribeFromCutsceneEvents()
	{
		if (_cutsceneState == null)
			return;

		_cutsceneState.CutsceneStarted -= OnCutsceneStarted;
		_cutsceneState.CutsceneOuted -= OnCutsceneOuted;
	}

	public void SwitchToGlobalCamera()
	{
		_mainCamera.Priority = PriorityHigh;
		_playerMobCamera.Priority = PriorityLow;
	}

	public void SwitchToPlayerMobCamera()
	{
		_mainCamera.Priority = PriorityLow;
		_playerMobCamera.Priority = PriorityHigh;
	}

	public void StartAbsorptionZoom()
	{
		_currentZoomTween?.Kill();
		CreateZoomTween(_absorptionTargetOrthoSize, AbsorptionZoom, _absorptionZoomDuration, _absorptionZoomEase);
	}

	public void EndAbsorptionZoom()
	{
		_currentZoomTween?.Kill();
		CreateZoomTween(_originalOrthoSize, AbsorptionZoomReturn, _returnZoomDuration, _returnZoomEase);
	}

	public void ResetCameraSize()
	{
		_currentZoomTween?.Kill();
		SetCameraOrthoSize(_originalOrthoSize);
	}

	public void SetCutsceneZoomInstant()
	{
		_currentZoomTween?.Kill();
		SetCameraOrthoSize(_cutsceneStartOrthoSize);
	}

	public void ReturnFromCutsceneZoom()
	{
		_currentZoomTween?.Kill();
		CreateZoomTween(_originalOrthoSize, CutsceneZoomReturn, _cutsceneTransitionDuration, _cutsceneReturnEase);
	}

	private void CreateZoomTween(float targetSize, string tweenId, float duration, Ease ease)
	{
		_currentZoomTween = DOTween.To(
			() => _mainCamera.Lens.OrthographicSize,
			SetCameraOrthoSize,
			targetSize,
			duration
		)
		.SetEase(ease)
		.SetId(tweenId);
	}

	private void SetCameraOrthoSize(float size)
	{
		var lens = _mainCamera.Lens;
		lens.OrthographicSize = size;
		_mainCamera.Lens = lens;
	}

	private void OnCutsceneStarted()
	{
		SetCutsceneZoomInstant();
	}

	private void OnCutsceneOuted()
	{
		ReturnFromCutsceneZoom();
	}
}
