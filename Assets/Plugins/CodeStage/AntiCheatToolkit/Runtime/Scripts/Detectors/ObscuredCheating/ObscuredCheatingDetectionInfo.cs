#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	using System;

	/// <summary>
	/// Contains detailed information about latest Obscured Types cheating detection.
	/// </summary>
	public class ObscuredCheatingDetectionInfo : ICheatDetectionInfo
	{
		/// <summary>
		/// Type of the source. Holds type of the obscured type instance which triggered the detection.
		/// </summary>
		public Type SourceType { get; }
		
		/// <summary>
		/// Indicates encrypted value passed hash validation and is genuine.
		/// </summary>
		public bool HashValid { get; }
		
		/// <summary>
		/// Actual encrypted value (in clean decrypted form) at the detection moment.
		/// </summary>
		/// <remarks>
		/// Please note, some types have both whole values and separate components checks,
		/// for example, ObscuredVector3 has checks for whole Vector3 and its components like Vector3.x,
		/// thus this value can hold either the whole struct or just one of its components.
		/// </remarks>
		public object ObscuredValue { get; }
		
		/// <summary>
		/// Faked "honeypot" value at the detection moment (if honeyPot option is enabled).
		/// </summary>
		/// <remarks>
		/// Please note, some types have both whole values and separate components checks,
		/// for example, ObscuredVector3 has checks for whole Vector3 and its components like Vector3.x,
		/// thus this value can hold either the whole struct or just one of its components.
		/// </remarks>
		public object FakeValue { get; }

		public ObscuredCheatingDetectionInfo(Type type, bool hashValid, object decrypted, object fake)
		{
			SourceType = type;
			HashValid = hashValid;
			ObscuredValue = decrypted;
			FakeValue = fake;
		}

		public string GetDetectionInfo()
		{
			return $"Type: {SourceType}\n" +
				   $"Hash Valid: {HashValid}\n" +
				   $"Decrypted: {ObscuredValue}\n" +
				   $"Fake: {FakeValue}";
		}

		public override string ToString()
		{
			return GetDetectionInfo();
		}
	}
}