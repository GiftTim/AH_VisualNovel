#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

namespace CodeStage.AntiCheat.Storage
{
	using System;

	[Serializable]
	internal struct ObscuredPrefsData
	{
		public StorageDataType type;
		public byte[] data;

		public ObscuredPrefsData(StorageDataType type, byte[] data)
		{
			this.type = type;
			this.data = data;
		}
	}
}