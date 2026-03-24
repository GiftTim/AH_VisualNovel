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
	internal class SerializedObscuredInt : MigratableSerializedObscuredType<int>
	{
		public int Hidden
		{
			get => HiddenProperty.intValue;
			set => HiddenProperty.intValue = value;
		}

		public int Key
		{
			get => KeyProperty.intValue;
			set => KeyProperty.intValue = value;
		}

		public override int Plain => ObscuredInt.Decrypt(Hidden, Key);
		private protected override byte TypeVersion => ObscuredInt.Version;

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
				var decrypted = ObscuredInt.DecryptFromV0(Hidden, Key);
				var validHash = HashUtils.CalculateHash(decrypted);
				Hidden = ObscuredInt.Encrypt(decrypted, Key);
				Hash = validHash;
			}
		}

		public override string GetMigrationResultString()
		{
			return ObscuredInt.DecryptFromV0(Hidden, Key).ToString(CultureInfo.InvariantCulture);
		}

	}
}

#endif