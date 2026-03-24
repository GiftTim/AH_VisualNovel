#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_EDITOR

using System.Runtime.InteropServices;
using CodeStage.AntiCheat.Utils;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
    [StructLayout(LayoutKind.Auto)]
	public partial struct ObscuredBigInteger : ISerializableObscuredType
    {
		internal const int Version = 1;
		
		[SerializeField] internal byte version;

		bool ISerializableObscuredType.IsDataValid => IsDefault() || hash == HashUtils.CalculateHash(Decrypt(hiddenValue, currentCryptoKey));
	}
}
#endif