using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GameEndUI : MonoBehaviour
{
	[Header("Texts")]
	[SerializeField, Required] private TextMeshProUGUI _killsText;
	[SerializeField, Required] private TextMeshProUGUI _soulAbsorbationsText;
	[SerializeField, Required] private TextMeshProUGUI _timeText;

	[Header("Visuals")]
	[SerializeField, Required] private Image _titleImage;
	[Space]
	[SerializeField, Required] private Sprite _victoryTitle;
	[SerializeField, Required] private Sprite _deathTitle;

	[Header("Components")]
	[SerializeField, Required] private GameEndUIAnimation _animation;

	private GameStatsCollector _statsCollector;

	public void Show(bool isVictory)
	{
		RefreshTitle(isVictory);
		RefreshStats(isVictory);

		gameObject.SetActive(true);
		_animation.PlayShow();
	}

	public void Hide()
	{
		_animation.PlayHide();
	}

	public void Initialize(GameStatsCollector statsCollector)
	{
		_statsCollector = statsCollector;
	}

	private void RefreshStats(bool isVictory)
	{
		if (_statsCollector == null)
			return;

		_killsText.text = _statsCollector.Kills.ToString();
		_soulAbsorbationsText.text = _statsCollector.AbsorbedSouls.ToString();

		float gameTime = _statsCollector.GameTime;
		int minutes = Mathf.FloorToInt(gameTime / 60f);
		int seconds = Mathf.FloorToInt(gameTime % 60f);
		_timeText.text = $"{minutes:00}:{seconds:00}";
	}

	private void RefreshTitle(bool isVictory)
	{
		if (isVictory)
		{
			_titleImage.sprite = _victoryTitle;
		}
		else
		{
			_titleImage.sprite = _deathTitle;
		}
	}
}