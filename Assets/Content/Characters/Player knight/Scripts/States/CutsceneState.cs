using UnityEngine;
using UnityEngine.InputSystem;

public class CutsceneState : PlayerState
{
	private readonly PlayerKnightAnimator _playerKnightAnimator;
	private readonly InputReader _inputReader;
	private readonly SwordController _swordController;
	private readonly Transform _swordTeleportTarget;
	private readonly PlayerLimbs _playerLimbs;
	private readonly AbsorptionScopeController _absorptionScopeController;

	private bool _isCutsceneOutActive = false;

	public event System.Action CutsceneCompleted;
	public event System.Action InputShouldBeDisabled;
	public event System.Action InputShouldBeEnabled;

	public CutsceneState(PlayerKnightAnimator playerKnightAnimator,
						InputReader inputReader,
						SwordController swordController,
						Transform swordTeleportTarget,
						PlayerLimbs playerLimbs,
						AbsorptionScopeController absorptionScopeController)
	{
		_playerKnightAnimator = playerKnightAnimator;
		_inputReader = inputReader;
		_swordController = swordController;
		_swordTeleportTarget = swordTeleportTarget;
		_playerLimbs = playerLimbs;
		_absorptionScopeController = absorptionScopeController;
	}

	public override void Enter()
	{
		_isCutsceneOutActive = false;

		DisableGameplay();

		_playerKnightAnimator.PlayStartIdle();

		_playerKnightAnimator.StartIdleParticles += OnStartIdleParticles;
		_playerKnightAnimator.StartIdleEnded += OnStartIdleEnded;
	}

	public override void Exit()
	{
		_playerKnightAnimator.StartIdleParticles -= OnStartIdleParticles;
		_playerKnightAnimator.StartIdleEnded -= OnStartIdleEnded;

		RestoreGameplay();
	}

	public override void OnMousePerformed(InputAction.CallbackContext context)
	{
		if (_isCutsceneOutActive == false)
		{
			if (_absorptionScopeController.IsPointInActivationZone())
			{
				StartCutsceneExit();
			}
		}
	}

	private void StartCutsceneExit()
	{
		_isCutsceneOutActive = true;
		_playerKnightAnimator.PlayStartIdleEnd();
	}

	private void OnStartIdleParticles()
	{
		_playerLimbs.RestoreAllLimbVisuals();
	}

	private void OnStartIdleEnded()
	{
		CompleteCutscene();
	}

	private void CompleteCutscene()
	{
		TeleportSword();

		CutsceneCompleted?.Invoke();
	}

	private void TeleportSword()
	{
		_swordController.SwordTransform.position = new Vector3(_swordTeleportTarget.position.x, _swordTeleportTarget.position.y, _swordController.SwordTransform.position.z);
		_swordController.SwordTransform.rotation = _swordTeleportTarget.rotation;
	}

	private void DisableGameplay()
	{
		_inputReader.Disable();

		_swordController.HideSword();
		_swordController.Deactivate();

		_playerLimbs.DisableAllLimbVisuals();

		InputShouldBeDisabled?.Invoke();
	}

	private void RestoreGameplay()
	{
		_inputReader.Enable();

		_swordController.ShowSword();
		_swordController.Activate();

		InputShouldBeEnabled?.Invoke();
	}
}
