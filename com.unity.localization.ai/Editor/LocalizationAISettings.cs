using UnityEditor;
using UnityEngine;
using System.Linq;

namespace Unity.Localization.AI.Editor
{
    public static class LocalizationAISettings
    {
        private static readonly string[] AvailableModels = 
        {
            "gpt-4o",
            "gpt-4o-mini",
            "gpt-4-turbo",
            "gpt-4",
            "o1-preview",
            "o1-mini"
        };

        [SettingsProvider]
        public static SettingsProvider CreateLocalizationAISettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Localization AI", SettingsScope.User)
            {
                label = "Localization AI",
                guiHandler = (searchContext) =>
                {
                    // Add standard preferences padding
                    var style = new GUIStyle { padding = new RectOffset(10, 10, 10, 10) };
                    EditorGUILayout.BeginVertical(style, GUILayout.ExpandWidth(true));
                    
                    // API Key
                    var apiKey = PlayerPrefs.GetString(OpenAITranslator.API_KEY_PLAYERPREFS_KEY, "");
                    EditorGUI.BeginChangeCheck();
                    apiKey = EditorGUILayout.PasswordField("OpenAI API Key", apiKey);
                    if (EditorGUI.EndChangeCheck())
                    {
                        PlayerPrefs.SetString(OpenAITranslator.API_KEY_PLAYERPREFS_KEY, apiKey);
                        PlayerPrefs.Save();
                    }
                    
                    EditorGUILayout.Space();

                    // Model Selection
                    var currentModel = PlayerPrefs.GetString(OpenAITranslator.MODEL_PLAYERPREFS_KEY, OpenAITranslator.DEFAULT_MODEL);
                    int selectedIndex = System.Array.IndexOf(AvailableModels, currentModel);
                    if (selectedIndex == -1) selectedIndex = 0;

                    EditorGUI.BeginChangeCheck();
                    selectedIndex = EditorGUILayout.Popup("GPT Model", selectedIndex, AvailableModels);
                    if (EditorGUI.EndChangeCheck())
                    {
                        PlayerPrefs.SetString(OpenAITranslator.MODEL_PLAYERPREFS_KEY, AvailableModels[selectedIndex]);
                        PlayerPrefs.Save();
                    }

                    EditorGUILayout.Space();

                    // Override Prompt
                    bool isOverride = PlayerPrefs.GetInt(OpenAITranslator.OVERRIDE_PROMPT_PLAYERPREFS_KEY, 0) == 1;
                    EditorGUI.BeginChangeCheck();
                    isOverride = EditorGUILayout.Toggle("Override System Prompt", isOverride);
                    if (EditorGUI.EndChangeCheck())
                    {
                        PlayerPrefs.SetInt(OpenAITranslator.OVERRIDE_PROMPT_PLAYERPREFS_KEY, isOverride ? 1 : 0);
                        PlayerPrefs.Save();
                    }

                    // System Prompt
                    string systemPrompt = PlayerPrefs.GetString(OpenAITranslator.SYSTEM_PROMPT_PLAYERPREFS_KEY, OpenAITranslator.DEFAULT_SYSTEM_PROMPT);
                    
                    EditorGUI.BeginDisabledGroup(!isOverride);
                    EditorGUI.BeginChangeCheck();
                    
                    EditorGUILayout.LabelField("System Prompt");
                    
                    // Force word wrap for TextArea to prevent layout expansion with long text
                    var textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                    systemPrompt = EditorGUILayout.TextArea(systemPrompt, textAreaStyle, GUILayout.Height(100), GUILayout.ExpandWidth(true));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        PlayerPrefs.SetString(OpenAITranslator.SYSTEM_PROMPT_PLAYERPREFS_KEY, systemPrompt);
                        PlayerPrefs.Save();
                    }
                    EditorGUI.EndDisabledGroup();

                    if (!isOverride)
                    {
                        EditorGUILayout.HelpBox("Default Prompt: " + OpenAITranslator.DEFAULT_SYSTEM_PROMPT, MessageType.None);
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Enter your OpenAI API Key and configure the translation prompt to use the AI-powered localization features.", MessageType.Info);

                    EditorGUILayout.EndVertical();
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Localization", "AI", "OpenAI", "API Key" })
            };

            return provider;
        }
    }
}
