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

	public bool IsActive { get; private set; }

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
		if (IsActive)
			return;

		IsActive = true;
		_absorptionScope.Activate();
		Activated?.Invoke();
	}

	public void StartSoulSearch()
	{
		if (IsActive == false)
			return;

		IsActive = false;
		_absorptionScope.SearchSoul();
		_absorptionScope.Hide();
	}
}