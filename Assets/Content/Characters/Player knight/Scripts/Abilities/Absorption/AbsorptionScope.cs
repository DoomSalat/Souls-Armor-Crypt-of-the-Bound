using Sirenix.OdinInspector;
using UnityEngine;

public class AbsorptionScope : MonoBehaviour
{
	[SerializeField, Required] private Transform _target;
	[SerializeField, Required] private AbsorptionScopeMove _movement;
	[SerializeField, Required] private AbsorptionScopeCollider _activatorCollider;
	[SerializeField, Required] private SoulFinder _finder;
	[SerializeField, Required] private AbsorptionScopeAnimation _animator;

	public event System.Action<ISoul> SoulFounded;
	public event System.Action SoulTargeted;

	private void OnEnable()
	{
		_animator.TargetLooked += OnSoulTarget;
		_animator.Hidden += OnHide;
	}

	private void OnDisable()
	{
		_animator.TargetLooked -= OnSoulTarget;
		_animator.Hidden -= OnHide;
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

	public void SearchSoul()
	{
		_finder.TryFindSoul(out ISoul findTargetSoul);
		_animator.SetTarget(findTargetSoul != null);

		SoulFounded?.Invoke(findTargetSoul);

		if (findTargetSoul != null)
		{
			_movement.SetTarget(findTargetSoul.Transform);
		}
	}

	public void Hide()
	{
		_animator.PlayDissapear();
	}

	private void Deactive()
	{
		_movement.SetFollowing(false);
		transform.position = _target.position;
		_animator.PlayDissapear();
	}

	private void OnSoulTarget()
	{
		SoulTargeted?.Invoke();
	}

	private void OnHide()
	{
		Deactive();
	}
}