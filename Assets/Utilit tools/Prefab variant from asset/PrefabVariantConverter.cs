using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PrefabVariantConverterWindow : EditorWindow
{
	private GameObject _modifiedPrefab;
	private GameObject _originalPrefab;
	private Vector2 _scrollPos;
	private List<string> _diffLog = new List<string>();
	private int _changeCount = 0;

	private GameObject _ctxSourceRoot;
	private GameObject _ctxTargetRoot;

	[MenuItem("Tools/Prefab Variant Converter")]
	public static void OpenWindow()
	{
		GetWindow<PrefabVariantConverterWindow>("Prefab Variant Converter");
	}

	private void OnGUI()
	{
		GUILayout.Label("Convert Modified Prefab into Prefab Variant", EditorStyles.boldLabel);

		_modifiedPrefab = (GameObject)EditorGUILayout.ObjectField("Modified Prefab", _modifiedPrefab, typeof(GameObject), false);
		_originalPrefab = (GameObject)EditorGUILayout.ObjectField("Reference (original) Prefab", _originalPrefab, typeof(GameObject), false);

		if (GUILayout.Button("Analyze Differences"))
		{
			_diffLog.Clear();
			_changeCount = 0;

			if (_modifiedPrefab == null || _originalPrefab == null)
			{
				_diffLog.Add("❌ Please assign both Modified Prefab and Original Prefab.");
			}
			else
			{
				GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(_originalPrefab);
				_ctxSourceRoot = _modifiedPrefab;
				_ctxTargetRoot = clone;

				ApplyDifferencesRecursive(_modifiedPrefab, clone, "");

				DestroyImmediate(clone);
				_ctxSourceRoot = null;
				_ctxTargetRoot = null;

				if (_changeCount == 0)
					_diffLog.Add("⚠️ No significant differences detected. Variant conversion not recommended.");
				else
					_diffLog.Add($"✅ Found {_changeCount} differences. Ready for conversion.");
			}
		}

		if (_modifiedPrefab != null && _originalPrefab != null && _changeCount > 0)
		{
			if (GUILayout.Button("Convert To Variant"))
			{
				ConvertToVariant();
			}
		}

		GUILayout.Space(10);
		GUILayout.Label("Diff Log:", EditorStyles.boldLabel);

		_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300));
		foreach (var log in _diffLog)
		{
			EditorGUILayout.LabelField(log, EditorStyles.wordWrappedLabel);
		}

		EditorGUILayout.EndScrollView();
	}

	public void ConvertToVariant()
	{
		string selectedPath = AssetDatabase.GetAssetPath(_modifiedPrefab);
		string folder = Path.GetDirectoryName(selectedPath);
		string fileName = Path.GetFileNameWithoutExtension(selectedPath);
		string newPath = Path.Combine(folder, fileName + "_Variant.prefab").Replace("\\", "/");

		GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(_originalPrefab);
		PrefabUtility.SaveAsPrefabAsset(instance, newPath);
		_diffLog.Add($"Prefab saved at: {newPath}");

		_ctxSourceRoot = _modifiedPrefab;
		_ctxTargetRoot = instance;

		ApplyDifferencesRecursive(_modifiedPrefab, instance, "");

		PrefabUtility.SaveAsPrefabAsset(instance, newPath);

		DestroyImmediate(instance);

		_diffLog.Add($"✅ Prefab Variant updated & saved: {newPath}");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		_ctxSourceRoot = null;
		_ctxTargetRoot = null;
	}

	private void ApplyDifferencesRecursive(GameObject source, GameObject target, string path)
	{
		string CleanName(string name) => name != null && name.EndsWith("(Clone)") ? name.Substring(0, name.Length - 7) : name;
		string objectPath = string.IsNullOrEmpty(path) ? CleanName(source.name) : path + "/" + CleanName(source.name);

		List<Transform> targetChildren = new List<Transform>();
		for (int childIndex = 0; childIndex < target.transform.childCount; childIndex++)
			targetChildren.Add(target.transform.GetChild(childIndex));
		foreach (var destinationChild in targetChildren)
		{
			if (source.transform.Find(destinationChild.name) == null)
			{
				_diffLog.Add($"[GameObject] {objectPath}: Removed child {CleanName(destinationChild.name)}");
				DestroyImmediate(destinationChild.gameObject);
				_changeCount++;
			}
		}

		if (target.transform.localPosition != source.transform.localPosition)
		{
			_diffLog.Add($"[{nameof(Transform)}] {objectPath} Position changed: {target.transform.localPosition} → {source.transform.localPosition}");
			target.transform.localPosition = source.transform.localPosition;
			_changeCount++;
		}
		if (target.transform.localRotation != source.transform.localRotation)
		{
			_diffLog.Add($"[{nameof(Transform)}] {objectPath} Rotation changed.");
			target.transform.localRotation = source.transform.localRotation;
			_changeCount++;
		}
		if (target.transform.localScale != source.transform.localScale)
		{
			_diffLog.Add($"[{nameof(Transform)}] {objectPath} Scale changed: {target.transform.localScale} → {source.transform.localScale}");
			target.transform.localScale = source.transform.localScale;
			_changeCount++;
		}

		var sourceComponentsAll = source.GetComponents<Component>();
		var destinationComponentsAll = target.GetComponents<Component>();

		for (int componentIndex = 0; componentIndex < destinationComponentsAll.Length; componentIndex++)
		{
			var destinationComponent = destinationComponentsAll[componentIndex];
			if (destinationComponent == null)
				continue;

			var componentType = destinationComponent.GetType();
			if (componentType == typeof(Transform) || componentType == typeof(RectTransform))
				continue;

			int sourceCount = CountComponentsOfType(source, componentType);
			int destinationCount = CountComponentsOfType(target, componentType);
			if (destinationCount > sourceCount)
			{
				var destinationList = new List<Component>(target.GetComponents(componentType));
				for (int removeIndex = destinationList.Count - 1; removeIndex >= sourceCount; removeIndex--)
				{
					_diffLog.Add($"[Component] {objectPath}: Removed component {componentType.Name}");
					DestroyImmediate(destinationList[removeIndex]);
					_changeCount++;
				}
			}
		}

		foreach (var sourceComponent in sourceComponentsAll)
		{
			if (sourceComponent == null)
				continue;

			var componentType = sourceComponent.GetType();
			if (componentType == typeof(Transform) || componentType == typeof(RectTransform))
				continue;

			var destinationList = new List<Component>(target.GetComponents(componentType));
			var sourceList = new List<Component>(source.GetComponents(componentType));

			while (destinationList.Count < sourceList.Count)
			{
				var addedComponent = target.AddComponent(componentType);
				_diffLog.Add($"[{nameof(Component)}] {objectPath}: Added component {componentType.Name}");
				_changeCount++;
				destinationList.Add(addedComponent);
			}

			for (int componentIndex = 0; componentIndex < sourceList.Count; componentIndex++)
			{
				var sourceComponentInstance = sourceList[componentIndex];
				var destinationComponentInstance = destinationList[componentIndex];

				SerializedObject sourceSerialized = new SerializedObject(sourceComponentInstance);
				SerializedObject destinationSerialized = new SerializedObject(destinationComponentInstance);

				var sourcePropertyIterator = sourceSerialized.GetIterator();
				while (sourcePropertyIterator.NextVisible(true))
				{
					if (sourcePropertyIterator.name == "m_Script")
						continue;

					var destinationProperty = destinationSerialized.FindProperty(sourcePropertyIterator.name);
					if (destinationProperty == null)
						continue;

					if (!SerializedPropertyEqualWithMap(sourcePropertyIterator, destinationProperty))
					{
						string oldValue = PropertyToString(destinationProperty);
						string newValue = PropertyToString(sourcePropertyIterator);

						if (sourcePropertyIterator.propertyType == SerializedPropertyType.ObjectReference)
						{
							var mappedReference = MapObjectReference(sourcePropertyIterator.objectReferenceValue);
							destinationProperty.objectReferenceValue = mappedReference;
							_diffLog.Add($"[{nameof(SerializedProperty)}] {objectPath}/{componentType.Name}.{sourcePropertyIterator.displayName} remapped ref: {oldValue} → {newValue}");
						}
						else
						{
							destinationSerialized.CopyFromSerializedProperty(sourcePropertyIterator);
							_diffLog.Add($"[{nameof(SerializedProperty)}] {objectPath}/{componentType.Name}.{sourcePropertyIterator.displayName} changed: {oldValue} → {newValue}");
						}

						_changeCount++;
					}
				}

				destinationSerialized.ApplyModifiedPropertiesWithoutUndo();
			}
		}

		for (int childIndex = 0; childIndex < source.transform.childCount; childIndex++)
		{
			Transform sourceChild = source.transform.GetChild(childIndex);
			Transform destinationChild = target.transform.Find(sourceChild.name);

			if (destinationChild == null)
			{
				GameObject newChild = Instantiate(sourceChild.gameObject, target.transform);
				newChild.name = sourceChild.name;
				_diffLog.Add($"[{nameof(GameObject)}] {objectPath}: Added child {CleanName(newChild.name)}");
				_changeCount++;

				ApplyDifferencesRecursive(sourceChild.gameObject, newChild, objectPath);
			}
			else
			{
				ApplyDifferencesRecursive(sourceChild.gameObject, destinationChild.gameObject, objectPath);
			}
		}
	}

	public static bool SerializedPropertyEqual(SerializedProperty a, SerializedProperty b)
	{
		if (a.propertyType != b.propertyType)
			return false;

		switch (a.propertyType)
		{
			case SerializedPropertyType.Integer: return a.intValue == b.intValue;
			case SerializedPropertyType.Boolean: return a.boolValue == b.boolValue;
			case SerializedPropertyType.Float: return Mathf.Approximately(a.floatValue, b.floatValue);
			case SerializedPropertyType.String: return a.stringValue == b.stringValue;
			case SerializedPropertyType.Color: return a.colorValue == b.colorValue;
			case SerializedPropertyType.ObjectReference: return a.objectReferenceValue == b.objectReferenceValue;
			case SerializedPropertyType.Enum: return a.enumValueIndex == b.enumValueIndex;
			case SerializedPropertyType.Vector2: return a.vector2Value == b.vector2Value;
			case SerializedPropertyType.Vector3: return a.vector3Value == b.vector3Value;
			case SerializedPropertyType.Vector4: return a.vector4Value == b.vector4Value;
			case SerializedPropertyType.Rect: return a.rectValue == b.rectValue;
			case SerializedPropertyType.Bounds: return a.boundsValue == b.boundsValue;
			case SerializedPropertyType.Quaternion: return a.quaternionValue == b.quaternionValue;
		}

		return SerializedProperty.DataEquals(a, b);
	}

	static string PropertyToString(SerializedProperty prop)
	{
		switch (prop.propertyType)
		{
			case SerializedPropertyType.Integer: return prop.intValue.ToString();
			case SerializedPropertyType.Boolean: return prop.boolValue.ToString();
			case SerializedPropertyType.Float: return prop.floatValue.ToString("0.###");
			case SerializedPropertyType.String: return prop.stringValue;
			case SerializedPropertyType.Color: return prop.colorValue.ToString();
			case SerializedPropertyType.ObjectReference: return prop.objectReferenceValue ? prop.objectReferenceValue.name : "null";
			case SerializedPropertyType.Enum: return prop.enumDisplayNames[prop.enumValueIndex];
			case SerializedPropertyType.Vector2: return prop.vector2Value.ToString();
			case SerializedPropertyType.Vector3: return prop.vector3Value.ToString();
			case SerializedPropertyType.Vector4: return prop.vector4Value.ToString();
			case SerializedPropertyType.Rect: return prop.rectValue.ToString();
			case SerializedPropertyType.Bounds: return prop.boundsValue.ToString();
			case SerializedPropertyType.Quaternion: return prop.quaternionValue.eulerAngles.ToString();
		}

		return prop.propertyType.ToString();
	}

	private int CountComponentsOfType(GameObject gameObject, System.Type componentType)
	{
		int count = 0;
		var componentsArray = gameObject.GetComponents<Component>();
		for (int componentIndex = 0; componentIndex < componentsArray.Length; componentIndex++)
		{
			if (componentsArray[componentIndex] != null && componentsArray[componentIndex].GetType() == componentType)
				count++;
		}

		return count;
	}

	private string GetRelativePath(Transform rootTransform, Transform targetTransform)
	{
		if (targetTransform == null || rootTransform == null)
			return null;

		if (!targetTransform.IsChildOf(rootTransform) && targetTransform != rootTransform)
			return null;

		System.Collections.Generic.Stack<string> pathStack = new System.Collections.Generic.Stack<string>();
		var currentTransform = targetTransform;
		while (currentTransform != null && currentTransform != rootTransform)
		{
			pathStack.Push(currentTransform.name);
			currentTransform = currentTransform.parent;
		}

		return string.Join("/", pathStack.ToArray());
	}

	private Transform FindByRelativePath(Transform rootTransform, string relativePath)
	{
		if (rootTransform == null)
			return null;

		if (string.IsNullOrEmpty(relativePath))
			return rootTransform;

		return rootTransform.Find(relativePath);
	}

	private Object MapObjectReference(Object sourceReference)
	{
		if (sourceReference == null)
			return null;

		if (_ctxSourceRoot == null || _ctxTargetRoot == null)
			return sourceReference;

		if (sourceReference is GameObject sourceGameObject)
		{
			var relativePath = GetRelativePath(_ctxSourceRoot.transform, sourceGameObject.transform);
			if (relativePath == null)
				return sourceReference;

			var destinationTransform = FindByRelativePath(_ctxTargetRoot.transform, relativePath);

			return destinationTransform ? destinationTransform.gameObject : null;
		}

		if (sourceReference is Transform sourceTransform)
		{
			var relativePath = GetRelativePath(_ctxSourceRoot.transform, sourceTransform);
			if (relativePath == null)
				return sourceReference;

			return FindByRelativePath(_ctxTargetRoot.transform, relativePath);
		}

		if (sourceReference is Component sourceComponent)
		{
			var relativePath = GetRelativePath(_ctxSourceRoot.transform, sourceComponent.transform);
			if (relativePath == null)
				return sourceReference;

			var destinationTransform = FindByRelativePath(_ctxTargetRoot.transform, relativePath);
			if (destinationTransform == null)
				return null;

			var componentType = sourceComponent.GetType();
			var sourceComponentList = sourceComponent.gameObject.GetComponents(componentType);
			var destinationComponentList = destinationTransform.gameObject.GetComponents(componentType);

			int componentIndex = System.Array.IndexOf(sourceComponentList, sourceComponent);
			if (componentIndex >= 0 && componentIndex < destinationComponentList.Length)
				return destinationComponentList[componentIndex];

			return destinationTransform.GetComponent(componentType);
		}

		return sourceReference;
	}

	private bool SerializedPropertyEqualWithMap(SerializedProperty sourceProperty, SerializedProperty destinationProperty)
	{
		if (sourceProperty.propertyType != destinationProperty.propertyType)
			return false;

		if (sourceProperty.propertyType == SerializedPropertyType.ObjectReference)
		{
			var mappedReference = MapObjectReference(sourceProperty.objectReferenceValue);
			return mappedReference == destinationProperty.objectReferenceValue;
		}

		switch (sourceProperty.propertyType)
		{
			case SerializedPropertyType.Integer: return sourceProperty.intValue == destinationProperty.intValue;
			case SerializedPropertyType.Boolean: return sourceProperty.boolValue == destinationProperty.boolValue;
			case SerializedPropertyType.Float: return Mathf.Approximately(sourceProperty.floatValue, destinationProperty.floatValue);
			case SerializedPropertyType.String: return sourceProperty.stringValue == destinationProperty.stringValue;
			case SerializedPropertyType.Color: return sourceProperty.colorValue == destinationProperty.colorValue;
			case SerializedPropertyType.Enum: return sourceProperty.enumValueIndex == destinationProperty.enumValueIndex;
			case SerializedPropertyType.Vector2: return sourceProperty.vector2Value == destinationProperty.vector2Value;
			case SerializedPropertyType.Vector3: return sourceProperty.vector3Value == destinationProperty.vector3Value;
			case SerializedPropertyType.Vector4: return sourceProperty.vector4Value == destinationProperty.vector4Value;
			case SerializedPropertyType.Rect: return sourceProperty.rectValue == destinationProperty.rectValue;
			case SerializedPropertyType.Bounds: return sourceProperty.boundsValue == destinationProperty.boundsValue;
			case SerializedPropertyType.Quaternion: return sourceProperty.quaternionValue == destinationProperty.quaternionValue;
		}

		return SerializedProperty.DataEquals(sourceProperty, destinationProperty);
	}
}
