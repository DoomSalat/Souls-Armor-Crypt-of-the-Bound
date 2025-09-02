using System.Collections.Generic;
using UnityEngine;

public interface IGroupController
{
	bool IsGroupLeader { get; }
	List<IGroupController> GroupMembers { get; }
	int GroupId { get; }

	IFollower GetFollower();
	Transform GetTransform();

	void InitializeGroup(int groupId, bool isLeader);
	void InitializeSwarm();
	void SetGroupId(int groupId);
	void ClearGroup();
	void TransferLeadership(IGroupController newLeader);
	void AddMemberToGroup(IGroupController member);
	void OnMemberAddedToGroup(IGroupController member);
	void TransferGroupTo(IGroupController newMember);
	void TransferGroupIdToSuccessor(IGroupController successor);

	bool CanControlled();

	void CheckAndActivateGroupState();

	bool IsInLeaderControlZone(IGroupController member);
}
