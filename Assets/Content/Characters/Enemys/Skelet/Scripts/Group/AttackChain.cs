using UnityEngine;
using System.Collections.Generic;

public class AttackChain
{
	public int ChainId;
	public Queue<Skelet> AttackQueue;
	public Coroutine Coroutine;
	public bool IsActive;

	public AttackChain(int id, Queue<Skelet> queue)
	{
		ChainId = id;
		AttackQueue = queue;
		IsActive = true;
	}
}
