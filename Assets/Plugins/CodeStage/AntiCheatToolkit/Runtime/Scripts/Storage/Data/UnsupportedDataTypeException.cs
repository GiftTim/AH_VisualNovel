#region copyright
// -------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// -------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	internal class UnsupportedDataTypeException : Exception
	{
		public UnsupportedDataTypeException(Type type):base($"Unsupported data type: {type}!")
		{ }
	}
}