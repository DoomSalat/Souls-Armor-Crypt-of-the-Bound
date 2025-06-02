using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
	private const int ChoosePriotity = 10;

	public static CameraController Instance { get; private set; }

	[SerializeField, Required] private CinemachineCamera _mainCamera;
	[SerializeField, Required] private CinemachineCamera _playerMobCamera;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

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