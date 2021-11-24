/*
 * Copyright (C) 2021 Jerry Verhoeven - All Rights Reserved
 * You may use, distribute and modify this code under the terms of the MIT license.
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GrinningPickle.EditorTools
{
	public class AssetRenamerWindow : EditorWindow
	{
		#region Properties
		//-----------------------------------------------------------------

		//-----------------------------------------------------------------
		#endregion Properties

		#region Members
		//-----------------------------------------------------------------

		private string m_Pattern;
		private string m_NewName;
		private bool m_IsPrefix = false;
		private bool m_IsPostFix = false;

		private Vector2 m_ScrollPosition = Vector2.zero;
		private bool m_SelectionChanged = false;
		private Texture2D m_Icon;

		//-----------------------------------------------------------------
		#endregion Members

		#region Unity Overloads
		//-----------------------------------------------------------------

		[MenuItem("Tools/Asset Renaming...", false, 0)]
		private static void Init()
		{
			EditorWindow.GetWindow<AssetRenamerWindow>(false, "Renamer", true);
		}

		private void OnSelectionChange()
		{
			m_SelectionChanged = true;
			this.Repaint();
		}

		private void OnGUI()
		{
			if (!this)
				return;

			if (m_Icon == null)
			{
				m_Icon = EditorGUIUtility.FindTexture("Project");
				this.titleContent = new GUIContent("Renamer", m_Icon);
			}

			EditorGUILayout.BeginVertical();
			{
				Object[] selectedAssetsArr = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

				List<Object> filteredSelectedAssetsList = new List<Object>();

				foreach (Object obj in selectedAssetsArr)
				{
					if (AssetDatabase.Contains(obj))
					{
						if (Path.HasExtension(AssetDatabase.GetAssetPath(obj)))
						{
							filteredSelectedAssetsList.Add(obj);
						}
					}
					//else
					//{
					//Debug.LogWarning("Object: " + obj.name + " is not a valid prefab. Type: " + prefabType.ToString());
					//}
				}

				//Auto-fill New Name field with name of first selected object.
				if ((string.IsNullOrEmpty(m_NewName) || m_SelectionChanged) && filteredSelectedAssetsList.Count > 0)
				{
					m_NewName = filteredSelectedAssetsList[0].name;
				}

				EditorGUILayout.BeginHorizontal();
				{
					m_Pattern = EditorGUILayout.TextField(new GUIContent("Pattern", "Regex pattern, this can just be a few letters, Regex will pick it up as a pattern."), m_Pattern);

					if (GUILayout.Button("clear", EditorStyles.miniButton, GUILayout.MaxWidth(40)))
					{
						m_Pattern = "";
					}
				}
				EditorGUILayout.EndHorizontal();

				m_NewName = EditorGUILayout.TextField(new GUIContent("New Name", "New Name, Regex replacement, prefix or postfix string"), m_NewName);
				m_IsPrefix = EditorGUILayout.Toggle(new GUIContent("Is Prefix", "Will use New Name as a prefix instead of a full replacement."), m_IsPrefix);
				m_IsPostFix = EditorGUILayout.Toggle(new GUIContent("Is Postfix", "Will use New Name as a postfix instead of a full replacement."), m_IsPostFix);

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
				{
					EditorGUILayout.LabelField("Selected Assets: " + filteredSelectedAssetsList.Count, GUILayout.ExpandWidth(true), GUILayout.MinWidth(0));
				}
				EditorGUILayout.EndHorizontal();

				m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
				{
					foreach (Object obj in filteredSelectedAssetsList)
					{
						EditorGUILayout.BeginHorizontal();

						EditorGUILayout.LabelField(obj.name, GUILayout.MaxWidth(position.width / 2));

						string prefix = m_IsPrefix ? m_NewName : "";
						string postfix = m_IsPostFix ? m_NewName : "";

						string newName = m_NewName;

						if (!string.IsNullOrEmpty(m_Pattern))
						{
							newName = Regex.Replace(obj.name, m_Pattern, m_NewName);
						}

						if (m_IsPrefix || m_IsPostFix)
						{
							newName = prefix + obj.name + postfix;
						}

						EditorGUILayout.LabelField(newName, GUILayout.MaxWidth(position.width / 2));

						EditorGUILayout.EndHorizontal();
					}
				}
				EditorGUILayout.EndScrollView();

				if (selectedAssetsArr.Length <= 0)
				{
					GUI.enabled = false;
				}

				if (GUILayout.Button("Rename"))
				{
					if (EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to rename these " + filteredSelectedAssetsList.Count + " objects?", "Yes", "Cancel"))
					{
						foreach (Object obj in filteredSelectedAssetsList)
						{
							string prefix = m_IsPrefix ? m_NewName : "";
							string postfix = m_IsPostFix ? m_NewName : "";

							string newName = m_NewName;

							if (!string.IsNullOrEmpty(m_Pattern))
							{
								newName = Regex.Replace(obj.name, m_Pattern, m_NewName);
							}

							if (m_IsPrefix || m_IsPostFix)
							{
								newName = prefix + obj.name + postfix;
							}

							string objectPath = AssetDatabase.GetAssetPath(obj);

							//Only need to pass in new name, not path.
							string result = AssetDatabase.RenameAsset(objectPath, newName);

							if (!string.IsNullOrEmpty(result))
							{
								Debug.LogError(result);
								break;
							}
						}
					}
				}

				if (selectedAssetsArr.Length <= 0)
				{
					GUI.enabled = true;
				}
			}
			EditorGUILayout.EndVertical();

			m_SelectionChanged = false;
		}

		//-----------------------------------------------------------------
		#endregion Unity Overloads

		#region Public Interface
		//-----------------------------------------------------------------

		//-----------------------------------------------------------------
		#endregion Public Interface

		#region Private Interface
		//-----------------------------------------------------------------

		//-----------------------------------------------------------------
		#endregion Private Interface
	}
}