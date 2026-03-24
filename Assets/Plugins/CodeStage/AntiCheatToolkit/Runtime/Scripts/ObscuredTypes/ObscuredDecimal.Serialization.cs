#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	public partial struct ObscuredDecimal : ISerializationCallbackReceiver
	{
		public void OnBeforeSerialize() { /* not used */ }
		public void OnAfterDeserialize()
		{
			if (IsDefault())
				fakeValue = default;
			else if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				fakeValue = DecimalLongBytesUnion.XorB16ToDecimal(hiddenValue, currentCryptoKey);
		}
	}
}