#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Examples
{
	using Time;
	using UnityEngine;

	// speed-hack resistant version of the InfiniteRotator.cs
	[AddComponentMenu("")]
	internal class InfiniteRotatorReliable : InfiniteRotator
	{
		private protected override float GetDeltaTime()
		{
			return SpeedHackProofTime.unscaledDeltaTime;
		}
	}
}