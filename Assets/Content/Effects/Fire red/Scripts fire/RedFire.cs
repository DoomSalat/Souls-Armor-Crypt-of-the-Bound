using UnityEngine;
using System.Collections;

public class RedFire : MonoBehaviour
{
	[SerializeField] private RedFireAnimator _redFireAnimator;
	[SerializeField] private HitBox _hitBox;
	[Space]
	[SerializeField] private float _lifeTime = 10f;

	private bool _isEnded = false;
	private Coroutine _lifeTimeCoroutine;

	public event System.Action<RedFire> FireDestroyed;

	private void OnEnable()
	{
		StartLifeTime();

		_redFireAnimator.AnimationEnded += OnAnimationEnded;
		_hitBox.Hitted += OnTargetHitted;
	}

	private void OnDisable()
	{
		if (_lifeTimeCoroutine != null)
		{
			StopCoroutine(_lifeTimeCoroutine);
			_lifeTimeCoroutine = null;
		}

		_redFireAnimator.AnimationEnded -= OnAnimationEnded;
	}

	public void Initialize()
	{
		StartLifeTime();
		_hitBox.enabled = true;
	}

	private void StartLifeTime()
	{
		if (_lifeTimeCoroutine != null)
		{
			StopCoroutine(_lifeTimeCoroutine);
		}

		_isEnded = false;
		_lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
	}

	private IEnumerator LifeTimeCoroutine()
	{
		yield return new WaitForSeconds(_lifeTime);

		_isEnded = true;
		_hitBox.enabled = false;
		_redFireAnimator.Stop();
	}

	private void Die()
	{
		FireDestroyed?.Invoke(this);
	}

	private void OnAnimationEnded()
	{
		Die();
	}

	private void OnTargetHitted(Collider2D target, DamageData damageData)
	{
		if (_isEnded == false)
		{
			if (_lifeTimeCoroutine != null)
			{
				StopCoroutine(_lifeTimeCoroutine);
			}

			_isEnded = true;
			_hitBox.enabled = false;
			_redFireAnimator.Stop();
		}
	}
}
