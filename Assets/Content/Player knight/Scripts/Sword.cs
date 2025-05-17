using Sirenix.OdinInspector;
using UnityEngine;

public class Sword : MonoBehaviour
{
	[SerializeField, Required] private InputReader _activeButton;
	[SerializeField, Required] private SpringJoint2D _targetController;
	[SerializeField, Required] private SmoothLook _eye;

	private void Start()
	{
		DeactiveFollow();
	}

	private void OnEnable()
	{
		_activeButton.InputActions.Player.Sword.performed += context => ActiveFollow();
		_activeButton.InputActions.Player.Sword.canceled += context => DeactiveFollow();
	}

	private void OnDisable()
	{
		_activeButton.InputActions.Player.Sword.performed -= context => ActiveFollow();
		_activeButton.InputActions.Player.Sword.canceled -= context => DeactiveFollow();
	}

	private void ActiveFollow()
	{
		_targetController.enabled = true;
		_eye.SetFollowing(true);
	}

	private void DeactiveFollow()
	{
		_targetController.enabled = false;
		_eye.SetFollowing(false);
	}
}
