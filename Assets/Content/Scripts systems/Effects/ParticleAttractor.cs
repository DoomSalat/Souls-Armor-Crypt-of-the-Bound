using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor : MonoBehaviour
{
	private const float MinVelocityThreshold = 0.2f;
	private const float SlowdownLerpFactor = 0.4f;
	private const float FinalStopProgress = 0.95f;
	private const float NewParticleLifetimeRatio = 0.98f;
	private const float MinDirectionMagnitude = 0.001f;
	private const float NoiseOffsetMultiplier = 0.1f;
	private const float FingerprintPositionMultiplier = 1000f;
	private const float FingerprintLifetimeMultiplier = 1000000f;
	private const uint FingerprintHashMultiplier = 31;

	[Title("Attraction Settings")]
	[SerializeField, Required] private Transform _target;
	[SerializeField, MinValue(0f)] private float _attractionStrength = 10f;
	[SerializeField, Range(0f, 1f)] private float _damping = 0.1f;

	[Title("Lifetime Behavior")]
	[SerializeField] private AnimationCurve _attractionOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	[SerializeField] private bool _useDistanceBasedAttraction = true;
	[SerializeField, ShowIf(nameof(_useDistanceBasedAttraction)), MinValue(0.01f)] private float _minDistance = 0.1f;

	[Title("Target Reach Behavior")]
	[SerializeField] private bool _stopWhenTargetReached = true;
	[SerializeField, ShowIf(nameof(_stopWhenTargetReached)), MinValue(0.01f)] private float _targetReachDistance = 0.5f;
	[SerializeField, ShowIf(nameof(_stopWhenTargetReached))] private bool _attractToCenter = true;
	[SerializeField] private bool _destroyOnTargetReach = false;
	[SerializeField, ShowIf("@_stopWhenTargetReached || _destroyOnTargetReach"), MinValue(0.01f)] private float _fadeOutTime = 0.2f;

	[Title("Trajectory Distortion")]
	[SerializeField] private bool _useTrajectoryDistortion = true;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0.1f, 5f)] private float _noiseScale = 1f;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0.1f, 3f)] private float _noiseIntensity = 1f;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0.1f, 2f)] private float _noiseSpeed = 1f;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0f, 1f)] private float _trajectorySmoothing = 0.5f;

	[Title("Optimization")]
	[SerializeField, MinValue(1)] private int _updateFrequency = 1;

	private ParticleSystem _particleSystem;
	private ParticleSystem.Particle[] _particles;
	private int _frameCounter = 0;

	private Vector3[] _particleNoiseOffsets;
	private float[] _particleNoiseSeeds;
	private bool[] _particlesInDestructionState;
	private uint[] _particleFingerprints;
	private bool[] _particlesInSlowdownState;
	private float[] _particleSlowdownStartTime;
	private Vector3[] _particleInitialVelocity;
	private bool[] _particlesFullyStopped;

	public Transform Target
	{
		get => _target;
		set => _target = value;
	}

	public float AttractionStrength
	{
		get => _attractionStrength;
		set => _attractionStrength = Mathf.Max(0f, value);
	}

	public bool DestroyOnTargetReach
	{
		get => _destroyOnTargetReach;
		set => _destroyOnTargetReach = value;
	}

	public bool UseTrajectoryDistortion
	{
		get => _useTrajectoryDistortion;
		set => _useTrajectoryDistortion = value;
	}

	public float NoiseIntensity
	{
		get => _noiseIntensity;
		set => _noiseIntensity = Mathf.Clamp(value, 0.1f, 3f);
	}

	public bool AttractToCenter
	{
		get => _attractToCenter;
		set => _attractToCenter = value;
	}

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		ConfigureParticleSystem();
	}

	private void OnValidate()
	{
		if (_particleSystem == null)
			_particleSystem = GetComponent<ParticleSystem>();
	}

	private void LateUpdate()
	{
		if (!ShouldUpdate())
			return;

		ApplyAttraction();
	}

	private void ConfigureParticleSystem()
	{
		if (_particleSystem != null)
		{
			var main = _particleSystem.main;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
		}
	}

	private bool ShouldUpdate()
	{
		if (_particleSystem == null || _target == null)
			return false;

		if (!Application.isPlaying && !_particleSystem.isPlaying)
			return false;

		_frameCounter++;
		if (_frameCounter < _updateFrequency)
			return false;

		_frameCounter = 0;
		return _particleSystem.particleCount > 0;
	}

	private void ApplyAttraction()
	{
		int particleCount = _particleSystem.particleCount;
		EnsureArrayCapacity(particleCount);

		_particleSystem.GetParticles(_particles, particleCount);
		Vector3 targetPosition = _target.position;

		for (int i = 0; i < particleCount; i++)
		{
			ProcessParticle(ref _particles[i], targetPosition, i);
		}

		_particleSystem.SetParticles(_particles, particleCount);
		CheckForDeadParticlesAndResetStates();
	}

	private void EnsureArrayCapacity(int particleCount)
	{
		if (_particles == null || _particles.Length < particleCount)
		{
			_particles = new ParticleSystem.Particle[particleCount];
			_particleNoiseOffsets = new Vector3[particleCount];
			_particleNoiseSeeds = new float[particleCount];
			_particlesInDestructionState = new bool[particleCount];
			_particleFingerprints = new uint[particleCount];
			_particlesInSlowdownState = new bool[particleCount];
			_particleSlowdownStartTime = new float[particleCount];
			_particleInitialVelocity = new Vector3[particleCount];
			_particlesFullyStopped = new bool[particleCount];

			InitializeParticleArrays(particleCount);
		}
	}

	private void InitializeParticleArrays(int particleCount)
	{
		for (int i = 0; i < particleCount; i++)
		{
			_particleNoiseSeeds[i] = Random.Range(0f, 1000f);
			_particleNoiseOffsets[i] = Random.insideUnitSphere * _noiseScale;
			_particlesInDestructionState[i] = false;
			_particleFingerprints[i] = 0;
			_particlesInSlowdownState[i] = false;
			_particleSlowdownStartTime[i] = -1f;
			_particleInitialVelocity[i] = Vector3.zero;
			_particlesFullyStopped[i] = false;
		}
	}

	private void ProcessParticle(ref ParticleSystem.Particle particle, Vector3 targetPosition, int particleIndex)
	{
		Vector3 directionToTarget = targetPosition - particle.position;
		float distanceToTarget = directionToTarget.magnitude;
		bool isInTargetZone = distanceToTarget <= _targetReachDistance;

		HandleTargetZoneLogic(ref particle, isInTargetZone, particleIndex);
		HandleSlowdownLogic(ref particle, targetPosition, particleIndex);
		HandleDestructionLogic(ref particle, particleIndex);

		if (ShouldSkipAttraction(particleIndex))
			return;

		if (distanceToTarget < _minDistance)
			return;

		ApplyAttractionForce(ref particle, directionToTarget, distanceToTarget, particleIndex);
	}

	private void HandleTargetZoneLogic(ref ParticleSystem.Particle particle, bool isInTargetZone, int particleIndex)
	{
		if (!isInTargetZone || particleIndex >= _particlesInDestructionState.Length)
			return;

		if (_destroyOnTargetReach)
		{
			_particlesInDestructionState[particleIndex] = true;
		}

		if (_stopWhenTargetReached && particleIndex < _particlesInSlowdownState.Length)
		{
			StartSlowdownIfNeeded(ref particle, particleIndex);
		}
	}

	private void StartSlowdownIfNeeded(ref ParticleSystem.Particle particle, int particleIndex)
	{
		if (_particlesInSlowdownState[particleIndex])
			return;

		particle.remainingLifetime = _fadeOutTime;
		_particlesInSlowdownState[particleIndex] = true;
	}

	private void HandleSlowdownLogic(ref ParticleSystem.Particle particle, Vector3 targetPosition, int particleIndex)
	{
		if (particleIndex >= _particlesInSlowdownState.Length || !_particlesInSlowdownState[particleIndex])
			return;

		ApplySlowdownToParticle(ref particle, targetPosition, particleIndex);
		CheckSlowdownCompletion(ref particle, particleIndex);
	}

	private void CheckSlowdownCompletion(ref ParticleSystem.Particle particle, int particleIndex)
	{
		if (particleIndex >= _particleSlowdownStartTime.Length || _particleSlowdownStartTime[particleIndex] < 0f)
			return;

		float elapsedTime = Time.fixedTime - _particleSlowdownStartTime[particleIndex];
		if (elapsedTime >= _fadeOutTime)
		{
			RemoveParticle(ref particle, particleIndex);
		}
	}

	private void RemoveParticle(ref ParticleSystem.Particle particle, int particleIndex)
	{
		particle.remainingLifetime = 0f;
		particle.velocity = Vector3.zero;
		_particlesFullyStopped[particleIndex] = true;
		_particleSystem.SetParticles(_particles, _particleSystem.particleCount);
	}

	private void HandleDestructionLogic(ref ParticleSystem.Particle particle, int particleIndex)
	{
		if (particleIndex >= _particlesInDestructionState.Length || !_particlesInDestructionState[particleIndex])
			return;

		if (_destroyOnTargetReach)
		{
			particle.remainingLifetime = Mathf.Min(particle.remainingLifetime, _fadeOutTime);
		}
	}

	private bool ShouldSkipAttraction(int particleIndex)
	{
		bool inSlowdown = particleIndex < _particlesInSlowdownState.Length && _particlesInSlowdownState[particleIndex];
		bool inDestruction = particleIndex < _particlesInDestructionState.Length && _particlesInDestructionState[particleIndex];
		bool fullyStopped = particleIndex < _particlesFullyStopped.Length && _particlesFullyStopped[particleIndex];

		if (fullyStopped && particleIndex < _particlesFullyStopped.Length)
		{
			_particles[particleIndex].velocity = Vector3.zero;
		}

		return inSlowdown || inDestruction || fullyStopped;
	}

	private void ApplyAttractionForce(ref ParticleSystem.Particle particle, Vector3 directionToTarget, float distanceToTarget, int particleIndex)
	{
		Vector3 baseDirection = directionToTarget / distanceToTarget;
		Vector3 finalDirection = ApplyTrajectoryDistortion(baseDirection, particle, particleIndex);
		float attractionForce = CalculateAttractionForce(particle, distanceToTarget);
		Vector3 acceleration = finalDirection * attractionForce * Time.deltaTime;
		particle.velocity = Vector3.Lerp(particle.velocity, particle.velocity + acceleration, 1f - _damping);
	}

	private void ApplySlowdownToParticle(ref ParticleSystem.Particle particle, Vector3 targetPosition, int particleIndex)
	{
		if (particleIndex >= _particleSlowdownStartTime.Length || particleIndex >= _particleInitialVelocity.Length)
			return;

		InitializeSlowdownIfNeeded(particle, particleIndex);

		float slowdownProgress = CalculateSlowdownProgress(particleIndex);
		Vector3 directionToTarget = (targetPosition - particle.position);

		if (directionToTarget.magnitude > MinDirectionMagnitude)
		{
			directionToTarget = directionToTarget.normalized;
		}
		else
		{
			directionToTarget = Vector3.up;
		}

		ApplyProgressiveSlowdown(ref particle, directionToTarget, slowdownProgress, particleIndex);
	}

	private void InitializeSlowdownIfNeeded(ParticleSystem.Particle particle, int particleIndex)
	{
		if (_particleSlowdownStartTime[particleIndex] > 0f)
			return;

		_particleSlowdownStartTime[particleIndex] = Time.fixedTime;
		_particleInitialVelocity[particleIndex] = particle.velocity;
	}

	private float CalculateSlowdownProgress(int particleIndex)
	{
		float elapsedTime = Time.fixedTime - _particleSlowdownStartTime[particleIndex];
		return Mathf.Clamp01(elapsedTime / _fadeOutTime);
	}

	private void ApplyProgressiveSlowdown(ref ParticleSystem.Particle particle, Vector3 directionToTarget, float slowdownProgress, int particleIndex)
	{
		if (_attractToCenter)
		{
			float targetSpeed = _particleInitialVelocity[particleIndex].magnitude * (1f - slowdownProgress);
			Vector3 targetVelocity = directionToTarget * targetSpeed;
			particle.velocity = Vector3.Lerp(particle.velocity, targetVelocity, SlowdownLerpFactor);
		}
		else
		{
			particle.velocity = Vector3.zero;
		}

		if (_attractToCenter && slowdownProgress >= FinalStopProgress)
		{
			particle.velocity = Vector3.zero;
		}
	}

	private float CalculateAttractionForce(ParticleSystem.Particle particle, float distance)
	{
		float normalizedLifetime = 1f - (particle.remainingLifetime / particle.startLifetime);
		float lifetimeMultiplier = _attractionOverLifetime.Evaluate(normalizedLifetime);
		float baseForce = _attractionStrength * lifetimeMultiplier;

		if (_useDistanceBasedAttraction)
		{
			float distanceMultiplier = 1f / (distance * distance + 1f);
			return baseForce * distanceMultiplier;
		}

		return baseForce;
	}

	private Vector3 ApplyTrajectoryDistortion(Vector3 baseDirection, ParticleSystem.Particle particle, int particleIndex)
	{
		if (!_useTrajectoryDistortion || particleIndex >= _particleNoiseSeeds.Length)
			return baseDirection;

		float particleTime = Time.time * _noiseSpeed + _particleNoiseSeeds[particleIndex];

		float noiseX = Mathf.PerlinNoise(particleTime, 0f) - 0.5f;
		float noiseY = Mathf.PerlinNoise(0f, particleTime) - 0.5f;
		float noiseZ = Mathf.PerlinNoise(particleTime, particleTime) - 0.5f;

		Vector3 noiseDirection = new Vector3(noiseX, noiseY, noiseZ) * _noiseIntensity;
		Vector3 personalOffset = _particleNoiseOffsets[particleIndex] * NoiseOffsetMultiplier;
		Vector3 distortedDirection = baseDirection + noiseDirection + personalOffset;
		Vector3 finalDirection = Vector3.Lerp(baseDirection, distortedDirection.normalized, _trajectorySmoothing);

		return finalDirection.normalized;
	}

	private void CheckForDeadParticlesAndResetStates()
	{
		if (_particlesInDestructionState == null || _particles == null || _particleFingerprints == null)
			return;

		int currentParticleCount = _particleSystem.particleCount;
		ResizeArraysIfNeeded(currentParticleCount);

		bool needsUpdate = false;
		for (int i = 0; i < currentParticleCount && i < _particles.Length; i++)
		{
			if (ProcessParticleFingerprint(i, out bool wasInDestruction))
			{
				if (wasInDestruction)
				{
					_particles[i].remainingLifetime = _particles[i].startLifetime;
					needsUpdate = true;
				}
			}
		}

		if (needsUpdate)
		{
			_particleSystem.SetParticles(_particles, currentParticleCount);
		}
	}

	private void ResizeArraysIfNeeded(int currentParticleCount)
	{
		if (_particlesInDestructionState.Length != currentParticleCount)
		{
			_particlesInDestructionState = new bool[currentParticleCount];
			_particleFingerprints = new uint[currentParticleCount];
			_particlesInSlowdownState = new bool[currentParticleCount];
			_particleSlowdownStartTime = new float[currentParticleCount];
			_particleInitialVelocity = new Vector3[currentParticleCount];
			_particlesFullyStopped = new bool[currentParticleCount];

			InitializeResizedArrays(currentParticleCount);
		}
	}

	private void InitializeResizedArrays(int currentParticleCount)
	{
		for (int j = 0; j < currentParticleCount; j++)
		{
			_particlesInSlowdownState[j] = false;
			_particleSlowdownStartTime[j] = -1f;
			_particleInitialVelocity[j] = Vector3.zero;
			_particlesFullyStopped[j] = false;
		}
	}

	private bool ProcessParticleFingerprint(int i, out bool wasInDestruction)
	{
		uint currentFingerprint = CreateParticleFingerprint(_particles[i]);
		wasInDestruction = _particlesInDestructionState[i];

		if (_particleFingerprints[i] == currentFingerprint)
			return false;

		_particleFingerprints[i] = currentFingerprint;
		_particlesInDestructionState[i] = false;

		float lifetimeRatio = _particles[i].remainingLifetime / _particles[i].startLifetime;
		bool isActuallyNewParticle = lifetimeRatio > NewParticleLifetimeRatio;

		if (isActuallyNewParticle)
		{
			ResetParticleStates(i);
		}

		return true;
	}

	private void ResetParticleStates(int i)
	{
		if (i < _particlesInSlowdownState.Length)
		{
			_particlesInSlowdownState[i] = false;
		}
		if (i < _particleSlowdownStartTime.Length)
		{
			_particleSlowdownStartTime[i] = -1f;
			_particleInitialVelocity[i] = Vector3.zero;
		}
		if (i < _particlesFullyStopped.Length)
		{
			_particlesFullyStopped[i] = false;
		}
	}

	private uint CreateParticleFingerprint(ParticleSystem.Particle particle)
	{
		uint hash = (uint)(particle.startLifetime * FingerprintLifetimeMultiplier);
		hash = hash * FingerprintHashMultiplier + (uint)(particle.randomSeed * FingerprintPositionMultiplier);
		hash = hash * FingerprintHashMultiplier + (uint)(particle.position.x * FingerprintPositionMultiplier);
		hash = hash * FingerprintHashMultiplier + (uint)(particle.position.y * FingerprintPositionMultiplier);
		hash = hash * FingerprintHashMultiplier + (uint)(particle.position.z * FingerprintPositionMultiplier);
		return hash;
	}

	private void OnDrawGizmos()
	{
		if (_target == null)
			return;

		Gizmos.color = _destroyOnTargetReach ? Color.red : Color.yellow;
		Gizmos.DrawWireSphere(_target.position, _targetReachDistance);

		Gizmos.color = _useTrajectoryDistortion ? Color.cyan : Color.red;
		Gizmos.DrawLine(transform.position, _target.position);

		Vector3 direction = (_target.position - transform.position).normalized;
		Gizmos.DrawRay(transform.position, direction * 2f);

		if (_destroyOnTargetReach)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(_target.position, Vector3.one * 0.2f);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_target == null)
			return;

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(_target.position, _minDistance);
	}
}