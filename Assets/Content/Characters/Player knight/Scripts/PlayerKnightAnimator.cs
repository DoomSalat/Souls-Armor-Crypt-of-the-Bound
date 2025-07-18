using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerKnightAnimator : MonoBehaviour
{
	private const float ShortMoveDuration = 0.005f;
	private const float NormalSpeed = 1.0f;
	private const int DefaultDirection = 1;
	private const int AbsorptionCaptureIndex = 1;

	private const int DirectionDown = 1;
	private const int DirectionUp = 2;
	private const int DirectionLeft = 3;
	private const int DirectionRight = 4;

	[SerializeField] private PlayerKnightAnimatorEvents _events;

	private Animator _animator;

	private Coroutine _shortMoveRoutine;
	private WaitForSecondsRealtime _shortMoveWait;

	public event System.Action AbdorptionAnimationEnded;

	public bool IsStepMove { get; private set; }

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_shortMoveWait = new WaitForSecondsRealtime(ShortMoveDuration);
	}

	public void SetDirection(int direction)
	{
		_animator.SetFloat(PlayerKnightAnimatorData.Params.direction, direction);
	}

	public int GetDirection()
	{
		return Mathf.RoundToInt(_animator.GetFloat(PlayerKnightAnimatorData.Params.direction));
	}

	public int GetDirectionIndex(Vector2 direction)
	{
		if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
		{
			return direction.y > 0 ? DirectionUp : DirectionDown;
		}
		else
		{
			return direction.x > 0 ? DirectionRight : DirectionLeft;
		}
	}

	public void SetMove(bool isMove)
	{
		if (_animator.GetBool(PlayerKnightAnimatorData.Params.isMove) == isMove)
			return;

		_animator.SetBool(PlayerKnightAnimatorData.Params.isMove, isMove);

		if (isMove == false)
		{
			DisallowStepMove();
		}
	}

	public void PlayShortMove()
	{
		if (_shortMoveRoutine != null)
		{
			StopCoroutine(_shortMoveRoutine);
		}

		_shortMoveRoutine = StartCoroutine(PlayShortMoveCoroutine());
	}

	public void SetCapture(bool isCapture)
	{
		_animator.SetBool(PlayerKnightAnimatorData.Params.isCapture, isCapture);
	}

	public void AbdorptionActive()
	{
		SetDirection(DefaultDirection);
		SetMove(false);

		_animator.SetTrigger(PlayerKnightAnimatorData.Params.abdorptionActive);

		_events.SwitchMainAbsorptionTo(0);
		_events.DeactivateAbsorptionAttractor();
		_events.PlayAbsorptionBody();
	}

	public void PlayHeaded()
	{
		_animator.SetTrigger(PlayerKnightAnimatorData.Params.headed);
	}

	public void AbdorptionSoulsCapture()
	{
		_events.SwitchMainAbsorptionTo(AbsorptionCaptureIndex);
		_events.ActivateAbsorptionAttractor(true);
	}

	public void AbdorptionDeactive()
	{
		_animator.SetTrigger(PlayerKnightAnimatorData.Params.abdorptionDeactive);
		_events.DeactivateAbsorptionAttractor();
	}

	public void AbdorptionAnimationEnd()
	{
		AbdorptionAnimationEnded?.Invoke();
		_events.StopAbsorptionBody();
	}

	public void SetSpeed(float speed)
	{
		_animator.speed = speed;
	}

	public void ResetSpeed()
	{
		SetSpeed(NormalSpeed);
	}

	public void AllowStepMove()
	{
		IsStepMove = true;
	}

	public void DisallowStepMove()
	{
		IsStepMove = false;
	}

	public void FallLegs()
	{
		_events.PlayFallLegs();
	}

	public void GetUpLegs()
	{
		_events.PlayGetUpLegs();
	}

	public void PlayHeadStateParticles()
	{
		_events.PlayHeadStateParticles();
	}

	public void StopHeadStateParticles()
	{
		_events.StopHeadStateParticles();
	}

	private IEnumerator PlayShortMoveCoroutine()
	{
		_animator.SetBool(PlayerKnightAnimatorData.Params.isMove, true);
		yield return _shortMoveWait;
		_animator.SetBool(PlayerKnightAnimatorData.Params.isMove, false);
	}
}
