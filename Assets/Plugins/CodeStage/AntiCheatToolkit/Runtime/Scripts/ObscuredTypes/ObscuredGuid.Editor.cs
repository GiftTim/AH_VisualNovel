#region copyright

// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------

#endregion

#if UNITY_EDITOR

using System;
using System.Runtime.InteropServices;
using CodeStage.AntiCheat.Utils;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	[StructLayout(LayoutKind.Auto)]
	public partial struct ObscuredGuid : ISerializableObscuredType
	{
		internal const int Version = 1;

		// ReSharper disable once NotAccessedField.Global - used explicitly
		[SerializeField] internal byte version;

		bool ISerializableObscuredType.IsDataValid => IsDefault() || ValidateGuidHash(hiddenValue1, hiddenValue2, currentCryptoKey, hash);
		
		private bool ValidateGuidHash(long encrypted1, long encrypted2, long key, int storedHash)
		{
			var decryptedGuid = Decrypt(encrypted1, encrypted2, key);
			var bytes = decryptedGuid.ToByteArray();
			var long1 = BitConverter.ToInt64(bytes, 0);
			var long2 = BitConverter.ToInt64(bytes, 8);
			var calculatedHash = CalculateGuidHashFromLongs(long1, long2);
			return storedHash == calculatedHash;
		}
	}
}
#endif 