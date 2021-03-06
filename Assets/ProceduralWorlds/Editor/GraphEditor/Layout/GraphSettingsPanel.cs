﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using System;
using ProceduralWorlds.Core;
using ProceduralWorlds;

namespace ProceduralWorlds.Editor
{
	[System.Serializable]
	public class BaseGraphSettingsPanel : LayoutPanel
	{
		//Settings bar datas:
		Vector2					scrollbarPosition;
		[SerializeField]
		TerrainPreviewDrawer	terrainPreview = new TerrainPreviewDrawer();

		public void DrawReloadButtons()
		{
			//reload and force reload buttons
			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Force reload"))
					graphEditor.Reload();
				if (GUILayout.Button("Force reload Once"))
					graphEditor.ReloadOnce();
			}
			EditorGUILayout.EndHorizontal();
		}

		public void DrawGraphSettings(Rect currentRect)
		{
			EditorGUILayout.Space();

			GUI.SetNextControlName("PWName");
			graphRef.name = EditorGUILayout.TextField("ProceduralWorld name: ", graphRef.name);

			EditorGUILayout.Separator();

			DrawReloadButtons();
			
			//unfocus all fields if we click outsize of the settings bar
			if ((e.type == EventType.MouseDown || e.type == EventType.Ignore)
				&& !GUILayoutUtility.GetLastRect().Contains(e.mousePosition)
				&& GUI.GetNameOfFocusedControl() == "PWName")
				GUI.FocusControl(null);
		}

		public void DrawTerrainPreview(Rect rect, bool drawPreviewCameraControlFields)
		{
			if (!terrainPreview.isEnabled)
				terrainPreview.OnEnable(graphRef);
			terrainPreview.OnGUI(rect, drawPreviewCameraControlFields);
		}
		
		public override void Draw(Rect rect)
		{
			Profiler.BeginSample("[PW] Rendering settings bar");

			GUI.DrawTexture(rect, ColorTheme.defaultBackgroundTexture);
	
			//add the texturePreviewRect size:
			scrollbarPosition = EditorGUILayout.BeginScrollView(scrollbarPosition, GUILayout.ExpandWidth(true));
			{
				base.Draw(rect);
			}
			EditorGUILayout.EndScrollView();
			
			//free focus of the selected fields
			if (Event.current.type == EventType.MouseDown)
				GUI.FocusControl(null);

			Profiler.EndSample();
		}

		public override void DrawDefault(Rect currentRect)
		{
			DrawTerrainPreview(currentRect, true);
			
			//draw main graphRef settings
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				DrawGraphSettings(currentRect);
			}
			EditorGUILayout.EndVertical();
		}
	}
}