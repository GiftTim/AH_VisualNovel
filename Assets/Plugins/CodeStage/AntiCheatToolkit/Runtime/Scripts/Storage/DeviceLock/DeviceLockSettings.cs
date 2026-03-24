#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	/// <summary>
	/// Controls Device Lock feature settings.
	/// </summary>
	public class DeviceLockSettings
	{
		/// <summary>
		/// Allows locking saved data to the current device.
		/// </summary>
		/// <remarks>
		/// Read more in <see cref="DeviceLockLevel"/> description.
		/// </remarks>
		/// <seealso cref="Sensitivity"/>
		public DeviceLockLevel Level { get; set; }

		/// <summary>
		/// Controls device lock tampering detection sensitivity.
		/// </summary>
		/// <remarks>
		/// Read more in <see cref="DeviceLockTamperingSensitivity"/> description.
		/// </remarks>
		/// <seealso cref="Level"/>
		public DeviceLockTamperingSensitivity Sensitivity { get; set; }
		
		/// <summary>
		/// Creates instance with custom settings.
		/// </summary>
		public DeviceLockSettings(DeviceLockLevel level = DeviceLockLevel.None, DeviceLockTamperingSensitivity sensitivity = DeviceLockTamperingSensitivity.Normal)
		{
			Level = level;
			Sensitivity = sensitivity;
		}
	}
}