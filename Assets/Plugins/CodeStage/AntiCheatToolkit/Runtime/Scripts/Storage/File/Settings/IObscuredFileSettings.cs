#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary>
	/// Specific settings to use with ObscuredFile instance.
	/// </summary>
	public interface IObscuredFileSettings
	{
		/// <summary>
		/// Controls file location. Read more at <see cref="ObscuredFileLocation"/> docs.
		/// </summary>
		ObscuredFileLocation LocationKind { get; }
		
		/// <summary>
		/// Controls file encryption settings. Read more at <see cref="EncryptionSettings"/> docs.
		/// </summary>
		EncryptionSettings EncryptionSettings { get; }

		/// <summary>
		/// Controls DeviceLock feature settings. Read more at <see cref="DeviceLockSettings"/> docs.
		/// </summary>
		DeviceLockSettings DeviceLockSettings { get; }

		/// <summary>
		/// Controls anti-tampering setting.
		/// </summary>
		/// <remarks>
		/// When enabled, data integrity is validated and invokes NotGenuineDataDetected event on violation.
		/// <br/>Disable to skip integrity validation entirely and improve performance.
		/// </remarks>
		bool ValidateDataIntegrity { get; }
		
		/// <summary>
		/// Switches Auto Save feature.
		/// </summary>
		/// <remarks>
		/// Auto Save feature allows making sure any unsaved data will persist
		/// when your app quits (on desktops) or loses the focus (on mobiles).
		/// It's enabled by default and this is the recommended setting,
		/// but you're free to turn it off at your own risk.
		/// </remarks>
		bool AutoSave { get; }
	}
}