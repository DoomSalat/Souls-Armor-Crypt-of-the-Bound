using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerKnightAnimator : MonoBehaviour
{
	[SerializeField] private PlayerKnightAnimatorEvents _events;

	private Animator _animator;

	public event System.Action AbdorptionAnimationEnded;

	public bool IsStepMove { get; private set; }

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void SetDirection(int direction)
	{
		_animator.SetFloat(PlayerKnightAnimatorData.Params.direction, direction);
	}

	public void SetMove(bool isMove)
	{
		_animator.SetBool(PlayerKnightAnimatorData.Params.isMove, isMove);

		if (isMove == false)
		{
			DisallowStepMove();
		}
	}

	public void SetCapture(bool isCapture)
	{
		_animator.SetBool(PlayerKnightAnimatorData.Params.isCapture, isCapture);
	}

	public void AbdorptionActive()
	{
		SetDirection(1);
		SetMove(false);

		_animator.SetTrigger(PlayerKnightAnimatorData.Params.abdorptionActive);

		_events.SwitchMainAbsorptionTo(0);
		_events.DeactivateAbsorptionAttractor();
		_events.PlayAbsorptionBody();
	}

	public void AbdorptionSoulsCapture()
	{
		_events.SwitchMainAbsorptionTo(1);
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

	public void AllowStepMove()
	{
		IsStepMove = true;
	}

	public void DisallowStepMove()
	{
		IsStepMove = false;
	}
}
