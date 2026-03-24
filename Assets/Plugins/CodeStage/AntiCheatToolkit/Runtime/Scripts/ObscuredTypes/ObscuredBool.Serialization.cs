#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	public partial struct ObscuredBool : ISerializationCallbackReceiver
	{
		public void OnBeforeSerialize() { /* not used */ }
		public void OnAfterDeserialize()
		{
			if (IsDefault())
			{
				_fakeValue = default;
				_fakeValueActive = false;
			}
			else if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
			{
				FakeValue = Decrypt(hiddenValue, currentCryptoKey);
			}
		}
	}
}