#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using CodeStage.EditorCommon.Tools;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace CodeStage.AntiCheat.EditorCode
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Common;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.Events;
	using Object = UnityEngine.Object;

	internal delegate bool ProcessSerializedProperty(Object target, SerializedProperty sp, AssetLocationData location, string type);
	
	internal static class EditorTools
	{
		private static SerializedObject audioManager;
		private static SerializedObject graphicsSettingsAsset;
		
		#region files and directories

		private static string directory;

		public static void DeleteFile(string path)
		{
			if (!File.Exists(path)) return;
			RemoveReadOnlyAttribute(path);
			File.Delete(path);
		}

		public static void RemoveDirectoryIfEmpty(string directoryName)
		{
			if (Directory.Exists(directoryName) && IsDirectoryEmpty(directoryName))
			{
				FileUtil.DeleteFileOrDirectory(directoryName);
				var metaFile = AssetDatabase.GetTextMetaFilePathFromAssetPath(directoryName);
				if (File.Exists(metaFile))
				{
					FileUtil.DeleteFileOrDirectory(metaFile);
				}
			}
		}

		public static bool IsDirectoryEmpty(string path)
		{
			var dirs = Directory.GetDirectories(path);
			var files = Directory.GetFiles(path);
			return dirs.Length == 0 && files.Length == 0;
		}

		public static string GetACTkDirectory()
		{
			if (!string.IsNullOrEmpty(directory))
			{
				return directory;
			}

			directory = ACTkMarker.GetAssetPath();

			if (!string.IsNullOrEmpty(directory))
			{
				if (directory.IndexOf("Editor/Scripts/ACTkMarker.cs", StringComparison.Ordinal) >= 0)
				{
					directory = directory.Replace("Editor/Scripts/ACTkMarker.cs", "");
				}
				else
				{
					directory = null;
					ACTk.PrintExceptionForSupport("Looks like Anti-Cheat Toolkit is placed in project incorrectly!");
				}
			}
			else
			{
				directory = null;
				ACTk.PrintExceptionForSupport("Can't locate the Anti-Cheat Toolkit directory!");
			}
			return directory;
		}
		
		#endregion

		public static bool CheckUnityEventHasActivePersistentListener(SerializedProperty unityEvent)
		{
			if (unityEvent == null) return false;

			var calls = unityEvent.FindPropertyRelative("m_PersistentCalls.m_Calls");
			if (calls == null)
			{
				ACTk.PrintExceptionForSupport("Can't find Unity Event calls!");
				return false;
			}
			if (!calls.isArray)
			{
				ACTk.PrintExceptionForSupport("Looks like Unity Event calls are not array anymore!");
				return false;
			}

			var result = false;

			var callsCount = calls.arraySize;
			for (var i = 0; i < callsCount; i++)
			{
				var call = calls.GetArrayElementAtIndex(i);

				var targetProperty = call.FindPropertyRelative("m_Target");
				var methodNameProperty = call.FindPropertyRelative("m_MethodName");
				var callStateProperty = call.FindPropertyRelative("m_CallState");

				if (targetProperty != null && methodNameProperty != null && callStateProperty != null &&
                    targetProperty.propertyType == SerializedPropertyType.ObjectReference &&
					methodNameProperty.propertyType == SerializedPropertyType.String &&
					callStateProperty.propertyType == SerializedPropertyType.Enum)
				{
					var target = targetProperty.objectReferenceValue;
					var methodName = methodNameProperty.stringValue;
					var callState = (UnityEventCallState)callStateProperty.enumValueIndex;

					if (target != null && !string.IsNullOrEmpty(methodName) && callState != UnityEventCallState.Off)
					{
						result = true;
						break;
					}
				}
				else
				{
					ACTk.PrintExceptionForSupport("Can't parse Unity Event call!");
				}
			}
			return result;
		}

		public static void RemoveReadOnlyAttribute(string path)
		{
			var attributes = File.GetAttributes(path);
			if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
		}

		public static string[] FindLibrariesAt(string folder, bool recursive = true)
		{
			folder = folder.Replace('\\', '/');

			if (!Directory.Exists(folder))
			{
				return Array.Empty<string>();
			}

			var allFiles = Directory.GetFiles(folder, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
			var result = new List<string>();

			foreach (var file in allFiles)
			{
				var extension = Path.GetExtension(file);
				if (string.IsNullOrEmpty(extension))
				{
					continue;
				}

				if (extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
				{
					var path = file.Replace('\\', '/');
					result.Add(path);
				}
			}

			return result.ToArray();
		}

		public static void OpenReadme()
		{
			Application.OpenURL("https://docs.codestage.net/actk/manual/");
		}
		
		public static Object GetPingableObject(Object target)
		{
			if (!AssetDatabase.Contains(target))
				return target;

			if (!(target is Component component))
				return target;

			target = component.gameObject;
			
			if (PrefabUtility.IsPartOfAnyPrefab(target))
			{
				var asset = PrefabUtility.GetCorrespondingObjectFromSource(target);
				if (asset != null)
					target = asset;
			}
			
			return target;
		}

		public static bool IsAudioManagerEnabled()
		{
			return !GetUpdatedAudioManagerDisableAudioProperty().boolValue;
		}

		public static string GetAudioManagerEnabledPropertyPath()
		{
			return GetUpdatedAudioManagerDisableAudioProperty().propertyPath;
		}
		
		private static SerializedProperty GetUpdatedAudioManagerDisableAudioProperty()
		{
			if (audioManager == null)
			{
				var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset")[0];
				audioManager = new SerializedObject(asset);
			}

			audioManager.Update();
			return audioManager.FindProperty("m_DisableAudio");
		}

		#region Graphics Settings

		/// <summary>
		/// Gets the Graphics Settings asset SerializedObject.
		/// </summary>
		/// <returns>SerializedObject for Graphics Settings, or null if not found.</returns>
		public static SerializedObject GetGraphicsSettingsAsset()
		{
			if (graphicsSettingsAsset == null)
			{
				var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
				foreach (var asset in assets)
				{
					if (asset is UnityEngine.Rendering.GraphicsSettings settings)
					{
						graphicsSettingsAsset = new SerializedObject(settings);
						break;
					}
				}
			}

			return graphicsSettingsAsset;
		}

		/// <summary>
		/// Gets the included shaders property from Graphics Settings.
		/// </summary>
		/// <returns>SerializedProperty for m_AlwaysIncludedShaders, or null if not found.</returns>
		public static SerializedProperty GetIncludedShadersProperty()
		{
			var graphicsAsset = GetGraphicsSettingsAsset();
			return graphicsAsset?.FindProperty("m_AlwaysIncludedShaders");
		}

		/// <summary>
		/// Finds the index of a shader in the Graphics Settings included shaders list.
		/// </summary>
		/// <param name="shaderName">Name of the shader to find.</param>
		/// <returns>Index of the shader, or -1 if not found.</returns>
		public static int FindShaderIndexInGraphicsSettings(string shaderName)
		{
			var graphicsAsset = GetGraphicsSettingsAsset();
			var includedShaders = GetIncludedShadersProperty();
			
			if (graphicsAsset == null || includedShaders == null) return -1;

			graphicsAsset.Update();
			var itemsCount = includedShaders.arraySize;
			
			for (var i = 0; i < itemsCount; i++)
			{
				var arrayItem = includedShaders.GetArrayElementAtIndex(i);
				if (arrayItem.objectReferenceValue != null)
				{
					var shader = arrayItem.objectReferenceValue as Shader;
					if (shader != null && shader.name == shaderName)
					{
						return i;
					}
				}
			}

			return -1;
		}

		/// <summary>
		/// Checks if a shader is included in Graphics Settings.
		/// </summary>
		/// <param name="shaderName">Name of the shader to check.</param>
		/// <returns>True if the shader is included, false otherwise.</returns>
		public static bool IsShaderIncludedInGraphicsSettings(string shaderName)
		{
			return FindShaderIndexInGraphicsSettings(shaderName) != -1;
		}

		/// <summary>
		/// Adds a shader to the Graphics Settings included shaders list.
		/// </summary>
		/// <param name="shaderName">Name of the shader to add.</param>
		/// <returns>True if the shader was added successfully, false otherwise.</returns>
		public static bool AddShaderToGraphicsSettings(string shaderName)
		{
			var shader = Shader.Find(shaderName);
			if (shader == null)
			{
				Debug.LogError($"{ACTk.LogPrefix}Could not find shader '{shaderName}'");
				return false;
			}

			var graphicsAsset = GetGraphicsSettingsAsset();
			var includedShaders = GetIncludedShadersProperty();
			
			if (graphicsAsset == null || includedShaders == null)
			{
				Debug.LogError($"{ACTk.LogPrefix}Could not access Graphics Settings");
				return false;
			}

			if (IsShaderIncludedInGraphicsSettings(shaderName))
			{
				return true;
			}

			graphicsAsset.Update();
			
			includedShaders.InsertArrayElementAtIndex(includedShaders.arraySize);
			var newElement = includedShaders.GetArrayElementAtIndex(includedShaders.arraySize - 1);
			newElement.objectReferenceValue = shader;
			
			graphicsAsset.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
			
			return true;
		}

		/// <summary>
		/// Removes a shader from the Graphics Settings included shaders list.
		/// </summary>
		/// <param name="shaderName">Name of the shader to remove.</param>
		/// <returns>True if the shader was removed successfully, false otherwise.</returns>
		public static bool RemoveShaderFromGraphicsSettings(string shaderName)
		{
			var graphicsAsset = GetGraphicsSettingsAsset();
			var includedShaders = GetIncludedShadersProperty();
			
			if (graphicsAsset == null || includedShaders == null)
			{
				Debug.LogError($"{ACTk.LogPrefix}Could not access Graphics Settings");
				return false;
			}

			var shaderIndex = FindShaderIndexInGraphicsSettings(shaderName);
			if (shaderIndex == -1)
			{
				return true;
			}

			graphicsAsset.Update();
			
			includedShaders.DeleteArrayElementAtIndex(shaderIndex);
			
			graphicsAsset.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();
			
			return true;
		}

		/// <summary>
		/// Opens the Graphics Settings window and highlights the included shaders section.
		/// </summary>
		public static void OpenGraphicsSettingsWithHighlight()
		{
			SettingsService.OpenProjectSettings("Project/Graphics");

			EditorApplication.delayCall += () =>
			{
				Highlighter.Highlight("Project Settings", "m_AlwaysIncludedShaders", HighlightSearchMode.Identifier);
			};
		}

		#endregion

		#region Traversal
		
		private static string GetTransformPath(Transform transform)
		{
			var path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}
		
		public static void TraverseSerializedScriptsAssets(string[] assetPaths, ProcessSerializedProperty itemCallback, string[] typesFilter)
		{
			var touchedObjectsCount = 0;
			var scannedObjectsCont = 0;
			var scannedAssetsCont = 0;
			
			try
			{
				const string progressHeader = "ACTk: Looking through assets";
				var targets = new Dictionary<Object, AssetLocationData>();

				EditorUtility.DisplayProgressBar(progressHeader, "Collecting data...", 0);
				
				AssetDatabase.SaveAssets();
				AssetDatabase.StartAssetEditing();

				if (assetPaths == null || assetPaths.Length == 0)
				{
					var guids = AssetDatabase.FindAssets("t:ScriptableObject t:Prefab");
					assetPaths = new string[guids.Length];
					for (var i = 0; i < guids.Length; i++)
					{
						var guid = guids[i];
						assetPaths[i] = AssetDatabase.GUIDToAssetPath(guid);
					}
				}

				var count = assetPaths.Length;
				foreach (var assetPath in assetPaths)
				{
					if (EditorUtility.DisplayCancelableProgressBar(progressHeader,
							"Asset " + (scannedAssetsCont + 1) + " from " + count,
							scannedAssetsCont / (float)count))
					{
						Debug.Log(ACTk.LogPrefix + "operation canceled by user.");
						break;
					}
					
					if (!assetPath.StartsWith("assets", StringComparison.OrdinalIgnoreCase)) continue;
					
					try
					{
						var isPrefab = Path.GetExtension(assetPath) == ".prefab";
						targets.Clear();
						
						if (!isPrefab)
						{
							try
							{
								var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
								foreach (var unityObject in objects)
								{
									if (unityObject == null) continue;
									if (unityObject.name == "Deprecated EditorExtensionImpl") continue;
									if (targets.ContainsKey(unityObject)) continue;
									targets.Add(unityObject, AssetTools.GetLocation(assetPath, unityObject));
								}
							}
							catch (Exception e)
							{
								ACTk.PrintExceptionForSupport($"Failed to load ScriptableObject asset: {assetPath}", e);
								Debug.LogError($"{ACTk.LogPrefix}Error loading asset: {assetPath}\nSkipping this asset due to corruption or serialization issues.", null);
								scannedAssetsCont++;
								continue;
							}
						}
						else
						{
							try
							{
								var root = AssetDatabase.LoadMainAssetAtPath(assetPath) as GameObject;
								if (!root)
								{
									Debug.LogWarning($"{ACTk.LogPrefix}Could not load prefab as GameObject: {assetPath}");
									scannedAssetsCont++;
									continue;
								}
								var components = root.GetComponentsInChildren<Component>(true);
								foreach (var component in components)
								{
									if (!component) continue;
									if (targets.ContainsKey(component)) continue;
									targets.Add(component, AssetTools.GetLocation(assetPath, component));
								}
							}
							catch (Exception e)
							{
								ACTk.PrintExceptionForSupport($"Failed to load prefab asset: {assetPath}", e);
								Debug.LogError($"{ACTk.LogPrefix}Error loading prefab: {assetPath}\nSkipping this asset due to corruption or serialization issues.", null);
								scannedAssetsCont++;
								continue;
							}
						}
						
						foreach (var target in targets)
						{
							var unityObject = target.Key;

							try
							{
								var so = new SerializedObject(unityObject);
								var modified = ProcessObject(unityObject, so, target.Value, typesFilter, itemCallback);

								if (modified)
								{
									touchedObjectsCount++;
									so.ApplyModifiedProperties();
								}

								scannedObjectsCont++;
							}
							catch (Exception e)
							{
								var objectName = unityObject?.name ?? "null";
								var objectType = unityObject?.GetType().Name ?? "unknown";
								ACTk.PrintExceptionForSupport($"Failed to process object '{objectName}' of type '{objectType}' in asset: {assetPath}", e);
								Debug.LogError($"{ACTk.LogPrefix}Error processing object '{objectName}' ({objectType}) in asset: {assetPath}\nSkipping this object due to serialization issues.", unityObject);
								scannedObjectsCont++;
								continue;
							}
						}
					}
					catch (Exception e)
					{
						ACTk.PrintExceptionForSupport($"Unexpected error while processing asset: {assetPath}", e);
						Debug.LogError($"{ACTk.LogPrefix}Unexpected error in asset: {assetPath}\nSkipping this asset.", null);
					}

					scannedAssetsCont++;
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Something went wrong while traversing objects!", e);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				AssetDatabase.SaveAssets();
				EditorUtility.ClearProgressBar();
			}

			Debug.Log($"{ACTk.LogPrefix}Objects modified: {touchedObjectsCount}, scanned: {scannedObjectsCont}");
		}

		
		public static void TraverseSerializedScriptsInOpenedScenes(ProcessSerializedProperty itemCallback, string[] typesFilter, bool skipSave = false)
		{
			var openedScenesPaths = CSSceneTools.GetOpenedValidScenesPaths();
			TraverseSerializedScriptsInScenes(openedScenesPaths, itemCallback, typesFilter, skipSave);
		}
		
		public static void TraverseSerializedScriptsInBuildProfilesScenes(ProcessSerializedProperty itemCallback, string[] typesFilter, bool skipSave = false)
		{
			var buildProfilesScenesPaths = CSSceneTools.GetBuildProfilesScenesPaths();
			var originalScenes = CSSceneTools.GetOpenedScenes();
			TraverseSerializedScriptsInScenes(buildProfilesScenesPaths, itemCallback, typesFilter, skipSave, originalScenes);
		}
		
		private static void TraverseSerializedScriptsInScenes(string[] scenePaths, ProcessSerializedProperty itemCallback, string[] typesFilter, bool skipSave, Scene[] originalScenes = null)
		{
			var touchedCount = 0;
			var scannedCont = 0;
			
			try
			{
				const string progressHeader = "ACTk: Looking through scenes";

				EditorUtility.DisplayProgressBar(progressHeader, "Collecting data...", 0);

				foreach (var scenePath in scenePaths)
				{
					try
					{
						var scene = SceneManager.GetSceneByPath(scenePath);
						if (!scene.IsValid())
							scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
						
						var roots = scene.GetRootGameObjects();
						var count = roots.Length;

						for (var j = 0; j < count; j++)
						{
							var root = roots[j];
							if (EditorUtility.DisplayCancelableProgressBar(progressHeader,
									"Item " + (j + 1) + " from " + count,
									j / (float)count))
							{
								Debug.Log(ACTk.LogPrefix + "operation canceled by user.");
								break;
							}

							try
							{
								var components = root.GetComponentsInChildren<Component>(true);

								foreach (var component in components)
								{
									if (!component) continue;
									
									try
									{
										var so = new SerializedObject(component);
										var modified = ProcessObject(component, so, AssetTools.GetLocation(scene.path, component), typesFilter, itemCallback);
										if (modified)
										{
											EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
											touchedCount++;
											so.ApplyModifiedProperties();
										}

										scannedCont++;
									}
									catch (Exception e)
									{
										var componentName = component?.name ?? "null";
										var componentType = component?.GetType().Name ?? "unknown";
										var gameObjectPath = component?.transform ? GetTransformPath(component.transform) : "unknown";
										ACTk.PrintExceptionForSupport($"Failed to process component '{componentName}' of type '{componentType}' at '{gameObjectPath}' in scene: {scenePath}", e);
										Debug.LogError($"{ACTk.LogPrefix}Error processing component '{componentName}' ({componentType}) at '{gameObjectPath}' in scene: {scenePath}\nSkipping this component due to serialization issues.", component);
										scannedCont++;
										continue;
									}
								}
							}
							catch (Exception e)
							{
								var rootName = root?.name ?? "null";
								ACTk.PrintExceptionForSupport($"Failed to process GameObject '{rootName}' in scene: {scenePath}", e);
								Debug.LogError($"{ACTk.LogPrefix}Error processing GameObject '{rootName}' in scene: {scenePath}\nSkipping this GameObject.", root);
								continue;
							}
						}
						
						if (scene.isDirty && !skipSave) 
							EditorSceneManager.SaveScene(scene);

						if (originalScenes != null && !Array.Exists(originalScenes,item => item == scene))
						{
							EditorSceneManager.CloseScene(scene, true);
						}
					}
					catch (Exception e)
					{
						ACTk.PrintExceptionForSupport($"Failed to process scene: {scenePath}", e);
						Debug.LogError($"{ACTk.LogPrefix}Error processing scene: {scenePath}\nSkipping this scene due to serialization or loading issues.", null);
						continue;
					}
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("Something went wrong while traversing objects!", e);
			}
			finally
			{
				AssetDatabase.SaveAssets();
				EditorUtility.ClearProgressBar();
			}

			Debug.Log($"{ACTk.LogPrefix}Objects modified: {touchedCount}, scanned: {scannedCont}");
		}
		
		private static bool ProcessObject(Object target, SerializedObject so, AssetLocationData location, string[] typesFilter,
			ProcessSerializedProperty callback)
		{
			var modified = false;

			var sp = so.GetIterator();
			if (sp == null) 
				return false;

			while (sp.NextVisible(true))
			{
				if (sp.propertyType != SerializedPropertyType.Generic) 
					continue;
				var type = sp.type;
				if (Array.IndexOf(typesFilter, type) == -1) 
					continue;
				if (sp.isArray)
					continue;
				
				modified |= callback(target, sp, location, type);
			}

			return modified;
		}

		#endregion
	}
}