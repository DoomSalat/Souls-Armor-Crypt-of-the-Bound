using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class AbsorptionScopeController : MonoBehaviour
{
	[SerializeField, Required] private AbsorptionScope _absorptionScope;
	[SerializeField, Required] private Collider2D _activationCollider;

	private Camera _mainCamera;
	private Vector3 _cachedMousePosition;
	private Vector3 _cachedWorldPosition;

	public event Action Activated;
	public event Action Deactivated;

	private void Awake()
	{
		_mainCamera = Camera.main;
	}

	public bool IsPointInActivationZone()
	{
		_cachedMousePosition = InputUtilits.GetMouseClampPosition();
		_cachedWorldPosition = _mainCamera.ScreenToWorldPoint(_cachedMousePosition);

		return _activationCollider.OverlapPoint(_cachedWorldPosition);
	}

	public void Activate()
	{
		_absorptionScope.Activate();
		Activated?.Invoke();
	}

	public void FindSoul()
	{
		_absorptionScope.Hide();
	}
}