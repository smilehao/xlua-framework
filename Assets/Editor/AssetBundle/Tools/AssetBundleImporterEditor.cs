using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using System.Reflection;
using System;

public class TestScriptableObject : ScriptableObject
{
    public int a = 50;
}

[CustomEditor(typeof(UnityEditor.DefaultAsset))]
public class AssetBundleImporterEditor : Editor
{
    protected string[] files = null;
    TestScriptableObject testScriptableObject = new TestScriptableObject();
    AssetImporter assetImporter = null;
    SerializedObject mySerializedObject = null;
    SerializedProperty serializedPropertyA = null;

    void OnCreate()
    {
        Debug.Log("OnCreate : " + serializedObject.targetObject.name);
    }

    void OnEnable()
    {
        var path = AssetDatabase.GetAssetPath(target);
        Debug.Log("AssetPath : " + path);
        string[] guids = AssetDatabase.FindAssets("t:prefab t:shader", new string[] { path });
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            sb.AppendLine(assetPath);
        }
        Debug.Log("FindAssets : " + sb.ToString());
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Attributes == FileAttributes.Directory)//判断类型是选择的是文件夹还是文件  
        {
            files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        }

        assetImporter = AssetImporter.GetAtPath(path);
        if (!string.IsNullOrEmpty(assetImporter.userData))
        {
            JsonUtility.FromJsonOverwrite(assetImporter.userData, testScriptableObject);
            Debug.Log("FromJsonOverwrite : " + testScriptableObject.a);
        }
        mySerializedObject = new UnityEditor.SerializedObject(testScriptableObject);
        serializedPropertyA = mySerializedObject.FindProperty("a");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        OnRefresh();
        OnTest();
    }

    void OnRefresh()
    {
        GUI.enabled = true;
        if (files != null && files.Length > 0)
        {
            GUILayout.Label("文件夹下的所有内容：");
            int i = 1;
            for (int j = 0; j < files.Length; j++)
            {
                if (!files[j].EndsWith(".meta"))
                {
                    GUILayout.Label(i + "、" + files[j]);
                    i++;
                }
            }
        }
    }

    void OnTest()
    {
        if (mySerializedObject != null)
        {
            mySerializedObject.Update();

            EditorGUILayout.PropertyField(serializedPropertyA);
            serializedPropertyA.serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Save"))
            {
                mySerializedObject.ApplyModifiedProperties();
                Debug.Log("Save");
            }
        }
    }

    void OnDisable()
    {
    }
}