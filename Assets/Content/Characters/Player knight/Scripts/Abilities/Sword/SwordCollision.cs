using Sirenix.OdinInspector;
using UnityEngine;

public class SwordCollision : MonoBehaviour
{
	[SerializeField, Required] private SwordSpeedTracker _speedTracker;
	[SerializeField, Required] private Collider2D _hitBox;
	[SerializeField, Required] private Collider2D _wall;

	[Space]
	[SerializeField, MinValue(0f)] private float _speedThreshold = 5f;

	[ShowInInspector, ReadOnly] private bool _isHighSpeed;

	private void Start()
	{
		UpdateCollisionObjects();
	}

	private void FixedUpdate()
	{
		CheckSpeedAndUpdateCollision();
	}

	private void CheckSpeedAndUpdateCollision()
	{
		bool wasHighSpeed = _isHighSpeed;
		_isHighSpeed = _speedTracker.CurrentSpeed >= _speedThreshold;

		if (wasHighSpeed != _isHighSpeed)
		{
			UpdateCollisionObjects();
		}
	}

	private void UpdateCollisionObjects()
	{
		if (_hitBox != null)
			_hitBox.enabled = _isHighSpeed;

		if (_wall != null)
			_wall.enabled = !_isHighSpeed;
	}
}