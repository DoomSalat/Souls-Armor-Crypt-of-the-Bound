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
	private readonly AbsorptionScope _absorptionScope;
	private readonly Transform _soulAbsorptionTarget;

	private bool _isCutsceneOutActive = false;
	private ISoul _currentSoul;

	public event System.Action CutsceneCompleted;
	public event System.Action InputShouldBeDisabled;
	public event System.Action InputShouldBeEnabled;

	public event System.Action CutsceneStarted;
	public event System.Action CutsceneOuted;

	public CutsceneState(PlayerKnightAnimator playerKnightAnimator,
						InputReader inputReader,
						SwordController swordController,
						Transform swordTeleportTarget,
						PlayerLimbs playerLimbs,
						AbsorptionScopeController absorptionScopeController,
						AbsorptionScope absorptionScope,
						Transform soulAbsorptionTarget)
	{
		_playerKnightAnimator = playerKnightAnimator;
		_inputReader = inputReader;
		_swordController = swordController;
		_swordTeleportTarget = swordTeleportTarget;
		_playerLimbs = playerLimbs;
		_absorptionScopeController = absorptionScopeController;
		_absorptionScope = absorptionScope;
		_soulAbsorptionTarget = soulAbsorptionTarget;
	}

	public override void Enter()
	{
		_isCutsceneOutActive = false;
		_currentSoul = null;

		CutsceneStarted?.Invoke();

		DisableGameplay();

		_playerKnightAnimator.PlayStartIdle();

		_playerKnightAnimator.StartIdleParticles += OnStartIdleParticles;
		_playerKnightAnimator.StartIdleEnded += OnStartIdleEnded;
		_playerKnightAnimator.CutsceneKilledSoul += OnCutsceneKilledSoul;

		_absorptionScope.SoulFounded += OnSoulFound;
		_absorptionScope.SoulTargeted += OnSoulTargeted;
	}

	public override void Exit()
	{
		_playerKnightAnimator.StartIdleParticles -= OnStartIdleParticles;
		_playerKnightAnimator.StartIdleEnded -= OnStartIdleEnded;
		_playerKnightAnimator.CutsceneKilledSoul -= OnCutsceneKilledSoul;

		_absorptionScope.SoulFounded -= OnSoulFound;
		_absorptionScope.SoulTargeted -= OnSoulTargeted;

		_currentSoul = null;

		RestoreGameplay();
	}

	public override void OnMousePerformed(InputAction.CallbackContext context)
	{
		if (_isCutsceneOutActive == false)
		{
			if (_absorptionScopeController.IsPointInActivationZone())
			{
				_absorptionScopeController.Activate();
				_playerKnightAnimator.PlayAbdorptionParticles();
			}
		}
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		if (_absorptionScopeController.IsActive)
		{
			_absorptionScopeController.StartSoulSearch();
		}
	}

	private void OnSoulFound(ISoul soul)
	{
		_currentSoul = soul;

		if (soul == null)
		{
			_absorptionScope.Hide();
			_playerKnightAnimator.StopAbdorptionParticles();
			return;
		}

		_playerKnightAnimator.PlayAbdorptionSoulsCapture();
	}

	private void OnSoulTargeted()
	{
		if (_currentSoul == null)
			return;

		_currentSoul.StartAttraction(_soulAbsorptionTarget, OnSoulAttractionCompleted);
	}

	private void OnSoulAttractionCompleted()
	{
		if (_currentSoul == null)
			return;

		_playerKnightAnimator.StopAbdorptionParticles();

		_absorptionScope.Hide();
		StartCutsceneExit();
	}

	private void StartCutsceneExit()
	{
		_isCutsceneOutActive = true;
		_playerKnightAnimator.PlayStartIdleEnd();

		CutsceneOuted?.Invoke();
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

	private void OnCutsceneKilledSoul()
	{
		KillSoul();
	}

	private void KillSoul()
	{
		if (_currentSoul == null)
			return;

		_currentSoul.OnAbsorptionCompleted();
		_currentSoul = null;
	}
}
