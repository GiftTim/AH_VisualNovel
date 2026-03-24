#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Interface for detection information objects that provide details about what triggered a cheat detection.
	/// </summary>
	public interface ICheatDetectionInfo
	{
		/// <summary>
		/// Returns a formatted string containing detailed information about the detection.
		/// </summary>
		/// <returns>Formatted string with detection details.</returns>
		string GetDetectionInfo();
	}
}

