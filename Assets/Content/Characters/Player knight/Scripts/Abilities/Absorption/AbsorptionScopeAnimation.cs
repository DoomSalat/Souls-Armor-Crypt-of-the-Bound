using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AbsorptionScopeAnimation : MonoBehaviour
{
	private Animator _animator;

	public event System.Action TargetLooked;
	public event System.Action Hidden;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		gameObject.SetActive(false);
	}

	public void PlayAppear()
	{
		gameObject.SetActive(true);
		_animator.SetTrigger(AbsorptionScopeAnimationData.Params.Appear);
	}

	public void PlayDissapear()
	{
		_animator.SetTrigger(AbsorptionScopeAnimationData.Params.Dissapear);
	}

	public void SetTarget(bool target)
	{
		_animator.SetBool(AbsorptionScopeAnimationData.Params.IsTarget, target);
	}

	public void LockTarget()
	{
		TargetLooked?.Invoke();
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		Hidden?.Invoke();

		_animator.SetBool(AbsorptionScopeAnimationData.Params.IsTarget, false);
		_animator.ResetTrigger(AbsorptionScopeAnimationData.Params.Dissapear);
		_animator.ResetTrigger(AbsorptionScopeAnimationData.Params.Appear);
	}
}
