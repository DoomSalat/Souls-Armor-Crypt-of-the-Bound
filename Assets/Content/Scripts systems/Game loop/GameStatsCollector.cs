using UnityEngine;

public class GameStatsCollector : MonoBehaviour
{
	[SerializeField] private int _kills = 0;
	[SerializeField] private int _absorbedSouls = 0;
	[SerializeField] private float _startTime;

	public int Kills => _kills;
	public int AbsorbedSouls => _absorbedSouls;
	public float GameTime => Time.time - _startTime;

	private void Start()
	{
		ResetStats();
	}

	public void AddKill()
	{
		_kills++;
	}

	public void AddAbsorbedSoul()
	{
		_absorbedSouls++;
	}

	public void ResetStats()
	{
		_kills = 0;
		_absorbedSouls = 0;
		_startTime = Time.time;
	}
}