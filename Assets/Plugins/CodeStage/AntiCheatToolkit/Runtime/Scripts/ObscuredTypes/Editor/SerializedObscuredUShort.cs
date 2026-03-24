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
	internal class SerializedObscuredUShort : SerializedObscuredType<ushort>
	{
		public ushort Hidden
		{
			get => (ushort)HiddenProperty.intValue;
			set => HiddenProperty.intValue = value;
		}

		public ushort Key
		{
			get => (ushort)KeyProperty.intValue;
			set => KeyProperty.intValue = value;
		}

		public override ushort Plain => ObscuredUShort.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredUShort.Version;

	}
}

#endif


