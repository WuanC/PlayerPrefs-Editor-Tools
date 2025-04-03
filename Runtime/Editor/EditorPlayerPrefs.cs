using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;




#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif
public enum PlayerPrefsType
{
    Unknown,
    Int,
    Float,
    String,
}
public class EditorPlayerPrefs : EditorWindow
{
    private Vector2 scrollPosition;
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
        DrawMain();

    }

    public void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        searchFilter = EditorGUILayout.TextField("Search: ", searchFilter);
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            searchFilter = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Delete All"))
        {
            if (EditorUtility.DisplayDialog("Delete All PlayerPrefs",
                               "Are you sure you want to delete ALL PlayerPrefs data?",
                                              "Yes", "No"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            }
        }
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(200));
        GUILayout.Label("Value", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
        GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(80));
        GUILayout.Label("Del", EditorStyles.boldLabel, GUILayout.Width(40));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

    }
    public void DrawMain()
    {

        List <string> playerPrefsKeys = GetAllPlayerPrefsKey();
        if (playerPrefsKeys == null || playerPrefsKeys.Count == 0) return;
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (string element in playerPrefsKeys)
        {
            EditorGUILayout.BeginHorizontal();


            DrawKey(element);
            EditorGUILayout.EndHorizontal();

        }
        EditorGUILayout.EndScrollView();
    }
    public List<string> GetAllPlayerPrefsKey()
    {
#if UNITY_EDITOR_WIN
        string registryPathh = $@"Software\Unity\UnityEditor\{Application.companyName}\{Application.productName}";
        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPathh))
        {
            if (key == null)
            {
                Debug.LogError($"Registry key not found: {registryPathh}");
                return null;
            }
             List<string> keyList = new List<string>();
            foreach (var valueName in key.GetValueNames())
            {
                if (ShouldIgnoreKey(valueName)) continue;
                if (key.GetValue(valueName) == null) continue;
                string keyName = FormatKeyName(valueName);
                keyList.Add(keyName);
            }
            return keyList;
        }
#endif
    }
    private bool ShouldIgnoreKey(string key)
    {
        if (key.StartsWith("unity.")) return true;
        return false;
    }
    public string FormatKeyName(string keyRaw)
    {
        if (Regex.IsMatch(keyRaw, @"_h\d{5,}$"))
        {
            string valueProcess = Regex.Split(keyRaw, @"_h")[0];
            return valueProcess;
        }
        return keyRaw;
    }

    public void DrawKey(string key)
    {
        EditorGUILayout.BeginHorizontal();
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefsType type = GetType(key);
            string value = "";
            if(type == PlayerPrefsType.Unknown) return;
            else if(type == PlayerPrefsType.Int)
            {
                value = PlayerPrefs.GetInt(key).ToString();
            }
            else if (type == PlayerPrefsType.Float)
            {
                value = PlayerPrefs.GetFloat(key).ToString();
            }
            else if (type == PlayerPrefsType.String)
            {
                value = PlayerPrefs.GetString(key).ToString();
            }
            EditorGUILayout.LabelField(key, GUILayout.Width(200));
            EditorGUILayout.LabelField(type.ToString(), GUILayout.ExpandWidth(true));
            EditorGUI.BeginChangeCheck();
            string newValue = EditorGUILayout.TextField(value, GUILayout.Width(200));

            if (EditorGUI.EndChangeCheck() && newValue != value)
            {
                    if (type == PlayerPrefsType.Unknown) return;
                    else if (type == PlayerPrefsType.Int && int.TryParse(newValue, out int iParseValue))
                    {
                        PlayerPrefs.SetInt(key, iParseValue);
                    }
                    else if (type == PlayerPrefsType.Float && float.TryParse(newValue, out float fParseValue))
                    {
                        PlayerPrefs.SetFloat(key, fParseValue);
                    }
                    else if (type == PlayerPrefsType.String)
                    {
                        PlayerPrefs.SetString(key, newValue);
                    }
                    PlayerPrefs.Save();
                
            }
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {

                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                    PlayerPrefs.Save();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }
    public PlayerPrefsType GetType(string key)
    {
        int intVal = PlayerPrefs.GetInt(key, int.MinValue);
        float floatVal = PlayerPrefs.GetFloat(key, float.MinValue);
        string stringVal = PlayerPrefs.GetString(key, null);

        if (intVal != int.MinValue) return PlayerPrefsType.Int;
        if ((Mathf.Abs(floatVal - float.MinValue) > 0.0001f)) return PlayerPrefsType.Float;
        if (stringVal != null) return PlayerPrefsType.String;
        return PlayerPrefsType.Unknown;
    }


}