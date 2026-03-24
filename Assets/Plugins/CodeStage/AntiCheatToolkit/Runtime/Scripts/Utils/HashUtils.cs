#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;
using System.Buffers;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CodeStage.AntiCheat.Utils
{
	/// <summary>
	/// Utility class for calculating and validating hash values for various data types.
	/// Used by ACTk's Obscured types for integrity checking and tamper detection.
	/// 
	/// <strong>⚠️ Important:</strong> Hash functions can produce collisions - different inputs may generate the same hash value.
	/// This is normal behavior for hash functions and should be considered when using these methods.
	/// </summary>
	public static class HashUtils
	{
		private const int Salt = 0x7E3779B9;

		public static bool ValidateHash(BigInteger input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(bool input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(decimal input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(int input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(long input, int hash)
		{
			return CalculateHash(input) == hash;
		}

		public static bool ValidateHash(uint input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(ulong input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(Quaternion input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(Vector2 input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(Vector2Int input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(Vector3 input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(Vector3Int input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(float input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(double input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(char[] input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(DateTimeOffset input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static bool ValidateHash(Guid input, int hash)
		{
			return CalculateHash(input) == hash;
		}
		
		public static int CalculateHashGeneric<T>(T input)
		{
			switch (input)
			{
				case BigInteger value:
					return CalculateHash(value);
				case bool value:
					return CalculateHash(value);	
				case byte value:
					return CalculateHash(value);	
				case char value:
					return CalculateHash(value);
				case DateTime value:
					return CalculateHash(value);
				case DateTimeOffset value:
					return CalculateHash(value);
				case decimal value:
					return CalculateHash(value);	
				case double value:
					return CalculateHash(value);	
				case float value:
					return CalculateHash(value);	
				case Guid value:
					return CalculateHash(value);
				case int value:
					return CalculateHash(value);	
				case long value:
					return CalculateHash(value);	
				case Quaternion value:
					return CalculateHash(value);	
				case sbyte value:
					return CalculateHash(value);	
				case short value:
					return CalculateHash(value);	
				case string value:
					return CalculateHash(value.ToCharArray());	
				case char[] value:
					return CalculateHash(value);	
				case uint value:
					return CalculateHash(value);	
				case ulong value:
					return CalculateHash(value);	
				case ushort value:
					return CalculateHash(value);	
				case Vector2 value:
					return CalculateHash(value);	
				case Vector2Int value:
					return CalculateHash(value);	
				case Vector3 value:
					return CalculateHash(value);	
				case Vector3Int value:
					return CalculateHash(value);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static int CalculateHash(BigInteger x)
		{	
			var bytesCount = x.GetByteCount();
			var localBuffer = ArrayPool<byte>.Shared.Rent(bytesCount);
			try
			{
				if (x.TryWriteBytes(localBuffer, out var bytesWritten))
					bytesCount = bytesWritten;
				else 
					throw new Exception("Unable to get bytes for hashing for " + x);
				
				var hash = xxHash.CalculateHash(localBuffer, bytesCount, Salt);
				return unchecked((int)hash);
			}
			catch (Exception e)
			{
				Common.ACTk.PrintExceptionForSupport("Unable to calculate hash!" + x, e);
				throw;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(localBuffer);
			}
		}
		
		public static int CalculateHash(bool x)
		{
			return x ? 354242342 : 756756344 ^ Salt;
		}
		
		public static int CalculateHash(decimal x)
		{
			var bits = decimal.GetBits(x);
			
			var high = ((long)bits[2] << 32) | (uint)bits[3];
			var low = ((long)bits[1] << 32) | (uint)bits[0];
			
			const int prime = 31;
			return unchecked(prime * CalculateHash(high) + CalculateHash(low) + Salt);
		}

		public static int CalculateHash(int x) // doesn't return 0 for any input
		{
			const int a = 16777618;
			var hash = unchecked((int)2166136262);

			hash = (hash ^ (x & 0xff)) * a;
			hash = (hash ^ ((x >> 8) & 0xff)) * a;
			hash = (hash ^ ((x >> 16) & 0xff)) * a;
			hash = (hash ^ ((x >> 24) & 0xff)) * a;

			return hash | 1;
		}
		
		public static int CalculateHash(DateTime x)
		{
			return CalculateHash(x.ToBinary());
		}
		
		public static int CalculateHash(DateTimeOffset x)
		{
			return CalculateHash(x.UtcTicks);
		}
		
		public static int CalculateHash(Guid x)
		{
			Span<byte> bytes = stackalloc byte[16];
			x.TryWriteBytes(bytes);
			var long1 = BitConverter.ToInt64(bytes);
			var long2 = BitConverter.ToInt64(bytes.Slice(8));
			var hash1 = CalculateHash(long1);
			var hash2 = CalculateHash(long2) << 2;
			return (hash1 ^ hash2) | 1;
		}
		
		public static int CalculateHash(long x)
		{
			var low = (int)(x & 0xFFFFFFFF);
			var high = (int)(x >> 32);
			
			return (CalculateHash(low) >> 2) ^ CalculateHash(high);
		}
		
		public static int CalculateHash(uint x)
		{
			return CalculateHash((int)(x & 0xFFFFFFFF));
		}
		
		public static int CalculateHash(ulong x)
		{
			var low = (int)(x & 0xFFFFFFFF);
			var high = (int)(x >> 32);
			
			return (CalculateHash(low) >> 2) ^ CalculateHash(high);
		}
		
		public static int CalculateHash(float x)
		{
			return CalculateHash(BitConverter.SingleToInt32Bits(x));
		}
		
		public static int CalculateHash(double x)
		{
			var bits = BitConverter.DoubleToInt64Bits(x);
			
			var hash = (int)(bits ^ (bits >> 32));
			const int prime = 31;
			return unchecked(prime * hash + (int)(bits >> 16) + Salt);
		}
		
		public static int CalculateHash(Quaternion input)
		{
			var xHash = CalculateHash(input.x);
			var yHash = CalculateHash(input.y) << 2;
			var zHash = CalculateHash(input.z) >> 2;
			var wHash = CalculateHash(input.w) >> 1;
			return (xHash ^ yHash ^ zHash ^ wHash) | 1;
		}
		
		public static int CalculateHash(Vector2 input)
		{
			var xHash = CalculateHash(input.x);
			var yHash = CalculateHash(input.y) << 2;
			return (xHash ^ yHash) | 1;
		}
		
		public static int CalculateHash(Vector2Int input)
		{
			var xHash = CalculateHash(input.x);
			var yHash = CalculateHash(input.y) << 2;
			return (xHash ^ yHash) | 1;
		}
		
		public static int CalculateHash(Vector3 input)
		{
			var xHash = CalculateHash(input.x);
			var yHash = CalculateHash(input.y) << 2;
			var zHash = CalculateHash(input.z) >> 2;
			return (xHash ^ yHash ^ zHash) | 1;
		}
		
		public static int CalculateHash(Vector3Int input)
		{
			var xHash = CalculateHash(input.x);
			var yHash = CalculateHash(input.y);
			var zHash = CalculateHash(input.z);
			return (xHash ^ yHash << 4 ^ yHash >> 28 ^ zHash >> 4 ^ zHash << 28) | 1;
		}
		
		public static int CalculateHash(char[] input)
		{
			var hash = Salt;
			
			if (input == null || input.Length == 0)
				return hash;

			unchecked
			{
				foreach (var c in input)
				{
					hash = ((hash << 5) + hash) ^ c;
				}
			}
			
			return hash | 1;
		}

        public static int CalculateHash(string input)
		{
			var hash = Salt;
			if (string.IsNullOrEmpty(input))
				return hash;

			unchecked
			{
				for (var i = 0; i < input.Length; i++)
				{
					hash = ((hash << 5) + hash) ^ input[i];
				}
			}

			return hash | 1;
		}
	}
}