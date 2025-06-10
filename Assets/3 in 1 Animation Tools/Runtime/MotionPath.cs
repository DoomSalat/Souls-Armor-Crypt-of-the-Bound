using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace SVassets.AnimationCrafter
{
	[Icon("Assets/3 in 1 Animation Tools/Editor/Icons/icon new-01.png")]
	[AddComponentMenu("SV Assets/3 in 1 Tools/Motion Path")]
	[System.Serializable]
	public class MotionPath : MonoBehaviour
	{
		public AnimationClip clip;
		public Animator animator;
		public Transform pathTarget; // The transform to trace
		public Transform pathParent; // path parent
		private Vector3 parentPos = Vector3.zero;
		public bool HoldAlPoints = false;


		public Color pathColor = Color.yellow; // Color of the path
		public Color pointColor = Color.white; // Color of the path
		public float pointSize = 0.1f;
		public bool showPoints = true;
		public bool ShowLines = true;
		public bool ShowCirles = true;



		public List<Vector3> points = new();
		public List<Vector3> valueEachTime = new();
		public AnimationCurve xCurve = new();
		public AnimationCurve yCurve = new();
		public AnimationCurve zCurve = new();


		public bool deselected = false;
		public bool alwaysShow = false;
		public bool AutoUpdatePath = false;


		public void OnDrawGizmos()
		{
			if (alwaysShow && deselected)
			{
				if (AutoUpdatePath)
				{
					UpdateMotionPath();
				}
				DrawLines();
				DrawPoints();
				DrawRailsPoints(valueEachTime);
			}

		}

		// updating motion paths
		public void UpdateMotionPath()
		{
			if (pathTarget != null && clip != null)
			{
				animator = pathTarget.GetComponent<Animator>();

				// get curves
				string targetPath = "";
				if (animator != null)
				{
					// Для аниматора определяем относительный путь от корня аниматора
					Transform current = pathTarget;
					List<string> pathParts = new List<string>();

					while (current != null && current != animator.transform)
					{
						pathParts.Add(current.name);
						current = current.parent;
					}

					if (pathParts.Count > 0)
					{
						pathParts.Reverse();
						targetPath = string.Join("/", pathParts);
					}
				}

				foreach (var binding in AnimationUtility.GetCurveBindings(clip))
				{
					bool isCorrectPath = false;

					if (animator != null)
					{
						isCorrectPath = binding.path == targetPath;
					}
					else
					{
						isCorrectPath = binding.path.EndsWith(pathTarget.name);
					}

					if (isCorrectPath)
					{
						if (binding.propertyName == "m_LocalPosition.x")
						{
							xCurve = AnimationUtility.GetEditorCurve(clip, binding);
						}
						if (binding.propertyName == "m_LocalPosition.y")
						{
							yCurve = AnimationUtility.GetEditorCurve(clip, binding);
						}
						if (binding.propertyName == "m_LocalPosition.z")
						{
							zCurve = AnimationUtility.GetEditorCurve(clip, binding);
						}
					}
				}

				// Sample points every second
				int sampleCount = Mathf.FloorToInt(clip.length * 60f) + 1;
				Vector3[] positionSamples = new Vector3[sampleCount];

				for (int i = 0; i < sampleCount; i++)
				{
					float time = i * (1 / 60f);
					float myxValue = GetValueFromCurveOrPosition(xCurve, time, pathTarget, "x");
					float myyValue = GetValueFromCurveOrPosition(yCurve, time, pathTarget, "y");
					float myzValue = GetValueFromCurveOrPosition(zCurve, time, pathTarget, "z");

					Vector3 pos = new Vector3(myxValue, myyValue, myzValue);

					// Применяем ту же трансформацию координат, что и для points
					if (animator != null)
					{
						// Для объектов с аниматором: преобразуем через корень аниматора
						pos = animator.transform.TransformPoint(pos);
					}
					else
					{
						// Для объектов без аниматора: преобразуем через непосредственного родителя
						if (pathTarget.parent != null)
						{
							pos = pathTarget.parent.TransformPoint(pos);
						}
					}

					positionSamples[i] = pos;
				}
				valueEachTime = positionSamples.ToList();



				if (xCurve.keys.Length > 0 && yCurve.keys.Length > 0 && zCurve.keys.Length > 0)
				{
					SortedSet<float> keyTimes = new();

					// Add keyframe times from both curves
					foreach (var keyframe in xCurve.keys)
					{
						keyTimes.Add(keyframe.time);
					}
					foreach (var keyframe in yCurve.keys)
					{
						keyTimes.Add(keyframe.time);
					}
					foreach (var keyframe in zCurve.keys)
					{
						keyTimes.Add(keyframe.time);
					}

					List<Vector3> pointArr = new();

					// Iterate through all the unique keyframe times in order
					foreach (float time in keyTimes)
					{
						float xValue = GetValueFromCurveOrPosition(xCurve, time, pathTarget, "x");

						float yValue = GetValueFromCurveOrPosition(yCurve, time, pathTarget, "y");

						float zValue = GetValueFromCurveOrPosition(zCurve, time, pathTarget, "z");

						Vector3 pos = new(xValue, yValue, zValue);

						// Применяем правильную трансформацию координат
						if (animator != null)
						{
							// Для объектов с аниматором: преобразуем через корень аниматора
							pos = animator.transform.TransformPoint(pos);
						}
						else
						{
							// Для объектов без аниматора: преобразуем через непосредственного родителя
							if (pathTarget.parent != null)
							{
								pos = pathTarget.parent.TransformPoint(pos);
							}
						}

						// Add the resulting Vector2 to the list
						pointArr.Add(pos);
					}

					points = pointArr;
				}

				if (xCurve.keys.Length < 1 && yCurve.keys.Length < 1)
				{
					ResetMotionPath();
				}

				SceneView.RepaintAll();

			}

		}

		private float GetValueFromCurveOrPosition(AnimationCurve curve, float time, Transform target, string axis)
		{
			// Check if the curve has keyframes
			if (curve.length > 0)
			{
				// Если время до начала анимации - используем значение первого ключа
				if (time <= curve.keys[0].time)
				{
					return curve.keys[0].value;
				}
				// Если время после окончания анимации - используем значение последнего ключа
				else if (time >= curve.keys[curve.length - 1].time)
				{
					return curve.keys[curve.length - 1].value;
				}
				// Если время в пределах анимации - вычисляем значение
				else
				{
					return curve.Evaluate(time);
				}
			}
			else
			{
				// Если кривая пустая - возвращаем 0 вместо текущей позиции
				return 0f;
			}
		}


		public void DrawLines()
		{
			if (pathTarget != null && clip != null)
			{
				if (valueEachTime == null || valueEachTime.Count < 2)
				{
					return;
				}

				if (ShowLines)
				{
					Handles.color = pathColor;

					// Draw the path by connecting the points
					for (int i = 0; i < valueEachTime.Count - 1; i++)
					{
						Handles.DrawLine(valueEachTime[i], valueEachTime[i + 1]);
					}
				}
			}
		}

		public void DrawPoints()
		{
			if (pathTarget != null && clip != null)
			{
				if (points == null)
					return;


				if (showPoints)
				{
					if (points.Count > 0)
					{
						Handles.color = pointColor;
						for (int i = 0; i < points.Count; i++)
						{
							float handleSize = (SceneView.currentDrawingSceneView.in2DMode) ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

							// Handles.DotHandleCap(i, points[i], Quaternion.LookRotation(Vector3.up), handleSize, EventType.Repaint);
							Vector3 newPointPos = Handles.FreeMoveHandle(points[i], handleSize, Vector3.zero, Handles.CubeHandleCap);

						}

					}
				}
			}

		}

		public void DrawRailsPoints(List<Vector3> MyPoints)
		{
			if (MyPoints == null)
				return;


			if (ShowCirles)
			{
				if (MyPoints.Count > 0)
				{
					Handles.color = pointColor;
					for (int i = 0; i < MyPoints.Count; i++)
					{
						float handleSize = (SceneView.currentDrawingSceneView.in2DMode) ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

						Handles.DrawWireDisc(MyPoints[i], Vector3.forward, handleSize * 0.3f);
					}

				}
			}
		}


		public void ResetMotionPath()
		{
			// check if, is there points
			if (points != null)
			{
				// Clear the points lists 
				points.Clear();
				xCurve = new AnimationCurve();
				yCurve = new AnimationCurve();
				SceneView.RepaintAll();

			}
		}

	}


}
