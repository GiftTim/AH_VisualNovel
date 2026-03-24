#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

using CodeStage.AntiCheat.Detectors;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	public partial class ObscuredString : ISerializationCallbackReceiver
	{
		public void OnBeforeSerialize() { /* not used */ }
		public void OnAfterDeserialize()
		{
			if (IsDefault())
			{
				fakeValue = default;
			}
			else 
			{
				if (cryptoKey == null || cryptoKey.Length == 0)
				{
					cryptoKey = InitKey();
					
					if (hiddenChars != null && hiddenChars.Length > 0) // workaround for regression introduced in 2024.x and fixed in 2024.3.2
						hiddenChars = Encrypt(hiddenChars, cryptoKey);
				}

				if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
					fakeValue = Decrypt(hiddenChars, cryptoKey);
			}
		}
	}
}