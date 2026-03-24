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
	public partial struct ObscuredQuaternion : ISerializableObscuredType
	{
		internal const int Version = 1;

		// ReSharper disable once NotAccessedField.Global - used explicitly
		[SerializeField] internal byte version;
		
#if !UNITY_DOTS_ENABLED
		[SerializeField] internal string migratedVersion; // obsolete
#endif

		bool ISerializableObscuredType.IsDataValid => IsDefault() || hash == HashUtils.CalculateHash(Decrypt(hiddenValue, currentCryptoKey));
    }
}
#endif