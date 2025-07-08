using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : PlayerState
{
	private readonly InputMove _inputMove;
	private readonly SwordController _swordController;
	private readonly InputReader _inputReader;
	private readonly PlayerKnightAnimator _playerKnightAnimator;
	private readonly PlayerHandsTarget _playerHandsTarget;
	private readonly PlayerLimbs _playerLimbs;

	private bool _isSwordControlRequested;

	public MovementState(PlayerKnightAnimator playerKnightAnimator, InputMove inputMove, SwordController swordController, InputReader inputReader, PlayerHandsTarget playerHandsTarget, PlayerLimbs playerLimbs)
	{
		_inputMove = inputMove;
		_swordController = swordController;
		_inputReader = inputReader;
		_playerKnightAnimator = playerKnightAnimator;
		_playerHandsTarget = playerHandsTarget;
		_playerLimbs = playerLimbs;
	}

	public override void Enter()
	{
		_inputReader.Enable();
	}

	public override void Update()
	{
		UpdateAnimationDirection();
		_playerHandsTarget.UpdateLook();
		UpdateSwordControl();
	}

	public override void FixedUpdate()
	{
		HandleMovement();
	}

	public override void Exit()
	{
		StopMovement();
		DeactivateSwordControl();
	}

	public override void OnMousePerformed(InputAction.CallbackContext context)
	{
		_isSwordControlRequested = true;
		_playerHandsTarget.ActivateLook();
		UpdateSwordControl();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_isSwordControlRequested = false;
		_playerHandsTarget.DeactivateLook();
		DeactivateSwordControl();
	}

	private void UpdateSwordControl()
	{
		if (_isSwordControlRequested == false)
		{
			DeactivateSwordControl();
			return;
		}

		var currentHand = _playerHandsTarget.GetCurrentHand();

		if (IsHandAvailable(currentHand))
		{
			_swordController.Activate();
		}
		else
		{
			DeactivateSwordControl();
		}
	}

	private void DeactivateSwordControl()
	{
		_swordController.Deactivate();
	}

	private void HandleMovement()
	{
		if (_playerLimbs.HasLegs() == false)
		{
			_inputMove.Stop();
			return;
		}

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
			int directionIndex = 0;

			if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
			{
				directionIndex = direction.y > 0 ? 2 : 1;
			}
			else
			{
				directionIndex = direction.x > 0 ? 4 : 3;
			}

			if (_playerLimbs.HasLegs())
			{
				_playerKnightAnimator.SetMove(true);
			}
			else if (_playerKnightAnimator.GetDirection() != directionIndex)
			{
				_playerKnightAnimator.PlayShortMove();
			}

			_playerKnightAnimator.SetDirection(directionIndex);
		}
		else
		{
			_playerKnightAnimator.SetMove(false);
		}
	}

	private bool IsHandAvailable(LimbType limbType)
	{
		var limbStates = _playerLimbs.LimbStates;
		return limbStates.ContainsKey(limbType) && limbStates[limbType];
	}
}