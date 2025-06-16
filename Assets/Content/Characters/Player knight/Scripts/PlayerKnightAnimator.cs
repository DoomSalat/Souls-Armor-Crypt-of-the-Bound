using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerKnightAnimator : MonoBehaviour
{
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
	}

	public void AbdorptionDeactive()
	{
		_animator.SetTrigger(PlayerKnightAnimatorData.Params.abdorptionDeactive);
	}

	public void AbdorptionAnimationEnd()
	{
		AbdorptionAnimationEnded?.Invoke();
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
