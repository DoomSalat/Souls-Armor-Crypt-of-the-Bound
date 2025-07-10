using UnityEngine;

public class MovementHeadState : PlayerState
{
	private readonly InputMove _inputMove;
	private readonly PlayerKnightAnimator _playerKnightAnimator;
	private readonly InputReader _inputReader;

	private Vector2 _movementDirection;
	private int _directionIndex;
	private float _speedMoveMultiplier = 3f;

	public MovementHeadState(PlayerKnightAnimator playerKnightAnimator, InputMove inputMove, InputReader inputReader)
	{
		_inputMove = inputMove;
		_playerKnightAnimator = playerKnightAnimator;
		_inputReader = inputReader;
	}

	public override void Enter()
	{
		_inputReader.Enable();
		InitializeMovementDirection();

		_playerKnightAnimator.SetMove(false);
		_playerKnightAnimator.PlayHeaded();
		_playerKnightAnimator.PlayHeadStateParticles();
	}

	public override void Update()
	{
		UpdateMovementDirection();
	}

	public override void FixedUpdate()
	{
		HandleMovement();
	}

	public override void Exit()
	{
		_inputMove.Stop();
		_playerKnightAnimator.ResetSpeed();
		_playerKnightAnimator.StopHeadStateParticles();
	}

	private void InitializeMovementDirection()
	{
		Vector2 inputDirection = _inputMove.GetInputDirection();

		if (inputDirection != Vector2.zero)
		{
			_movementDirection = inputDirection;
		}
		else
		{
			_movementDirection = GetDirectionFromAnimator();
		}
	}

	private void UpdateMovementDirection()
	{
		Vector2 inputDirection = _inputMove.GetInputDirection();

		if (inputDirection != Vector2.zero)
		{
			_movementDirection = inputDirection;
			UpdateAnimationDirection(inputDirection);
		}
	}

	private void HandleMovement()
	{
		_inputMove.Move(_movementDirection, _speedMoveMultiplier);

		UpdateAnimationDirection(_movementDirection);
	}

	private void UpdateAnimationDirection(Vector2 direction)
	{
		if (_playerKnightAnimator.GetDirectionIndex(direction) != _directionIndex)
		{
			_directionIndex = _playerKnightAnimator.GetDirectionIndex(direction);
			_playerKnightAnimator.SetDirection(_directionIndex);
		}
	}

	private Vector2 GetDirectionFromAnimator()
	{
		int animatorDirection = _playerKnightAnimator.GetDirection();

		return animatorDirection switch
		{
			1 => Vector2.down,
			2 => Vector2.up,
			3 => Vector2.left,
			4 => Vector2.right,
			_ => Vector2.down
		};
	}
}