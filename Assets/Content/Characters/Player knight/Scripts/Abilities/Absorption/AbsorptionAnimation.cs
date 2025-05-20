using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AbsorptionAnimation : MonoBehaviour
{
	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		Hide();
	}

	public void PlayAppear()
	{
		gameObject.SetActive(true);
		_animator.SetTrigger(AbsorptionAnimationData.Params.Appear);
	}

	public void PlayDissapear()
	{
		_animator.SetTrigger(AbsorptionAnimationData.Params.Dissapear);
	}

	public void SetTarget(bool target)
	{
		_animator.SetBool(AbsorptionAnimationData.Params.IsTarget, target);
	}

	public void Hide()
	{
		gameObject.SetActive(false);

		_animator.SetBool(AbsorptionAnimationData.Params.IsTarget, false);
		_animator.ResetTrigger(AbsorptionAnimationData.Params.Dissapear);
		_animator.ResetTrigger(AbsorptionAnimationData.Params.Appear);
	}
}
