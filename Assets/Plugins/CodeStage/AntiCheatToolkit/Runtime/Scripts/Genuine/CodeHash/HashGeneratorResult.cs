#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeStage.AntiCheat.Genuine.CodeHash
{
	/// <summary>
	/// Result produced by CodeHashGenerator. Contains resulting code hash or errors information.
	/// </summary>
	public class HashGeneratorResult
	{
		[Obsolete("Please use SummaryHash property instead.", true)]
		public string CodeHash => SummaryHash;

		/// <summary>
		/// Summary hash for all files in currently running build.
		/// May be null in case <see cref="Success"/> is not true.
		/// </summary>
		/// <remarks>
		/// Use with caution: summary hash for runtime build may differ from the summary hash
		/// you got in Editor, for example, for Android App Bundles.
		/// Use <see cref="FileHashes"/> for more accurate hashes comparison control.
		/// </remarks>
		public string SummaryHash => buildHashes.SummaryHash;

		/// <summary>
		/// Hashes for all files in currently running build.
		/// </summary>
		/// <remarks>
		/// Feel free to compare it against hashes array you got in Editor to find if
		/// runtime version has new unknown hashes (this is an indication build was altered).
		/// </remarks>
		public IReadOnlyList<FileHash> FileHashes => buildHashes.FileHashes;

		/// <summary>
		/// Error message you could find useful in case <see cref="Success"/> is not true.
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// True if generation was successful and resulting hashes are stored in <see cref="FileHashes"/>,
		/// otherwise check <see cref="ErrorMessage"/> to find out error cause.
		/// </summary>
		public bool Success => ErrorMessage == null;
		
		/// <summary>
		/// Hashing duration in seconds. Will be 0 if hashing was not succeed.
		/// </summary>
		public double DurationSeconds => buildHashes.DurationSeconds;

		private string summaryCodeHash;
		private BuildHashes buildHashes;

		private HashGeneratorResult() { }

		internal static HashGeneratorResult FromError(string errorMessage)
		{
			return new HashGeneratorResult
			{
				ErrorMessage = errorMessage
			};
		}

		internal static HashGeneratorResult FromBuildHashes(BuildHashes buildHashes)
		{
			return new HashGeneratorResult
			{
				buildHashes = buildHashes
			};
		}

		/// <summary>
		/// Checks is passes hash exists in file hashes of this instance.
		/// </summary>
		/// <param name="hash">Target file hash.</param>
		/// <returns>True if such hash presents at <see cref="FileHashes"/> and false otherwise.</returns>
		public bool HasFileHash(string hash)
		{
			return buildHashes.HasFileHash(hash);
		}

		/// <summary>
		/// Prints found hashes to the console (if any).
		/// </summary>
		public void PrintToConsole()
		{
			if (Success)
				buildHashes.PrintToConsole();
			else
				Debug.LogError(ErrorMessage);
		}
	}
}