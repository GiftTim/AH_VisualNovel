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
	/// Use it instead of regular <c>string</c> for any cheating-sensitive properties, fields and other long-term declarations.
	/// </summary>
	/// <remarks>
	/// <strong>📝 Important Notes:</strong><br/>
	/// • Regular type is faster and memory wiser comparing to the obscured one!<br/>
	/// • Use regular type for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).<br/><br/>
	/// <strong>🚫 DOTS:</strong> Not supported - uses managed arrays and reference types incompatible with IComponentData
	/// </remarks>
	[Serializable]
    public sealed partial class ObscuredString : IObscuredType
    {
        public int Hash => hash;

		[SerializeField] internal int hash;

		[SerializeField] internal char[] cryptoKey;
		[SerializeField] internal char[] hiddenChars;
		
		[NonSerialized] private string fakeValue = string.Empty;

		// for serialization purposes
		private ObscuredString(){}

		private ObscuredString(string value)
		{
			cryptoKey = InitKey();
			var chars = value?.ToCharArray();
			hiddenChars = InternalEncryptDecrypt(chars, cryptoKey);
			hash = HashUtils.CalculateHash(chars);
			
			fakeValue = ObscuredCheatingDetector.IsRunningInHoneypotMode ? value : string.Empty;
			
#if UNITY_EDITOR
			version = Version;
#endif
		}

		internal static char[] InitKey()
		{
			var key = new char[7];
			GenerateKey(ref key);
			return key;
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static char[] Encrypt(string value, string key)
		{
			return Encrypt(value, key.ToCharArray());
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static char[] Encrypt(string value, char[] key)
		{
			return Encrypt(value?.ToCharArray(), key);
		}

		/// <summary>
		/// Encrypts passed value using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static char[] Encrypt(char[] value, char[] key)
		{
			return InternalEncryptDecrypt(value, key);
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static string Decrypt(char[] value, string key)
		{
			return Decrypt(value, key.ToCharArray());
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static string Decrypt(char[] value, char[] key)
		{
			return value == null ? null : new string(InternalEncryptDecrypt(value, key));
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
		public static ObscuredString FromEncrypted(char[] encrypted, char[] key)
		{
			var instance = new ObscuredString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}

		/// <summary>
		/// Generates random key in new allocated array. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <returns>Key suitable for manual Encrypt() calls.</returns>
		public static char[] GenerateKey()
		{
			var arrayToFill = new char[7];
			GenerateKey(ref arrayToFill);
			return arrayToFill;
		}

		/// <summary>
		/// Generates random key. Used internally and can be used to generate key for manual Encrypt() calls.
		/// </summary>
		/// <param name="arrayToFill">Preallocated char array. Only first 7 bytes are filled.</param>
		public static void GenerateKey(ref char[] arrayToFill)
		{
			RandomUtils.GenerateCharArrayKey(ref arrayToFill);
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
		public char[] GetEncrypted(out char[] key)
		{
			if (IsDefault())
			{
				var encrypted = new ObscuredString(string.Empty).GetEncrypted(out key);
				SetEncrypted(encrypted, key);
			}
			
			key = cryptoKey;
			return hiddenChars;
		}

		/// <summary>
		/// Allows to explicitly set current obscured value. Crypto key should be same as when encrypted value was got with GetEncrypted().
		/// </summary>
		/// <remarks>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		/// </remarks>
		/// <seealso cref="FromEncrypted"/>
		public void SetEncrypted(char[] encrypted, char[] key)
		{
			cryptoKey = key;
			var plain = InternalEncryptDecrypt(encrypted, key);
			hiddenChars = encrypted;
			hash = HashUtils.CalculateHash(plain);

			if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				fakeValue = new string(plain);
		}

		/// <summary>
		/// Alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value.</returns>
		public string GetDecrypted()
		{
			return InternalDecryptToString();
		}

		/// <summary>
		/// GC-friendly alternative to the type cast, use if you wish to get decrypted value
		/// but can't or don't want to use cast to the regular type.
		/// </summary>
		/// <returns>Decrypted value as a raw chars array in case you don't wish to allocate new string.</returns>
		public char[] GetDecryptedToChars()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			var decrypted = InternalDecrypt();
			GenerateKey(ref cryptoKey);
			HideValue(decrypted);
		}
		
		private static bool ValidateHash(char[] input, int hash)
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
		
		private void HideValue(char[] plain)
		{
			hiddenChars = Encrypt(plain, cryptoKey);
			hash = HashUtils.CalculateHash(plain);
		}

		internal static char[] InternalEncryptDecrypt(char[] value, char[] key)
		{
			if (value == null || value.Length == 0)
				return value;

			if (key.Length == 0)
			{
				Debug.LogError(ACTk.LogPrefix + "Empty key can't be used for string encryption or decryption!");
				return value;
			}

			var valueLength = value.Length;
			var result = new char[valueLength];

			for (var i = 0; i < valueLength; i++)
			{
				result[i] = XorAt(value, key, i);
			}

			return result;
		}

		internal static char XorAt(char[] value, char[] key, int index)
		{
			return (char)(value[index] ^ key[index % key.Length]);
		}

		internal static bool IsNullOrWhiteSpaceInternal(ObscuredString value)
		{
			if ((object)value == null || value.IsDefault()) return true;
			var enc = value.hiddenChars;
			var key = value.cryptoKey;
			if (enc == null || enc.Length == 0) return true;
			if (key == null || key.Length == 0) return false;
			for (var i = 0; i < enc.Length; i++)
			{
				var ch = XorAt(enc, key, i);
				if (!char.IsWhiteSpace(ch))
					return false;
			}
			return true;
		}

		private string InternalDecryptToString()
		{
			return new string(InternalDecrypt());
		}

		private char[] InternalDecrypt()
		{ 
			if (IsDefault())
			{
				var encrypted = new ObscuredString(string.Empty).GetEncrypted(out var key);
				SetEncrypted(encrypted, key);
			}
			
			var realValue = InternalEncryptDecrypt(hiddenChars, cryptoKey);
			var hashValid = ValidateHash(realValue, hash);
			
			if (hashValid && fakeValue == string.Empty)
			{
				// init honeypot if it wasn't initialized yet
				if (realValue != null && realValue.Length != 0 && ObscuredCheatingDetector.IsRunningInHoneypotMode)
					fakeValue = new string(realValue);
			}
			
			var honeypotValid = Compare(realValue, fakeValue); 
			ObscuredCheatingDetector.TryDetectCheating(this, hashValid, hash, honeypotValid, realValue, fakeValue); 
			
			return realValue;
		}
		
		public bool IsDefault()
		{
			return (cryptoKey == default || cryptoKey?.Length == 0) &&
				   (hiddenChars == default || hiddenChars?.Length == 0) &&
				   hash == default;
		}

		private bool Compare(char[] chars, string s)
		{
			if (chars?.Length != s?.Length) return false;

			for (var i = 0; i < chars?.Length; i++)
			{
				if (chars[i] != s[i])
					return false;
			}

			return true;
		}

		//! @cond

		#region obsolete
	
#pragma warning disable 0649
		[SerializeField] internal string currentCryptoKey; // deprecated
		[SerializeField] internal byte[] hiddenValue; // deprecated
#pragma warning restore 0649


		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public static void SetNewCryptoKey(string newKey) {}

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public void ApplyNewCryptoKey() {}

		[Obsolete("Please use new Encrypt(value, key) or Decrypt(value, key) API instead.", true)]
		public static string EncryptDecrypt(string value) { throw new Exception(); }


		[Obsolete("Please use new Encrypt(value, key) or Decrypt(value, key) APIs instead. " +
		          "This API will be removed in future updates.")]
		public static string EncryptDecrypt(string value, string key)
		{
			return EncryptDecryptObsolete(value, key);
		}

/*		[Obsolete("Please use new FromEncrypted(encrypted, key) API instead.", true)]
		public static ObscuredString FromEncrypted(string encrypted) { throw new Exception(); }*/

		[Obsolete("Please use new GetEncrypted(out key) API instead.", true)]
		public string GetEncrypted() { throw new Exception(); }

		[Obsolete("Please use new SetEncrypted(char[], char[]) API instead.", true)]
		public void SetEncrypted(string encrypted) {}
		
		[Obsolete("Use this only to decrypt data encrypted with previous ACTk versions. " +
				  "Please use FromEncrypted(char[], char[]) in other cases.")]
		public static ObscuredString FromEncrypted(string encrypted, string key = "4441")
		{
			var instance = new ObscuredString();
			instance.SetEncrypted(encrypted, key);
			return instance;
		}
		
		[Obsolete("Use this only to decrypt data encrypted with previous ACTk versions. " +
				  "Please use SetEncrypted(char[], char[]) in other cases.")]
		public void SetEncrypted(string encrypted, string key)
		{
			var decrypted = EncryptDecryptObsolete(encrypted, key);
			cryptoKey = GenerateKey();
			hiddenChars = Encrypt(decrypted, cryptoKey);
			hash = HashUtils.CalculateHash(decrypted.ToCharArray());

			if (ObscuredCheatingDetector.IsRunningInHoneypotMode)
				fakeValue = decrypted;
		}
		
		[Obsolete("Please use version with ref argument or without arguments instead.", true)]
		public static char[] GenerateKey(char[] arrayToFill)
		{
			RandomUtils.GenerateCharArrayKey(ref arrayToFill);
			return arrayToFill;
		}
		
		internal static string EncryptDecryptObsolete(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError(ACTk.LogPrefix + "Empty key can't be used for string encryption or decryption!");
				return string.Empty;
			}

			var keyLength = key.Length;
			var valueLength = value.Length;
			var result = new char[valueLength];

			for (var i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return new string(result);
		}
		
		internal static string GetStringObsolete(byte[] bytes)
		{
			var chars = new char[bytes.Length / sizeof(char)];
			Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		internal static byte[] GetBytesObsolete(string str)
		{
			var bytes = new byte[str.Length * sizeof(char)];
			Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}
		
		#endregion

		//! @endcond

		private static bool ArraysEquals(char[] a1, char[] a2)
		{
			if (a1 == a2) return true;
			if (a1 == null || a2 == null) return false;
			if (a1.Length != a2.Length) return false;

			for (var i = 0; i < a1.Length; i++)
			{
				if (a1[i] != a2[i])
					return false;
			}
			return true;
		}
	}
}