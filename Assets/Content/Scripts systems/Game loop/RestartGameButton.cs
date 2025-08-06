using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Button))]
public class RestartGameButton : MonoBehaviour
{
	[SerializeField, Required] private GameFlow _gameFlow;

	private Button _button;

	private void OnEnable()
	{
		if (_button == null)
			_button = GetComponent<Button>();

		_button.onClick.AddListener(OnRestartButtonClicked);
	}

	private void OnDisable()
	{
		_button.onClick.RemoveListener(OnRestartButtonClicked);
	}

	public void OnRestartButtonClicked()
	{
		_gameFlow.RestartGame();
	}
}