using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class SoulAnimatorEvent : MonoBehaviour
{
	[SerializeField, Required] private ParticleReverseSystem _particleExplosion;
	[SerializeField, MinValue(0f)] private float _particleDeathEndDelay = 2f;

	private Coroutine _deathExplosionRoutine;
	private WaitForSeconds _waitParticleDeathEndDelay;

	public event System.Action DeathExplosionStarted;
	public event System.Action DeathExplosionEnded;

	private void Awake()
	{
		_waitParticleDeathEndDelay = new WaitForSeconds(_particleDeathEndDelay);
	}

	public void PlayDeathExplosion()
	{
		if (_deathExplosionRoutine != null)
		{
			StopCoroutine(_deathExplosionRoutine);
		}

		_deathExplosionRoutine = StartCoroutine(DeathExplosion());
	}

	public void Reset()
	{
		_deathExplosionRoutine = null;
	}

	private IEnumerator DeathExplosion()
	{
		DeathExplosionStarted?.Invoke();
		_particleExplosion.Play();
		yield return _waitParticleDeathEndDelay;

		DeathExplosionEnded?.Invoke();
	}
}
