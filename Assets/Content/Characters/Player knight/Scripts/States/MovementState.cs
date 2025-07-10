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

	private LimbType _currentLeg = LimbType.RightLeg;
	private bool _previousIsStepMove = false;
	private float _animationSpeedMultiplier = 2.0f;

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
		ResetStepCounter();
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
		_playerKnightAnimator.ResetSpeed();
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
		Vector2 inputDirection = _inputMove.GetInputDirection();

		if (inputDirection == Vector2.zero)
		{
			_inputMove.Stop();
			_playerKnightAnimator.ResetSpeed();
			ResetStepCounter();
			return;
		}

		if (_playerLimbs.HasLegs() == false)
		{
			_inputMove.Stop();
			_playerKnightAnimator.ResetSpeed();
			return;
		}

		UpdateStepCounter();

		if (_playerKnightAnimator.IsStepMove)
		{
			if (IsLegAvailable(_currentLeg))
			{
				_playerKnightAnimator.ResetSpeed();
			}
			else
			{
				_playerKnightAnimator.SetSpeed(_animationSpeedMultiplier);
			}

			_inputMove.Move();
		}
		else
		{
			_playerKnightAnimator.ResetSpeed();
			_inputMove.Stop();
		}
	}

	private void UpdateStepCounter()
	{
		if (_previousIsStepMove == true && _playerKnightAnimator.IsStepMove == false)
		{
			SwitchToNextLeg();
		}

		_previousIsStepMove = _playerKnightAnimator.IsStepMove;
	}

	private void StopMovement()
	{
		_inputMove.Stop();
		_playerKnightAnimator.SetMove(false);
		_playerKnightAnimator.ResetSpeed();
		ResetStepCounter();
	}

	private void UpdateAnimationDirection()
	{
		Vector2 direction = _inputMove.GetInputDirection();

		if (direction != Vector2.zero)
		{
			int directionIndex = _playerKnightAnimator.GetDirectionIndex(direction);

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

	private bool IsLegAvailable(LimbType legType)
	{
		var available = _playerLimbs.LimbStates.ContainsKey(legType) && _playerLimbs.LimbStates[legType];
		return available;
	}

	private void SwitchToNextLeg()
	{
		_currentLeg = _currentLeg == LimbType.LeftLeg ? LimbType.RightLeg : LimbType.LeftLeg;
	}

	private void ResetStepCounter()
	{
		_currentLeg = LimbType.RightLeg;
		_previousIsStepMove = false;
	}
}