#if UNITY_DOTS_ENABLED
using UnityEngine;
using Unity.Entities;
using CodeStage.AntiCheat.Storage;
using CodeStage.AntiCheat.Detectors;
using CodeStage.AntiCheat.Time;
using CodeStage.AntiCheat.Genuine.CodeHash;
#if UNITY_ANDROID
using CodeStage.AntiCheat.Genuine.Android;
#endif

namespace CodeStage.AntiCheat.Examples
{
	/// <summary>
	/// Clean, professional DOTS UI following ECS patterns
	/// Separates UI presentation from data management
	/// </summary>
	public class ActkDotsUI : MonoBehaviour
	{
		private EntityManager em;
		private Rect windowRect = new(10, 10, 600, 500);
		private const int WindowId = 0xD05;

		// UI input fields (local state only)
		private string prefsValue = "0";
		private string fileValue = "0";
		private string filePrefsValue = "0";
		
		// Flags to track when to update input fields from ECS data
		private bool shouldUpdatePrefsValue = false;
		private bool shouldUpdateFileValue = false;
		private bool shouldUpdateFilePrefsValue = false;

		private void Awake()
		{
			var world = World.DefaultGameObjectInjectionWorld;
			em = world.EntityManager;
			
			ObscuredFilePrefs.Init();
		}

		private void OnDestroy()
		{
			SpeedHackProofTime.Dispose();
		}

		private void OnGUI()
		{
			windowRect = GUILayout.Window(WindowId, windowRect, DrawWindow, "ACTk • DOTS HUD");
		}

		private void DrawWindow(int id)
		{
			// Get UI data from ECS
			var uiData = GetUIData();
			
			UIHelpers.BeginTwoColumnLayout();
			
			// Left Column - Player Data & Storage
			DrawPlayerDataSection(uiData);
			UIHelpers.DrawSpacer();
			DrawStorageSection();
			
			UIHelpers.BeginSecondColumn();
			
			// Right Column - Code Hash, Android Features, Detectors
			DrawCodeHashSection(uiData);
			UIHelpers.DrawSpacer();
			
#if UNITY_ANDROID
			DrawAndroidSection(uiData);
			UIHelpers.DrawSpacer();
#endif
			
			DrawSpeedHackProofTimeSection(uiData);
			UIHelpers.DrawSpacer();
			DrawDetectorsSection(uiData);
			
			UIHelpers.EndTwoColumnLayout();
			
			GUI.DragWindow(new Rect(0, 0, 10000, 20));
		}

		#region UI Data Access

		private UIData GetUIData()
		{
			try
			{
				if (em.CreateEntityQuery(typeof(UIData)).TryGetSingleton<UIData>(out var uiData))
					return uiData;
			}
			catch (System.Exception e)
			{
				Debug.LogError($"[ACTk] Failed to get UI data: {e.Message}");
			}
			return new UIData(); // Return default if not found
		}

		#endregion

		#region UI Sections

		private void DrawPlayerDataSection(UIData uiData)
		{
			UIHelpers.DrawSectionHeader("Player Data:");
			GUILayout.Label($"Score:  {uiData.DisplayScore}");
			GUILayout.Label($"Health: {uiData.DisplayHealth:0}");
			UIHelpers.DrawSpacer();

			UIHelpers.DrawButtonGroup(
				("+10 Score", () => CreateUIAction(UIActionType.ModifyScore, 10), true),
				("Reset Score", () => CreateUIAction(UIActionType.SetScore, 0), true)
			);

			UIHelpers.DrawButtonGroup(
				("+10 Health", () => CreateUIAction(UIActionType.ModifyHealth, 0, 10f), true),
				("Reset Health", () => CreateUIAction(UIActionType.SetHealth, 0, 100f), true)
			);

			UIHelpers.DrawSpacer();
			GUILayout.Label($"Cheats: {uiData.CheatCount} (last code: {uiData.LastCheatCode})");

			UIHelpers.DrawSmallButtonGroup(
				("Simulate Cheat", () => CreateUIAction(UIActionType.SimulateCheat)),
				("Clear Cheats", () => CreateUIAction(UIActionType.ClearCheats))
			);
		}

