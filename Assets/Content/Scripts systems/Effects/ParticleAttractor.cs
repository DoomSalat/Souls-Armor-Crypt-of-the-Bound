using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class ParticleAttractor : MonoBehaviour
{
	private const float SlowdownLerpFactor = 0.4f;
	private const float FinalStopProgress = 0.95f;
	private const float NewParticleLifetimeRatio = 0.98f;
	private const float MinDirectionMagnitude = 0.001f;
	private const float NoiseOffsetMultiplier = 0.1f;
	private const float FingerprintPositionMultiplier = 1000f;
	private const float FingerprintLifetimeMultiplier = 1000000f;
	private const uint FingerprintHashMultiplier = 31;

	// Timing Constants
	private const float DefaultUpdateInterval = 0.016f;
	private const float SlowdownCompletionThreshold = 0.7f;
	private const float TrailFadeMinTime = 0.02f;
	private const float ForcedTimeoutMultiplier = 0.5f;

	// Velocity Constants
	private const float ParticleStoppedThreshold = 0.01f;
	private const float SlowdownVelocityThreshold = 0.15f;

	// Progress Constants
	private const float TrailFadeProgressThreshold = 0.6f;

	private const float HighSpeedThreshold = 2.0f;

	// Mathematical Constants
	private const float Half = 0.5f;
	private const float RandomSeedMultiplier = 1000f;
	private const int QuadraticCoefficientMultiplier = 2;
	private const int QuadraticDiscriminantMultiplier = 4;
	private const float IntersectionParameterMin = 0f;
	private const float IntersectionParameterMax = 1f;

	// Noise Constants
	private const float NoiseCenter = 0.5f;

	// Gizmos Constants
	private const float GizmosCubeSize = 0.2f;
	private const float GizmosRayLength = 2f;

	// Common Values
	private const float InvalidTime = -1f;

	[Header("Attraction Settings")]
	[SerializeField] private Transform _target;
	[SerializeField, MinValue(0f)] private float _attractionStrength = 10f;
	[SerializeField, Range(0f, 1f)] private float _damping = 0.1f;
	[SerializeField] private bool _ignoreZAxis = false;

	[Header("Lifetime Behavior")]
	[SerializeField] private AnimationCurve _attractionOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	[SerializeField] private bool _useDistanceBasedAttraction = true;
	[SerializeField, ShowIf(nameof(_useDistanceBasedAttraction)), MinValue(0.01f)] private float _minDistance = 0.1f;

	[Header("Target Reach Behavior")]
	[SerializeField] private bool _stopWhenTargetReached = true;
	[SerializeField, ShowIf(nameof(_stopWhenTargetReached)), MinValue(0.01f)] private float _targetReachDistance = 0.5f;
	[SerializeField, ShowIf(nameof(_stopWhenTargetReached))] private bool _attractToCenter = true;
	[SerializeField] private bool _destroyOnTargetReach = false;
	[SerializeField, ShowIf("@_stopWhenTargetReached || _destroyOnTargetReach"), MinValue(0.01f)] private float _fadeOutTime = 0.8f;

	[Header("Trajectory Distortion")]
	[SerializeField] private bool _useTrajectoryDistortion = true;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0.1f, 5f)] private float _noiseScale = 1f;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0.1f, 3f)] private float _noiseIntensity = 1f;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0.1f, 2f)] private float _noiseSpeed = 1f;
	[SerializeField, ShowIf(nameof(_useTrajectoryDistortion)), Range(0f, 1f)] private float _trajectorySmoothing = Half;

	[Header("Optimization")]
	[SerializeField, MinValue(0.001f)] private float _updateInterval = DefaultUpdateInterval;

	private ParticleSystem _particleSystem;
	private ParticleSystem.Particle[] _particles;
	private float _lastUpdateTime = 0f;

	private ParticleData[] _particleData;

	[System.Flags]
	private enum ParticleState
	{
		None = 0,
		MarkedForRecycling = 1 << 0,
		InSlowdown = 1 << 1,
		WaitingForTrailFade = 1 << 2
	}

	private struct ParticleData
	{
		public ParticleState state;
		public uint fingerprint;
		public float slowdownStartTime;
		public Vector3 initialVelocity;
		public float trailFadeStartTime;
		public Vector3 noiseOffset;
		public float noiseSeed;
		public Vector3 previousPosition;

		public bool HasState(ParticleState flag) => (state & flag) == flag;
		public void AddState(ParticleState flag) => state |= flag;
		public void RemoveState(ParticleState flag) => state &= ~flag;
	}

	private void Awake()
	{
		_particleSystem = GetComponent<ParticleSystem>();

		if (_particleSystem.main.prewarm && Application.isPlaying)
		{
			PreWarmSystem();
		}
	}

	private void OnEnable()
	{
		if (_particleSystem != null && _particleSystem.main.prewarm && Application.isPlaying)
		{
			PreWarmSystem();
		}
	}

	private void OnValidate()
	{
		if (_particleSystem == null)
			_particleSystem = GetComponent<ParticleSystem>();
	}

	private void FixedUpdate()
	{
		if (Application.isPlaying)
		{
			UpdateParticles();
		}
	}

	private void LateUpdate()
	{
		if (Application.isEditor)
		{
			UpdateParticles();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_target == null)
			return;

		Gizmos.color = _destroyOnTargetReach ? Color.red : Color.yellow;
		Gizmos.DrawWireSphere(_target.position, _targetReachDistance);

		Gizmos.color = _useTrajectoryDistortion ? Color.cyan : Color.red;
		Gizmos.DrawLine(transform.position, _target.position);

		Vector3 direction = (_target.position - transform.position).normalized;
		Gizmos.DrawRay(transform.position, direction * GizmosRayLength);

		if (_destroyOnTargetReach)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube(_target.position, Vector3.one * GizmosCubeSize);
		}

		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(_target.position, _minDistance);
	}

	public void SetTarget(Transform target)
	{
		_target = target;
	}

	public void SetStrength(float strength)
	{
		_attractionStrength = strength;
	}

	public void SetReachDistance(float reachDistance)
	{
		_targetReachDistance = reachDistance;
	}

	public void SetFadeOutTime(float fadeOutTime)
	{
		_fadeOutTime = fadeOutTime;
	}

	private void UpdateParticles()
	{
		if (ShouldUpdate() == false)
			return;

		ApplyAttraction();
	}

	private float GetCurrentTime()
	{
		return Application.isPlaying ? Time.fixedTime : Time.unscaledTime;
	}

	private float GetDeltaTime()
	{
		return Application.isPlaying ? Time.fixedDeltaTime : Time.deltaTime;
	}

	private bool ShouldUpdate()
	{
		if (_particleSystem == null || _target == null)
			return false;

		if (Application.isPlaying && _particleSystem.particleCount == 0)
			return false;

		float currentTime = Time.unscaledTime;
		float deltaTime = currentTime - _lastUpdateTime;

		if (deltaTime < _updateInterval)
			return false;

		if (deltaTime > _updateInterval * 3f)
		{
			_lastUpdateTime = currentTime - _updateInterval;
		}
		else
		{
			_lastUpdateTime = currentTime;
		}

		return true;
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
			_particleData = new ParticleData[particleCount];

			InitializeParticleData(particleCount);
		}
	}

	private void InitializeParticleData(int particleCount)
	{
		for (int i = 0; i < particleCount; i++)
		{
			_particleData[i] = new ParticleData
			{
				state = ParticleState.None,
				fingerprint = 0,
				slowdownStartTime = InvalidTime,
				initialVelocity = Vector3.zero,
				trailFadeStartTime = InvalidTime,
				noiseOffset = Random.insideUnitSphere * _noiseScale,
				noiseSeed = Random.Range(0, RandomSeedMultiplier),
				previousPosition = Vector3.zero
			};
		}
	}

	private void CheckAndResetParticleState(ref ParticleSystem.Particle particle, int particleIndex)
	{
		if (particleIndex >= _particleData.Length)
			return;

		uint currentFingerprint = CreateParticleFingerprint(particle);

		if (_particleData[particleIndex].fingerprint != currentFingerprint)
		{
			bool wasMarkedForRecycling = _particleData[particleIndex].HasState(ParticleState.MarkedForRecycling);
			_particleData[particleIndex].fingerprint = currentFingerprint;

			float lifetimeRatio = particle.remainingLifetime / particle.startLifetime;
			bool isActuallyNewParticle = lifetimeRatio > NewParticleLifetimeRatio;

			if (isActuallyNewParticle || wasMarkedForRecycling)
			{
				ResetParticleStates(particleIndex);
			}
		}
	}

	private void ProcessParticle(ref ParticleSystem.Particle particle, Vector3 targetPosition, int particleIndex)
	{
		CheckAndResetParticleState(ref particle, particleIndex);

		Vector3 particlePosition = GetParticleWorldPosition(particle);
		Vector3 directionToTarget = targetPosition - particlePosition;

		Vector3 distanceVector = directionToTarget;
		if (_ignoreZAxis)
		{
			distanceVector.z = 0f;
		}

		float distanceToTarget = distanceVector.magnitude;

		bool isInTargetZone = IsParticleInTargetZone(ref particle, targetPosition, distanceToTarget, particleIndex);

		HandleTargetZoneLogic(ref particle, isInTargetZone, particleIndex);
		HandleSlowdownLogic(ref particle, targetPosition, particleIndex);
		HandleTrailFadeLogic(ref particle, particleIndex);

		if (particleIndex < _particleData.Length)
		{
			_particleData[particleIndex].previousPosition = particlePosition;
		}

		if (ShouldSkipAttraction(particleIndex))
			return;

		if (distanceToTarget < _minDistance)
			return;

		ApplyAttractionForce(ref particle, directionToTarget, distanceToTarget, particleIndex);
	}

	private Vector3 GetParticleWorldPosition(ParticleSystem.Particle particle)
	{
		var main = _particleSystem.main;
		if (main.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			return transform.TransformPoint(particle.position);
		}
		return particle.position;
	}

	private bool IsParticleInTargetZone(ref ParticleSystem.Particle particle, Vector3 targetPosition, float distanceToTarget, int particleIndex)
	{
		if (distanceToTarget <= _targetReachDistance)
			return true;

		if (particleIndex < _particleData.Length && particle.velocity.magnitude > HighSpeedThreshold)
		{
			ref var data = ref _particleData[particleIndex];

			if (data.previousPosition != Vector3.zero)
			{
				Vector3 lineStart = data.previousPosition;
				Vector3 lineEnd = particle.position;
				Vector3 sphereCenter = targetPosition;

				if (_ignoreZAxis)
				{
					lineStart.z = sphereCenter.z;
					lineEnd.z = sphereCenter.z;
				}

				if (LineIntersectsSphere(lineStart, lineEnd, sphereCenter, _targetReachDistance))
				{
					return true;
				}
			}
		}

		return false;
	}

	private bool LineIntersectsSphere(Vector3 lineStart, Vector3 lineEnd, Vector3 sphereCenter, float sphereRadius)
	{
		Vector3 rayDirection = lineEnd - lineStart;
		Vector3 sphereToLineStart = lineStart - sphereCenter;

		float quadraticA = Vector3.Dot(rayDirection, rayDirection);
		float quadraticB = QuadraticCoefficientMultiplier * Vector3.Dot(sphereToLineStart, rayDirection);
		float quadraticC = Vector3.Dot(sphereToLineStart, sphereToLineStart) - sphereRadius * sphereRadius;

		float discriminant = quadraticB * quadraticB - QuadraticDiscriminantMultiplier * quadraticA * quadraticC;

		if (discriminant < 0)
			return false;

		discriminant = Mathf.Sqrt(discriminant);
		float intersectionParam1 = (-quadraticB - discriminant) / (QuadraticCoefficientMultiplier * quadraticA);
		float intersectionParam2 = (-quadraticB + discriminant) / (QuadraticCoefficientMultiplier * quadraticA);

		return (intersectionParam1 >= IntersectionParameterMin && intersectionParam1 <= IntersectionParameterMax) ||
			   (intersectionParam2 >= IntersectionParameterMin && intersectionParam2 <= IntersectionParameterMax) ||
			   (intersectionParam1 < IntersectionParameterMin && intersectionParam2 > IntersectionParameterMax);
	}

	private void HandleTargetZoneLogic(ref ParticleSystem.Particle particle, bool isInTargetZone, int particleIndex)
	{
		if (!isInTargetZone || particleIndex >= _particleData.Length)
			return;

		ref var data = ref _particleData[particleIndex];
		bool alreadyMarkedForRecycling = data.HasState(ParticleState.MarkedForRecycling);

		if (_destroyOnTargetReach && !alreadyMarkedForRecycling)
		{
			data.AddState(ParticleState.MarkedForRecycling);
			particle.remainingLifetime = Mathf.Min(particle.remainingLifetime, _fadeOutTime);
		}

		if (_stopWhenTargetReached && !alreadyMarkedForRecycling)
		{
			StartSlowdownIfNeeded(ref particle, particleIndex);
			data.AddState(ParticleState.MarkedForRecycling);
		}
	}

	private void StartSlowdownIfNeeded(ref ParticleSystem.Particle particle, int particleIndex)
	{
		ref var data = ref _particleData[particleIndex];

		if (data.HasState(ParticleState.InSlowdown))
			return;

		particle.remainingLifetime = Mathf.Min(particle.remainingLifetime, _fadeOutTime);
		data.AddState(ParticleState.InSlowdown);
	}

	private void HandleSlowdownLogic(ref ParticleSystem.Particle particle, Vector3 targetPosition, int particleIndex)
	{
		if (particleIndex >= _particleData.Length)
			return;

		ref var data = ref _particleData[particleIndex];

		if (!data.HasState(ParticleState.InSlowdown))
			return;

		ApplySlowdownToParticle(ref particle, targetPosition, particleIndex);
		CheckSlowdownCompletion(ref particle, particleIndex);
	}

	private void CheckSlowdownCompletion(ref ParticleSystem.Particle particle, int particleIndex)
	{
		ref var data = ref _particleData[particleIndex];

		if (data.slowdownStartTime < 0)
			return;

		float elapsedTime = GetCurrentTime() - data.slowdownStartTime;
		bool timeElapsed = elapsedTime >= (_fadeOutTime * SlowdownCompletionThreshold);
		bool particleSlowed = particle.velocity.magnitude < SlowdownVelocityThreshold;

		if ((timeElapsed && particleSlowed) || elapsedTime >= _fadeOutTime)
		{
			if (!data.HasState(ParticleState.WaitingForTrailFade))
			{
				StartTrailFadeWaiting(particleIndex);
			}
		}
	}

	private bool ShouldSkipAttraction(int particleIndex)
	{
		if (particleIndex >= _particleData.Length)
			return false;

		var data = _particleData[particleIndex];
		return data.HasState(ParticleState.MarkedForRecycling) ||
			   data.HasState(ParticleState.InSlowdown) ||
			   data.HasState(ParticleState.WaitingForTrailFade);
	}

	private void ApplyAttractionForce(ref ParticleSystem.Particle particle, Vector3 directionToTarget, float distanceToTarget, int particleIndex)
	{
		Vector3 baseDirection = directionToTarget / distanceToTarget;

		if (_ignoreZAxis)
		{
			baseDirection.z = 0f;
			baseDirection = baseDirection.normalized;
		}

		Vector3 finalDirection = ApplyTrajectoryDistortion(baseDirection, particle, particleIndex);
		float attractionForce = CalculateAttractionForce(particle, distanceToTarget);
		float safeDeltaTime = Mathf.Min(GetDeltaTime(), 0.05f);
		Vector3 acceleration = finalDirection * attractionForce * safeDeltaTime;

		var main = _particleSystem.main;
		if (main.simulationSpace == ParticleSystemSimulationSpace.Local)
		{
			acceleration = transform.InverseTransformDirection(acceleration);
		}

		float frameIndependentDamping = 1f - Mathf.Pow(_damping, safeDeltaTime / DefaultUpdateInterval);
		particle.velocity = Vector3.Lerp(particle.velocity, particle.velocity + acceleration, frameIndependentDamping);
	}

	private void ApplySlowdownToParticle(ref ParticleSystem.Particle particle, Vector3 targetPosition, int particleIndex)
	{
		ref var data = ref _particleData[particleIndex];

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
		ref var data = ref _particleData[particleIndex];

		if (data.slowdownStartTime > 0)
			return;

		data.slowdownStartTime = GetCurrentTime();
		data.initialVelocity = particle.velocity;
	}

	private float CalculateSlowdownProgress(int particleIndex)
	{
		var data = _particleData[particleIndex];
		float elapsedTime = GetCurrentTime() - data.slowdownStartTime;
		return Mathf.Clamp01(elapsedTime / _fadeOutTime);
	}

	private void ApplyProgressiveSlowdown(ref ParticleSystem.Particle particle, Vector3 directionToTarget, float slowdownProgress, int particleIndex)
	{
		ref var data = ref _particleData[particleIndex];

		if (_attractToCenter)
		{
			if (_ignoreZAxis)
			{
				directionToTarget.z = 0f;
				directionToTarget = directionToTarget.normalized;
			}

			float targetSpeed = data.initialVelocity.magnitude * (1 - slowdownProgress);
			Vector3 targetVelocity = directionToTarget * targetSpeed;
			float frameIndependentLerpFactor = 1f - Mathf.Pow(1f - SlowdownLerpFactor, GetDeltaTime() / DefaultUpdateInterval);
			particle.velocity = Vector3.Lerp(particle.velocity, targetVelocity, frameIndependentLerpFactor);
		}
		else
		{
			particle.velocity = Vector3.zero;
		}

		if (_attractToCenter && slowdownProgress >= FinalStopProgress)
		{
			particle.velocity = Vector3.zero;
		}

		if (particle.velocity.magnitude < SlowdownVelocityThreshold && slowdownProgress > TrailFadeProgressThreshold)
		{
			if (!data.HasState(ParticleState.WaitingForTrailFade))
			{
				StartTrailFadeWaiting(particleIndex);
			}
		}
	}

	private float CalculateAttractionForce(ParticleSystem.Particle particle, float distance)
	{
		float normalizedLifetime = 1 - (particle.remainingLifetime / particle.startLifetime);
		float lifetimeMultiplier = _attractionOverLifetime.Evaluate(normalizedLifetime);
		float baseForce = _attractionStrength * lifetimeMultiplier;

		if (_useDistanceBasedAttraction)
		{
			float distanceMultiplier = 1 / (distance * distance + 1);
			return baseForce * distanceMultiplier;
		}

		return baseForce;
	}

	private Vector3 ApplyTrajectoryDistortion(Vector3 baseDirection, ParticleSystem.Particle particle, int particleIndex)
	{
		if (!_useTrajectoryDistortion || particleIndex >= _particleData.Length)
			return baseDirection;

		var data = _particleData[particleIndex];
		float smoothTime = GetCurrentTime() * _noiseSpeed + data.noiseSeed;

		float noiseX = Mathf.PerlinNoise(smoothTime, 0) - NoiseCenter;
		float noiseY = Mathf.PerlinNoise(0, smoothTime) - NoiseCenter;
		float noiseZ = Mathf.PerlinNoise(smoothTime, smoothTime) - NoiseCenter;

		Vector3 noiseDirection = new Vector3(noiseX, noiseY, noiseZ) * _noiseIntensity;
		Vector3 personalOffset = data.noiseOffset * NoiseOffsetMultiplier;
		Vector3 distortedDirection = baseDirection + noiseDirection + personalOffset;
		Vector3 finalDirection = Vector3.Lerp(baseDirection, distortedDirection.normalized, _trajectorySmoothing);

		return finalDirection.normalized;
	}

	private void CheckForDeadParticlesAndResetStates()
	{
		if (_particleData == null || _particles == null)
			return;

		int currentParticleCount = _particleSystem.particleCount;
		ResizeArraysIfNeeded(currentParticleCount);

		bool needsUpdate = false;
		for (int i = 0; i < currentParticleCount && i < _particles.Length; i++)
		{
			if (ProcessParticleFingerprint(i, out bool wasMarkedForRecycling))
			{
				if (wasMarkedForRecycling && !_particleData[i].HasState(ParticleState.MarkedForRecycling))
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
		if (_particleData.Length != currentParticleCount)
		{
			_particleData = new ParticleData[currentParticleCount];
			InitializeResizedArrays(currentParticleCount);
		}
	}

	private void InitializeResizedArrays(int currentParticleCount)
	{
		for (int j = 0; j < currentParticleCount; j++)
		{
			_particleData[j] = new ParticleData
			{
				state = ParticleState.None,
				fingerprint = 0,
				slowdownStartTime = InvalidTime,
				initialVelocity = Vector3.zero,
				trailFadeStartTime = InvalidTime,
				noiseOffset = Random.insideUnitSphere * _noiseScale,
				noiseSeed = Random.Range(0, RandomSeedMultiplier),
				previousPosition = Vector3.zero
			};
		}
	}

	private bool ProcessParticleFingerprint(int i, out bool wasMarkedForRecycling)
	{
		uint currentFingerprint = CreateParticleFingerprint(_particles[i]);
		wasMarkedForRecycling = _particleData[i].HasState(ParticleState.MarkedForRecycling);

		if (_particleData[i].fingerprint == currentFingerprint)
			return false;

		_particleData[i].fingerprint = currentFingerprint;
		_particleData[i].state = ParticleState.None;

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
		_particleData[i] = new ParticleData
		{
			state = ParticleState.None,
			fingerprint = _particleData[i].fingerprint,
			slowdownStartTime = InvalidTime,
			initialVelocity = Vector3.zero,
			trailFadeStartTime = InvalidTime,
			noiseOffset = _particleData[i].noiseOffset,
			noiseSeed = _particleData[i].noiseSeed,
			previousPosition = Vector3.zero
		};
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

	private void HandleTrailFadeLogic(ref ParticleSystem.Particle particle, int particleIndex)
	{
		if (particleIndex >= _particleData.Length)
			return;

		ref var data = ref _particleData[particleIndex];

		if (!data.HasState(ParticleState.WaitingForTrailFade))
			return;

		if (data.trailFadeStartTime > 0)
		{
			bool particleCompletelyStoppped = particle.velocity.magnitude < ParticleStoppedThreshold;
			float waitTime = GetCurrentTime() - data.trailFadeStartTime;
			bool forcedTimeout = waitTime > (_fadeOutTime * ForcedTimeoutMultiplier);

			if (IsTrailReadyForRecycling(particleIndex) || (particleCompletelyStoppped && forcedTimeout))
			{
				RecycleParticleImmediately(ref particle, particleIndex);
				data.RemoveState(ParticleState.WaitingForTrailFade);
			}
		}
	}

	private void StartTrailFadeWaiting(int particleIndex)
	{
		if (particleIndex >= _particleData.Length)
			return;

		ref var data = ref _particleData[particleIndex];
		data.AddState(ParticleState.WaitingForTrailFade);
		data.trailFadeStartTime = GetCurrentTime();

		if (particleIndex < _particles.Length)
		{
			_particles[particleIndex].velocity = Vector3.zero;
		}
	}

	private bool IsTrailReadyForRecycling(int particleIndex)
	{
		var trails = _particleSystem.trails;
		if (!trails.enabled)
		{
			return true;
		}

		var data = _particleData[particleIndex];
		float elapsedTime = GetCurrentTime() - data.trailFadeStartTime;
		bool particleStopped = _particles[particleIndex].velocity.magnitude < ParticleStoppedThreshold;
		bool minTimeElapsed = elapsedTime >= TrailFadeMinTime;

		return particleStopped && minTimeElapsed;
	}

	private void RecycleParticleImmediately(ref ParticleSystem.Particle particle, int particleIndex)
	{
		particle.remainingLifetime = 0;
		particle.velocity = Vector3.zero;
		ResetParticleStates(particleIndex);
	}

	private void PreWarmSystem()
	{
		if (_particleSystem == null || _target == null)
			return;

		float originalSimulationTime = _particleSystem.main.duration;
		float simulationStep = _updateInterval;

		for (float time = 0; time < originalSimulationTime; time += simulationStep)
		{
			_particleSystem.Simulate(simulationStep, true, false, true);
			ApplyAttraction();
		}

		_particleSystem.Simulate(0, true, false, true);
		_particleSystem.Play();
	}
}