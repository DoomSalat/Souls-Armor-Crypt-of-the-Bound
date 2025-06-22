using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleReverseSystem : MonoBehaviour
{
	private const float SlowdownStartProgress = 0.5f;
	private const float MinRemainingTime = 0.01f;
	private const float ReturnProgressThreshold = 0.99f;
	private const int MaxInitAttempts = 60;
	private const float Half = 0.5f;

	[SerializeField, Range(0.1f, 0.9f)] private float _returnStartThreshold = 0.8f;
	[SerializeField] private float _durationAfterReturn = 0.5f;
	[SerializeField, Range(0.1f, 10f)] private float _slowdownSmoothness = 2f;
	[SerializeField, Range(0.1f, 10f)] private float _returnAcceleration = 3f;

	private ParticleSystem _particleSystem;
	private ParticleSystem.Particle[] _particles;
	private Vector3[] _calculatedInitialPositions;
	private float[] _startLifetimes;
	private bool[] _hasReturned;
	private Vector3[] _originalVelocities;
	private float[] _originalSpeeds;
	private bool _isActive;
	private int _initAttempts;
	private bool _wasPlaying;
	private float _lastSimulationTime;

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
	}

	private void OnValidate()
	{
		if (_particleSystem == null)
			_particleSystem = GetComponent<ParticleSystem>();
	}

	private void Update()
	{
		bool isCurrentlyPlaying = _particleSystem.isPlaying;
		float currentSimulationTime = _particleSystem.time;

		bool shouldActivate = false;

		if (isCurrentlyPlaying && !_wasPlaying)
		{
			shouldActivate = true;
		}
		else if (isCurrentlyPlaying && _wasPlaying && currentSimulationTime < _lastSimulationTime)
		{
			shouldActivate = true;
		}

		if (shouldActivate)
		{
			_isActive = true;
			_initAttempts = 0;
			_particles = null;
		}

		_wasPlaying = isCurrentlyPlaying;
		_lastSimulationTime = currentSimulationTime;

		if (_isActive)
		{
			if (_particles == null)
			{
				TryInitialize();
			}
			else
			{
				ProcessParticles();
			}
		}
	}

	[ContextMenu("Play")]
	public void Play()
	{
		_particleSystem.Play();
	}

	private void TryInitialize()
	{
		_initAttempts++;

		int count = _particleSystem.particleCount;
		if (count > 0)
		{
			_particles = new ParticleSystem.Particle[count];
			_calculatedInitialPositions = new Vector3[count];
			_startLifetimes = new float[count];
			_hasReturned = new bool[count];
			_originalVelocities = new Vector3[count];
			_originalSpeeds = new float[count];

			_particleSystem.GetParticles(_particles, count);

			for (int i = 0; i < count; i++)
			{
				float timeAlive = _particles[i].startLifetime - _particles[i].remainingLifetime;
				Vector3 currentPos = _particles[i].position;
				Vector3 currentVelocity = _particles[i].velocity;

				Vector3 gravity = Physics.gravity * _particleSystem.main.gravityModifier.constant;
				_calculatedInitialPositions[i] = currentPos - (currentVelocity * timeAlive) - (Half * gravity * timeAlive * timeAlive);

				_startLifetimes[i] = _particles[i].startLifetime;
				_originalVelocities[i] = currentVelocity;
				_originalSpeeds[i] = currentVelocity.magnitude;
				_hasReturned[i] = false;
			}
		}
		else if (_initAttempts > MaxInitAttempts)
		{
			_isActive = false;
		}
	}

	private void ProcessParticles()
	{
		int count = _particleSystem.particleCount;
		if (count == 0)
		{
			_isActive = false;
			return;
		}

		_particleSystem.GetParticles(_particles, count);

		for (int i = 0; i < Mathf.Min(count, _particles.Length); i++)
		{
			float workingLifetime = _startLifetimes[i] - _durationAfterReturn;
			float lifeProgress = (_startLifetimes[i] - _particles[i].remainingLifetime) / workingLifetime;

			if (lifeProgress >= SlowdownStartProgress && !_hasReturned[i])
			{
				float transitionPoint = SlowdownStartProgress + (1f - SlowdownStartProgress) * _returnStartThreshold;

				if (lifeProgress < transitionPoint)
				{
					float slowdownPhaseProgress = (lifeProgress - SlowdownStartProgress) / (transitionPoint - SlowdownStartProgress);
					slowdownPhaseProgress = Mathf.Clamp01(slowdownPhaseProgress);

					float easedProgress = 1f - Mathf.Pow(1f - slowdownPhaseProgress, _slowdownSmoothness);
					float slowdownMultiplier = 1f - easedProgress;

					Vector3 originalDirection = _originalVelocities[i].normalized;
					float targetSpeed = _originalSpeeds[i] * slowdownMultiplier;

					_particles[i].velocity = originalDirection * targetSpeed;
				}
				else
				{
					float returnPhaseProgress = (lifeProgress - transitionPoint) / (1f - transitionPoint);
					returnPhaseProgress = Mathf.Clamp01(returnPhaseProgress);

					Vector3 currentPos = _particles[i].position;
					Vector3 targetPos = _calculatedInitialPositions[i];
					Vector3 directionToTarget = (targetPos - currentPos).normalized;

					float remainingReturnTime = _particles[i].remainingLifetime - _durationAfterReturn;

					if (remainingReturnTime > MinRemainingTime && returnPhaseProgress < ReturnProgressThreshold)
					{
						float easedProgress = Mathf.Pow(returnPhaseProgress, 4f / _returnAcceleration);

						float distance = Vector3.Distance(currentPos, targetPos);
						float baseSpeed = distance / remainingReturnTime;

						float accelerationMultiplier = 1f + easedProgress * _returnAcceleration;
						float finalSpeed = baseSpeed * accelerationMultiplier;

						_particles[i].velocity = directionToTarget * finalSpeed;
					}
					else
					{
						_particles[i].position = _calculatedInitialPositions[i];
						_particles[i].velocity = Vector3.zero;
						_particles[i].remainingLifetime = _durationAfterReturn;
						_hasReturned[i] = true;
					}
				}
			}
		}

		_particleSystem.SetParticles(_particles, count);
	}
}