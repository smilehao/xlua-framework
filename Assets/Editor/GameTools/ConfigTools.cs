using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

public class ConfigTools : EditorWindow
{
    private static string xlsxFolder = string.Empty;
    private static string protoFolder = string.Empty;

    private bool xlsxGenLuaFinished = false;
    private bool protoGenLuaFinished = false;

    static ConfigTools()
    {
        ReadPath();
    }

    [MenuItem("Tools/LuaConfig")]
    static void Init()
    {
        GetWindow(typeof(ConfigTools));
        ReadPath();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("xlsx path : ", EditorStyles.boldLabel, GUILayout.Width(80));
        xlsxFolder = GUILayout.TextField(xlsxFolder, GUILayout.Width(240));
        if (GUILayout.Button("...", GUILayout.Width(40)))
        {
            SelectXlsxFolder();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("proto path : ", EditorStyles.boldLabel, GUILayout.Width(80));
        protoFolder = GUILayout.TextField(protoFolder, GUILayout.Width(240));
        if (GUILayout.Button("...", GUILayout.Width(40)))
        {
            SelectProtoFolder();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Label("---------------------");
        if (GUILayout.Button("xlsx gen lua", GUILayout.Width(100)))
        {
            XlsxGenLua();
        }
        GUILayout.Label("---------------------");
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        GUILayout.Label("---------------------");
        if (GUILayout.Button("proto gen lua", GUILayout.Width(100)))
        {
            ProtoGenLua();
        }
        GUILayout.Label("---------------------");
        GUILayout.EndHorizontal();
    }

    private void XlsxGenLua()
    {
        if (!CheckXlsxPath(xlsxFolder))
        {
            return;
        }

        Process p = new Process();
        p.StartInfo.FileName = @"python";
        p.StartInfo.Arguments = xlsxFolder + "/tools/toconfigs.py";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WorkingDirectory = xlsxFolder + "/tools";
        p.Start();
        p.BeginOutputReadLine();
        p.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log(e.Data);
                if (e.Data.Contains("SUCCEEDED"))
                {
                    Process pr = sender as Process;
                    if (pr != null)
                    {
                        pr.Close();
                    }
                    xlsxGenLuaFinished = true;
                }
            }
        });
    }
    
    private void ProtoGenLua()
    {
        if (!CheckProtoPath(protoFolder))
        {
            return;
        }

        Process p = new Process();
        p.StartInfo.FileName = protoFolder + "/make_proto.bat";
        p.StartInfo.Arguments = "";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WorkingDirectory = protoFolder;
        p.Start();
        p.BeginOutputReadLine();
        p.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log(e.Data);
                if (e.Data.Contains("DONE"))
                {
                    Process pr = sender as Process;
                    if (pr != null)
                    {
                        pr.Close();
                    }
                    protoGenLuaFinished = true;
                }
            }
        });
    }
    
    void Update()
    {
        if (protoGenLuaFinished)
        {
            protoGenLuaFinished = false;
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Succee", "Proto gen lua finished!", "Conform");
        }

        if (xlsxGenLuaFinished)
        {
            xlsxGenLuaFinished = false;

            // copy files
            string destPath = Application.dataPath + "/LuaScripts/Config/Data";
            if (Directory.Exists(destPath))
            {
                Directory.Delete(destPath, true);
            }
            Directory.CreateDirectory(destPath);

            string[] luaFiles = Directory.GetFiles(xlsxFolder + "/tools/sconfig");
            foreach (var oneFile in luaFiles)
            {
                string destFileName = Path.Combine(destPath, Path.GetFileName(oneFile));
                UnityEngine.Debug.Log("Copy : " + destFileName);
                File.Copy(oneFile, destFileName, true);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Succee", "Xlsx gen lua finished!", "Conform");
        }
    }

    private bool CheckXlsxPath(string xlsxPath)
    {
        if (string.IsNullOrEmpty(xlsxPath))
        {
            return false;
        }

        if (!File.Exists(xlsxPath + "/tools/client_batch_csv.py"))
        {
            EditorUtility.DisplayDialog("Error", "Err path :\nNo find ./tools/client_batch_csv.py", "Conform");
            return false;
        }

        return true;
    }
    
    private bool CheckProtoPath(string protoPath)
    {
        if (string.IsNullOrEmpty(protoPath))
        {
            return false;
        }

        if (!File.Exists(protoPath + "/make_proto.bat"))
        {
            EditorUtility.DisplayDialog("Error", "Err path :\nNo find ./make_proto.bat", "Conform");
            return false;
        }

        return true;
    }

    private void SelectXlsxFolder()
    {
        var selXlsxPath = EditorUtility.OpenFolderPanel("Select xlsx folder", "", "");
        if (!CheckXlsxPath(selXlsxPath))
        {
            return;
        }

        xlsxFolder = selXlsxPath;
        SavePath();
    }

    private void SelectProtoFolder()
    {
        var selProtoPath = EditorUtility.OpenFolderPanel("Select proto folder", "", "");
        if (!CheckProtoPath(selProtoPath))
        {
            return;
        }

        protoFolder = selProtoPath;
        SavePath();
    }

    static private void SavePath()
    {
        EditorPrefs.SetString("xlsxFolder", xlsxFolder);
        EditorPrefs.SetString("protoFolder", protoFolder);
    }

    static private void ReadPath()
    {
        xlsxFolder = EditorPrefs.GetString("xlsxFolder");
        protoFolder = EditorPrefs.GetString("protoFolder");
    }
}
