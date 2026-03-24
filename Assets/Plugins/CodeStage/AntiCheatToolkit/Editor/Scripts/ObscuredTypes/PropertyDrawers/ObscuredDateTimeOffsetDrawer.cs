#region copyright
// ------------------------------------------------------
// Copyright (C) Dmitry Yuhanov [https://codestage.net]
// ------------------------------------------------------
#endregion

using System;
using System.Globalization;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.ObscuredTypes.EditorCode;
using CodeStage.AntiCheat.Utils;
using UnityEditor;
using UnityEngine;

namespace CodeStage.AntiCheat.EditorCode.PropertyDrawers
{
	[CustomPropertyDrawer(typeof(ObscuredDateTimeOffset))]
	internal class ObscuredDateTimeOffsetDrawer : ObscuredTypeDrawer<SerializedObscuredDateTimeOffset, DateTimeOffset>
	{
		private string input;

		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			var dateString = plain.ToString("o", DateTimeFormatInfo.InvariantInfo);
			input = EditorGUI.DelayedTextField(position, label, dateString);
		}

		private protected override void ApplyChanges()
		{
			DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out plain);

			var newKey = ObscuredDateTimeOffset.GenerateKey();
			serialized.Key = newKey;
			serialized.Hidden = ObscuredDateTimeOffset.Encrypt(plain, newKey);
			serialized.Hash = HashUtils.CalculateHash(plain.UtcTicks);
			serialized.Version = ObscuredDateTimeOffset.Version;
		}
	}
} 