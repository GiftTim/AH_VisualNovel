#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System.Numerics;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.ObscuredTypes.EditorCode;
using CodeStage.AntiCheat.Utils;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredBigInteger))]
	internal class ObscuredBigIntegerDrawer : ObscuredTypeDrawer<SerializedObscuredBigInteger, BigInteger>
	{
		private string input;
		
		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
#if UNITY_2022_1_OR_NEWER
			input = EditorGUI.DelayedTextField(position, label, plain.ToString());
#else
			input = EditorGUI.TextField(position, label, plain.ToString());
#endif
		}

		private protected override void ApplyChanges()
		{
			if (!BigInteger.TryParse(input, out var newValue))
				newValue = 0;

			plain = newValue;
			
			var newKey = ObscuredBigInteger.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredBigInteger.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain);
			serialized.Version = ObscuredBigInteger.Version;
		}
	}
}