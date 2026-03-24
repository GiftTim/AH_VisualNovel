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
	internal class SerializedObscuredBool : MigratableSerializedObscuredType<bool>
	{
		public int Hidden
		{
			get => HiddenProperty.intValue;
			set => HiddenProperty.intValue = value;
		}

		public byte Key
		{
			get => (byte)KeyProperty.intValue;
			set => KeyProperty.intValue = value;
		}

		public override bool Plain => TargetInstance != null ? 
			!TargetInstance.IsDefault() && ObscuredBool.Decrypt(Hidden, Key) : 
			ObscuredBool.Decrypt(Hidden, Key);

		private protected override byte TypeVersion => ObscuredBool.Version;

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
				var decrypted = ObscuredBool.DecryptFromV0(Hidden, Key);
				var validHash = HashUtils.CalculateHash(decrypted);
				Hidden = ObscuredBool.Encrypt(decrypted, Key);
				Hash = validHash;
			}
		}

		public override string GetMigrationResultString()
		{
			return ObscuredBool.DecryptFromV0(Hidden, Key).ToString(CultureInfo.InvariantCulture);
		}
	}
}

#endif