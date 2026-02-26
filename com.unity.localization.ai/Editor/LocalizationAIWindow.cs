using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEditor.Localization;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Localization.AI.Editor
{
    public class LocalizationAIWindow : EditorWindow
    {
        private Locale sourceLocale;
        private Locale targetLocale;
        private bool overwrite = false;
        private List<StringTableCollection> selectedCollections = new List<StringTableCollection>();
        
        private string testInputText = "";
        private string testResultText = "";
        private bool isTesting = false;

        private Vector2 scrollPos;

        [MenuItem("Window/Asset Management/Localization AI")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationAIWindow>("Localization AI");
        }

        private void OnEnable()
        {
            // Try to set default locales
            if (LocalizationSettings.AvailableLocales != null)
            {
                var locales = LocalizationSettings.AvailableLocales.Locales;
                if (locales.Count >= 2)
                {
                    sourceLocale = locales.FirstOrDefault(l => l.Identifier == "ko-KR") ?? locales[0];
                    targetLocale = locales.FirstOrDefault(l => l.Identifier == "en") ?? (locales.Count > 1 ? locales[1] : locales[0]);
                }
                else if (locales.Count > 0)
                {
                    sourceLocale = locales[0];
                    targetLocale = locales[0];
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            GUILayout.Label("Localization AI Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            sourceLocale = (Locale)EditorGUILayout.ObjectField("Source Locale", sourceLocale, typeof(Locale), false);
            targetLocale = (Locale)EditorGUILayout.ObjectField("Target Locale", targetLocale, typeof(Locale), false);
            overwrite = EditorGUILayout.Toggle("Overwrite Existing Translation", overwrite);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            GUILayout.Label("Select Tables to Translate", EditorStyles.boldLabel);
            DrawTableSelection();

            EditorGUILayout.Space();

            DrawTestPanel();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(selectedCollections.Count == 0 || sourceLocale == null || targetLocale == null || sourceLocale == targetLocale);
            if (GUILayout.Button("Start Translation Process", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Confirm Translation", 
                    $"Start translating {selectedCollections.Count} tables from {sourceLocale.Identifier} to {targetLocale.Identifier}?\nOverwrite: {overwrite}", 
                    "Yes", "Cancel"))
                {
                    _ = LocalizationBatchProcessor.ProcessTables(selectedCollections, sourceLocale, targetLocale, overwrite);
                }
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        private void DrawTableSelection()
        {
            var allCollections = LocalizationEditorSettings.GetStringTableCollections();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button("Select All"))
            {
                selectedCollections = allCollections.ToList();
            }
            if (GUILayout.Button("Deselect All"))
            {
                selectedCollections.Clear();
            }
            EditorGUILayout.Space();

            foreach (var collection in allCollections)
            {
                bool isSelected = selectedCollections.Contains(collection);
                EditorGUI.BeginChangeCheck();
                isSelected = EditorGUILayout.ToggleLeft(collection.TableCollectionName, isSelected);
                if (EditorGUI.EndChangeCheck())
                {
                    if (isSelected) selectedCollections.Add(collection);
                    else selectedCollections.Remove(collection);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTestPanel()
        {
            GUILayout.Label("Test Translation Preview", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Input Text (Source)");
            testInputText = EditorGUILayout.TextArea(testInputText, GUILayout.Height(60));

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(testInputText) || targetLocale == null || isTesting);
            if (GUILayout.Button("Test Translate"))
            {
            _ = RunTestTranslation();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Result Text (Target)");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(testResultText, GUILayout.Height(60));
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private async Task RunTestTranslation()
        {
            isTesting = true;
            testResultText = "Translating...";
            Repaint();

            try
            {
                testResultText = await OpenAITranslator.TranslateAsync(testInputText, targetLocale.Identifier.Code);
            }
            catch (System.Exception e)
            {
                testResultText = $"Error: {e.Message}";
                Debug.LogException(e);
            }
            finally
            {
                isTesting = false;
                Repaint();
            }
        }
    }
}
