using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class VictoryEvents : MonoBehaviour
{
	public event Action LevelCompleted;

	[Button(nameof(Play))]
	public void Play()
	{
		LevelCompleted?.Invoke();
	}
}
