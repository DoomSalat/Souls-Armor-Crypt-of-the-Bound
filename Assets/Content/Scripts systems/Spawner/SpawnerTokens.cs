using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SpawnerSystem
{
	public class SpawnerTokens : MonoBehaviour
	{
		private const int SectionCount = 8;
		private const int EnemyKindCount = 4;
		private const float ScreenHeightMultiplier = 0.5f;
		private const float SectionAngleRadians = Mathf.PI / 4f;
		private const float DefaultWeight = 1f;

		[SerializeField, MinValue(0)] private float _baseCostPerEnemy = 1f;
		[SerializeField, MinValue(0)] private float _decayPerSecond = 0.25f;
		[SerializeField, MinValue(0)] private float _minCost = 1f;
		[SerializeField, MinValue(0)] private float _spawnDistanceFromCenter = 3f;
		[SerializeField, MinValue(0)] private float _desiredPowerPerSection = 5f;
		[SerializeField, Range(0, 1)] private float _glitchChance = 0.05f;
		[SerializeField, MinValue(0)] private int _maxSectionDeviation = 2;

		[Header("Weights by kind")]
		[SerializeField] private EnemyTier[] _kindWeights;

		[Header("Manual Mode")]
		[SerializeField] private bool _manualMode;

		public bool ManualMode => _manualMode;

		private Transform _player;
		private Camera _camera;

		private readonly float[] _costBySection = new float[SectionCount];
		private readonly int[,] _enemyKindCountsBySection = new int[SectionCount, EnemyKindCount];
		private int _currentTargetSection;
		private float _currentSectionPower;
		private bool _isTargetingNewSection = true;

		private void Awake()
		{
			_camera = Camera.main;

			for (int i = 0; i < _costBySection.Length; i++)
				_costBySection[i] = _minCost;
		}

		private void Update()
		{
			float decay = _decayPerSecond * Time.deltaTime;

			for (int i = 0; i < _costBySection.Length; i++)
			{
				_costBySection[i] = Mathf.Max(_minCost, _costBySection[i] - decay);
			}
		}

		public void Init(Transform player)
		{
			_player = player;
		}

		public SpawnSection SelectSection()
		{
			if (UnityEngine.Random.value < _glitchChance)
			{
				return (SpawnSection)UnityEngine.Random.Range(0, SectionCount);
			}

			if (_isTargetingNewSection || _currentSectionPower >= _desiredPowerPerSection)
			{
				_currentTargetSection = SelectNewTargetSection();
				_currentSectionPower = 0f;
				_isTargetingNewSection = false;
			}

			return (SpawnSection)_currentTargetSection;
		}

		private int SelectNewTargetSection()
		{
			int lowestPowerSection = 0;
			float lowestPower = _costBySection[0];

			for (int i = 1; i < _costBySection.Length; i++)
			{
				if (_costBySection[i] < lowestPower)
				{
					lowestPower = _costBySection[i];
					lowestPowerSection = i;
				}
			}

			int oppositeSection = (lowestPowerSection + SectionCount / 2) % SectionCount;

			int deviation = UnityEngine.Random.Range(-_maxSectionDeviation, _maxSectionDeviation + 1);
			int targetSection = (oppositeSection + deviation + SectionCount) % SectionCount;

			return targetSection;
		}

		public void ForceRetarget()
		{
			_isTargetingNewSection = true;
		}

		public void Commit(SpawnSection section, int enemiesSpawned)
		{
			if (enemiesSpawned <= 0)
				return;

			int index = (int)section;
			float powerAdded = enemiesSpawned * Mathf.Max(_baseCostPerEnemy, _minCost);
			_costBySection[index] += powerAdded;

			if (index == _currentTargetSection)
			{
				_currentSectionPower += powerAdded;
			}
		}

		public void Release(SpawnSection section, int enemiesReturned)
		{
			if (enemiesReturned <= 0)
				return;

			int index = (int)section;
			_costBySection[index] = Mathf.Max(_minCost, _costBySection[index] - enemiesReturned * _baseCostPerEnemy);
		}

		public void CommitKind(SpawnSection section, EnemyKind kind, int amount = 1)
		{
			int sectionIndex = (int)section;
			int kindIndex = (int)kind;

			_enemyKindCountsBySection[sectionIndex, kindIndex] += amount;
		}

		public void ReleaseKind(SpawnSection section, EnemyKind kind, int amount = 1)
		{
			int sectionIndex = (int)section;
			int kindIndex = (int)kind;

			_enemyKindCountsBySection[sectionIndex, kindIndex] = Mathf.Max(0, _enemyKindCountsBySection[sectionIndex, kindIndex] - amount);
		}

		public EnemyKind SuggestComplementaryKind(SpawnSection section)
		{
			int sectionIndex = (int)section;
			int soul = _enemyKindCountsBySection[sectionIndex, (int)EnemyKind.Soul];
			int vase = _enemyKindCountsBySection[sectionIndex, (int)EnemyKind.SoulVase];
			int skelet = _enemyKindCountsBySection[sectionIndex, (int)EnemyKind.Skelet];
			int knight = _enemyKindCountsBySection[sectionIndex, (int)EnemyKind.Knight];

			if (knight > 0 && skelet == 0)
				return EnemyKind.Skelet;
			if (skelet > 0 && knight == 0)
				return EnemyKind.Knight;
			if (soul + vase > 2)
				return EnemyKind.Skelet;

			return EnemyKind.Soul;
		}


		public float GetKindWeight(EnemyKind kind)
		{
			for (int i = 0; i < (_kindWeights?.Length ?? 0); i++)
			{
				if (_kindWeights[i].Kind == kind)
					return Mathf.Max(0f, _kindWeights[i].Weight);
			}

			return DefaultWeight;
		}

		public Vector3 GetSpawnPosition(SpawnSection section)
		{
			if (_player == null)
				throw new InvalidOperationException("Player reference is required for spawn positioning");

			Vector3 topScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			Vector3 bottomScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(_camera.transform.position.z - _player.position.z)));
			float screenHeight = Vector3.Distance(topScreen, bottomScreen);
			float circleRadius = screenHeight * ScreenHeightMultiplier + _spawnDistanceFromCenter;

			float angleInRadians = (int)section * SectionAngleRadians;

			Vector3 spawnDirection = new Vector3(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians), 0f);
			Vector3 spawnPoint = _player.position + spawnDirection * circleRadius;
			spawnPoint.z = _player.position.z;

			return spawnPoint;
		}

		public SpawnSection GetSectionByDirection(SpawnDirection direction)
		{
			return (SpawnSection)((int)direction);
		}
	}
}
