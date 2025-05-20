using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbsorptionScopeController : MonoBehaviour
{
	[SerializeField, Required] private AbsorptionScope _absorptionScope;
	[SerializeField, Required] private Collider2D _activationCollider;

	[SerializeField, Required] private InputReader _inputReader;

	private Camera _mainCamera;
	private bool _isScopeActive = false;

	private void Awake()
	{
		_mainCamera = Camera.main;
	}

	private void OnEnable()
	{
		_inputReader.InputActions.Player.Mouse.performed += OnMouseClickPerformed;
		_inputReader.InputActions.Player.Mouse.canceled += OnMouseClickCanceled;
	}

	private void OnDisable()
	{
		_inputReader.InputActions.Player.Mouse.performed -= OnMouseClickPerformed;
		_inputReader.InputActions.Player.Mouse.canceled -= OnMouseClickCanceled;
	}

	private void OnMouseClickPerformed(InputAction.CallbackContext context)
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		Vector2 worldPosition = _mainCamera.ScreenToWorldPoint(mousePosition);

		if (_activationCollider.OverlapPoint(worldPosition))
		{
			_absorptionScope.Activate();
			_isScopeActive = true;
		}
	}

	private void OnMouseClickCanceled(InputAction.CallbackContext context)
	{
		if (_isScopeActive)
		{
			_absorptionScope.Hide();
			_isScopeActive = false;
		}
	}
}