		private void DrawStorageSection()
		{
			UIHelpers.DrawSectionHeader("Storage Examples:");
			
			// Update input fields from ECS data only when flags are set
			if (shouldUpdatePrefsValue)
			{
				var uiData = GetUIData();
				prefsValue = uiData.PrefsValue.ToString();
				shouldUpdatePrefsValue = false;
			}
			
			if (shouldUpdateFileValue)
			{
				var uiData = GetUIData();
				fileValue = uiData.FileValue.ToString();
				shouldUpdateFileValue = false;
			}
			
			if (shouldUpdateFilePrefsValue)
			{
				var uiData = GetUIData();
				filePrefsValue = uiData.FilePrefsValue.ToString();
				shouldUpdateFilePrefsValue = false;
			}
			
			UIHelpers.DrawStorageField("ObscuredPrefs:", ref prefsValue,
				() => { if (int.TryParse(prefsValue, out int val)) CreateUIAction(UIActionType.SetObscuredPrefs, val); },
				() => {
					CreateUIAction(UIActionType.GetObscuredPrefs);
					shouldUpdatePrefsValue = true;
				}
			);
			
			UIHelpers.DrawStorageField("ObscuredFile:", ref fileValue,
				() => { if (int.TryParse(fileValue, out int val)) CreateUIAction(UIActionType.SetObscuredFile, val); },
				() => {
					CreateUIAction(UIActionType.GetObscuredFile);
					shouldUpdateFileValue = true;
				}
			);
			
			UIHelpers.DrawStorageField("ObscuredFilePrefs:", ref filePrefsValue,
				() => { if (int.TryParse(filePrefsValue, out int val)) CreateUIAction(UIActionType.SetObscuredFilePrefs, val); },
				() => {
					CreateUIAction(UIActionType.GetObscuredFilePrefs);
					shouldUpdateFilePrefsValue = true;
				}
			);
		}

		private void DrawCodeHashSection(UIData uiData)
		{
			UIHelpers.DrawSectionHeader("Code Hash Generator:");
			GUILayout.Label($"Hash: {uiData.CodeHash}");
			
			bool canGenerate = !uiData.IsGeneratingHash && CodeHashGenerator.IsTargetPlatformCompatible();
			GUI.enabled = canGenerate;
			UIHelpers.DrawSmallButtonGroup(
				("Generate Hash", () => CreateUIAction(UIActionType.GenerateCodeHash))
			);
			GUI.enabled = true;
		}

#if UNITY_ANDROID
		private void DrawAndroidSection(UIData uiData)
		{
			UIHelpers.DrawSectionHeader("App Installation Source:");
			GUILayout.Label($"Source: {uiData.InstallationSource}");
			UIHelpers.DrawSmallButtonGroup(
				("Check Source", () => CreateUIAction(UIActionType.CheckInstallationSource))
			);
			
			UIHelpers.DrawSpacer();
			UIHelpers.DrawSectionHeader("Screen Recording Blocker:");
			GUILayout.Label($"Status: {(uiData.IsScreenRecordingBlocked ? "Blocked" : "Allowed")}");
			UIHelpers.DrawSmallButtonGroup(
				("Block Recording", () => CreateUIAction(UIActionType.BlockScreenRecording)),
				("Allow Recording", () => CreateUIAction(UIActionType.AllowScreenRecording))
			);
		}
#endif

		private void DrawSpeedHackProofTimeSection(UIData uiData)
		{
			UIHelpers.DrawSectionHeader("SpeedHackProofTime:");
			GUILayout.Label($"Time: {uiData.ProofTime:F2}");
			GUILayout.Label($"DeltaTime: {uiData.ProofDeltaTime:F4}");
			GUILayout.Label($"UnscaledTime: {uiData.ProofUnscaledTime:F2}");
			GUILayout.Label($"RealtimeSinceStartup: {uiData.ProofRealtimeSinceStartup:F2}");
		}

		private void DrawDetectorsSection(UIData uiData)
		{
			UIHelpers.DrawSectionHeader("Detectors:");
			UIHelpers.DrawDetectorStatus("Speed Hack", SpeedHackDetector.Instance?.IsRunning ?? false, uiData.SpeedHackDetected);
			UIHelpers.DrawDetectorStatus("Wall Hack", WallHackDetector.Instance?.IsRunning ?? false, uiData.WallHackDetected);
			UIHelpers.DrawDetectorStatus("Time Cheating", TimeCheatingDetector.Instance?.IsRunning ?? false, uiData.TimeCheatingDetected);
			UIHelpers.DrawDetectorStatus("Obscured Cheating", ObscuredCheatingDetector.Instance?.IsRunning ?? false, uiData.ObscuredCheatingDetected);
#if !ENABLE_IL2CPP
			UIHelpers.DrawDetectorStatus("Mono Injection", InjectionDetector.Instance?.IsRunning ?? false, uiData.InjectionDetected);
#endif
		}

		#endregion

		#region Action Creation

		private void CreateUIAction(UIActionType actionType, int intValue = 0, float floatValue = 0f, string stringValue = "")
		{
			try
			{
				UIHelpers.CreateUIAction(em, actionType, intValue, floatValue, stringValue);
			}
			catch (System.Exception e)
			{
				Debug.LogError($"[ACTk] Failed to create UI action {actionType}: {e.Message}");
			}
		}

		#endregion
	}
}
#endif
