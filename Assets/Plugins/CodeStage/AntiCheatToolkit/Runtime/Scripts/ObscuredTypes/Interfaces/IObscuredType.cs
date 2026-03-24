#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.ObscuredTypes
{
	/// <summary>
	/// Base interface for all obscured types.
	/// </summary>
	public interface IObscuredType
	{
		/// <summary>
		/// Stable, integrity-check hash of the decrypted value stored alongside the encrypted data.
		/// Use for zero-GC dictionary keys and lookups. May change when the value changes.
		/// </summary>
		int Hash { get; }
		/// <summary>
		/// Allows to change current crypto key to the new random value and re-encrypt variable using it.
		/// Use it for extra protection against 'unknown value' search.
		/// Just call it sometimes when your variable doesn't change to fool the cheater.
		/// </summary>
		void RandomizeCryptoKey();
	}

	public interface ISerializableObscuredType
	{
#if UNITY_EDITOR
		bool IsDataValid { get; }
		bool IsDefault();
#endif
	}
}