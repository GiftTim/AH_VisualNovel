#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes.EditorCode;
using UnityEngine.SceneManagement;

namespace CodeStage.AntiCheat.EditorCode
{
	using Common;
	using ObscuredTypes;
	using UnityEditor;
	using UnityEngine;
	using Object = UnityEngine.Object;

	/// <summary>
	/// Class with utility functions to help with ACTk migrations after updates.
	/// </summary>
	public static class ObscuredTypesMigrator
	{
		private const string ModuleName = "Obscured Types Migration";
		private static bool fixOnlyMode = false;
		
		private delegate bool MigrateDelegate(SerializedProperty sp, bool fixOnly);
		
		private static readonly Dictionary<Type, MigrateDelegate> MigrateMappings = new Dictionary<Type, MigrateDelegate>();

		/// <summary>
		/// Checks all assets in project for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateProjectAssets(bool skipInteraction = false)
		{
			MigrateProjectAssets(false, skipInteraction);
		}

		/// <summary>
		/// Checks all assets in project for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateProjectAssets(bool fixOnly, bool skipInteraction)
		{
			MigrateProjectAssets(null, fixOnly, skipInteraction);
		}
		
		/// <summary>
		/// Checks specified assets in project for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateProjectAssets(string[] assetPaths, bool fixOnly, bool skipInteraction)
		{
			var result = 0;
			
			if (!skipInteraction)
			{
				if (assetPaths == null || assetPaths.Length == 0)
				{
					result = EditorUtility.DisplayDialogComplex(ModuleName,
						"Are you sure you wish to scan ALL Prefabs and Scriptable Objects and automatically migrate and / or fix invalid values?\n" +
						"Select 'Migrate and fix' only if you did update from the older ACTk version.",
						"Migrate and fix", "Cancel", "Fix only");
				}
				else if (assetPaths.Length > 1000)
					result = EditorUtility.DisplayDialogComplex(ModuleName,
						$"Are you sure you wish to scan {assetPaths.Length} Prefabs and Scriptable Objects and automatically migrate and / or fix invalid values?\n" +
						"Select 'Migrate and fix' only if you did update from the older ACTk version.",
						"Migrate and fix", "Cancel", "Fix only");
			}
			
			if (!skipInteraction)
			{
				switch (result)
				{
					case 0:
						fixOnly = false;
						break;
					case 2:
						fixOnly = true;
						break;
					default:
						Debug.Log(ACTk.LogPrefix + ModuleName + ": canceled by user.");
						return;
				}
			}
			
			fixOnlyMode = fixOnly;
			BuildDynamicMappings();
			var typeNames = TypeCache.GetTypesDerivedFrom<ISerializableObscuredType>()
				.Where(t => !t.IsAbstract && !t.IsInterface)
				.Select(t => t.Name)
				.ToArray();
			EditorTools.TraverseSerializedScriptsAssets(assetPaths, ProcessProperty, typeNames);
			
			Debug.Log(ACTk.LogPrefix + ModuleName + ": complete.");
		}
		
		/// <summary>
		/// Checks all currently opened scenes for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateOpenedScenes(bool skipInteraction = false)
		{
			MigrateOpenedScenes(false, skipInteraction);
		}
		
		/// <summary>
		/// Checks all currently opened scenes for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateOpenedScenes(bool fixOnly, bool skipInteraction, bool skipSave = false)
		{
			if (!skipInteraction)
			{
				var confirmationResult = ConfirmMigration("all opened Scenes");
				switch (confirmationResult)
				{
					case 0:
						fixOnly = false;
						break;
					case 2:
						fixOnly = true;
						break;
					default:
						Debug.Log(ACTk.LogPrefix + ModuleName + ": canceled by user.");
						return;
				}
			}

			BuildDynamicMappings();
			MigrateScenes(EditorTools.TraverseSerializedScriptsInOpenedScenes, fixOnly, skipSave);
		}

		/// <summary>
		/// Checks all scenes in Build Settings for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateBuildProfilesScenes(bool skipInteraction = false)
		{
			MigrateBuildProfilesScenes(false, skipInteraction);
		}
		
