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
	[CustomPropertyDrawer(typeof(ObscuredVector2Int))]
	internal class ObscuredVector2IntDrawer : WideObscuredTypeDrawer<SerializedObscuredVector2Int, Vector2Int>
	{
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			plain = EditorGUI.Vector2IntField(position, label, plain);
		}

		private protected override void ApplyChanges()
		{
			var newKey = ObscuredVector2Int.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredVector2Int.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredVector2Int.Version;
		}
	}
}