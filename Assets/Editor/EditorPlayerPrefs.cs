using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class EditorPlayerPrefs : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showAllKeys = true;
    private string searchFilter = "";

    [MenuItem("Tools/PlayerPrefs Viewer")]
    public static void ShowWindow()
    {
        GetWindow<EditorPlayerPrefs>("PlayerPrefs Viewer");
    }

    void OnGUI()
    {
        GUILayout.Label("PlayerPrefs Viewer", EditorStyles.boldLabel);
        DrawToolbar();



        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Get all keys (this is a workaround since Unity doesn't provide a direct way)
        List<string> allKeys = new List<string>();

        // For Windows registry
#if UNITY_EDITOR_WIN
        try
        {
            var regex = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Unity\UnityEditor\" + PlayerSettings.companyName + @"\" + PlayerSettings.productName);
            if (regex != null)
            {
                foreach (string valueName in regex.GetValueNames())
                {
                    if (showAllKeys || string.IsNullOrEmpty(searchFilter) || valueName.ToLower().Contains(searchFilter.ToLower()))
                    {
                        if(valueName.StartsWith("unity.")) continue;
                        if(Regex.IsMatch(valueName, @"_h\d{5,}$"))
                        {
                            string valueProcess = Regex.Split(valueName, @"_h")[0];
                            allKeys.Add(valueProcess);
                            continue;
                        }
                        allKeys.Add(valueName);
                    }
                }
                regex.Close();
            }
        }
        catch
        {
            // Fallback if registry access fails
            Debug.LogWarning("Couldn't access registry, showing only known keys");
        }
#endif

        // Fallback for other platforms or if registry access fails
        if (allKeys.Count == 0)
        {
            // Note: This is just for demonstration. In reality, you can't get all keys without knowing them first.
            // You would need to maintain your own list of known keys.
            if (PlayerPrefs.HasKey("known_key_1")) allKeys.Add("known_key_1");
            if (PlayerPrefs.HasKey("known_key_2")) allKeys.Add("known_key_2");
            // Add all your known keys here
        }

        // Display all keys and values
        foreach (string key in allKeys)
        {
            EditorGUILayout.BeginHorizontal();

            // Key name
            EditorGUILayout.LabelField(key, GUILayout.Width(200));

            // Value (try different types)
            if (PlayerPrefs.HasKey(key))
            {
                // Try int first
                int intValue = PlayerPrefs.GetInt(key, int.MinValue);
                if (intValue != int.MinValue)
                {
                    EditorGUILayout.LabelField("(int)", GUILayout.Width(40));
                    EditorGUILayout.LabelField(intValue.ToString());
                }
                else
                {
                    // Try float
                    float floatValue = PlayerPrefs.GetFloat(key, float.MinValue);
                    if (floatValue != float.MinValue)
                    {
                        EditorGUILayout.LabelField("(float)", GUILayout.Width(40));
                        EditorGUILayout.LabelField(floatValue.ToString());
                    }
                    else
                    {
                        // Default to string
                        EditorGUILayout.LabelField("(string)", GUILayout.Width(40));
                        EditorGUILayout.LabelField(PlayerPrefs.GetString(key));
                    }
                }
            }

            // Delete button
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

    }
    private bool ShouldIgnoreKey(string key)
    {
        // Bỏ qua key có hash
        if (Regex.IsMatch(key, @"_h\d{5,}$")) return true;

        // Bỏ qua key hệ thống
        if (key.StartsWith("unity.")) return true;

        return false;
    }
    public void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        searchFilter = EditorGUILayout.TextField("Search: ", searchFilter);
        if(GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            searchFilter = "";
            GUI.FocusControl(null);        
        }
        EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Delete All"))
        {
            if(EditorUtility.DisplayDialog("Delete All PlayerPrefs",
                               "Are you sure you want to delete ALL PlayerPrefs data?",
                                              "Yes", "No"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            }
        }

    }
    public void DrawMain()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.EndScrollView();
    }
    public List<string> GetAllKeys()
    {
        List<string> allKeys = new List<string>();
    }

}