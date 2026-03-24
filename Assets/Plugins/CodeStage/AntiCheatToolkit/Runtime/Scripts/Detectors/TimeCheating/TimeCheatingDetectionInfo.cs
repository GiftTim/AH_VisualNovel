#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Contains detailed information about latest Time Cheating detection.
	/// </summary>
	public class TimeCheatingDetectionInfo : ICheatDetectionInfo
	{
		/// <summary>
		/// Result of the check that triggered detection.
		/// </summary>
		public TimeCheatingDetector.CheckResult Result { get; }

		/// <summary>
		/// Error kind, if any.
		/// </summary>
		public TimeCheatingDetector.ErrorKind Error { get; }

		public TimeCheatingDetectionInfo(TimeCheatingDetector.CheckResult result, TimeCheatingDetector.ErrorKind error)
		{
			Result = result;
			Error = error;
		}

		public string GetDetectionInfo()
		{
			if (Error != TimeCheatingDetector.ErrorKind.NoError)
				return $"Time cheating detected. Result: {Result}, Error: {Error}";

			return $"Time cheating detected. Result: {Result}";
		}

		public override string ToString()
		{
			return GetDetectionInfo();
		}
	}
}