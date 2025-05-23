using Sirenix.OdinInspector;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
	private const int ChoosePriotity = 10;

	public static CameraController Instance { get; private set; }

	[SerializeField, Required] private CinemachineCamera _globalCamera;
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

		_globalCamera.Priority = ChoosePriotity;
		_playerMobCamera.Priority = 0;
	}

	public void SwitchToGlobalCamera()
	{
		_globalCamera.Priority = ChoosePriotity;
		_playerMobCamera.Priority = 0;
	}

	public void SwitchToPlayerMobCamera()
	{
		_globalCamera.Priority = 0;
		_playerMobCamera.Priority = ChoosePriotity;
	}

	public void SetPlayerMobCameraTarget(Transform mob)
	{
		//_playerMobCamera.LookAt = midPoint;
	}
}