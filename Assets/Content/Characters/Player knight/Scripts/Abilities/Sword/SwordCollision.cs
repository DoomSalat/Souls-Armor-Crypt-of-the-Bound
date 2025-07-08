using Sirenix.OdinInspector;
using UnityEngine;

public class SwordCollision : MonoBehaviour
{
	[SerializeField, Required] private SwordSpeedTracker _speedTracker;
	[SerializeField, Required] private Collider2D _hitBox;
	[SerializeField, Required] private Collider2D _wall;
	[SerializeField, Required] private SwordBladeVisualizer _bladeVisualizer;

	[Space]
	[SerializeField, MinValue(0f)] private float _speedThreshold = 5f;

	[ShowInInspector, ReadOnly] private float _currentSpeed;
	private bool _isHighSpeed;

	private void Start()
	{
		UpdateCollisionObjects();
		UpdateBladeVisualization();
	}

	private void FixedUpdate()
	{
		CheckSpeedAndUpdateCollision();
	}

	private void CheckSpeedAndUpdateCollision()
	{
		bool wasHighSpeed = _isHighSpeed;
		_currentSpeed = _speedTracker.CurrentSpeed;
		_isHighSpeed = _currentSpeed >= _speedThreshold;

		if (wasHighSpeed != _isHighSpeed)
		{
			UpdateCollisionObjects();
			UpdateBladeVisualization();
		}
	}

	private void UpdateCollisionObjects()
	{
		if (_hitBox != null)
			_hitBox.enabled = _isHighSpeed;

		if (_wall != null)
			_wall.enabled = !_isHighSpeed;
	}

	private void UpdateBladeVisualization()
	{
		if (_bladeVisualizer == null)
			return;

		if (_isHighSpeed)
			_bladeVisualizer.StartMovingVisualization();
		else
			_bladeVisualizer.StopMovingVisualization();
	}
}