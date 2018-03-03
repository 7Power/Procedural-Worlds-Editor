﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using System;
using PW;

namespace PW.Editor
{
	public class PWTerrainSettingsPanel : PWLayoutPanel
	{
		//terrain base game object reference
		public static PWTerrainGenericBase	terrainReference;

		PWTerrainGenericBase			terrain;

		Dictionary< MaterializerType, Type > materializerTypes = new Dictionary< MaterializerType, Type >()
		{
			{MaterializerType.SquareTileMap, typeof(PWTopDown2DTerrainSquare)},
		};

		public void DrawTerrainSettings(Rect settingsRect, MaterializerType type)
		{
			if (terrain == null)
			{
				var go = GameObject.FindObjectOfType< PWTerrainGenericBase >();
				terrain = (go);
			}
			
			if (terrain == null)
				return ;
			
			terrainReference = terrain;
			
			Type expectedType = materializerTypes[type];

			if (terrainReference.GetType() != expectedType)
			{
				GameObject go = terrainReference.gameObject;
				GameObject.DestroyImmediate(terrainReference);
				terrainReference = go.AddComponent(expectedType) as PWTerrainGenericBase;
			}
			
			if (terrainReference.terrainStorage == null)
				terrainReference.terrainStorage = Resources.Load< PWTerrainStorage >(PWConstants.memoryTerrainStorageAsset);

			terrain.renderDistance = EditorGUILayout.IntSlider("chunk Render distance", terrain.renderDistance, 0, 24);

			EditorGUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Generate terrain"))
					ReloadChunks();
			}
			EditorGUILayout.EndHorizontal();
		}

		public override void DrawDefault(Rect rect)
		{
			EditorGUILayout.LabelField("TODO");
		}

		//Warning: this will destroy all loaded chunks and regenerate them
		public void ReloadChunks()
		{
			if (EditorApplication.isPlaying || EditorApplication.isPaused)
			{
				Debug.LogError("[Editor Terrain Manager] can't reload chunks from the editor in play mode");
				return ;
			}

			PWMainGraph mainGraph = graphRef as PWMainGraph;

			if (mainGraph != null)
			{
				//if the graph we have is not the same / have been modified since last generation, we replace it
				if (terrain.graph != null && terrain.graph.GetHashCode() != graphRef.GetHashCode())
					GameObject.DestroyImmediate(terrain.graph);
				
				terrain.InitGraph(graphRef.Clone() as PWMainGraph);
				
				terrain.DestroyAllChunks();


				//updateChunks will regenerate all deleted chunks
				terrain.UpdateChunks();
			}
		}
	}
}