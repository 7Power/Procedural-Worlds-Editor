﻿using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Editor
{
	public class ColorPickerPopup : Popup
	{
		readonly Vector2	colorPickerPadding = new Vector2(10, 10);

		Texture2D			colorPickerTexture;
		Texture2D			colorPickerThumb;

		bool				colorPicking;

		Color				currentColor;
		Vector2				thumbPosition;

		public static void OpenPopup(Color color, PWGUISettings guiSettings)
		{
			var popup = Popup.OpenPopup< ColorPickerPopup >(guiSettings.GetHashCode(), new Vector2(windowSize.x, 340));

			popup.currentColor = color;
			popup.name = "Cheap color picker";
			popup.thumbPosition = guiSettings.thumbPosition;
		}

		protected override void OnGUIEnable()
		{
			colorPickerTexture = Resources.Load("GUI/colorPicker") as Texture2D;
			colorPickerThumb = Resources.Load("GUI/colorPickerThumb") as Texture2D;
		}

		void DrawRainbowPicker()
		{
			GUILayout.Label(colorPickerTexture, GUILayout.Width(windowSize.x - colorPickerPadding.x), GUILayout.Height(windowSize.x - colorPickerPadding.y));
			Rect colorPickerRect = GUILayoutUtility.GetLastRect();

			float mouseX = Mathf.Clamp(e.mousePosition.x, colorPickerPadding.x, windowSize.x - colorPickerPadding.x);
			float mouseY = Mathf.Clamp(e.mousePosition.y, colorPickerPadding.y, windowSize.x - colorPickerPadding.y);

			Vector2 colorPickerMousePosition = new Vector2(mouseX, mouseY);

			if (e.type == EventType.MouseDown && colorPickerRect.Contains(e.mousePosition))
				colorPicking = true;
			
			if (e.type == EventType.MouseUp)
				colorPicking = false;

			if (colorPicking)
			{
				Vector2 relativeMousePos = colorPickerMousePosition - colorPickerPadding;
				Vector2 mouseRatio = relativeMousePos / (windowSize.x - colorPickerPadding.x * 2);
				Vector2 textureCoord = new Vector2(mouseRatio.x * colorPickerTexture.width, mouseRatio.y * colorPickerTexture.height);
				textureCoord.y = Mathf.Clamp(colorPickerTexture.height - textureCoord.y, 0, colorPickerTexture.height - 1);

				currentColor = colorPickerTexture.GetPixel((int)textureCoord.x, (int)textureCoord.y);
				thumbPosition = colorPickerMousePosition + new Vector2(-5, -6);

				if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown)
					GUI.changed = true;
			}

			Rect colorPickerThumbRect = new Rect(thumbPosition, new Vector2(8, 8));
			GUI.DrawTexture(colorPickerThumbRect, colorPickerThumb);
		}

		void DrawRGBComponents()
		{
			byte r, g, b, a;
			ColorUtils.ColorToByte(currentColor, out r, out g, out b, out a);
			EditorGUIUtility.labelWidth = 20;
			r = (byte)EditorGUILayout.IntSlider("R", r, 0, 255);
			g = (byte)EditorGUILayout.IntSlider("G", g, 0, 255);
			b = (byte)EditorGUILayout.IntSlider("B", b, 0, 255);
			a = (byte)EditorGUILayout.IntSlider("A", a, 0, 255);
			currentColor = (SerializableColor)ColorUtils.ByteToColor(r, g, b, a);
			EditorGUIUtility.labelWidth = 0;
		}

		void DrawHexComponents()
		{
			byte a = (byte)(int)(currentColor.a * 255);
			int hex = ColorUtils.ColorToHex(currentColor, false); //get color without alpha
			
			EditorGUIUtility.labelWidth = 80;
			EditorGUI.BeginChangeCheck();
			string hexColor = EditorGUILayout.TextField("Hex color", hex.ToString("X6"));
			if (EditorGUI.EndChangeCheck())
				a = 255;
			EditorGUIUtility.labelWidth = 0;
			Regex reg = new Regex(@"[^A-F0-9 -]");
			hexColor = reg.Replace(hexColor, "");
			hexColor = hexColor.Substring(0, Mathf.Min(hexColor.Length, 6));
			if (hexColor == "")
				hexColor = "0";
			hex = int.Parse(a.ToString("X2") + hexColor, System.Globalization.NumberStyles.HexNumber);
			currentColor = (SerializableColor)ColorUtils.HexToColor(hex, false);
		}

		protected override void GUIUpdate()
		{
			EditorGUI.BeginChangeCheck();
			{
				DrawRainbowPicker();
	
				DrawRGBComponents();
	
				EditorGUILayout.Space();
	
				DrawHexComponents();
			}
			if (EditorGUI.EndChangeCheck())
			{
				SendUpdate("ColorPickerUpdate");
				Repaint();
			}
		}

		public static void UpdateDatas(PWGUISettings settings)
		{
			var popup = FindPopup< ColorPickerPopup >();

			if (popup == null)
				return ;
			
			settings.c = (SerializableColor)popup.currentColor;
			settings.thumbPosition = popup.thumbPosition;
		}

	}
}