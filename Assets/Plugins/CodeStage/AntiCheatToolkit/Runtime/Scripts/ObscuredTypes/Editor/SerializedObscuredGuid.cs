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
	internal class SerializedObscuredGuid : MigratableSerializedObscuredType<Guid>
	{
		public long Hidden1
		{
			get => HiddenProperty.longValue;
			set => HiddenProperty.longValue = value;
		}

		public long Hidden2
		{
			get => Target.FindPropertyRelative(nameof(ObscuredGuid.hiddenValue2)).longValue;
			set => Target.FindPropertyRelative(nameof(ObscuredGuid.hiddenValue2)).longValue = value;
		}

		public long Key
		{
			get => KeyProperty.longValue;
			set => KeyProperty.longValue = value;
		}

		public override Guid Plain => ObscuredGuid.Decrypt(Hidden1, Hidden2, Key);
		
		private protected override string HiddenPropertyRelativePath => nameof(ObscuredGuid.hiddenValue1);
		private protected override byte TypeVersion => ObscuredGuid.Version;

		private protected override bool PerformMigrate()
		{
			// For now, no migration needed for Guid since it's a new type
			return false;
		}
		
		public override string GetMigrationResultString()
		{
			return Plain.ToString("D", CultureInfo.InvariantCulture);
		}
	}
}

#endif 