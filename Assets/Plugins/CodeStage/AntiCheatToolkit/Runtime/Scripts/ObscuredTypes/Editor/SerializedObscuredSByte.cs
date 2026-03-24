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
	internal class SerializedObscuredSByte : SerializedObscuredType<sbyte>
	{
		public sbyte Hidden
		{
			get => (sbyte)HiddenProperty.intValue;
			set => HiddenProperty.intValue = value;
		}

		public sbyte Key
		{
			get => (sbyte)KeyProperty.intValue;
			set => KeyProperty.intValue = value;
		}

		public override sbyte Plain => ObscuredSByte.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredSByte.Version;

	}
}

#endif


