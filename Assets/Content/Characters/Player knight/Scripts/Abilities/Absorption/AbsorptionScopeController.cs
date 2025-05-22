using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class AbsorptionScopeController : MonoBehaviour
{
	[SerializeField, Required] private AbsorptionScope _absorptionScope;
	[SerializeField, Required] private Collider2D _activationCollider;

	private Camera _mainCamera;
	private bool _isScopeActive = false;

	public event Action OnActivated;
	public event Action OnDeactivated;

	private void Awake()
	{
		_mainCamera = Camera.main;
	}

	public bool IsPointInActivationZone(Vector2 worldPosition)
	{
		return _activationCollider.OverlapPoint(worldPosition);
	}

	public void OnMouseClickPerformed(InputAction.CallbackContext context)
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);

		if (IsPointInActivationZone(worldPosition))
		{
			Activate();
		}
	}

	public void OnMouseClickCanceled(InputAction.CallbackContext context)
	{
		if (_isScopeActive)
		{
			Deactivate();
		}
	}

	private void Activate()
	{
		_absorptionScope.Activate();
		_isScopeActive = true;
		OnActivated?.Invoke();
	}

	private void Deactivate()
	{
		_absorptionScope.Hide();
		_isScopeActive = false;
		OnDeactivated?.Invoke();
	}
}