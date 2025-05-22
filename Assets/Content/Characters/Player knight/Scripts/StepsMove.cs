using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InputMove))]
public class StepsMove : MonoBehaviour
{
	[SerializeField] private float _stepInterval = 0.5f;
	[SerializeField] private float _stepDuration = 0.2f;

	private InputMove _inputMove;
	private int _legsCount = 2;
	private float _stepTimer;
	private bool _canStep = true;
	private Coroutine _stepCoroutine;

	private WaitForSeconds _stepDurationWait;

	private void Awake()
	{
		_inputMove = GetComponent<InputMove>();
		_stepDurationWait = new WaitForSeconds(_stepDuration);
	}

	private void Update()
	{
		if (_legsCount == 0)
			return;

		_stepTimer += Time.deltaTime;

		if (_stepTimer >= _stepInterval)
		{
			_canStep = true;
		}
	}

	public void Move()
	{
		if (_legsCount == 0 || _canStep == false)
			return;

		Vector2 direction = _inputMove.GetInputDirection();
		if (direction == Vector2.zero)
		{
			_inputMove.Stop();
			return;
		}

		_inputMove.Move();
		_canStep = false;
		_stepTimer = 0f;

		if (_stepCoroutine != null)
			StopCoroutine(_stepCoroutine);

		_stepCoroutine = StartCoroutine(StepRoutine());
	}

	public void Stop()
	{
		_inputMove.Stop();
		_canStep = true;
		_stepTimer = 0f;

		if (_stepCoroutine != null)
		{
			StopCoroutine(_stepCoroutine);
			_stepCoroutine = null;
		}
	}

	private IEnumerator StepRoutine()
	{
		yield return _stepDurationWait;

		_inputMove.Stop();
		_stepCoroutine = null;
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
}