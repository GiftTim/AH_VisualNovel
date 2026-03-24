#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	public partial struct ObscuredDateTimeOffset : IFormattable, IEquatable<ObscuredDateTimeOffset>, IEquatable<DateTimeOffset>, IComparable<ObscuredDateTimeOffset>, IComparable<DateTimeOffset>, IComparable
	{
		/// <summary>Gets the number of ticks that represent the date and time of this instance.</summary>
		/// <returns>The number of ticks that represent the date and time of this instance. The value is between <see langword="DateTimeOffset.MinValue.Ticks" /> and <see langword="DateTimeOffset.MaxValue.Ticks" />.</returns>
		public long Ticks => GetDecrypted().Ticks;
		
		/// <summary>Gets the date component of this instance.</summary>
		/// <returns>A new object with the same date as this instance, and the time value set to 12:00:00 midnight (00:00:00).</returns>
		public DateTime Date => GetDecrypted().Date;

		/// <summary>Gets the date and time represented by this instance.</summary>
		/// <returns>The date and time represented by this instance.</returns>
		public DateTime DateTime => GetDecrypted().DateTime;

		/// <summary>Gets the day of the month represented by this instance.</summary>
		/// <returns>The day component, expressed as a value between 1 and 31.</returns>
		public int Day => GetDecrypted().Day;

		/// <summary>Gets the day of the week represented by this instance.</summary>
		/// <returns>An enumerated constant that indicates the day of the week of this <see cref="DateTimeOffset" /> value.</returns>
		public DayOfWeek DayOfWeek => GetDecrypted().DayOfWeek;

		/// <summary>Gets the day of the year represented by this instance.</summary>
		/// <returns>The day of the year, expressed as a value between 1 and 366.</returns>
		public int DayOfYear => GetDecrypted().DayOfYear;

		/// <summary>Gets the hour component of the time represented by this instance.</summary>
		/// <returns>The hour component, expressed as a value between 0 and 23.</returns>
		public int Hour => GetDecrypted().Hour;

		/// <summary>Gets the millisecond component of the time represented by this instance.</summary>
		/// <returns>The millisecond component, expressed as a value between 0 and 999.</returns>
		public int Millisecond => GetDecrypted().Millisecond;

		/// <summary>Gets the minute component of the time represented by this instance.</summary>
		/// <returns>The minute component, expressed as a value between 0 and 59.</returns>
		public int Minute => GetDecrypted().Minute;

		/// <summary>Gets the month component of the date represented by this instance.</summary>
		/// <returns>The month component, expressed as a value between 1 and 12.</returns>
		public int Month => GetDecrypted().Month;

		/// <summary>Gets the time's offset from Coordinated Universal Time (UTC).</summary>
		/// <returns>The time's offset from Coordinated Universal Time (UTC).</returns>
		public TimeSpan Offset => GetDecrypted().Offset;

		/// <summary>Gets the second component of the clock time represented by this instance.</summary>
		/// <returns>The second component, expressed as a value between 0 and 59.</returns>
		public int Second => GetDecrypted().Second;

		/// <summary>Gets the time of day for this instance.</summary>
		/// <returns>A time interval that represents the fraction of the day that has elapsed since midnight.</returns>
		public TimeSpan TimeOfDay => GetDecrypted().TimeOfDay;

		/// <summary>Gets a <see cref="DateTime" /> value that represents the Coordinated Universal Time (UTC) date and time equivalent to the current <see cref="DateTimeOffset" /> object.</summary>
		/// <returns>A value that represents the UTC equivalent of the current <see cref="DateTimeOffset" /> object.</returns>
		public DateTime UtcDateTime => GetDecrypted().UtcDateTime;

		/// <summary>Gets the year component of the date represented by this instance.</summary>
		/// <returns>The year, between 1 and 9999.</returns>
		public int Year => GetDecrypted().Year;

		[System.Reflection.Obfuscation(Exclude = true)]
		public static implicit operator ObscuredDateTimeOffset(DateTimeOffset value)
		{
			return new ObscuredDateTimeOffset(value);
		}
		
		[System.Reflection.Obfuscation(Exclude = true)]
		public static implicit operator DateTimeOffset(ObscuredDateTimeOffset value)
		{
			return value.InternalDecryptAsDateTimeOffset();
		}

		public override int GetHashCode()
		{
			return InternalDecryptAsDateTimeOffset().GetHashCode();
		}

		public override string ToString()
		{
			return InternalDecryptAsDateTimeOffset().ToString();
		}

		public string ToString(string format)
		{
			return InternalDecryptAsDateTimeOffset().ToString(format);
		}

		public string ToString(IFormatProvider provider)
		{
			return InternalDecryptAsDateTimeOffset().ToString(provider);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return InternalDecryptAsDateTimeOffset().ToString(format, provider);
		}

		public override bool Equals(object other)
		{
			return other is ObscuredDateTimeOffset o && Equals(o) ||
				   other is DateTimeOffset r && Equals(r);
		}

		public bool Equals(ObscuredDateTimeOffset other)
		{
			return currentCryptoKey == other.currentCryptoKey ? hiddenValue.Equals(other.hiddenValue) : 
				InternalDecrypt().Equals(other.InternalDecrypt());
		}
		
		public bool Equals(DateTimeOffset other)
		{
			return InternalDecryptAsDateTimeOffset().Equals(other);
		}

		public int CompareTo(ObscuredDateTimeOffset other)
		{
			return InternalDecryptAsDateTimeOffset().CompareTo(other.InternalDecryptAsDateTimeOffset());
		}

		public int CompareTo(DateTimeOffset other)
		{
			return InternalDecryptAsDateTimeOffset().CompareTo(other);
		}

		public int CompareTo(object obj)
		{
			return (InternalDecryptAsDateTimeOffset() as IComparable).CompareTo(obj);
		}

		public static DateTimeOffset operator +(ObscuredDateTimeOffset d, TimeSpan t)
		{
			return (DateTimeOffset)d + t;
		}
		
		public static DateTimeOffset operator -(ObscuredDateTimeOffset d, TimeSpan t)
		{
			return (DateTimeOffset)d - t;
		}

		public static TimeSpan operator -(ObscuredDateTimeOffset d1, DateTimeOffset d2)
		{
			return (DateTimeOffset)d1 - d2;
		}
		
		public static TimeSpan operator -(DateTimeOffset d1, ObscuredDateTimeOffset d2)
		{
			return d1 - (DateTimeOffset)d2;
		}

		public static TimeSpan operator -(ObscuredDateTimeOffset d1, ObscuredDateTimeOffset d2)
		{
			return (DateTimeOffset)d1 - (DateTimeOffset)d2;
		}

		public static bool operator ==(ObscuredDateTimeOffset d1, DateTimeOffset d2)
		{
			return (DateTimeOffset)d1 == d2;
		}
		
		public static bool operator ==(DateTimeOffset d1, ObscuredDateTimeOffset d2)
		{
			return d1 == (DateTimeOffset)d2;
		}

		public static bool operator ==(ObscuredDateTimeOffset d1, ObscuredDateTimeOffset d2)
		{
			return (DateTimeOffset)d1 == (DateTimeOffset)d2;
		}

		public static bool operator !=(ObscuredDateTimeOffset d1, DateTimeOffset d2)
		{
			return (DateTimeOffset)d1 != d2;
		}
		
		public static bool operator !=(DateTimeOffset d1, ObscuredDateTimeOffset d2)
		{
			return d1 != (DateTimeOffset)d2;
		}

		public static bool operator !=(ObscuredDateTimeOffset d1, ObscuredDateTimeOffset d2)
		{
			return (DateTimeOffset)d1 != (DateTimeOffset)d2;
		}

		public static bool operator <(ObscuredDateTimeOffset t1, DateTimeOffset t2)
		{
			return (DateTimeOffset)t1 < t2;
		}
		
		public static bool operator <(DateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return t1 < (DateTimeOffset)t2;
		}

		public static bool operator <(ObscuredDateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return (DateTimeOffset)t1 < (DateTimeOffset)t2;
		}
		
		public static bool operator <=(ObscuredDateTimeOffset t1, DateTimeOffset t2)
		{
			return (DateTimeOffset)t1 <= t2;
		}
		
		public static bool operator <=(DateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return t1 <= (DateTimeOffset)t2;
		}

		public static bool operator <=(ObscuredDateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return (DateTimeOffset)t1 <= (DateTimeOffset)t2;
		}

		public static bool operator >(ObscuredDateTimeOffset t1, DateTimeOffset t2)
		{
			return (DateTimeOffset)t1 > t2;
		}
		
		public static bool operator >(DateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return t1 > (DateTimeOffset)t2;
		}

		public static bool operator >(ObscuredDateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return (DateTimeOffset)t1 > (DateTimeOffset)t2;
		}
		
		public static bool operator >=(ObscuredDateTimeOffset t1, DateTimeOffset t2)
		{
			return (DateTimeOffset)t1 >= t2;
		}
		
		public static bool operator >=(DateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return t1 >= (DateTimeOffset)t2;
		}

		public static bool operator >=(ObscuredDateTimeOffset t1, ObscuredDateTimeOffset t2)
		{
			return (DateTimeOffset)t1 >= (DateTimeOffset)t2;
		}
	}
} 