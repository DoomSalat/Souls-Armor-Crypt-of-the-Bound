using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace BlockAnimationSystem
{
	[ExecuteAlways]
	public class GroupArrayController : MonoBehaviour
	{
		private const int MinGroupSize = 1;

		[Header("Group Selection")]
		[SerializeField][PropertyRange(-1, "$MaxGroupIndex")] private int _activeGroupIndex = -1; // -1 means no group is active
		[SerializeField] private bool _isActiveControl = true;

		[Header("Groups Configuration")]
		[SerializeField] private GroupSettings[] _groups;

		[System.Serializable]
		public class GroupSettings
		{
			[HideInInspector] public string GroupFoldoutName; // Used for dynamic foldout naming

			[FoldoutGroup("$GroupFoldoutName")]
			public string GroupName = "Group";
			[FoldoutGroup("$GroupFoldoutName")]
			public GameObject[] Objects;

			[FoldoutGroup("$GroupFoldoutName/Transform Control")]
			public bool ControlPosition;
			[FoldoutGroup("$GroupFoldoutName/Transform Control")]
			public Vector3 TargetPosition;
			[FoldoutGroup("$GroupFoldoutName/Transform Control")]
			public bool ControlRotation;
			[FoldoutGroup("$GroupFoldoutName/Transform Control")]
			public Vector3 TargetRotation; // Euler angles
			[FoldoutGroup("$GroupFoldoutName/Transform Control")]
			public bool ControlScale;
			[FoldoutGroup("$GroupFoldoutName/Transform Control")]
			public Vector3 TargetScale = Vector3.one;

			[FoldoutGroup("$GroupFoldoutName/Custom Actions")]
			public UnityEvent GroupUpdated;

			public void UpdateFoldoutName(int index)
			{
				GroupFoldoutName = $"{index}: {GroupName}";
			}
		}

		private int MaxGroupIndex => _groups != null ? _groups.Length - 1 : -1;

		private void OnEnable()
		{
			UpdateGroupNames();
			ApplyAllChanges();
		}

		private void Update()
		{
			UpdateGroupNames();
			ApplyAllChanges();
		}

		private void UpdateGroupNames()
		{
			if (_groups == null)
				return;

			for (int i = 0; i < _groups.Length; i++)
			{
				_groups[i].UpdateFoldoutName(i);
			}
		}

		private void ApplyAllChanges()
		{
			if (!IsValid())
				return;

			for (int i = 0; i < _groups.Length; i++)
			{
				bool isActive = i == _activeGroupIndex;
				ApplyGroupChanges(_groups[i], isActive);
			}
		}

		private bool IsValid()
		{
			return _groups is { Length: >= MinGroupSize } && _groups[0].Objects is { Length: >= MinGroupSize } && _groups[0].Objects[0] != null;
		}

		private void ApplyGroupChanges(GroupSettings group, bool isActive)
		{
			if (group.Objects == null || group.Objects.Length < MinGroupSize) return;

			foreach (GameObject obj in group.Objects)
			{
				if (obj == null)
					continue;

				if (_isActiveControl)
					obj.SetActive(isActive);

				Transform objTransform = obj.transform;
				if (group.ControlPosition)
				{
					objTransform.localPosition = group.TargetPosition;
				}
				if (group.ControlRotation)
				{
					objTransform.localRotation = Quaternion.Euler(group.TargetRotation);
				}
				if (group.ControlScale)
				{
					objTransform.localScale = group.TargetScale;
				}
			}

			if (isActive)
				group.GroupUpdated?.Invoke();
		}

		[Button("Copy Transform from First in All Groups")]
		private void CopyTransformFromFirstInAll()
		{
			if (!IsValid())
				return;

			foreach (GroupSettings group in _groups)
			{
				if (group.Objects != null && group.Objects.Length > 0 && group.Objects[0] != null)
				{
					Transform firstTransform = group.Objects[0].transform;
					group.TargetPosition = firstTransform.localPosition;
					group.TargetRotation = firstTransform.localEulerAngles;
					group.TargetScale = firstTransform.localScale;
				}
			}
		}
	}
}