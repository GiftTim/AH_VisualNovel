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
	/// Use it instead of regular <c>int</c> for any cheating-sensitive properties, fields and other long-term declarations.
	/// </summary>
	/// <remarks>
	/// <strong>📝 Important Notes:</strong><br/>
	/// • Regular type is faster and memory wiser comparing to the obscured one!<br/>
	/// • Use regular type for all short-term operations and calculations while keeping obscured type only at the long-term declaration (i.e. class field).<br/><br/>
	/// <strong>✅ DOTS:</strong> Supports IComponentData out of the box
	/// 
	/// <example>
	/// <code>
	/// <![CDATA[
	/// public class PlayerData : MonoBehaviour
	/// {
	///     [SerializeField] private ObscuredInt coins = 100;
	///     [SerializeField] private ObscuredInt health = 100;
	///     [SerializeField] private ObscuredInt score = 0;
	///     
	///     public void AddCoins(int amount)
	///     {
	///         coins += amount; // Implicit casting works seamlessly
	///     }
	///     
	///     public void TakeDamage(int damage)
	///     {
	///         health -= damage;
	///         if (health <= 0) GameOver();
	///     }
	///     
	///     public int GetScore() => score; // Automatic decryption
	/// }
	/// ]]>
	/// </code>
	/// </example>
	/// 
	/// <example>
	/// <code>
	/// <![CDATA[
	/// // Field declaration and initialization
	/// private ObscuredInt playerLevel = 1;
	/// private ObscuredInt experience = 0;
	/// 
	/// // Arithmetic operations work transparently
	/// experience += 50;
	/// playerLevel = (int)Mathf.Floor(experience / 100f) + 1;
	/// 
	/// // Comparison operations
	/// if (experience >= 1000) UnlockAchievement();
	/// 
	/// // Debug output (automatic decryption)
	/// Debug.Log($"Player Level: {playerLevel}, XP: {experience}");
	/// ]]>
	/// </code>
	/// </example>
	/// </remarks>
	[Serializable]
    public partial struct ObscuredInt : IObscuredType
    {
        public int Hash => hash;

		[SerializeField] internal int hash;
		[SerializeField] internal int hiddenValue;
		[SerializeField] internal int currentCryptoKey;

		[NonSerialized] internal int fakeValue;

		private ObscuredInt(int value)
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
		/// Encrypts passed value using passed key.
		/// </summary>
		/// <remarks>
		/// Key can be generated automatically using GenerateKey().
		/// </remarks>
		/// <seealso cref="Decrypt"/>
		/// <seealso cref="GenerateKey"/>
		public static int Encrypt(int value, int key)
		{
			unchecked
			{
				return (value ^ key) + key;
			}
		}

		/// <summary>
		/// Decrypts passed value you got from Encrypt() using same key.
		/// </summary>
		/// <seealso cref="Encrypt"/>
		public static int Decrypt(int value, int key)
		{
			unchecked
			{
				return (value - key) ^ key;
			}
		}

		/// <summary>
		/// Creates and fills obscured variable with raw encrypted value previously got from GetEncrypted().
		/// </summary>
		/// <remarks>
		/// Literally does same job as SetEncrypted() but makes new instance instead of filling existing one,
		/// making it easier to initialize new variables from saved encrypted values.
		/// </remarks>
		/// <param name="encrypted">Raw encrypted value you got from GetEncrypted().</param>
		/// <param name="key">Encryption key you've got from GetEncrypted().</param>
		/// <returns>New obscured variable initialized from specified encrypted value.</returns>
		/// <seealso cref="GetEncrypted"/>
		/// <seealso cref="SetEncrypted"/>
		public static ObscuredInt FromEncrypted(int encrypted, int key)
		{
			var instance = new ObscuredInt();
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
		/// <remarks>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		/// </remarks>
		/// <param name="key">Encryption key needed to decrypt returned value.</param>
		/// <returns>Encrypted value as is.</returns>
		/// <seealso cref="FromEncrypted"/>
		/// <seealso cref="SetEncrypted"/>
		public int GetEncrypted(out int key)
		{
			if (IsDefault()) this = new ObscuredInt(default);
			
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
		public void SetEncrypted(int encrypted, int key)
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
		public int GetDecrypted()
		{
			return InternalDecrypt();
		}

		public void RandomizeCryptoKey()
		{
			hiddenValue = InternalDecrypt();
			currentCryptoKey = GenerateKey();
			HideValue(hiddenValue);
		}
		
		private static bool ValidateHash(int input, int hash)
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
		
		private void HideValue(int plain)
		{
			hiddenValue = Encrypt(plain, currentCryptoKey);
			hash = HashUtils.CalculateHash(plain);
		}

		private int InternalDecrypt()
		{
			if (IsDefault()) this = new ObscuredInt(default);
			
			var plain = Decrypt(hiddenValue, currentCryptoKey);
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

		#region obsolete
		
		//! @cond

		[Obsolete("This API is redundant and does not perform any actions.", true)]
		public static void SetNewCryptoKey(int newKey) {}

		[Obsolete("This API is redundant and does not perform any actions. It will be removed in future updates.", true)]
		public void ApplyNewCryptoKey() {}

		/// <summary>
		/// Decrypts data encrypted in ACTk 2024.0 or earlier.
		/// </summary>
		public static int DecryptFromV0(int value, int key)
		{
			return value ^ key;
		}
		
		//! @endcond

		#endregion
	}
}