using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerHandsTarget : MonoBehaviour
{
	[SerializeField, Required] private PlayerLimbs _playerLimbs;
	[SerializeField, Required] private SwordController _swordController;
	[SerializeField, Required] private PlayerKnightAnimator _playerKnightAnimator;

	[Header("Hand Trackers")]
	[SerializeField, Required] private TargetLook _leftHandTracker;
	[SerializeField, Required] private TargetLook _rightHandTracker;
	[SerializeField] private float _sideOffsetY = 1f;

	private Transform _swordTransform;
	private HandType _currentActiveHand = HandType.None;
	private bool _isLogicActive;

	private enum HandType
	{
		None,
		Left,
		Right
	}

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

	public void ActivateLook()
	{
		_isLogicActive = true;
	}

	public void DeactivateLook()
	{
		_isLogicActive = false;
		DeactivateHandControl();
	}

	public void UpdateLook()
	{
		if (_isLogicActive && IsSwordControlActive())
		{
			UpdateHandControl();
		}
		else if (_isLogicActive == false)
		{
			DeactivateHandControl();
		}
	}

	private bool IsSwordControlActive()
	{
		var limbStates = _playerLimbs.LimbStates;
		return limbStates[LimbType.LeftArm] && limbStates[LimbType.RightArm] && limbStates[LimbType.Sword];
	}

	private void UpdateHandControl()
	{
		var nearestHand = DetermineNearestHand();

		if (nearestHand != _currentActiveHand)
		{
			SetActiveHand(nearestHand);
		}
		else if (_currentActiveHand != HandType.None)
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

	private HandType DetermineNearestHand()
	{
		var playerDirection = GetPlayerDirection();
		var swordPosition = _swordTransform.position;
		var playerPosition = transform.position;
		var relativePosition = swordPosition - playerPosition;

		switch (playerDirection)
		{
			case Direction.Down:
				return relativePosition.x > 0 ? HandType.Left : HandType.Right;

			case Direction.Up:
				return relativePosition.x > 0 ? HandType.Right : HandType.Left;

			case Direction.Right:
				return relativePosition.y > _sideOffsetY ? HandType.Left : HandType.Right;

			case Direction.Left:
				return relativePosition.y > _sideOffsetY ? HandType.Right : HandType.Left;

			default:
				return HandType.None;
		}
	}

	private float CalculateHandZOffset()
	{
		var playerDirection = GetPlayerDirection();

		if (playerDirection != Direction.Left && playerDirection != Direction.Right)
		{
			return float.NaN;
		}

		var relativePosition = _swordTransform.position - transform.position;

		if (playerDirection == Direction.Right && relativePosition.y > _sideOffsetY && _currentActiveHand == HandType.Left)
		{
			return 0f;
		}

		if (playerDirection == Direction.Left && relativePosition.y > _sideOffsetY && _currentActiveHand == HandType.Right)
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

	private void SetActiveHand(HandType newActiveHand)
	{
		if (_currentActiveHand != HandType.None)
		{
			DeactivateCurrentHand();
		}

		_currentActiveHand = newActiveHand;

		if (_currentActiveHand != HandType.None)
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
		if (_currentActiveHand != HandType.None)
		{
			DeactivateCurrentHand();
			_currentActiveHand = HandType.None;
		}
	}

	private TargetLook GetHandTracker(HandType handType)
	{
		return handType == HandType.Left ? _leftHandTracker : _rightHandTracker;
	}
}