using System.Collections.Generic;
using UnityEngine;

public interface IGroupController
{
	bool IsGroupLeader { get; }
	List<IGroupController> GroupMembers { get; }

	IFollower GetFollower();
	Transform GetTransform();

	void InitializeGroup(List<IGroupController> groupMembers);
	void ClearGroup();

	bool CanControlled();
}
