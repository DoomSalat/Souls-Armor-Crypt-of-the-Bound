using Sirenix.OdinInspector;
using UnityEngine;

public class TimeController : MonoBehaviour
{
	[SerializeField, MinValue(0f)] private float _defaultTimeScale = 1f;

	private float _currentTimeScale;

	private void Awake()
	{
		_currentTimeScale = _defaultTimeScale;
		Time.timeScale = _currentTimeScale;
	}

	public void SetTimeScale(float scale)
	{
		_currentTimeScale = Mathf.Max(0f, scale);
		Time.timeScale = _currentTimeScale;
	}

	public void ResetTimeScale()
	{
		SetTimeScale(_defaultTimeScale);
	}

	public float GetCurrentTimeScale() => _currentTimeScale;

	public void SlowTime(float factor = 0.5f)
	{
		SetTimeScale(_defaultTimeScale * factor);
	}

	public void SpeedUpTime(float factor = 2f)
	{
		SetTimeScale(_defaultTimeScale * factor);
	}

	public void StopTime()
	{
		SetTimeScale(0f);
	}

	public void ResumeTime()
	{
		ResetTimeScale();
	}

	[Button]
	public void DebugTimeScale(float timeScale)
	{
		SetTimeScale(timeScale);
	}
}