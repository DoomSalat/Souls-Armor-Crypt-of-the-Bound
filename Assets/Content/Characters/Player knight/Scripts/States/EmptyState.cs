using UnityEngine;

public class EmptyState : PlayerState
{
	private readonly InputMove _inputMove;

	public EmptyState(InputMove inputMove)
	{
		_inputMove = inputMove;
	}

	public override void Enter()
	{
		_inputMove.InputReader.Disable();
		_inputMove.Stop();
	}
}
