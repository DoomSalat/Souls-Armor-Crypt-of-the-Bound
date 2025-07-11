using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SwordPlayerDetector : MonoBehaviour
{
	[Header("Zone Settings")]
	[SerializeField, MinValue(0)] private float _followRadius = 2f;
	[SerializeField, MinValue(0)] private float _stopFollowRadius = 4f;

	[Header("Debug")]
	[SerializeField] private bool _showDebugGizmos = true;

	private CircleCollider2D _triggerCollider;

	public event System.Action EnteredRadius;
	public event System.Action ExitedRadius;

	private void Awake()
	{
		_triggerCollider = GetComponent<CircleCollider2D>();

		_triggerCollider.isTrigger = true;
		_triggerCollider.radius = _stopFollowRadius;
	}

	private void Start()
	{
		ForceFindPlayer();
	}

	private void OnDrawGizmosSelected()
	{
		if (_showDebugGizmos == false)
			return;

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, _followRadius);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _stopFollowRadius);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player player))
		{
			EnteredRadius?.Invoke();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.TryGetComponent(out Player player))
		{
			ExitedRadius?.Invoke();
		}
	}

	private void ForceFindPlayer()
	{
		Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, _stopFollowRadius, LayerMask.GetMask("Default"));

		if (playerCollider != null && playerCollider.TryGetComponent(out Player player))
		{
			EnteredRadius?.Invoke();
		}
	}
}