using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.AntiCheat.EditorCode
{
	internal struct AssetLocationData
	{
		public string AssetPath { get; }
		public string TransformPath { get; }
		public string ObjectName { get; }
		
		public AssetLocationData(string path, Object target)
		{
			AssetPath = path;

			if (target is Component component)
				TransformPath = GetFullTransformPath(component.transform);
			else
				TransformPath = string.Empty;
			
			ObjectName = $"{GetObjectName(target)} (InstanceID {target.GetInstanceID()})";
		}

		private static string GetObjectName(Object target)
		{
			if (target is Component component)
			{
				var result = component.GetType().Name;

				if ((component.hideFlags & HideFlags.HideInInspector) != 0)
					result += " (HideInInspector)";

				return result;
			}
			
			return target.name;
		}

		private static string GetFullTransformPath(Transform transform, Transform stopAt = null)
		{
			var transformPath = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				if (transform == stopAt) break;
				transformPath = transform.name + "/" + transformPath;
			}
			return transformPath;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(TransformPath))
			{
				return $"Path: {AssetPath}\n" +
					   $"Object: {ObjectName})";
			}
			else
			{
				return $"Path: {AssetPath}\n" +
					   $"Transform: {TransformPath}\n" +
					   $"Component: {ObjectName})";
			}
		}
	}
	
	internal static class AssetTools
	{
		public static AssetLocationData GetLocation(string path, Object target)
		{
			return new AssetLocationData(path, target);
		}
		
		public static string[] GetMigratableAssetsFilePaths(string[] targets)
		{
			if (targets == null || targets.Length == 0) return null;

			var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var target in targets)
			{
				if (string.IsNullOrWhiteSpace(target)) continue;

				var path = target;
				
				if (!path.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
					path = AssetDatabase.GUIDToAssetPath(target);
				
				if (string.IsNullOrEmpty(path)) continue;
				path = path.TrimEnd('/', '\\');
				
				if (AssetDatabase.IsValidFolder(path))
				{
					AddMigratableAssetsFromFolder(path, paths);
				}
				else
				{
					if (IsMigratableAsset(path))
					{
						paths.Add(path);
					}
				}
			}

			return paths.Count > 0 ? paths.ToArray() : null;
		}
		
		private static void AddMigratableAssetsFromFolder(string folderPath, HashSet<string> paths)
		{
			var guidsInFolder = AssetDatabase.FindAssets("t:ScriptableObject t:Prefab", new[] { folderPath });
			
			foreach (var innerGuid in guidsInFolder)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(innerGuid);
				if (IsMigratableAsset(assetPath))
				{
					paths.Add(assetPath);
				}
			}
		}
		
		private static bool IsMigratableAsset(string assetPath)
		{
			if (string.IsNullOrEmpty(assetPath)) return false;

			var extension = Path.GetExtension(assetPath).ToLowerInvariant();
			return extension == ".asset" || extension == ".prefab";
		}
	}
}