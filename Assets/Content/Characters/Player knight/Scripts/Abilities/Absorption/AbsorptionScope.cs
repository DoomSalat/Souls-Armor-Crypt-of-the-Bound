using Sirenix.OdinInspector;
using UnityEngine;

public class AbsorptionScope : MonoBehaviour
{
	[SerializeField, Required] private Transform _target;
	[SerializeField, Required] private AbsorptionScopeMove _movment;
	[SerializeField, Required] private AbsorptionScopeCollider _activatorCollider;
	[SerializeField, Required] private SoulFinder _finder;
	[SerializeField, Required] private AbsorptionScopeAnimation _animator;

	private void OnEnable()
	{
		_animator.HideCompleted += OnHideCompleted;
	}

	private void OnDisable()
	{
		_animator.HideCompleted -= OnHideCompleted;
	}

	private void Start()
	{
		Deactive();
	}

	private void FixedUpdate()
	{
		_movment.Move();

		_activatorCollider.UpdateCollider(_target.position);
	}

	public void Activate()
	{
		gameObject.SetActive(true);

		transform.position = _target.position;
		_activatorCollider.UpdateCollider(_target.position);
		_movment.SetFollowing(true);

		_animator.PlayAppear();
	}

	public void Hide()
	{
		bool findTarget = _finder.TryFindSoul();

		if (findTarget)
		{
			_movment.SetFollowing(false);
		}

		_animator.SetTarget(findTarget);
		_animator.PlayDissapear();
	}

	private void Deactive()
	{
		_movment.SetFollowing(false);

		transform.position = _target.position;
		gameObject.SetActive(false);

		_animator.PlayDissapear();
	}

	private void OnHideCompleted()
	{
		Deactive();
	}
}