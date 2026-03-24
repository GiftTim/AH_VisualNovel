#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	public partial struct ObscuredGuid : IFormattable, IEquatable<ObscuredGuid>, IEquatable<Guid>, IComparable<ObscuredGuid>, IComparable<Guid>, IComparable
	{
		[System.Reflection.Obfuscation(Exclude = true)]
		public static implicit operator ObscuredGuid(Guid value)
		{
			return new ObscuredGuid(value);
		}
		
		[System.Reflection.Obfuscation(Exclude = true)]
		public static implicit operator Guid(ObscuredGuid value)
		{
			return value.InternalDecryptAsGuid();
		}

		public override int GetHashCode()
		{
			return InternalDecryptAsGuid().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecryptAsGuid().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecryptAsGuid().ToString(format);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecryptAsGuid().ToString(format, provider);
		}

		public override bool Equals(object other)
		{
			return other is ObscuredGuid o && Equals(o) ||
				   other is Guid r && Equals(r);
		}

		public bool Equals(ObscuredGuid other)
		{
			return currentCryptoKey == other.currentCryptoKey ? 
				(hiddenValue1.Equals(other.hiddenValue1) && hiddenValue2.Equals(other.hiddenValue2)) : 
				InternalDecryptAsGuid().Equals(other.InternalDecryptAsGuid());
		}
		
		public bool Equals(Guid other)
		{
			return InternalDecryptAsGuid().Equals(other);
		}

		public int CompareTo(ObscuredGuid other)
		{
			return InternalDecryptAsGuid().CompareTo(other.InternalDecryptAsGuid());
		}

		public int CompareTo(Guid other)
		{
			return InternalDecryptAsGuid().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			return InternalDecryptAsGuid().CompareTo(obj);
		}

		public static bool operator ==(ObscuredGuid g1, Guid g2)
		{
			return (Guid)g1 == g2;
		}
		
		public static bool operator ==(Guid g1, ObscuredGuid g2)
		{
			return g1 == (Guid)g2;
		}

		public static bool operator ==(ObscuredGuid g1, ObscuredGuid g2)
		{
			return (Guid)g1 == (Guid)g2;
		}

		public static bool operator !=(ObscuredGuid g1, Guid g2)
		{
			return (Guid)g1 != g2;
		}
		
		public static bool operator !=(Guid g1, ObscuredGuid g2)
		{
			return g1 != (Guid)g2;
		}

		public static bool operator !=(ObscuredGuid g1, ObscuredGuid g2)
		{
			return (Guid)g1 != (Guid)g2;
		}
	}
} 