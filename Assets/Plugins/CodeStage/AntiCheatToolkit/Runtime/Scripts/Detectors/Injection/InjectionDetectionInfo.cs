#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Contains detailed information about latest Injection detection.
	/// </summary>
	public class InjectionDetectionInfo : ICheatDetectionInfo
	{
		/// <summary>
		/// Reason for the detection (e.g., assembly name or detection cause).
		/// </summary>
		public string Reason { get; }

		public InjectionDetectionInfo(string reason)
		{
			Reason = reason;
		}

		public string GetDetectionInfo()
		{
			return $"Injection detected. Reason: {Reason}";
		}

		public override string ToString()
		{
			return GetDetectionInfo();
		}
	}
}