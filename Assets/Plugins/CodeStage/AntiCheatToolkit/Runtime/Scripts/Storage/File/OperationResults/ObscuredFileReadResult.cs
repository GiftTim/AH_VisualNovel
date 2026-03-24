#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	/// <summary> Contains ObscuredFile read operation results. </summary>
	public readonly struct ObscuredFileReadResult
	{
		/// <summary>
		/// Returns true in case <see cref="Data"/> is not null and <see cref="Error"/>'s ErrorCode is ObscuredFileErrorCode.NoError, returns false otherwise.
		/// </summary>
		/// <remarks>
		/// <strong>📝 Note:</strong> it will be
		/// false if data is not genuine or was loaded from another device even if Data itself was read successfully and not null.
		/// Listen to the NotGenuineDataDetected and DataFromAnotherDeviceDetected events
		/// (at ObscuredFile or ObscuredFilePrefs) or check <see cref="CheatingDetected"/>, <see cref="DataIsNotGenuine"/> and <see cref="DataFromAnotherDevice"/>
		/// properties explicitly to react on the possible cheating.
		/// </remarks>
		public bool Success => IsValid && Data != null && !CheatingDetected && Error.ErrorCode == ObscuredFileErrorCode.NoError;
		
		/// <summary>
		/// Contains read bytes. Will be null if data was damaged, file does not exists or device lock feature prevented data read.
		/// </summary>
		public byte[] Data { get; }

		/// <summary>
		/// Indicates either <see cref="DataIsNotGenuine"/> or <see cref="DataFromAnotherDevice"/> is true.
		/// </summary>
		public bool CheatingDetected => DataIsNotGenuine || DataFromAnotherDevice;
		
		/// <summary>
		/// Returns true if saved data has correct header but signature does not matches file contents. Returns false otherwise.
		/// </summary>
		public bool DataIsNotGenuine { get; }
		
		/// <summary>
		/// Returns true if device lock feature detected data from another device.
		/// </summary>
		public bool DataFromAnotherDevice { get; }
		
		/// <summary>
		/// Contains specific error in case <see cref="Success"/> is not true but <see cref="IsValid"/> is true.
		/// </summary>
		public ObscuredFileError Error { get; }

		/// <summary>
		/// Returns true if this struct was filled with actual data, otherwise will stay false.
		/// </summary>
		public bool IsValid { get; }
		
		internal ObscuredFileReadResult(byte[] data, bool dataIsNotGenuine, bool dataFromAnotherDevice)
		{
			Data = data;
			DataIsNotGenuine = dataIsNotGenuine;
			DataFromAnotherDevice = dataFromAnotherDevice;
			Error = default;
			IsValid = true;
		}
		
		private ObscuredFileReadResult(ObscuredFileError error)
		{
			Data = default;
			DataIsNotGenuine = default;
			DataFromAnotherDevice = default;
			Error = error;
			IsValid = true;
		}
		
		internal static ObscuredFileReadResult FromError(Exception exception)
		{
			return new ObscuredFileReadResult(new ObscuredFileError(exception));
		}

		internal static ObscuredFileReadResult FromError(ObscuredFileErrorCode errorCode)
		{
			return new ObscuredFileReadResult(new ObscuredFileError(errorCode));
		}

		/// <summary>
		/// Returns contents of this operation result.
		/// </summary>
		/// <returns>Human-readable operation result.</returns>
		public override string ToString()
		{
			return $"{nameof(IsValid)}: {IsValid}\n" +
				   $"Read data length: {Data?.Length ?? 0}\n" +
				   $"{nameof(DataIsNotGenuine)}: {DataIsNotGenuine}\n" +
				   $"{nameof(DataFromAnotherDevice)}: {DataFromAnotherDevice}\n" +
				   $"{nameof(Error)}: {Error}";
		}
	}
}