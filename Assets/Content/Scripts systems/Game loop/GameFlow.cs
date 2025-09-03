using UnityEngine;
using Sirenix.OdinInspector;

public class GameFlow : MonoBehaviour
{
	[Header("Event Sources")]
	[SerializeField, Required] private PlayerDamage _playerDamage;
	[SerializeField, Required] private VictoryEvents _victoryEvents;

	[Header("UI")]
	[SerializeField, Required] private GameEndUI _gameEndUI;

	[Header("Services")]
	[SerializeField, Required] private TimeController _timeController;
	[SerializeField, Required] private InputReader _inputReader;
	[SerializeField, Required] private GameStatsCollector _statsCollector;
	[SerializeField, Required] private SceneActivator _sceneActivator;
	[SerializeField, Required] private PlayerStateMachine _playerStateMachine;

	[Header("UI Controls")]
	[SerializeField, Required] private RectTransform _joysticksCanvas;

	private bool _isGameOver;

	private void Start()
	{
		_joysticksCanvas.gameObject.SetActive(false);
		_gameEndUI.gameObject.SetActive(false);

		_gameEndUI.Initialize(_statsCollector);
	}

	private void OnEnable()
	{
		_playerDamage.DiedEnd += OnPlayerDeath;
		_victoryEvents.LevelCompleted += OnPlayerVictory;
		_sceneActivator.FadeCompleted += OnFadeCompleted;

		_playerStateMachine.StatesInitialized += OnStatesInitialized;
		if (_playerStateMachine.GetState<CutsceneState>() != null)
		{
			SubscribeToCutsceneEvents();
		}
	}

	private void OnDisable()
	{
		_playerDamage.DiedEnd -= OnPlayerDeath;
		_victoryEvents.LevelCompleted -= OnPlayerVictory;
		_sceneActivator.FadeCompleted -= OnFadeCompleted;

		_playerStateMachine.StatesInitialized -= OnStatesInitialized;
	}

	private void OnDestroy()
	{
		UnsubscribeFromCutsceneEvents();
	}

	private void OnStatesInitialized()
	{
		SubscribeToCutsceneEvents();
	}

	public void ActivateInput()
	{
		_joysticksCanvas.gameObject.SetActive(true);
		_inputReader.Disable();
	}

	public void DeactiveInput()
	{
		_joysticksCanvas.gameObject.SetActive(false);
		_inputReader.Enable();
	}

	[ContextMenu(nameof(RestartGame))]
	public void RestartGame()
	{
		_timeController.ResumeTime();
		_inputReader.Enable();
		_statsCollector.ResetStats();

		_isGameOver = false;
		_sceneActivator.RestartScene();
		_gameEndUI.Hide();
	}

	private void OnPlayerDeath() => EndGame(false);
	private void OnPlayerVictory() => EndGame(true);

	private void OnFadeCompleted()
	{
		if (_playerStateMachine.IsCurrentState<CutsceneState>())
		{
			Debug.Log($"{nameof(GameFlow)} Fade completed during cutscene, skipping input activation");
			return;
		}

		_joysticksCanvas.gameObject.SetActive(true);
		_inputReader.Enable();
	}

	private void EndGame(bool isVictory)
	{
		if (_isGameOver)
			return;

		_isGameOver = true;

		_timeController.StopTime();
		_inputReader.Disable();
		_joysticksCanvas.gameObject.SetActive(false);

		_gameEndUI.Show(isVictory);
	}

	private void OnCutsceneInputShouldBeDisabled()
	{
		DeactiveInput();
	}

	private void OnCutsceneInputShouldBeEnabled()
	{
		ActivateInput();
	}

	private void SubscribeToCutsceneEvents()
	{
		var cutsceneState = _playerStateMachine.GetState<CutsceneState>();

		cutsceneState.InputShouldBeDisabled += OnCutsceneInputShouldBeDisabled;
		cutsceneState.InputShouldBeEnabled += OnCutsceneInputShouldBeEnabled;
	}

	private void UnsubscribeFromCutsceneEvents()
	{
		var cutsceneState = _playerStateMachine.GetState<CutsceneState>();

		cutsceneState.InputShouldBeDisabled -= OnCutsceneInputShouldBeDisabled;
		cutsceneState.InputShouldBeEnabled -= OnCutsceneInputShouldBeEnabled;
	}
}
