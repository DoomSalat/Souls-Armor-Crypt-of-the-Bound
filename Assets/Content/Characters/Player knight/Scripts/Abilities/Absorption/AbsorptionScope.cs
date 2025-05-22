using Sirenix.OdinInspector;
using UnityEngine;

public class AbsorptionScope : MonoBehaviour
{
	[SerializeField, Required] private Transform _target;
	[SerializeField, Required] private AbsorptionScopeMove _movement;
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
		_movement.Move();

		_activatorCollider.UpdateCollider(_target.position);
	}

	public void Activate()
	{
		transform.position = _target.position;
		_activatorCollider.UpdateCollider(_target.position);
		_movement.SetFollowing(true);

		_animator.PlayAppear();
	}

	public void Hide()
	{
		bool findTarget = _finder.TryFindSoul();

		if (findTarget)
		{
			_movement.SetFollowing(false);
		}

		_animator.SetTarget(findTarget);
		_animator.PlayDissapear();
	}

	private void Deactive()
	{
		_movement.SetFollowing(false);
		transform.position = _target.position;

		_animator.PlayDissapear();
	}

	private void OnHideCompleted()
	{
		Deactive();
	}
}