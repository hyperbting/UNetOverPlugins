using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UNetOverPlugins.Editors
{
    public class IntegrationHelperEditor : EditorWindow
    {
        private BuildTargetGroup[] _allTargets;
        private Vector2 _scrollPos = new Vector2();
        private Color _grayishColor;

        [MenuItem("Window/UNetOverPlugins Setup", false, 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<IntegrationHelperEditor>(true, "UNetOverPlugins Integrations Setup", true);
        }

        public void OnEnable()
        {
            _allTargets = new BuildTargetGroup[]
            {
                BuildTargetGroup.Standalone,
                BuildTargetGroup.iOS,
                BuildTargetGroup.Android,
                BuildTargetGroup.WebGL,
                BuildTargetGroup.WSA,
                BuildTargetGroup.Tizen,
                BuildTargetGroup.PSP2,
                BuildTargetGroup.PS4,
                BuildTargetGroup.PSM,
                BuildTargetGroup.XboxOne,
                BuildTargetGroup.SamsungTV
            };

            _grayishColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        }

        public void OnGUI()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            ShowIntegration("TimeOfDay", ".", "UNetOverTimeOfDay");
            ShowIntegration("UFPS", ".", "UNetOverUFPS");
            ShowIntegration("Uniblocks", ".", "UNetOverUniblocks");

            GUILayout.EndScrollView();
        }

        protected void ShowIntegration(string name, string description, string defineName, bool showBox = true)
        {
            if (showBox)
                EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(IsEnabled(defineName), name))
            {
                EnableIntegration(defineName);
            }
            else
            {
                DisableIntegration(defineName);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUI.color = _grayishColor;
            EditorGUILayout.LabelField(description, Devdog.General.Editors.EditorStyles.labelStyle);
            GUI.color = Color.white;

            if (showBox)
                EditorGUILayout.EndVertical();

            GUILayout.Space(10);
        }

        protected bool IsEnabled(string name)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains(name);
        }

        protected void DisableIntegration(string name)
        {
            if (IsEnabled(name) == false) // Already disabled
                return;

            foreach (var target in _allTargets)
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                string[] items = symbols.Split(';');
                var l = new List<string>(items);
                l.Remove(name);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", l.ToArray()));
            }
        }

        protected void EnableIntegration(string name)
        {
            if (IsEnabled(name)) // Already enabled
                return;

            foreach (var target in _allTargets)
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                string[] items = symbols.Split(';');
                var l = new List<string>(items);
                l.Add(name);

                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, string.Join(";", l.ToArray()));
            }
        }

    }
}
