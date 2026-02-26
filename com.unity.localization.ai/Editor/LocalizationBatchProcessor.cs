using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization;
using System.Threading.Tasks;
using Unity.Localization.AI;

namespace Unity.Localization.AI.Editor
{
    public static class LocalizationBatchProcessor
    {
        public static async Task ProcessTables(List<StringTableCollection> collections, Locale sourceLocale, Locale targetLocale, bool overwrite)
        {
            if (collections == null || collections.Count == 0) return;
            if (sourceLocale == null || targetLocale == null) return;

            int totalCollections = collections.Count;
            for (int i = 0; i < totalCollections; i++)
            {
                var collection = collections[i];
                var sourceTable = collection.GetTable(sourceLocale.Identifier) as StringTable;
                var targetTable = collection.GetTable(targetLocale.Identifier) as StringTable;

                if (sourceTable == null || targetTable == null)
                {
                    Debug.LogWarning($"Skipping collection {collection.TableCollectionName}: Source or Target table missing for selected locales.");
                    continue;
                }

                var entries = sourceTable.Values.ToList();
                int entryCount = entries.Count;

                for (int j = 0; j < entryCount; j++)
                {
                    var sourceEntry = entries[j];
                    string sourceText = sourceEntry.Value;
                    if (string.IsNullOrEmpty(sourceText)) continue;

                    var targetEntry = targetTable.GetEntry(sourceEntry.KeyId);
                    
                    if (targetEntry != null && !string.IsNullOrEmpty(targetEntry.Value) && !overwrite)
                    {
                        // Skip if already translated and overwrite is false
                        continue;
                    }

                    float progress = (float)i / totalCollections + ((float)j / entryCount) / totalCollections;
                    if (EditorUtility.DisplayCancelableProgressBar($"Translating Tables", $"[{i + 1}/{totalCollections}] Processing {collection.TableCollectionName}: {sourceText}", progress))
                    {
                        Debug.LogWarning("Translation process cancelled by user.");
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    try
                    {
                        string translatedText = await OpenAITranslator.TranslateAsync(sourceText, targetLocale.Identifier.Code);
                        if (!string.IsNullOrEmpty(translatedText))
                        {
                            targetTable.AddEntry(sourceEntry.KeyId, translatedText);
                        }
                    }
                    catch (System.Exception e)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorUtility.DisplayDialog("Translation Error", $"An error occurred during translation of '{sourceText}':\n{e.Message}", "OK");
                        Debug.LogException(e);
                        return;
                    }
                }
                
                EditorUtility.SetDirty(targetTable);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            Debug.Log("Batch Translation Completed.");
        }
    }
}
