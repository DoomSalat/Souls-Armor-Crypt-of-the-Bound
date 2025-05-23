using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(InputMove))]
public class StepsMove : MonoBehaviour
{
	[SerializeField] private float _stopDuration = 0.5f;
	[SerializeField] private float _moveDuration = 0.2f;

	private InputMove _inputMove;
	private int _legsCount = 2;
	private bool _canStep = true;

	private Coroutine _stepRoutine;
	private WaitForSeconds _waitMoveDuration;
	private WaitForSeconds _waitForStopDuration;

	private void Awake()
	{
		_inputMove = GetComponent<InputMove>();

		_waitMoveDuration = new WaitForSeconds(_moveDuration);
		_waitForStopDuration = new WaitForSeconds(_stopDuration);
	}

	public void Move()
	{
		if (IsCanMove() == false)
			return;

		Vector2 direction = _inputMove.GetInputDirection();

		if (direction == Vector2.zero)
		{
			Stop();
			return;
		}

		if (_canStep)
			_inputMove.Move();

		if (_stepRoutine == null)
		{
			_stepRoutine = StartCoroutine(StepDuration());
		}
	}

	public void Stop()
	{
		if (_stepRoutine != null)
		{
			StopCoroutine(_stepRoutine);
			_stepRoutine = null;
		}

		_inputMove.Stop();
		_canStep = true;
	}

	public void LoseLeg()
	{
		if (_legsCount <= 0)
		{
			Debug.LogError("Попытка потерять ногу, когда их уже нет!");
			return;
		}

		_legsCount--;
		Stop();

		if (_legsCount == 0)
		{
			Debug.LogWarning("Рыцарь потерял все ноги и не может двигаться!");
		}
	}

	private IEnumerator StepDuration()
	{
		_canStep = true;
		yield return _waitMoveDuration;

		_canStep = false;
		_inputMove.Stop();
		yield return _waitForStopDuration;

		_stepRoutine = null;
	}

	private bool IsCanMove()
	{
		return _legsCount > 0;
	}
}