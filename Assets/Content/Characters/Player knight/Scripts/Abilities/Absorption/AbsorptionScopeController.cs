using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class AbsorptionScopeController : MonoBehaviour
{
	[SerializeField, Required] private AbsorptionScope _absorptionScope;
	[SerializeField, Required] private Collider2D _activationCollider;

	private Camera _mainCamera;
	private bool _wasActivatedThisFrame;
	private Vector2 _cachedMousePosition;
	private Vector2 _cachedWorldPosition;

	public event Action OnActivated;
	public event Action OnDeactivated;

	private void Awake()
	{
		_mainCamera = Camera.main;
	}

	private void LateUpdate()
	{
		_wasActivatedThisFrame = false;
	}

	public bool IsPointInActivationZone()
	{
		_cachedMousePosition = Mouse.current.position.ReadValue();
		_cachedWorldPosition = _mainCamera.ScreenToWorldPoint(_cachedMousePosition);

		return _activationCollider.OverlapPoint(_cachedWorldPosition);
	}

	public bool TryActivate()
	{
		if (_wasActivatedThisFrame)
			return false;

		if (IsPointInActivationZone())
		{
			Activate();
			_wasActivatedThisFrame = true;
			return true;
		}

		return false;
	}

	public void OnMouseClickCanceled(InputAction.CallbackContext context)
	{
		Deactivate();
	}

	private void Activate()
	{
		_absorptionScope.Activate();
		OnActivated?.Invoke();
	}

	private void Deactivate()
	{
		_absorptionScope.Hide();
		OnDeactivated?.Invoke();
	}
}