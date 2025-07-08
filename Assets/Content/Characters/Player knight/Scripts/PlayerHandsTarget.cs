using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class PlayerHandsTarget : MonoBehaviour
{
	[SerializeField, Required] private SwordController _swordController;
	[SerializeField, Required] private PlayerKnightAnimator _playerKnightAnimator;

	[Header("Hand Trackers")]
	[SerializeField, Required] private TargetLook _leftHandTracker;
	[SerializeField, Required] private TargetLook _rightHandTracker;
	[SerializeField] private float _sideOffsetY = 1f;

	private Transform _swordTransform;
	private LimbType _currentActiveHand = LimbType.None;
	private bool _isLogicActive;

	private enum Direction
	{
		Down = 1,
		Up = 2,
		Left = 3,
		Right = 4
	}

	private void Awake()
	{
		_swordTransform = _swordController.transform;
	}

	public LimbType GetCurrentHand()
	{
		return DetermineNearestHand();
	}

	public void ActivateLook()
	{
		_isLogicActive = true;
		UpdateHandControl();
	}

	public void DeactivateLook()
	{
		_isLogicActive = false;
		DeactivateHandControl();
	}

	public void UpdateLook()
	{
		if (_isLogicActive)
		{
			UpdateHandControl();
		}
		else
		{
			DeactivateHandControl();
		}
	}

	private void UpdateHandControl()
	{
		var nearestHand = DetermineNearestHand();

		if (nearestHand != _currentActiveHand)
		{
			SetActiveHand(nearestHand);
		}
		else if (_currentActiveHand != LimbType.None)
		{
			UpdateActiveHandZPosition();
		}
	}

	private void UpdateActiveHandZPosition()
	{
		var customZOffset = CalculateHandZOffset();

		if (!float.IsNaN(customZOffset))
		{
			var handTracker = GetHandTracker(_currentActiveHand);
			handTracker.UpdateZPosition(customZOffset);
		}
	}

	private LimbType DetermineNearestHand()
	{
		var playerDirection = GetPlayerDirection();
		var swordPosition = _swordTransform.position;
		var playerPosition = transform.position;
		var relativePosition = swordPosition - playerPosition;

		switch (playerDirection)
		{
			case Direction.Down:
				return relativePosition.x > 0 ? LimbType.RightArm : LimbType.LeftArm;

			case Direction.Up:
				return relativePosition.x > 0 ? LimbType.LeftArm : LimbType.RightArm;

			case Direction.Right:
				return relativePosition.y > _sideOffsetY ? LimbType.RightArm : LimbType.LeftArm;

			case Direction.Left:
				return relativePosition.y > _sideOffsetY ? LimbType.LeftArm : LimbType.RightArm;

			default:
				return LimbType.None;
		}
	}

	private float CalculateHandZOffset()
	{
		var playerDirection = GetPlayerDirection();

		if (playerDirection == Direction.Up)
		{
			return 0f;
		}

		if (playerDirection != Direction.Left && playerDirection != Direction.Right)
		{
			return float.NaN;
		}

		var relativePosition = _swordTransform.position - transform.position;

		if (playerDirection == Direction.Right && relativePosition.y > _sideOffsetY && _currentActiveHand == LimbType.RightArm)
		{
			return 0f;
		}

		if (playerDirection == Direction.Left && relativePosition.y > _sideOffsetY && _currentActiveHand == LimbType.LeftArm)
		{
			return 0f;
		}

		return float.NaN;
	}

	private Direction GetPlayerDirection()
	{
		var directionValue = _playerKnightAnimator.GetDirection();
		return (Direction)directionValue;
	}

	private void SetActiveHand(LimbType newActiveHand)
	{
		if (_currentActiveHand != LimbType.None)
		{
			DeactivateCurrentHand();
		}

		_currentActiveHand = newActiveHand;

		if (_currentActiveHand != LimbType.None)
		{
			ActivateCurrentHand();
		}
	}

	private void ActivateCurrentHand()
	{
		var handTracker = GetHandTracker(_currentActiveHand);
		handTracker.SetTarget(_swordTransform);

		var customZOffset = CalculateHandZOffset();

		if (float.IsNaN(customZOffset))
		{
			handTracker.ActivateTracking();
		}
		else
		{
			handTracker.ActivateTracking(customZOffset);
		}
	}

	private void DeactivateCurrentHand()
	{
		var handTracker = GetHandTracker(_currentActiveHand);
		handTracker.DeactivateTracking();
	}

	private void DeactivateHandControl()
	{
		if (_currentActiveHand != LimbType.None)
		{
			DeactivateCurrentHand();
			_currentActiveHand = LimbType.None;
		}
	}

	private TargetLook GetHandTracker(LimbType limbType)
	{
		return limbType == LimbType.LeftArm ? _leftHandTracker : _rightHandTracker;
	}
}