		/// <summary>
		/// Checks all scenes in Build Settings for old version of obscured types and tries to migrate values to the new version
		/// or fix corrupt states if possible.
		/// </summary>
		public static void MigrateBuildProfilesScenes(bool fixOnly, bool skipInteraction, bool skipSave = false)
		{
			if (!skipInteraction)
			{
				var confirmationResult = ConfirmMigration("all scenes in " + ACTkMenuItems.BuildProfilesLabel);
				switch (confirmationResult)
				{
					case 0:
						fixOnly = false;
						break;
					case 2:
						fixOnly = true;
						break;
					default:
						Debug.Log(ACTk.LogPrefix + ModuleName + ": canceled by user.");
						return;
				}
			}

			BuildDynamicMappings();
			MigrateScenes(EditorTools.TraverseSerializedScriptsInBuildProfilesScenes, fixOnly, skipSave);
		}

		private static void MigrateScenes(Action<ProcessSerializedProperty, string[], bool> traverseAction, bool fixOnly, bool skipSave)
		{
			fixOnlyMode = fixOnly;
			var types = TypeCache.GetTypesDerivedFrom<ISerializableObscuredType>().Where(t => !t.IsAbstract && !t.IsInterface)
				.Select(type => type.Name).ToArray();
			traverseAction(ProcessProperty, types, skipSave);

			Debug.Log(ACTk.LogPrefix + ModuleName + ": complete.");
		}

		private static int ConfirmMigration(string target)
		{
			return EditorUtility.DisplayDialogComplex(ModuleName,
				$"Are you sure you wish to scan {target} and automatically migrate and / or fix invalid values?\n" +
				"Select 'Migrate and fix' only if you did update from the older ACTk version.",
				"Migrate and fix", "Cancel", "Fix only");
		}

		private static bool ProcessProperty(Object target, SerializedProperty sp, AssetLocationData location, string type)
		{
			var obscured = sp.GetValue<ISerializableObscuredType>();
            if (obscured == null || obscured.IsDataValid)
                return false;

			if (MigrateMappings.TryGetValue(obscured.GetType(), out var migrate))
            {
                var modified = migrate(sp, fixOnlyMode);

                if (modified)
                    Debug.Log($"{ACTk.LogPrefix}{ModuleName} migrated property {sp.displayName}:{type} at\n{location.ToString()}", target);

                return modified;
            }

            return false;
		}
		
		private static bool Migrate<TSerialized>(SerializedProperty sp, bool fixOnly) where TSerialized : ISerializedObscuredType, new()
		{
			var serialized = new TSerialized();
			serialized.Init(sp);

			if (!fixOnly && serialized.IsCanMigrate)
				return serialized.Migrate();
			
			return serialized.Fix();
		}

		private static void BuildDynamicMappings()
		{
			MigrateMappings.Clear();
			var baseGeneric1 = typeof(SerializedObscuredType<>);
			var baseGeneric2 = typeof(MigratableSerializedObscuredType<>);
			var targetAssembly = typeof(SerializedObscuredInt).Assembly;
			var allTypes = targetAssembly.GetTypes();
			var obscuredAssembly = typeof(ObscuredInt).Assembly;
			foreach (var helperType in allTypes.Where(t => t.IsClass && !t.IsAbstract && (IsSubclassOfGeneric(t, baseGeneric1) || IsSubclassOfGeneric(t, baseGeneric2))))
			{
				var obscuredTypeName = helperType.Name.Replace("Serialized", string.Empty);
				var obscuredType = obscuredAssembly.GetType($"{typeof(ObscuredInt).Namespace}.{obscuredTypeName}");
				if (obscuredType == null) continue;
				var method = typeof(ObscuredTypesMigrator).GetMethod(nameof(Migrate), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
				var generic = method.MakeGenericMethod(helperType);
				MigrateMappings[obscuredType] = (sp, fixOnly) => (bool)generic.Invoke(null, new object[] { sp, fixOnly });
			}
		}

		private static bool IsSubclassOfGeneric(Type type, Type genericBase)
		{
			while (type != null && type != typeof(object))
			{
				var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
				if (current == genericBase) return true;
				type = type.BaseType;
			}
			return false;
		}
	}
}