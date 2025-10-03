using UnityEngine;

namespace SpawnerSystem
{
	[System.Serializable]
	public class GroupMetaData
	{
		[SerializeField] private int _tokensToReturn;
		[SerializeField] private float _timerReductionOnDeath;
		[SerializeField] private bool _hasData;

		public int TokensToReturn => _tokensToReturn;
		public float TimerReductionOnDeath => _timerReductionOnDeath;
		public bool HasData => _hasData;

		public GroupMetaData()
		{
			_tokensToReturn = 0;
			_timerReductionOnDeath = 0f;
			_hasData = false;
		}

		public GroupMetaData(int tokensToReturn, float timerReduction)
		{
			_tokensToReturn = tokensToReturn;
			_timerReductionOnDeath = timerReduction;
			_hasData = true;
		}

		public void SetData(int tokensToReturn, float timerReduction)
		{
			_tokensToReturn = tokensToReturn;
			_timerReductionOnDeath = timerReduction;
			_hasData = true;
		}

		public void ClearData()
		{
			_tokensToReturn = 0;
			_timerReductionOnDeath = 0f;
			_hasData = false;
		}

		public GroupMetaData GetCopy()
		{
			return new GroupMetaData(_tokensToReturn, _timerReductionOnDeath);
		}
	}
}
