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
using UnityEngine.Serialization;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	/// <summary>
	/// Use it instead of regular <c>bool</c> for any cheating-sensitive properties, fields and other long-term declarations.
	/// </summary>
	/// <remarks>
	/// <strong>📝 Important Notes:</strong><br/>
	/// • Regular type is faster and memory wiser comparing to the obscured one!<br/>
	/// • Use regular type for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).<br/><br/>
	/// <strong>✅ DOTS:</strong> Supports IComponentData out of the box
	/// </remarks>
	[Serializable]
    public partial struct ObscuredBool : IObscuredType
    {
        public int Hash => hash;

		[SerializeField] internal int hash;
		[SerializeField] internal int hiddenValue;
		[SerializeField] internal byte currentCryptoKey;

		[NonSerialized] private bool _fakeValue;
		[NonSerialized] private bool _fakeValueActive;
		internal bool FakeValue
		{
			set
			{
				_fakeValue = value;
				_fakeValueActive = value;
			}

			get => _fakeValue;
		}

		private ObscuredBool(bool value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);
			hash = HashUtils.CalculateHash(value);
			_fakeValue = ObscuredCheatingDetector.IsRunningInHoneypotMode ? value : default;
			_fakeValueActive = ObscuredCheatingDetector.IsRunningInHoneypotMode;

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
		public static int Encrypt(bool value, byte key)
		{
			unchecked
			{
				return ((value ? 213 : 181) ^ key) + key;
			}
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static bool Decrypt(int value, byte key)
		{
			unchecked
			{
				return ((value - key) ^ key) != 181;
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
		public static ObscuredBool FromEncrypted(int encrypted, byte key)
		{
			var instance = new ObscuredBool();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static byte GenerateKey()
		{
			return RandomUtils.GenerateByteKey();
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
		public int GetEncrypted(out byte key)
		{
			if (IsDefault()) this = new ObscuredBool(default);
			
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
		/// <seealso cref="GetEncrypted"/>
		public void SetEncrypted(int encrypted, byte key)
		{
			currentCryptoKey = key;
			var plain = Decrypt(encrypted, key);
			hiddenValue = encrypted;
			hash = HashUtils.CalculateHash(plain);

			if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				FakeValue = plain;
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public bool GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			HideValue(decrypted);
		}
		
		private static bool ValidateHash(bool input, int hash)
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

		private void HideValue(bool plain)
		{
			hiddenValue = Encrypt(plain, currentCryptoKey);
			hash = HashUtils.CalculateHash(plain);
		}

		private bool InternalDecrypt()
		{
			if (IsDefault()) this = new ObscuredBool(default);

			var plain = Decrypt(hiddenValue, currentCryptoKey);
			var hashValid = ValidateHash(plain, hash);

			if (hashValid && FakeValue == default && !_fakeValueActive)
			{
				// init honeypot if it wasn't initialized yet
				if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
					FakeValue = plain;
			}
			
			var honeypotValid = plain == FakeValue;
			ObscuredCheatingDetector.TryDetectCheating(this, hashValid, hash, honeypotValid, plain, FakeValue); 
			
			return plain;
		}
		
		public bool IsDefault()
		{
			return hiddenValue == default &&
				   currentCryptoKey == default &&
				   hash == default;
		}
		
		//! @cond

		#region obsolete

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public static void SetNewCryptoKey(byte newKey) {}

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public void ApplyNewCryptoKey() {}
		
		/// <summary>
		/// Decrypts data encrypted in ACTk 2024.0 or earlier.
		/// </summary>
		public static bool DecryptFromV0(int value, byte key)
		{
			return (value ^ key) != 181;
		}

		#endregion

		//! @endcond
	}
}