using UnityEngine;
using UnityEditor;

public static class UIAnchorTool
{
	private const string MenuPath = "Tools/UI Tools/";

	[MenuItem(MenuPath + "Set Anchors to Current Position")]
	public static void SetAnchorsForSelected()
	{
		GameObject selected = Selection.activeGameObject;

		if (selected == null)
		{
			Debug.LogWarning("No GameObject selected. Please select a UI object.");
			return;
		}

		RectTransform rect = selected.GetComponent<RectTransform>();

		if (rect == null)
		{
			Debug.LogWarning("Selected GameObject does not have a RectTransform component.");
			return;
		}

		SetAnchorsToCurrentPosition(rect);
		Debug.Log("Anchors set successfully for " + selected.name);
	}

	[MenuItem(MenuPath + "Stretch to Anchors")]
	public static void StretchToAnchorsForSelected()
	{
		GameObject[] selected = Selection.gameObjects;

		if (selected.Length == 0)
		{
			Debug.LogWarning("No GameObjects selected. Please select UI objects.");
			return;
		}

		int count = 0;

		foreach (GameObject go in selected)
		{
			RectTransform rt = go.GetComponent<RectTransform>();
			if (rt != null)
			{
				StretchToAnchors(rt);
				count++;
			}
		}

		Debug.Log("Stretched to anchors successfully for " + count + " UI objects.");
	}

	public static void SetAnchorsToCurrentPosition(RectTransform childRT)
	{
		RectTransform parentRT = childRT.parent as RectTransform;

		if (parentRT == null)
		{
			Debug.LogWarning("Parent of the selected UI object is not a RectTransform.");
			return;
		}

		Vector3[] worldCorners = new Vector3[4];
		childRT.GetWorldCorners(worldCorners);

		Vector2 localBottomLeft = parentRT.InverseTransformPoint(worldCorners[0]);
		Vector2 localTopRight = parentRT.InverseTransformPoint(worldCorners[2]);

		Rect parentRect = parentRT.rect;

		float relativeXMin = (localBottomLeft.x - parentRect.xMin) / parentRect.width;
		float relativeYMin = (localBottomLeft.y - parentRect.yMin) / parentRect.height;
		float relativeXMax = (localTopRight.x - parentRect.xMin) / parentRect.width;
		float relativeYMax = (localTopRight.y - parentRect.yMin) / parentRect.height;

		childRT.anchorMin = new Vector2(relativeXMin, relativeYMin);
		childRT.anchorMax = new Vector2(relativeXMax, relativeYMax);

		childRT.offsetMin = Vector2.zero;
		childRT.offsetMax = Vector2.zero;
	}

	public static void StretchToAnchors(RectTransform rt)
	{
		Undo.RecordObject(rt, "Stretch to Anchors");
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
	}
}