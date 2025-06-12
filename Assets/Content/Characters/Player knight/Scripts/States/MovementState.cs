using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : PlayerState
{
	private readonly InputMove _inputMove;
	private readonly SwordController _swordController;
	private readonly InputReader _inputReader;
	private readonly PlayerKnightAnimator _playerKnightAnimator;

	public MovementState(InputMove inputMove, SwordController swordController, InputReader inputReader, PlayerKnightAnimator playerKnightAnimator)
	{
		_inputMove = inputMove;
		_swordController = swordController;
		_inputReader = inputReader;
		_playerKnightAnimator = playerKnightAnimator;
	}

	public override void Enter()
	{
		_inputReader.Enable();
	}

	public override void Update()
	{
		UpdateAnimationDirection();
	}

	public override void FixedUpdate()
	{
		HandleMovement();
	}

	public override void Exit()
	{
		StopMovement();
		_swordController.Deactivate();
	}

	public override void OnMousePerformed(InputAction.CallbackContext context)
	{
		_swordController.Activate();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_swordController.Deactivate();
	}

	private void HandleMovement()
	{
		if (_playerKnightAnimator.IsStepMove)
		{
			_inputMove.Move();
		}
		else
		{
			_inputMove.Stop();
			return;
		}
	}

	private void StopMovement()
	{
		_inputMove.Stop();
		_playerKnightAnimator.SetMove(false);
	}

	private void UpdateAnimationDirection()
	{
		Vector2 direction = _inputMove.GetInputDirection();

		if (direction != Vector2.zero)
		{
			_playerKnightAnimator.SetMove(true);

			if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
			{
				_playerKnightAnimator.SetDirection(direction.y > 0 ? 2 : 1);
			}
			else
			{
				_playerKnightAnimator.SetDirection(direction.x > 0 ? 4 : 3);
			}
		}
		else
		{
			_playerKnightAnimator.SetMove(false);
		}
	}
}