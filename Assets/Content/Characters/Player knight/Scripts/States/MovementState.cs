using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : PlayerState
{
	private const float JoystickMinMagnitude = 0.1f;
	private const float DefaultSwordSpeed = 2f;
	private const float DefaultLegSpeed = 1f;

	private readonly InputMove _inputMove;
	private readonly SwordController _swordController;
	private readonly InputReader _inputReader;
	private readonly PlayerKnightAnimator _playerKnightAnimator;
	private readonly PlayerHandsTarget _playerHandsTarget;
	private readonly PlayerLimbs _playerLimbs;
	private readonly AbilityInitializer _abilityInitializer;

	private bool _isSwordControlRequested;

	private LimbType _currentLeg = LimbType.RightLeg;
	private bool _previousIsStepMove = false;
	private float _animationSpeedMultiplier = 2.0f;
	private float _swordDeactivateDelay = 0.75f;

	private bool _isMouseControlled = false;
	private float _swordDeactivateTimer = 0f;

	public MovementState(PlayerKnightAnimator playerKnightAnimator, InputMove inputMove, SwordController swordController, InputReader inputReader, PlayerHandsTarget playerHandsTarget, PlayerLimbs playerLimbs, AbilityInitializer abilityInitializer)
	{
		_inputMove = inputMove;
		_swordController = swordController;
		_inputReader = inputReader;
		_playerKnightAnimator = playerKnightAnimator;
		_playerHandsTarget = playerHandsTarget;
		_playerLimbs = playerLimbs;
		_abilityInitializer = abilityInitializer;
	}

	public override void Enter()
	{
		_inputReader.Enable();
		ResetStepCounter();
		_swordController.SetMouseControlled(_isMouseControlled);
	}

	public override void Update()
	{
		UpdateAnimationDirection();
		_playerHandsTarget.UpdateLook();
		UpdateSwordControl();
		UpdateSwordDeactivateTimer();
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
		if (_isMouseControlled == false)
			return;

		_isSwordControlRequested = true;
		UpdateSwordControl();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		if (_isMouseControlled == false)
			return;

		_isSwordControlRequested = false;
		UpdateSwordControl();
	}

	private void UpdateSwordControl()
	{
		if (_isMouseControlled)
		{
			MouseSwordControl();
		}
		else
		{
			JoystickSwordControl();
		}
	}

	private void JoystickSwordControl()
	{
		Vector2 joystickInput = _inputReader.JoystickInput;

		if (CanControlSwordJoystick(joystickInput) == false)
		{
			_playerHandsTarget.DeactivateLook();
			StartSwordDeactivateTimer();
			return;
		}

		_swordDeactivateTimer = 0f;
		_playerHandsTarget.ActivateLook();

		if (_swordController.IsControlled == false)
		{
			_swordController.Activate();
		}

		if (_swordController.IsControlled)
		{
			_swordController.MoveTarget(joystickInput, GetCurrentSwordSpeed());
		}
	}

	private bool CanControlSwordJoystick(Vector2 joystickInput)
	{
		if (joystickInput.magnitude < JoystickMinMagnitude)
		{
			return false;
		}

		var currentHand = _playerHandsTarget.GetCurrentHand();
		if (IsHandAvailable(currentHand) == false)
		{
			return false;
		}

		return true;
	}

	private void MouseSwordControl()
	{
		if (_isSwordControlRequested)
		{
			_playerHandsTarget.ActivateLook();
		}
		else
		{
			_playerHandsTarget.DeactivateLook();
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
			_playerHandsTarget.DeactivateLook();
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

			float legSpeedMultiplier = GetCurrentLegSpeed();
			_inputMove.Move(legSpeedMultiplier);
		}
		else
		{
			_playerKnightAnimator.ResetSpeed();
			_inputMove.Stop();
		}
	}

	private void UpdateStepCounter()
	{
		if (_previousIsStepMove && _playerKnightAnimator.IsStepMove == false)
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
		return limbStates.ContainsKey(limbType) && limbStates[limbType].IsPresent;
	}

	private bool IsLegAvailable(LimbType legType)
	{
		var available = _playerLimbs.LimbStates.ContainsKey(legType) && _playerLimbs.LimbStates[legType].IsPresent;
		return available;
	}

	private void SwitchToNextLeg()
	{
		var ability = _abilityInitializer.GetCurrentLegAbility(_currentLeg);
		if (ability is IAbilityLeg legAbility)
		{
			ability.Activate();
		}

		_currentLeg = _currentLeg == LimbType.LeftLeg ? LimbType.RightLeg : LimbType.LeftLeg;
	}

	private float GetCurrentSwordSpeed()
	{
		var currentHand = _playerHandsTarget.GetCurrentHand();

		if (currentHand == LimbType.None)
			return DefaultSwordSpeed;

		if (IsHandAvailable(currentHand) == false)
			return DefaultSwordSpeed;

		var ability = _abilityInitializer.GetCurrentArmAbility(currentHand);

		if (ability is IAbilityArm armAbility)
		{
			return armAbility.SwordSpeed;
		}

		return DefaultSwordSpeed;
	}

	private float GetCurrentLegSpeed()
	{
		if (IsLegAvailable(_currentLeg) == false)
			return DefaultLegSpeed;

		var ability = _abilityInitializer.GetCurrentLegAbility(_currentLeg);

		if (ability is IAbilityLeg legAbility)
		{
			return legAbility.Speed;
		}

		return DefaultLegSpeed;
	}

	private void ResetStepCounter()
	{
		_currentLeg = LimbType.RightLeg;
		_previousIsStepMove = false;
	}

	private void StartSwordDeactivateTimer()
	{
		if (_swordDeactivateTimer == 0f && _swordController.IsControlled)
		{
			_swordDeactivateTimer = Time.time;
		}
	}

	private void UpdateSwordDeactivateTimer()
	{
		if (_swordDeactivateTimer > 0f && _swordController.IsControlled)
		{
			if (Time.time - _swordDeactivateTimer >= _swordDeactivateDelay)
			{
				DeactivateSwordControl();
				_swordDeactivateTimer = 0f;
			}
		}
	}

	//Доработать как способность
	public void SetMouseControlled(bool useMouseControl)
	{
		_isMouseControlled = useMouseControl;
		_swordController.SetMouseControlled(_isMouseControlled);

		_playerHandsTarget.DeactivateLook();
		DeactivateSwordControl();
	}
}