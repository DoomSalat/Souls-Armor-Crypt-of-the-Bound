using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemchineGroupTargetSelection : MonoBehaviour
{
	private const int MembersCount = 2;
	private const int TargetNum = 1;

	private CinemachineTargetGroup _cameraTargetGroup;

	private void Awake()
	{
		_cameraTargetGroup = GetComponent<CinemachineTargetGroup>();

		if (_cameraTargetGroup.Targets.Count != MembersCount)
		{
			Debug.LogWarning($"Need {MembersCount} participants to customize key target members");
		}
	}

	public void SetTarget(Transform target)
	{
		if (target == null)
		{
			_cameraTargetGroup.RemoveMember(_cameraTargetGroup.Targets[TargetNum].Object);
			return;
		}

		_cameraTargetGroup.Targets[TargetNum].Object = target;
	}
}
