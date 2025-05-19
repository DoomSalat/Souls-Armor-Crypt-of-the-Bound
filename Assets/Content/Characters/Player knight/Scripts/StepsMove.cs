using UnityEngine;

[RequireComponent(typeof(InputMove))]
public class StepsMove : MonoBehaviour
{
	[SerializeField] private float _stepInterval = 0.5f;
	[SerializeField] private float _stepDuration = 0.2f;

	private InputMove _inputMove;
	private bool _isMoving = false;
	private int _legsCount = 2;
	private float _stepTimer = 0f;
	private bool _isStepActive = false;

	private void Awake()
	{
		_inputMove = GetComponent<InputMove>();
	}

	private void Update()
	{
		if (_legsCount == 0)
			return;

		_stepTimer += Time.deltaTime;

		Vector2 direction = _inputMove.GetInputDirection();

		if (direction != Vector2.zero && _isMoving == false && _stepTimer >= _stepInterval)
		{
			_isMoving = true;
			_isStepActive = true;
			_stepTimer = 0f;
		}
	}

	private void FixedUpdate()
	{
		if (_isMoving == false || _legsCount == 0)
			return;

		if (_isStepActive)
		{
			Vector2 direction = _inputMove.GetInputDirection();

			if (direction != Vector2.zero)
			{
				_inputMove.Move();
			}
			else
			{
				_inputMove.Stop();
				_isStepActive = false;
			}

			if (_stepTimer >= _stepDuration)
			{
				_inputMove.Stop();
				_isStepActive = false;
			}
		}

		if (_stepTimer >= _stepInterval)
		{
			_isMoving = false;
		}
	}

	public void LoseLeg()
	{
		if (_legsCount > 0)
		{
			_legsCount--;

			if (_legsCount == 0)
			{
				Debug.LogWarning("Рыцарь потерял все ноги и не может двигаться!");
				_inputMove.Stop();
			}
		}
		else
		{
			Debug.LogError("Попытка потерять ногу, когда их уже нет!");
		}
	}
}