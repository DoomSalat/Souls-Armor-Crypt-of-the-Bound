using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class WallStuck : MonoBehaviour
{
	private const float MinimumMovementThreshold = 0.01f;
	private const float DefaultDistanceForTriggers = 0.01f;

	[SerializeField] private LayerMask _wallLayer;
	[Space]
	[SerializeField, MinValue(0)] private float _stuckDepthThreshold = 0.2f;
	[SerializeField, MinValue(0)] private float _exitDepthThreshold = 0.1f;

	[Header("Debug")]
	[ShowInInspector, ReadOnly] private bool _isStuck;
	[ShowInInspector, ReadOnly] private float _currentPenetrationDepth;
	[ShowInInspector, ReadOnly] private Vector2 _wallNormal;

	public bool IsStuck => _isStuck;
	public float CurrentPenetrationDepth => _currentPenetrationDepth;
	public Vector2 WallNormal => _wallNormal;

	private Rigidbody2D _rigidbody;
	private Collider2D _collider;
	private ContactPoint2D[] _contacts = new ContactPoint2D[8];
	private int _contactCount;
	private Vector2 _lastPosition;
	private float _lastRotation;
	private RigidbodyConstraints2D _originalConstraints;
	private RigidbodyType2D _originalBodyType;
	private Vector2 _previousPosition;

	private void Awake()
	{
		_collider = GetComponent<Collider2D>();
		_rigidbody = GetComponent<Rigidbody2D>();
		_lastPosition = _rigidbody.position;
		_lastRotation = _rigidbody.rotation;
		_previousPosition = _rigidbody.position;
		_originalConstraints = _rigidbody.constraints;
		_originalBodyType = _rigidbody.bodyType;

		_collider.isTrigger = true;
	}

	public void UpdateStuck()
	{
		if (_isStuck)
		{
			BlockMovementIntoWall();
		}
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		int objectLayer = other.gameObject.layer;
		bool isInWallLayer = (_wallLayer.value & (1 << objectLayer)) != 0;

		if (!isInWallLayer)
			return;

		if (_collider.isTrigger)
		{
			Vector2 penetrationVector = CalculatePenetrationVector(other);
			_currentPenetrationDepth = penetrationVector.magnitude;
			_wallNormal = penetrationVector.normalized;
		}
		else
		{
			_contactCount = _collider.GetContacts(_contacts);
			if (_contactCount == 0)
				return;

			float maxDepth = 0f;
			Vector2 dominantNormal = Vector2.zero;

			for (int i = 0; i < _contactCount; i++)
			{
				if (_contacts[i].separation < maxDepth)
				{
					maxDepth = _contacts[i].separation;
					dominantNormal = _contacts[i].normal;
				}
			}

			_currentPenetrationDepth = Mathf.Abs(maxDepth);
			_wallNormal = dominantNormal;
		}

		if (!_isStuck && _currentPenetrationDepth > _stuckDepthThreshold)
		{
			_isStuck = true;
			_lastPosition = _rigidbody.position;
			_lastRotation = _rigidbody.rotation;
			_previousPosition = _rigidbody.position;
		}
		else if (_isStuck && _currentPenetrationDepth < _exitDepthThreshold)
		{
			_isStuck = false;
			RestoreOriginalConstraints();
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		int objectLayer = other.gameObject.layer;
		bool isInWallLayer = (_wallLayer.value & (1 << objectLayer)) != 0;

		if (isInWallLayer)
		{
			_isStuck = false;
			_currentPenetrationDepth = 0f;
			_lastPosition = _rigidbody.position;
			_lastRotation = _rigidbody.rotation;
			_previousPosition = _rigidbody.position;
			RestoreOriginalConstraints();
		}
	}

	private Vector2 CalculatePenetrationVector(Collider2D wallCollider)
	{
		Vector2 direction = Vector2.zero;
		float distance = 0f;

		ColliderDistance2D colliderDistance = Physics2D.Distance(_collider, wallCollider);

		if (colliderDistance.isOverlapped)
		{
			direction = -colliderDistance.normal;
			distance = colliderDistance.distance;
		}
		else
		{
			direction = (wallCollider.bounds.center - _collider.bounds.center).normalized;
			distance = DefaultDistanceForTriggers;
		}

		return direction * Mathf.Abs(distance);
	}

	private void BlockMovementIntoWall()
	{
		if (_isStuck == false || _rigidbody == null)
			return;

		Vector2 currentPosition = _rigidbody.position;
		Vector2 movementDelta = currentPosition - _previousPosition;

		if (movementDelta.magnitude > MinimumMovementThreshold && _wallNormal != Vector2.zero)
		{
			float movementDotNormal = Vector2.Dot(movementDelta, _wallNormal);

			if (movementDotNormal > 0)
			{
				_rigidbody.bodyType = _originalBodyType;
				_rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
				_lastPosition = currentPosition;
			}
			else
			{
				_rigidbody.bodyType = RigidbodyType2D.Kinematic;
				_rigidbody.MovePosition(_lastPosition);
				_rigidbody.MoveRotation(_lastRotation);
			}
		}
		else
		{
			_rigidbody.bodyType = RigidbodyType2D.Kinematic;
			_rigidbody.MoveRotation(_lastRotation);
		}

		_previousPosition = _rigidbody.position;
	}

	private void RestoreOriginalConstraints()
	{
		_rigidbody.bodyType = _originalBodyType;
		_rigidbody.constraints = _originalConstraints;
	}
}