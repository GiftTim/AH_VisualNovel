#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode.Editors
{
	using Detectors;

	using UnityEditor;
	using UnityEditor.EditorTools;
	using UnityEngine;
	using EditorTools = EditorCode.EditorTools;

	[CustomEditor(typeof (SpeedHackDetector))]
	internal class SpeedHackDetectorEditor : KeepAliveBehaviourEditor<SpeedHackDetector>
	{
		private SerializedProperty interval;
		private SerializedProperty threshold;
		private SerializedProperty maxFalsePositives;
		private SerializedProperty coolDown;
		private SerializedProperty timeJumpThreshold;
		private SerializedProperty useDsp;
		private SerializedProperty watchTimeScale;

		private protected override void FindUniqueDetectorProperties()
		{
			interval = serializedObject.FindProperty("interval");
			threshold = serializedObject.FindProperty("threshold");
			maxFalsePositives = serializedObject.FindProperty("maxFalsePositives");
			coolDown = serializedObject.FindProperty("coolDown");
			timeJumpThreshold = serializedObject.GetProperty(nameof(SpeedHackDetector.TimeJumpThreshold));
			
			useDsp = serializedObject.GetProperty(nameof(SpeedHackDetector.UseDsp));
			watchTimeScale = serializedObject.GetProperty(nameof(SpeedHackDetector.WatchTimeScale));
		}

		private protected override bool DrawUniqueDetectorProperties()
		{
			DrawHeader("Specific settings");

			EditorGUILayout.PropertyField(interval);
			EditorGUILayout.PropertyField(threshold);
			EditorGUILayout.PropertyField(maxFalsePositives);
			EditorGUILayout.PropertyField(coolDown);
			EditorGUILayout.PropertyField(timeJumpThreshold);

			EditorGUILayout.PropertyField(useDsp);
			if (useDsp.boolValue) 
				EditorGUILayout.HelpBox("Dsp timers may cause false positives on some hardware.\nMake sure to test on target devices before using this in production.", MessageType.Warning);
			
#if UNITY_AUDIO_MODULE
			if (!EditorTools.IsAudioManagerEnabled())
			{
				EditorGUILayout.HelpBox("Dsp option is not available since Disable Unity Audio option is enabled.", MessageType.Error);
				if (GUILayout.Button("Open Audio Settings"))
				{
					SettingsService.OpenProjectSettings("Project/Audio");
					Highlighter.Highlight("Project Settings", EditorTools.GetAudioManagerEnabledPropertyPath(), HighlightSearchMode.Identifier);
				}
			}
#else
			EditorGUILayout.HelpBox("Dsp option is not available since built-in Audio module is disabled.", MessageType.Error);
#endif

			EditorGUILayout.PropertyField(watchTimeScale);
			if (watchTimeScale.boolValue)
			{
				EditorGUILayout.HelpBox("TimeScale watching monitors for unauthorized changes to Time.timeScale.\n" +
				                        "Use SpeedHackDetector.SetTimeScale and AllowAnyTimeScale APIs to change timeScale safely.", MessageType.Info);
			}
			
			return true;
		}
	}
}