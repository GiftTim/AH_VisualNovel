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
	internal class SerializedObscuredULong : MigratableSerializedObscuredType<ulong>
	{
		public ulong Hidden
		{
			get => (ulong)HiddenProperty.longValue;
			set => HiddenProperty.longValue = (long)value;
		}

		public ulong Key
		{
			get => (ulong)KeyProperty.longValue;
			set => KeyProperty.longValue = (long)value;
		}

		public override ulong Plain => ObscuredULong.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredULong.Version;

		private protected override bool PerformMigrate()
		{
			if (Version == 0 || TypeVersion == 1)
			{
				MigrateFromV0();
				Version = TypeVersion;
				return true;
			}

			return false;

			void MigrateFromV0()
			{
				var decrypted = ObscuredULong.DecryptFromV0(Hidden, Key);
				var validHash = HashUtils.CalculateHash(decrypted);
				Hidden = ObscuredULong.Encrypt(decrypted, Key);
				Hash = validHash;
			}
		}
		
		public override string GetMigrationResultString()
		{
			return ObscuredULong.DecryptFromV0(Hidden, Key).ToString(CultureInfo.InvariantCulture);
		}
	}
}

#endif