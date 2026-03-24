#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary>
	/// Specifies file location.
	/// </summary>
	public enum ObscuredFileLocation : byte
    {
		/// <summary>
		/// Corresponds to the <a href="https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html">Application.persistentDataPath</a>.
		/// </summary>
        PersistentData = 5,
		
		/// <summary>
		/// Allows setting custom file path.
		/// </summary>
        Custom = 10,
    }

	/// <summary>
	/// Specific settings to use with ObscuredFile instance.
	/// </summary>
	public class ObscuredFileSettings : IObscuredFileSettings
	{
		public ObscuredFileLocation LocationKind { get; set; }
        public EncryptionSettings EncryptionSettings { get; set; }
		public DeviceLockSettings DeviceLockSettings { get; set; }
		public bool ValidateDataIntegrity { get; set; }
		public bool AutoSave { get; set; }

		/// <summary>
		/// Creates default settings instance.
		/// </summary>
		/// <remarks>
		/// Default settings are:
		/// - LocationKind set to ObscuredFileLocation.PersistentData
		/// - EncryptionSettings.ObscurationMode set to ObscurationMode.Plain
		/// - DeviceLockSettings.Level set to DeviceLockLevel.None
		/// - ValidateDataIntegrity set to true
		/// - AutoSave set to true
		/// </remarks>
		public ObscuredFileSettings():this(ObscuredFileLocation.PersistentData)
		{
		}

		/// <summary>
		/// Creates settings instance with specified LocationKind.
		/// </summary>
		/// <remarks>
		/// Default settings are:
		/// - EncryptionSettings.ObscurationMode set to ObscurationMode.Plain
		/// - DeviceLockSettings.Level set to DeviceLockLevel.None
		/// - ValidateDataIntegrity set to true
		/// - AutoSave set to true
		/// </remarks>
		public ObscuredFileSettings(ObscuredFileLocation locationKind) : this(
			new EncryptionSettings(), new DeviceLockSettings(), locationKind) { }

		/// <summary>
		/// Creates settings instance with specified DeviceLockSettings.
		/// </summary>
		/// <remarks>
		/// Default settings are:
		/// - LocationKind set to ObscuredFileLocation.PersistentData
		/// - EncryptionSettings.ObscurationMode set to ObscurationMode.Plain
		/// - ValidateDataIntegrity set to true
		/// - AutoSave set to true
		/// </remarks>
		public ObscuredFileSettings(DeviceLockSettings deviceLockSettings) : this(
			new EncryptionSettings(), deviceLockSettings)
		{
;
		}

		/// <summary>
		/// Creates user-specified settings instance.
		/// </summary>
		public ObscuredFileSettings(EncryptionSettings encryptionSettings, DeviceLockSettings deviceLockSettings,
			ObscuredFileLocation locationKind = ObscuredFileLocation.PersistentData, bool validateDataIntegrity = true, 
			bool autoSave = true)
		{
			EncryptionSettings = encryptionSettings;
			DeviceLockSettings = deviceLockSettings;
			LocationKind = locationKind;
			ValidateDataIntegrity = validateDataIntegrity;
			AutoSave = autoSave;
		}
	}
}