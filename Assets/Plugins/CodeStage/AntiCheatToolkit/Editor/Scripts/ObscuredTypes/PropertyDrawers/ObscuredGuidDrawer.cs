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
	[CustomPropertyDrawer(typeof(ObscuredGuid))]
	internal class ObscuredGuidDrawer : ObscuredTypeDrawer<SerializedObscuredGuid, Guid>
	{
		private string input;

		private protected override void DrawProperty(Rect position, SerializedProperty sp, GUIContent label)
		{
			var posW = position.width;
			position.width *= 0.75f;
			
			var buttonRect = position;
			buttonRect.x = position.xMax + 5;
			buttonRect.width = posW - position.width - 5;
			
			var guidString = plain.ToString("D", CultureInfo.InvariantCulture);
			input = EditorGUI.DelayedTextField(position, label, guidString);
			
			if (GUI.Button(buttonRect, "New"))
			{
				plain = Guid.NewGuid();
				input = plain.ToString("D", CultureInfo.InvariantCulture);
				ApplyChanges();
			}
		}

		private protected override void ApplyChanges()
		{
			if (Guid.TryParse(input, out var newGuid))
			{
				plain = newGuid;
			}

			var newKey = ObscuredGuid.GenerateKey();
			serialized.Key = newKey;
			ObscuredGuid.Encrypt(plain, newKey, out var encrypted1, out var encrypted2);
			serialized.Hidden1 = encrypted1;
			serialized.Hidden2 = encrypted2;
			var bytes = plain.ToByteArray();
			var long1 = BitConverter.ToInt64(bytes, 0);
			var long2 = BitConverter.ToInt64(bytes, 8);
			serialized.Hash = ObscuredGuid.CalculateGuidHashFromLongs(long1, long2);
			serialized.Version = ObscuredGuid.Version;
		}
	}
} 