#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Detectors
{
	/// <summary>
	/// Contains detailed information about latest Wall Hack detection.
	/// </summary>
	public class WallHackDetectionInfo : ICheatDetectionInfo
	{
		/// <summary>
		/// Indicates if Rigidbody module detected cheating.
		/// </summary>
		public bool DetectedByRigidbody { get; }

		/// <summary>
		/// Indicates if CharacterController module detected cheating.
		/// </summary>
		public bool DetectedByController { get; }

		/// <summary>
		/// Indicates if Wireframe module detected cheating.
		/// </summary>
		public bool DetectedByWireframe { get; }

		/// <summary>
		/// Indicates if Raycast module detected cheating.
		/// </summary>
		public bool DetectedByRaycast { get; }

		public WallHackDetectionInfo(bool detectedByRigidbody, bool detectedByController, bool detectedByWireframe, bool detectedByRaycast)
		{
			DetectedByRigidbody = detectedByRigidbody;
			DetectedByController = detectedByController;
			DetectedByWireframe = detectedByWireframe;
			DetectedByRaycast = detectedByRaycast;
		}

		public string GetDetectionInfo()
		{
			var modules = string.Empty;
			if (DetectedByRigidbody) modules += "Rigidbody\n";
			if (DetectedByController) modules += "CharacterController\n";
			if (DetectedByWireframe) modules += "Wireframe\n";
			if (DetectedByRaycast) modules += "Raycast\n";

			if (string.IsNullOrEmpty(modules))
				return "Wall hack detected (unknown module)";

			return $"Wall hack detected via:\n{modules}";
		}

		public override string ToString()
		{
			return GetDetectionInfo();
		}
	}
}