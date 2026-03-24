#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;
using CodeStage.AntiCheat.Common;
using CodeStage.AntiCheat.Detectors;
using CodeStage.AntiCheat.Utils;
using UnityEngine;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	/// <summary>
	/// Use it instead of regular <c>Guid</c> for any cheating-sensitive properties, fields and other long-term declarations.
	/// </summary>
	/// <remarks>
	/// <strong>📝 Important Notes:</strong><br/>
	/// • Regular type is faster and memory wiser comparing to the obscured one!<br/>
	/// • Use regular type for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).
	/// </remarks>
	[Serializable]
    public partial struct ObscuredGuid : IObscuredType
    {
        public int Hash => hash;

		[SerializeField] internal int hash;
		[SerializeField] internal long hiddenValue1;
		[SerializeField] internal long hiddenValue2;
		[SerializeField] internal long currentCryptoKey;
		
		[NonSerialized] internal Guid fakeValue;

		private ObscuredGuid(Guid value)
		{
			currentCryptoKey = GenerateKey();
			var bytes = value.ToByteArray();
			var long1 = BitConverter.ToInt64(bytes, 0);
			var long2 = BitConverter.ToInt64(bytes, 8);
			
			hiddenValue1 = EncryptLong(long1, currentCryptoKey);
			hiddenValue2 = EncryptLong(long2, currentCryptoKey);

			hash = CalculateGuidHashFromLongs(long1, long2);
			fakeValue = ObscuredCheatingDetector.IsRunningInHoneypotMode ? value : default;

#if UNITY_EDITOR
			version = Version;
#endif
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static void Encrypt(Guid value, long key, out long encrypted1, out long encrypted2)
		{
			var bytes = value.ToByteArray();
			var long1 = BitConverter.ToInt64(bytes, 0);
			var long2 = BitConverter.ToInt64(bytes, 8);
			
			encrypted1 = EncryptLong(long1, key);
			encrypted2 = EncryptLong(long2, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static Guid Decrypt(long encrypted1, long encrypted2, long key)
		{
			try
			{
				var long1 = DecryptLong(encrypted1, key);
				var long2 = DecryptLong(encrypted2, key);
				
				Span<byte> bytes = stackalloc byte[16];
				BitConverter.TryWriteBytes(bytes, long1);
				BitConverter.TryWriteBytes(bytes.Slice(8), long2);
				
				return new Guid(bytes);
			}
			catch (Exception e)
			{
				Debug.LogWarning(ACTk.LogPrefix + $"Error while decrypting {nameof(ObscuredGuid)}:\n{e}");
				return default;
			}
		}

		/// <summary>
		/// Creates and fills obscured variable with raw encrypted value previously got from GetEncrypted().
		/// </summary>
		/// <remarks>
		/// Literally does same job as SetEncrypted() but makes new instance instead of filling existing one,
		/// making it easier to initialize new variables from saved encrypted values.
		/// </remarks>
		///
		/// <param name="encrypted1">First part of raw encrypted value you got from GetEncrypted().</param>
		/// <param name="encrypted2">Second part of raw encrypted value you got from GetEncrypted().</param>
		/// <param name="key">Encryption key you've got from GetEncrypted().</param>
		/// <returns>New obscured variable initialized from specified encrypted value.</returns>
		/// <seealso cref="GetEncrypted"/>
		/// <seealso cref="SetEncrypted"/>
		public static ObscuredGuid FromEncrypted(long encrypted1, long encrypted2, long key)
		{
			var instance = new ObscuredGuid();
			instance.SetEncrypted(encrypted1, encrypted2, key);
			return instance;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static long GenerateKey()
		{
			return RandomUtils.GenerateLongKey();
		}
		
		internal static int CalculateGuidHashFromLongs(long long1, long long2)
		{
			var hash1 = HashUtils.CalculateHash(long1);
			var hash2 = HashUtils.CalculateHash(long2) << 2;
			return (hash1 ^ hash2) | 1;
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <param name="encrypted1">First part of encrypted value as is.</param>
		/// <param name="encrypted2">Second part of encrypted value as is.</param>
		/// <remarks>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// </remarks>
		/// <seealso cref="FromEncrypted"/>
		/// <seealso cref="SetEncrypted"/>
		public void GetEncrypted(out long key, out long encrypted1, out long encrypted2)
		{
			if (IsDefault()) this = new ObscuredGuid(default);
			
			key = currentCryptoKey;
			encrypted1 = hiddenValue1;
			encrypted2 = hiddenValue2;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// <remarks>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// </remarks>
		/// <seealso cref="FromEncrypted"/>
		public void SetEncrypted(long encrypted1, long encrypted2, long key)
		{
			currentCryptoKey = key;
			var plain = Decrypt(encrypted1, encrypted2, key);
			hiddenValue1 = encrypted1;
			hiddenValue2 = encrypted2;
			
			var bytes = plain.ToByteArray();
			var long1 = BitConverter.ToInt64(bytes, 0);
			var long2 = BitConverter.ToInt64(bytes, 8);
			hash = CalculateGuidHashFromLongs(long1, long2);

			if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				fakeValue = plain;
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public Guid GetDecrypted()
		{
			return InternalDecryptAsGuid();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecryptAsGuid();
			currentCryptoKey = GenerateKey();
			HideValue(decrypted);
		}
		
		private static bool ValidateHash(Guid input, int hash)
		{
			Span<byte> bytes = stackalloc byte[16];
			input.TryWriteBytes(bytes);
			return ValidateHash(bytes, hash);
		}

		private static bool ValidateHash(ReadOnlySpan<byte> input, int hash)
		{
			var long1 = BitConverter.ToInt64(input);
			var long2 = BitConverter.ToInt64(input.Slice(8));
			var expectedHash = CalculateGuidHashFromLongs(long1, long2);
			
#if DEBUG
			if (hash == default && expectedHash != default)
				Debug.LogError(ACTk.LogPrefix + $"{nameof(hash)} is not initialized properly!\n" +
							   "It will produce false positive cheating detection.\n" +
							   "Can happen when migrating from older ACTk versions.\n" +
							   "Please call Tools > Code Stage > Anti-Cheat Toolkit > Migrate > * menu item to try fixing this.");
#endif
			return hash == expectedHash;
		}

		private void HideValue(Guid plain)
		{
			var bytes = plain.ToByteArray();
			var long1 = BitConverter.ToInt64(bytes, 0);
			var long2 = BitConverter.ToInt64(bytes, 8);
			
			hiddenValue1 = EncryptLong(long1, currentCryptoKey);
			hiddenValue2 = EncryptLong(long2, currentCryptoKey);
			hash = CalculateGuidHashFromLongs(long1, long2);
		}

		private static long EncryptLong(long value, long key)
		{
			unchecked
			{
				return (value ^ key) + key;
			}
		}
		
		private static long DecryptLong(long value, long key)
		{
			unchecked
			{
				return (value - key) ^ key;
			}
		}

		private Guid InternalDecryptAsGuid()
		{
			if (IsDefault()) this = new ObscuredGuid(default);

			var long1 = DecryptLong(hiddenValue1, currentCryptoKey);
			var long2 = DecryptLong(hiddenValue2, currentCryptoKey);
			
			Span<byte> bytes = stackalloc byte[16];
			BitConverter.TryWriteBytes(bytes, long1);
			BitConverter.TryWriteBytes(bytes.Slice(8), long2);

			var hashValid = ValidateHash(bytes, hash);
			var decryptedGuid = new Guid(bytes);

			if (hashValid && fakeValue == default)
			{
				// init honeypot if it wasn't initialized yet
				if (decryptedGuid != Guid.Empty && ObscuredCheatingDetector.IsRunningInHoneypotMode)
					fakeValue = decryptedGuid;
			}
			
			var honeypotValid = decryptedGuid == fakeValue;
			ObscuredCheatingDetector.TryDetectCheating(this, hashValid, hash, honeypotValid, decryptedGuid, fakeValue); 
			
			return decryptedGuid;
		}

		private static bool CompareArrays(byte[] array1, byte[] array2)
		{
			if (array1 == null && array2 == null) return true;
			if (array1 == null || array2 == null) return false;
			if (array1.Length != array2.Length) return false;
			
			for (int i = 0; i < array1.Length; i++)
			{
				if (array1[i] != array2[i]) return false;
			}
			return true;
		}
		
		public bool IsDefault()
		{
			return hiddenValue1 == default &&
				   hiddenValue2 == default &&
				   currentCryptoKey == default &&
				   hash == default;
		}
	}
} 