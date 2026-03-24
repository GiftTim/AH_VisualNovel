#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Utils
{
	/// <summary>
	/// Allows preventing screenshots or screen recording of your app using Android's builtin security feature. Can be helpful against some bots on non-rooted devices.
	/// </summary>
	/// <remarks>
	/// While Android makes its best to prevent screenshots and video recording, it's not guaranteed it will work with some custom ROMs built-in software.
	/// Please keep in mind anyone still can use another camera to shoot your app footage from current device screen.
	/// </remarks>
	public static class AndroidScreenRecordingBlocker
	{
		public static void PreventScreenRecording()
		{
#if UNITY_ANDROID
			AndroidRoutines.SetSecureFlag();
#elif DEBUG
			UnityEngine.Debug.LogWarning($"{nameof(AndroidScreenRecordingBlocker)} does work on Android platform only.");
#endif
		}
		
		public static void AllowScreenRecording()
		{
#if UNITY_ANDROID
			AndroidRoutines.RemoveSecureFlag();
#elif DEBUG
			UnityEngine.Debug.LogWarning($"{nameof(AndroidScreenRecordingBlocker)} does work on Android platform only.");
#endif
		}
	}
}