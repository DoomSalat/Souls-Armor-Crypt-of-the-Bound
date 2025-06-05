using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
	private const int ChoosePriotity = 10;

	[SerializeField, Required] private CinemachineCamera _mainCamera;
	[SerializeField, Required] private CinemachineCamera _playerMobCamera;

	private void Awake()
	{
		_mainCamera.Priority = ChoosePriotity;
		_playerMobCamera.Priority = 0;
	}

	public void SwitchToGlobalCamera()
	{
		_mainCamera.Priority = ChoosePriotity;
		_playerMobCamera.Priority = 0;
	}

	public void SwitchToPlayerMobCamera()
	{
		_mainCamera.Priority = 0;
		_playerMobCamera.Priority = ChoosePriotity;
	}
}