using UnityEngine;
using UnityEditor;
using System.IO;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class ToLuaMenu
{
    static void CopyLuaBytesFiles(string sourceDir, string destDir, bool appendext = true, string searchPattern = "*.lua", SearchOption option = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(sourceDir))
        {
            return;
        }

        string[] files = Directory.GetFiles(sourceDir, searchPattern, option);
        int len = sourceDir.Length;

        if (sourceDir[len - 1] == '/' || sourceDir[len - 1] == '\\')
        {
            --len;
        }         

        for (int i = 0; i < files.Length; i++)
        {
            string str = files[i].Remove(0, len);
            string dest = destDir + "/" + str;
            if (appendext) dest += ".bytes";
            string dir = Path.GetDirectoryName(dest);
            Directory.CreateDirectory(dir);
            File.Copy(files[i], dest, true);
        }
    }


    [MenuItem("Lua/Copy Lua  files to Resources", false, 51)]
    public static void CopyLuaFilesToRes()
    {
        string destDir = Application.dataPath + "/Resources" + "/Lua";
        CopyLuaBytesFiles(Application.dataPath + "/LuaScripts", destDir);
        AssetDatabase.Refresh();
        Debug.Log("Copy lua files over");
    }
}
