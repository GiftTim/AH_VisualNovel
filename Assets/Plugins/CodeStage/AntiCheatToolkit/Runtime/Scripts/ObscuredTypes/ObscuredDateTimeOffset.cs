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
	/// Use it instead of regular <c>DateTimeOffset</c> for any cheating-sensitive properties, fields and other long-term declarations.
	/// </summary>
	/// <remarks>
	/// <strong>📝 Important Notes:</strong><br/>
	/// • Regular type is faster and memory wiser comparing to the obscured one!<br/>
	/// • Use regular type for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).
	/// </remarks>
	[Serializable]
	public partial struct ObscuredDateTimeOffset : IObscuredType
	{
		public int Hash => hash;

		[SerializeField] internal int hash;
		[SerializeField] internal long hiddenValue;
		[SerializeField] internal long currentCryptoKey;
		
		[NonSerialized] internal long fakeValue;

		private ObscuredDateTimeOffset(DateTimeOffset value)
		{
			currentCryptoKey = GenerateKey();
			var utcTicks = value.UtcTicks;
			hiddenValue = EncryptBinary(utcTicks, currentCryptoKey);

			hash = HashUtils.CalculateHash(utcTicks);
			fakeValue = ObscuredCheatingDetector.IsRunningInHoneypotMode ? utcTicks : default;

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
		public static long Encrypt(DateTimeOffset value, long key)
		{
			return EncryptBinary(value.UtcTicks, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static DateTimeOffset Decrypt(long value, long key)
		{
			try
			{
				var utcTicks = DecryptBinary(value, key);
				return new DateTimeOffset(utcTicks, TimeSpan.Zero);
			}
			catch (Exception e)
			{
				Debug.LogWarning(ACTk.LogPrefix + $"Error while decrypting {nameof(ObscuredDateTimeOffset)}:\n{e}");
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
		/// <param name="encrypted">Raw encrypted value you got from GetEncrypted().</param>
		/// <param name="key">Encryption key you've got from GetEncrypted().</param>
		/// <returns>New obscured variable initialized from specified encrypted value.</returns>
		/// <seealso cref="GetEncrypted"/>
		/// <seealso cref="SetEncrypted"/>
		public static ObscuredDateTimeOffset FromEncrypted(long encrypted, long key)
		{
			var instance = new ObscuredDateTimeOffset();
			instance.SetEncrypted(encrypted, key);
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

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// <remarks>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// </remarks>
		/// <seealso cref="FromEncrypted"/>
		/// <seealso cref="SetEncrypted"/>
		public long GetEncrypted(out long key)
		{
			if (IsDefault()) this = new ObscuredDateTimeOffset(default);
			
			key = currentCryptoKey;
			return hiddenValue;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// <remarks>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// </remarks>
		/// <seealso cref="FromEncrypted"/>
		public void SetEncrypted(long encrypted, long key)
		{
			currentCryptoKey = key;
			var plain = Decrypt(encrypted, key);
			hiddenValue = encrypted;
			hash = HashUtils.CalculateHash(plain.UtcTicks);

			if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				fakeValue = plain.UtcTicks;
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public DateTimeOffset GetDecrypted()
		{
			return new DateTimeOffset(InternalDecrypt(), TimeSpan.Zero);
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			HideValue(decrypted);
		}
		
		private static bool ValidateHash(DateTimeOffset input, int hash)
		{
			return ValidateHash(input.UtcTicks, hash);
		}

		private static bool ValidateHash(long input, int hash)
		{
#if DEBUG
			if (hash == default && HashUtils.CalculateHash(input) != default)
				Debug.LogError(ACTk.LogPrefix + $"{nameof(hash)} is not initialized properly!\n" +
							   "It will produce false positive cheating detection.\n" +
							   "Can happen when migrating from older ACTk versions.\n" +
							   "Please call Tools > Code Stage > Anti-Cheat Toolkit > Migrate > * menu item to try fixing this.");
#endif
			return HashUtils.ValidateHash(input, hash);
		}

		private void HideValue(long plain)
		{
			hiddenValue = EncryptBinary(plain, currentCryptoKey);
			hash = HashUtils.CalculateHash(plain);
		}

		private static long EncryptBinary(long value, long key)
		{
			unchecked
			{
				return (value ^ key) + key;
			}
		}
		
		private static long DecryptBinary(long value, long key)
		{
			unchecked
			{
				return (value - key) ^ key;
			}
		}

		private DateTimeOffset InternalDecryptAsDateTimeOffset()
		{
			return new DateTimeOffset(InternalDecrypt(), TimeSpan.Zero);
		}

		private long InternalDecrypt()
		{
			if (IsDefault()) this = new ObscuredDateTimeOffset(default);

			var plain = DecryptBinary(hiddenValue, currentCryptoKey);
			var hashValid = ValidateHash(plain, hash);

			if (hashValid && fakeValue == default)
			{
				// init honeypot if it wasn't initialized yet
				if (plain != default && ObscuredCheatingDetector.IsRunningInHoneypotMode)
					fakeValue = plain;
			}
			
			var honeypotValid = plain == fakeValue;
			ObscuredCheatingDetector.TryDetectCheating(this, hashValid, hash, honeypotValid, plain, fakeValue); 
			
			return plain;
		}
		
		public bool IsDefault()
		{
			return hiddenValue == default &&
				   currentCryptoKey == default &&
				   hash == default;
		}
	}
} 