#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.ObscuredTypes.EditorCode;
using CodeStage.AntiCheat.Utils;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredULong))]
	internal class ObscuredULongDrawer : ObscuredTypeDrawer<SerializedObscuredULong, ulong>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = (ulong)EditorGUI.LongField(position, label, (long)plain);
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredULong.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredULong.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredULong.Version;
		}
	}
}