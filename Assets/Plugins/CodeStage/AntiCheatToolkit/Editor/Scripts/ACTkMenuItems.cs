#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;
using CodeStage.AntiCheat.Common;
using CodeStage.AntiCheat.Detectors;
using CodeStage.AntiCheat.EditorCode.PostProcessors;
using CodeStage.AntiCheat.EditorCode.Processors;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode
{
	internal static class ACTkMenuItems
	{
#if UNITY_6000_0_OR_NEWER
		public const string BuildProfilesLabel = "Build Profiles";
#else
		public const string BuildProfilesLabel = "Build Settings";
#endif
		
		// ---------------------------------------------------------------
		//  Main menu items
		// ---------------------------------------------------------------

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Settings...", false, 100)]
		private static void ShowSettingsWindow()
		{
			ACTkSettings.Show();
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Injection Detector Whitelist Editor...", false, 1000)]
		private static void ShowAssembliesWhitelistWindow()
		{
			UserWhitelistEditor.ShowWindow();
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Calculate external build hashes", false, 1200)]
		private static async void HashExternalBuild()
		{
			try
			{
				var buildHashes = await CodeHashGeneratorPostprocessor.CalculateExternalBuildHashesAsync(null, true);
				if (buildHashes == null || buildHashes.FileHashes.Count == 0)
				{
					Debug.LogError(ACTk.LogPrefix + "External build hashing was not successful. " +
								   "See previous log messages for possible details.");
				}
			}
			catch (Exception e)
			{
				ACTk.PrintExceptionForSupport("External build hashing exception!", e);
			}
		}
		
		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Configure proguard-user.txt", false, 1201)]
		private static void CheckProGuard()
		{
			BuildPreProcessor.CheckProGuard(true);
		}
		
		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate obscured types in Prefabs and Scriptable Objects...", false, 1500)]
		private static void MigrateObscuredTypesInAssets()
		{
			MigrateAssets();
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate obscured types in opened scenes...", false, 1501)]
		private static void MigrateObscuredTypesInScene()
		{
			ObscuredTypesMigrator.MigrateOpenedScenes();
		}		
		
		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Migrate/Migrate obscured types in " + BuildProfilesLabel + " scenes...",
			false, 1502)]
		private static void MigrateObscuredTypesInBuildSettingsScenes()
		{
			ObscuredTypesMigrator.MigrateBuildProfilesScenes();
		}
		
		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Validate/Validate obscured types in Prefabs and Scriptable Objects...", false, 1500)]
		private static void ValidateObscuredTypesInAssets()
		{
			ValidateAssets();
		}

		private static void ValidateAssets(string[] assetPaths = null)
		{
			var invalidAssetsPaths = ObscuredTypesValidator.ValidateProjectAssets(assetPaths);
			if (invalidAssetsPaths != null && invalidAssetsPaths.Length > 0)
			{
				var result = ConfirmMigrationAfterValidation();
				
				switch (result)
				{
					case 0:
						MigrateAssets(invalidAssetsPaths, true);
						break;
					case 2:
						MigrateAssets(invalidAssetsPaths, true, true);
						break;
				}
			}
		}

		private static void MigrateAssets(string[] assetPaths = null, bool fixOnly = false, bool skipInteraction = false)
		{
			ObscuredTypesMigrator.MigrateProjectAssets(assetPaths, fixOnly, skipInteraction);
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Validate/Validate obscured types in opened scenes...", false,
			1501)]
		private static void ValidateObscuredTypesInOpenedScenes()
		{
			ValidateObscuredTypesInScenes(() => ObscuredTypesValidator.ValidateOpenedScenes(),
				(fixOnly, skipInteraction) => ObscuredTypesMigrator.MigrateOpenedScenes(fixOnly, skipInteraction));
		}

		[MenuItem(ACTkEditorConstants.ToolsMenuPath + "Validate/Validate obscured types in " + BuildProfilesLabel + " scenes...",
			false, 1502)]
		private static void ValidateObscuredTypesInBuildSettingsScenes()
		{
			ValidateObscuredTypesInScenes(() => ObscuredTypesValidator.ValidateBuildProfilesScenes(),
				(fixOnly, skipInteraction) =>
					ObscuredTypesMigrator.MigrateBuildProfilesScenes(fixOnly, skipInteraction));
		}

		private static void ValidateObscuredTypesInScenes(Func<int> validateFunc, Action<bool, bool> migrateFunc)
		{
			var invalidPropertiesFound = validateFunc();
			if (invalidPropertiesFound > 0)
			{
				var result = ConfirmMigrationAfterValidation();
				switch (result)
				{
					case 0:
						migrateFunc(true, false);
						break;
					case 2:
						migrateFunc(true, true);
						break;
				}
			}
		}

		// ---------------------------------------------------------------
		//  GameObject menu items
		// ---------------------------------------------------------------

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + "All detectors", false, 0)]
		private static void AddAllDetectorsToScene()
		{
			AddInjectionDetectorToScene();
			AddObscuredCheatingDetectorToScene();
			AddSpeedHackDetectorToScene();
			AddWallHackDetectorToScene();
			AddTimeCheatingDetectorToScene();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + InjectionDetector.ComponentName, false, 1)]
		private static void AddInjectionDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<InjectionDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + ObscuredCheatingDetector.ComponentName, false, 1)]
		private static void AddObscuredCheatingDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<ObscuredCheatingDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + SpeedHackDetector.ComponentName, false, 1)]
		private static void AddSpeedHackDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<SpeedHackDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + WallHackDetector.ComponentName, false, 1)]
		private static void AddWallHackDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<WallHackDetector>();
		}

		[MenuItem(ACTkEditorConstants.GameObjectMenuPath + TimeCheatingDetector.ComponentName, false, 1)]
		private static void AddTimeCheatingDetectorToScene()
		{
			DetectorTools.SetupDetectorInScene<TimeCheatingDetector>();
		}
		
		// ---------------------------------------------------------------
		//  Project menu items
		// ---------------------------------------------------------------
		
		[MenuItem(ACTkEditorConstants.AssetsMenuPath + "Validate Obscured Types on Prefabs and Scriptable Objects", false, 200)]
		private static void ValidateObscuredTypesInSelectedFolder()
		{
			var targetFiles = AssetTools.GetMigratableAssetsFilePaths(Selection.assetGUIDs);
			ValidateAssets(targetFiles);
		}
		
		[MenuItem(ACTkEditorConstants.AssetsMenuPath + "Validate Obscured Types on Prefabs and Scriptable Objects", true, 200)]
		private static bool ValidateObscuredTypesInSelectedFolderValidate()
		{
			var targetFiles = AssetTools.GetMigratableAssetsFilePaths(Selection.assetGUIDs);
			return targetFiles != null && targetFiles.Length > 0;
		}
		
		[MenuItem(ACTkEditorConstants.AssetsMenuPath + "Migrate Obscured Types on Prefabs and Scriptable Objects", false, 200)]
		private static void MigrateObscuredTypesInSelectedFolder()
		{
			var targetFiles = AssetTools.GetMigratableAssetsFilePaths(Selection.assetGUIDs);
			MigrateAssets(targetFiles);
		}
		
		[MenuItem(ACTkEditorConstants.AssetsMenuPath + "Migrate Obscured Types on Prefabs and Scriptable Objects", true, 200)]
		private static bool MigrateObscuredTypesInSelectedFolderValidate()
		{
			var targetFiles = AssetTools.GetMigratableAssetsFilePaths(Selection.assetGUIDs);
			return targetFiles != null && targetFiles.Length > 0;
		}

		private static int ConfirmMigrationAfterValidation()
		{
			return EditorUtility.DisplayDialogComplex(ObscuredTypesValidator.ModuleName,
				"Invalid types found. It's recommended to try migrate and / or fix them.\n" +
				"Select 'Migrate and fix' only if you did update from the older ACTk version.",
				"Migrate and fix", "Cancel", "Fix only");
		}
	}
}