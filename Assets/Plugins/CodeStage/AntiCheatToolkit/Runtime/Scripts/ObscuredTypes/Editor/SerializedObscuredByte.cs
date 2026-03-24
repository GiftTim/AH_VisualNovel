#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_EDITOR

using System.Globalization;
using CodeStage.AntiCheat.Utils;

namespace CodeStage.AntiCheat.ObscuredTypes.EditorCode
{
	internal class SerializedObscuredByte : SerializedObscuredType<byte>
	{
		public byte Hidden
		{
			get => (byte)HiddenProperty.intValue;
			set => HiddenProperty.intValue = value;
		}

		public byte Key
		{
			get => (byte)KeyProperty.intValue;
			set => KeyProperty.intValue = value;
		}

		public override byte Plain => ObscuredByte.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredByte.Version;

	}
}

#endif


