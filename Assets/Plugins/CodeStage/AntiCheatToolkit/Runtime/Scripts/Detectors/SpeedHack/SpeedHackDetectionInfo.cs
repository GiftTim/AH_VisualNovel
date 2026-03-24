#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Contains detailed information about latest Speed Hack detection.
	/// </summary>
	public class SpeedHackDetectionInfo : ICheatDetectionInfo
	{
		/// <summary>
		/// Indicates if environment ticks were cheated.
		/// </summary>
		public bool CheatedEnvironment { get; }

		/// <summary>
		/// Indicates if realtime ticks were cheated.
		/// </summary>
		public bool CheatedRealtime { get; }

		/// <summary>
		/// Indicates if DSP ticks were cheated.
		/// </summary>
		public bool CheatedDsp { get; }

		/// <summary>
		/// Indicates if timeScale was cheated.
		/// </summary>
		public bool CheatedTimeScale { get; }

		/// <summary>
		/// Indicates if reliable ticks were cheated.
		/// </summary>
		public bool CheatedReliable { get; }

		public SpeedHackDetectionInfo(bool cheatedEnvironment, bool cheatedRealtime, bool cheatedDsp, bool cheatedTimeScale, bool cheatedReliable)
		{
			CheatedEnvironment = cheatedEnvironment;
			CheatedRealtime = cheatedRealtime;
			CheatedDsp = cheatedDsp;
			CheatedTimeScale = cheatedTimeScale;
			CheatedReliable = cheatedReliable;
		}

		public string GetDetectionInfo()
		{
			var sources = string.Empty;	
			if (CheatedEnvironment) sources += "Environment\n";
			if (CheatedRealtime) sources += "Realtime\n";
			if (CheatedDsp) sources += "DSP\n";
			if (CheatedTimeScale) sources += "TimeScale\n";
			if (CheatedReliable) sources += "Reliable\n";

			if (string.IsNullOrEmpty(sources))
				return "Speed hack detected (unknown source)";

			return $"Speed hack detected via:\n{sources}";
		}

		public override string ToString()
		{
			return GetDetectionInfo();
		}
	}
}