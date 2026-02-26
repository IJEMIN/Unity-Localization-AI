using UnityEditor;
using UnityEngine;

namespace Unity.Localization.AI.Editor
{
    public static class LocalizationAISettings
    {
        [SettingsProvider]
        public static SettingsProvider CreateLocalizationAISettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Localization AI", SettingsScope.User)
            {
                label = "Localization AI",
                guiHandler = (searchContext) =>
                {
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
                    systemPrompt = EditorGUILayout.TextArea(systemPrompt, GUILayout.Height(100));
                    
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
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Localization", "AI", "OpenAI", "API Key" })
            };

            return provider;
        }
    }
}
