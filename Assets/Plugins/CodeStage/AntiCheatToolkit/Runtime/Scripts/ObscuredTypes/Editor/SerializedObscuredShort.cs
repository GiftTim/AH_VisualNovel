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
	internal class SerializedObscuredShort : MigratableSerializedObscuredType<short>
	{
		public short Hidden
		{
			get => (short)HiddenProperty.intValue;
			set => HiddenProperty.intValue = value;
		}

		public short Key
		{
			get => (short)KeyProperty.intValue;
			set => KeyProperty.intValue = value;
		}

		public override short Plain => ObscuredShort.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredShort.Version;

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
				var decrypted = ObscuredShort.DecryptFromV0(Hidden, Key);
				var validHash = HashUtils.CalculateHash(decrypted);
				Hidden = ObscuredShort.Encrypt(decrypted, Key);
				Hash = validHash;
			}
		}
		
		public override string GetMigrationResultString()
		{
			return ObscuredShort.DecryptFromV0(Hidden, Key).ToString(CultureInfo.InvariantCulture);
		}

	}
}

#endif