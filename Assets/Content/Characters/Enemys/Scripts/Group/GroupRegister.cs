using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GroupRegister
{
	private static readonly Dictionary<int, Dictionary<IGroupController, List<IGroupController>>> _groups = new Dictionary<int, Dictionary<IGroupController, List<IGroupController>>>();
	private static int _nextGroupId = 1;

	public static int CreateGroup(IGroupController leader, List<IGroupController> members)
	{
		if (leader == null || members == null)
		{
			return -1;
		}

		var newGroup = new Dictionary<IGroupController, List<IGroupController>>
		{
			{ leader, new List<IGroupController>(members) }
		};

		int groupId = _nextGroupId++;
		_groups[groupId] = newGroup;
		return groupId;
	}

	public static void RemoveGroup(int groupId)
	{
		if (groupId <= 0)
		{
			return;
		}

		_groups.Remove(groupId);
	}

	public static IGroupController SetRandomLeader(int groupId)
	{
		if (groupId <= 0 || !_groups.ContainsKey(groupId))
		{
			return null;
		}

		var group = _groups[groupId];
		if (group.Count == 0)
		{
			return null;
		}

		var currentLeader = group.Keys.First();
		var members = group[currentLeader];

		if (members.Count == 0)
		{
			return currentLeader;
		}

		var newLeader = members[Random.Range(0, members.Count)];
		members.Remove(newLeader);
		members.Add(currentLeader);

		group.Remove(currentLeader);
		group[newLeader] = members;

		return newLeader;
	}

	public static void LeaderDied(int groupId)
	{
		if (groupId <= 0 || !_groups.ContainsKey(groupId))
		{
			return;
		}

		var group = _groups[groupId];
		var currentLeader = group.Keys.First();
		var members = group[currentLeader];

		if (members.Count == 0)
		{
			_groups.Remove(groupId);
			return;
		}

		if (members.Count == 1)
		{
			_groups.Remove(groupId);
			return;
		}

		var newLeader = members[Random.Range(0, members.Count)];
		members.Remove(newLeader);

		group.Remove(currentLeader);
		group[newLeader] = members;

		newLeader.InitializeGroup(groupId, true);
	}

	public static void ReplaceLeader(int groupId, IGroupController newLeader)
	{
		if (groupId <= 0 || !_groups.ContainsKey(groupId) || newLeader == null)
		{
			return;
		}

		var group = _groups[groupId];
		var currentLeader = group.Keys.First();
		var members = group[currentLeader];

		if (members.Contains(newLeader))
		{
			members.Remove(newLeader);
		}

		group.Remove(currentLeader);
		group[newLeader] = members;

		newLeader.InitializeGroup(groupId, true);
	}

	public static void ReinitializeGroupMembers(int groupId)
	{
		if (groupId <= 0 || !_groups.ContainsKey(groupId))
		{
			return;
		}

		var group = _groups[groupId];
		var leader = group.Keys.First();
		var members = group[leader];

		group[leader] = new List<IGroupController>(members);
		leader.InitializeSwarm();
	}

	public static void UpdateGroupMembers(int groupId, List<IGroupController> newMembers)
	{
		if (groupId <= 0 || !_groups.ContainsKey(groupId))
		{
			return;
		}

		var group = _groups[groupId];
		var leader = group.Keys.First();

		group[leader] = new List<IGroupController>(newMembers);
		leader.InitializeSwarm();
	}

	public static Dictionary<IGroupController, List<IGroupController>> GetGroup(int groupId)
	{
		if (groupId <= 0 || !_groups.ContainsKey(groupId))
		{
			return null;
		}

		return _groups[groupId];
	}

	public static void ClearAllGroups()
	{
		_groups.Clear();
		_nextGroupId = 1;
	}
}
