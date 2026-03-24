#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

#if UNITY_EDITOR

using System;
using System.Globalization;
using CodeStage.AntiCheat.Utils;

namespace CodeStage.AntiCheat.ObscuredTypes.EditorCode
{
	internal class SerializedObscuredDateTimeOffset : MigratableSerializedObscuredType<DateTimeOffset>
	{
		public long Hidden
		{
			get => HiddenProperty.longValue;
			set => HiddenProperty.longValue = value;
		}

		public long Key
		{
			get => KeyProperty.longValue;
			set => KeyProperty.longValue = value;
		}

		public override DateTimeOffset Plain => ObscuredDateTimeOffset.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredDateTimeOffset.Version;

		private protected override bool PerformMigrate()
		{
			// For now, no migration needed for DateTimeOffset since it's a new type
			return false;
		}
		
		public override string GetMigrationResultString()
		{
			return Plain.ToString("o", DateTimeFormatInfo.InvariantInfo);
		}
	}
}

#endif 