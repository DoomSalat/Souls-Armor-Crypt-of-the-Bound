using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(InputMove))]
public class Player : MonoBehaviour, IDamagable
{
	[SerializeField, Required] private InputMove _inputMove;

	private void Awake()
	{
		_inputMove = GetComponent<InputMove>();
	}

	private void FixedUpdate()
	{
		_inputMove.Move();
	}

	public void TakeDamage(DamageData damageData)
	{
		Debug.Log($"Take damage: {gameObject.name}");
	}
}
