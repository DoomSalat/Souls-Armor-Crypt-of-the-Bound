using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InventorySoulAnimator : MonoBehaviour
{
	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void ActivateSoul()
	{
		_animator.Play(InventorySoulAnimatorData.Params.Activate);
	}

	public void HideSoul()
	{
		_animator.Play(InventorySoulAnimatorData.Params.Hide);
	}
}
