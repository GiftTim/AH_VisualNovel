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
	/// Use it instead of regular <c>Vector2Int</c> for any cheating-sensitive properties, fields and other long-term declarations.
	/// </summary>
	/// <remarks>
	/// <strong>⚠️ Warning:</strong> Doesn't mimic regular type API, thus should be used with extra caution. Cast it to regular, not obscured type to work with regular APIs.<br/><br/>
	/// <strong>📝 Important Notes:</strong><br/>
	/// • Regular type is faster and memory wiser comparing to the obscured one!<br/>
	/// • Use regular type for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).
	/// </remarks>
	[Serializable]
	public partial struct ObscuredVector2Int : IObscuredType
	{
		public int Hash => hash;

		[SerializeField] internal int hash;
		[SerializeField] internal RawEncryptedVector2Int hiddenValue;
		[SerializeField] internal int currentCryptoKey;

		[NonSerialized] internal Vector2Int fakeValue;

		private ObscuredVector2Int(Vector2Int value)
		{
			currentCryptoKey = GenerateKey();
			hiddenValue = Encrypt(value, currentCryptoKey);
			hash = HashUtils.CalculateHash(value);
			fakeValue = ObscuredCheatingDetector.IsRunningInHoneypotMode ? value : default;

#if UNITY_EDITOR
			version = Version;
#endif
		}

		/// <summary>
		/// Mimics constructor of regular Vector2Int.
		/// </summary>
		/// <param name="x">X component of the vector</param>
		/// <param name="y">Y component of the vector</param>
		public ObscuredVector2Int(int x, int y) : this(new Vector2Int(x, y)) { }

		public int x
		{
			get => InternalDecrypt().x;
			set => this = new ObscuredVector2Int(value, y);
		}

		public int y
		{
			get => InternalDecrypt().y;
			set => this = new ObscuredVector2Int(x, value);
		}

		public int this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return x;
					case 1:
						return y;
					default:
						throw new IndexOutOfRangeException($"Invalid {nameof(ObscuredVector2Int)} index!");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					default:
						throw new IndexOutOfRangeException($"Invalid {nameof(ObscuredVector2Int)} index!");
				}
			}
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static RawEncryptedVector2Int Encrypt(Vector2Int value, int key)
		{
			return Encrypt(value.x, value.y, key);
		}

		/// <summary>
		/// Encrypts passed components using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static RawEncryptedVector2Int Encrypt(int x, int y, int key)
		{
			RawEncryptedVector2Int result;
			result.x = ObscuredInt.Encrypt(x, key);
			result.y = ObscuredInt.Encrypt(y, key);

			return result;
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static Vector2Int Decrypt(RawEncryptedVector2Int value, int key)
		{
			var result = new Vector2Int
			{
				x = ObscuredInt.Decrypt(value.x, key), 
				y = ObscuredInt.Decrypt(value.y, key)
			};

			return result;
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
		public static ObscuredVector2Int FromEncrypted(RawEncryptedVector2Int encrypted, int key)
		{
			var instance = new ObscuredVector2Int();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static int GenerateKey()
		{
			return RandomUtils.GenerateIntKey();
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
		public RawEncryptedVector2Int GetEncrypted(out int key)
		{
			if (IsDefault()) this = new ObscuredVector2Int(default);

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
		public void SetEncrypted(RawEncryptedVector2Int encrypted, int key)
		{
			currentCryptoKey = key;
			var plain = Decrypt(encrypted, key);
			hiddenValue = encrypted;
			hash = HashUtils.CalculateHash(plain);

			if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				fakeValue = plain;
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public Vector2Int GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			HideValue(decrypted);
		}
		
		private static bool ValidateHash(Vector2Int input, int hash)
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
		
		private void HideValue(Vector2Int plain)
		{
			hiddenValue = Encrypt(plain, currentCryptoKey);
			hash = HashUtils.CalculateHash(plain);
		}

		private Vector2Int InternalDecrypt()
		{
			if (IsDefault()) this = new ObscuredVector2Int(default);

			var realValue = Decrypt(hiddenValue, currentCryptoKey);
			var hashValid = ValidateHash(realValue, hash);
			
			if (hashValid && fakeValue == default)
			{
				// init honeypot if it wasn't initialized yet
				if (realValue != default && ObscuredCheatingDetector.IsRunningInHoneypotMode)
					fakeValue = realValue;
			}

			var honeypotValid = realValue == fakeValue;
			ObscuredCheatingDetector.TryDetectCheating(this, hashValid, hash, honeypotValid, realValue, fakeValue); 
			
			return realValue;
		}
		
		public bool IsDefault()
		{
			return hiddenValue.x == default &&
				   hiddenValue.y == default &&
				   currentCryptoKey == default &&
				   hash == default;
		}

		//! @cond

		#region obsolete

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public static void SetNewCryptoKey(int newKey) { }

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public void ApplyNewCryptoKey() { }
		
		/// <summary>
		/// Decrypts data encrypted in ACTk 2024.0 or earlier.
		/// </summary>
		public static Vector2Int DecryptFromV0(RawEncryptedVector2Int value, int key)
		{
			var result = new Vector2Int
			{
				x = ObscuredInt.DecryptFromV0(value.x, key), 
				y = ObscuredInt.DecryptFromV0(value.y, key)
			};

			return result;
		}

		#endregion

		//! @endcond
	}
}