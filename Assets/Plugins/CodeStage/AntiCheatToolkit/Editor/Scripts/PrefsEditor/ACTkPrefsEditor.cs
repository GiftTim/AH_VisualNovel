#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.EditorCode
{
	using Common;
	using Storage;

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using UnityEditor;
	using UnityEditor.Callbacks;
	using UnityEngine;
#if UNITY_EDITOR_WIN
	using Microsoft.Win32;
#elif UNITY_EDITOR_OSX
	using System.IO;
#else // LINUX
	using System.IO;
	using System.Xml;
#endif

	internal partial class ACTkPrefsEditor : EditorWindow, IHasCustomMenu
	{
		internal const string UnknownValueDescription = "Value corrupted / wrong Unity version";
		internal const string UnsupportedValueDescription = "Not editable value";
		
		private const int RecordsPerPage = 50;
		private const string StringTooLong = "String is too long";

		internal static ACTkPrefsEditor instance;

		// 180, 255, 180, 255
		// note the 2 alpha - it's to make disabled components look as usual
		private readonly Color obscuredColor = new Color(0.706f, 1f, 0.706f, 2f);
		
		// Light violet color for Unity-managed prefs
		private readonly Color unityManagedColor = new Color(0.8f, 0.7f, 1f, 2f);
		
		internal Color UnityManagedColor => unityManagedColor;

		[SerializeField]
		private SortingType sortingType = SortingType.KeyAscending;

		[SerializeField]
		private string searchPattern;

		[SerializeField]
		private List<PrefsRecord> allRecords;

		[SerializeField]
		private List<PrefsRecord> filteredRecords;

		[SerializeField]
		private Vector2 scrollPosition;

		[SerializeField]
		private int recordsCurrentPage;

		[SerializeField]
		private int recordsTotalPages;

		[SerializeField]
		private bool addingNewRecord;

		[SerializeField]
		private int newRecordType;

		[SerializeField]
		private bool newRecordEncrypted;

		[SerializeField]
		private string newRecordKey;

		[SerializeField]
		private string newRecordStringValue;

		[SerializeField]
		private int newRecordIntValue;

		[SerializeField]
		private float newRecordFloatValue;

		[SerializeField]
		private bool hasPendingChanges;

		[DidReloadScripts]
		private static void OnRecompile()
		{
			if (instance) instance.Repaint();
		}

		private void OnEnable()
		{
			instance = this;
			RefreshData();
		}

		private void OnFocus()
		{
			if (!hasPendingChanges)
			{
				RefreshData();
			}
		}

		private void OnGUI()
		{
			DrawGUI();
		}
		
		private void AddNewRecord()
		{
			if (string.IsNullOrEmpty(newRecordKey))
			{
				ShowNotification(new GUIContent("Please enter a key name!"));
				return;
			}

			PrefsRecord newRecord;

			switch (newRecordType)
			{
				case 0:
					newRecord = new PrefsRecord(newRecordKey, newRecordStringValue, newRecordEncrypted);
					break;
				case 1:
					newRecord = new PrefsRecord(newRecordKey, newRecordIntValue, newRecordEncrypted);
					break;
				default:
					newRecord = new PrefsRecord(newRecordKey, newRecordFloatValue, newRecordEncrypted);
					break;
			}

			if (newRecord.Save())
			{
				allRecords.Add(newRecord);
				ApplySorting();
				CloseNewRecordPanel();
				UpdateUnsavedChangesState();
			}
		}
		
		private void PerformEncryptAll()
		{
			PerformBulkOperation("Obscure ALL prefs in list?", 
				"This will apply obscuration to ALL unobscured prefs in the list (excluding Unity-managed prefs).\nAre you sure you wish to do this?", 
				record => 
				{
					if (!IsUnityManagedPref(record))
					{
						record.Encrypt();
					}
				});
		}
		
		private void PerformDecryptAll()
		{
			PerformBulkOperation("UnObscure ALL prefs in list?", 
				"This will remove obscuration from ALL obscured prefs in the list if possible (excluding Unity-managed prefs).\nAre you sure you wish to do this?", 
				record => 
				{
					if (!IsUnityManagedPref(record))
					{
						record.Decrypt();
					}
				});
		}
		
		private void PerformSaveAll()
		{
			foreach (var record in filteredRecords)
			{
				record.Save();
			}
			GUIUtility.keyboardControl = 0;
			ApplySorting();
			UpdateUnsavedChangesState();
		}
		
		private void PerformDeleteAll()
		{
			var totalCount = filteredRecords.Count;
			var unityManagedCount = filteredRecords.Count(IsUnityManagedPref);
			var userManagedCount = totalCount - unityManagedCount;
			
			var message = $"Are you sure you wish to delete all {totalCount} prefs in the list? This can't be undone!";
			if (totalCount > 0)
			{
				message += $"\n\nBreakdown:\n• {userManagedCount} user-managed prefs\n• {unityManagedCount} Unity-managed prefs";
			}
			
			if (ConfirmAction("Delete ALL prefs in list?", message))
			{
				foreach (var record in filteredRecords)
				{
					record.Delete();
				}

				RefreshData();
				GUIUtility.keyboardControl = 0;
			}
		}

		private void RefreshData()
		{
			var keys = new List<string>();
#if UNITY_EDITOR_WIN
			keys.AddRange(ReadKeysWin());
#elif UNITY_EDITOR_OSX
			keys.AddRange(ReadKeysOSX());
#else // LINUX
			keys.AddRange(ReadKeysLinux());
#endif
			keys.RemoveAll(IgnoredPrefsKeys);

			if (allRecords == null) allRecords = new List<PrefsRecord>();
			if (filteredRecords == null) filteredRecords = new List<PrefsRecord>();

			allRecords.Clear();
			filteredRecords.Clear();

			var keysCount = keys.Count;
			var showProgress = keysCount >= 500;

			for (var i = 0; i < keysCount; i++)
			{
				var keyName = keys[i];
				if (showProgress)
				{
					if (EditorUtility.DisplayCancelableProgressBar("Reading PlayerPrefs [" + (i + 1) + " of " + keysCount + "]", "Reading " + keyName, (float)i/keysCount))
					{
						break;
					}
				}
				allRecords.Add(new PrefsRecord(keyName));
			}

			if (showProgress) EditorUtility.ClearProgressBar();

			ApplySorting();
			
			hasPendingChanges = false;
		}

		private void UpdateUnsavedChangesState()
		{
			hasPendingChanges = allRecords?.Any(record => record.dirtyKey || record.dirtyValue) ?? false;
		}

		private bool ConfirmAction(string title, string message)
		{
			return EditorUtility.DisplayDialog(title, message, "Yep", "Oh, no!");
		}

		private void PerformBulkOperation(string title, string message, Action<PrefsRecord> operation)
		{
			if (ConfirmAction(title, message))
			{
				foreach (var record in filteredRecords)
				{
					operation(record);
				}
				GUIUtility.keyboardControl = 0;
				ApplySorting();
				UpdateUnsavedChangesState();
			}
		}

		private void ApplyFiltering()
		{
			filteredRecords.Clear();
			var showUnityPrefs = ACTkEditorPrefsSettings.ShowUnityPrefs;
			
			if (string.IsNullOrEmpty(searchPattern))
			{
				foreach (var record in allRecords)
				{
					if (showUnityPrefs || !IsUnityManagedPref(record))
					{
						filteredRecords.Add(record);
					}
				}
			}
			else
			{
				foreach (var record in allRecords)
				{
					if (record.Key.ToLowerInvariant().Contains(searchPattern.Trim().ToLowerInvariant()))
					{
						if (showUnityPrefs || !IsUnityManagedPref(record))
						{
							filteredRecords.Add(record);
						}
					}
				}
			}
		}

		private void ApplySorting()
		{
			switch (sortingType)
			{
				case SortingType.KeyAscending:
					allRecords.Sort(PrefsRecord.SortByNameAscending);
					break;
				case SortingType.KeyDescending:
					allRecords.Sort(PrefsRecord.SortByNameDescending);
					break;
				case SortingType.Type:
					allRecords.Sort(PrefsRecord.SortByType);
					break;
				case SortingType.Obscurance:
					allRecords.Sort(PrefsRecord.SortByObscurance);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			ApplyFiltering();
		}

		private bool IgnoredPrefsKeys(string key)
		{
			return key == ObscuredPrefs.PrefsKey ||
			       key == "OwsfBQ8qISsHHho6ACAJAiAAKRI2GjIDFh4EIQ0o";
		}
		
		private bool IsUnityManagedPref(string key)
		{
			return key == "UnityGraphicsQuality" ||
			       key == "UnitySelectMonitor" ||
			       key == "Screenmanager Resolution Width" ||
			       key == "Screenmanager Resolution Height" ||
			       key == "Screenmanager Is Fullscreen mode" ||
			       key == "unity.cloud_userid" ||
			       key == "unity.player_session_background_time" ||
			       key == "unity.player_session_elapsed_time" ||
			       key == "unity.player_sessionid" ||
			       key == "unity.player_session_count" ||
			       key == "unity_connect.installation_id" ||
			       key == "unity_connect.mega_session_id" ||
			       key == "unity_connect.session_id" ||
			       key == "PT_Run" ||
			       key == "PT_Settings" ||
			       key == "UnityUdpSdkImported" ||
			       key == "ToolchainAutomaticallyInstallPackage" ||
			       key.StartsWith("PackageUpdaterLastChecked");
		}
		
		internal bool IsUnityManagedPref(PrefsRecord record)
		{
			return IsUnityManagedPref(record.Key);
		}
		
		private void OnCopyPrefsPath()
		{
			EditorGUIUtility.systemCopyBuffer = GetPrefsPath();
			instance.ShowNotification(new GUIContent("Player Prefs path copied to clipboard"));
		}

		private string GetPrefsPath()
		{
#if UNITY_EDITOR_WIN
			return "Software\\Unity\\UnityEditor\\" + PlayerSettings.companyName + "\\" + PlayerSettings.productName;
#elif UNITY_EDITOR_OSX
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Preferences/unity." +
				PlayerSettings.companyName + "." + PlayerSettings.productName + ".plist";
#else  // LINUX!
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.config/unity3d/" +
				PlayerSettings.companyName + "/" + PlayerSettings.productName + "/prefs";
#endif
		}

#if UNITY_EDITOR_WIN
		private string[] ReadKeysWin()
		{
			var prefsPath = GetPrefsPath();
			var registryLocation = Registry.CurrentUser.CreateSubKey(prefsPath);
			if (registryLocation == null)
			{
				Debug.LogWarning($"{ACTk.LogPrefix}Couldn't locate / access Player Prefs at path {prefsPath}. " +
								 $"This message is harmless unless you already saved anything to the {nameof(PlayerPrefs)} or {nameof(ObscuredPrefs)}.\n" +
								 $"In such case, please report this problem to {ACTk.SupportContact} and include these data:\n" +
								 $"{ACTk.GenerateBugReport()}");
				return new string[0];
			}

			var names = registryLocation.GetValueNames();
			var result = new string[names.Length];

			for (var i = 0; i < names.Length; i++)
			{
				var key = names[i];

				if (key.IndexOf('_') > 0)
				{
					result[i] = key.Substring(0, key.LastIndexOf('_'));
				}
				else
				{
					result[i] = key;
				}
			}

			return result;
		}

#elif UNITY_EDITOR_OSX

		private string[] ReadKeysOSX()
		{
			var plistPath = GetPrefsPath();
			if (!File.Exists (plistPath))
			{
				Debug.LogWarning($"{ACTk.LogPrefix}Couldn't locate / access Player Prefs at path {plistPath}. " +
								 $"This message is harmless unless you already saved anything to the {nameof(PlayerPrefs)} or {nameof(ObscuredPrefs)}.\n" +
								 $"In such case, please report this problem to {ACTk.SupportContact} and include these data:\n" +
								 $"{ACTk.GenerateBugReport()}");
				return new string[0];
			}

			var parsedPlist = (Dictionary<string, object>)Plist.readPlist(plistPath);

			var keys = new string[parsedPlist.Keys.Count];
			parsedPlist.Keys.CopyTo (keys, 0);

			return keys;
		}

#else // LINUX!

		private string[] ReadKeysLinux()
		{
			var prefsPath = GetPrefsPath();
			if (!File.Exists(prefsPath))
			{
				Debug.LogWarning($"{ACTk.LogPrefix}Couldn't locate / access Player Prefs at path {prefsPath}. " +
								 $"This message is harmless unless you already saved anything to the {nameof(PlayerPrefs)} or {nameof(ObscuredPrefs)}.\n" +
								 $"In such case, please report this problem to {ACTk.SupportContact} and include these data:\n" +
								 $"{ACTk.GenerateBugReport()}");
				return new string[0];
			}

			var prefsXML = new XmlDocument();
			prefsXML.Load(prefsPath);
			var prefsList = prefsXML.SelectNodes("/unity_prefs/pref");

			var keys = new string[prefsList.Count];

			for (var i = 0; i < keys.Length; i++)
			{
				keys[i] = prefsList[i].Attributes["name"].Value;
			}

			return keys;
		}

#endif
		private enum SortingType : byte
		{
			KeyAscending = 0,
			KeyDescending = 2,
			Type = 5,
			Obscurance = 10
		}
	}
}