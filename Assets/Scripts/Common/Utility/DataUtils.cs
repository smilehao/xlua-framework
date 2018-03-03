using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class DataUtils
{
    public static void CopyBytes(byte[] copyTo, int offsetTo, byte[] copyFrom, int offsetFrom, int count)
    {
        Array.Copy(copyFrom, offsetFrom, copyTo, offsetTo, count);
    }
    
    public static byte[] StringToBytes(string str)
    {
        return System.Text.Encoding.Default.GetBytes(str);
    }

    public static byte[] StringToUTFBytes(string str)
    {
        return System.Text.Encoding.UTF8.GetBytes(str);
    }

    public static string BytesToString(byte[] bytes)
    {
        return System.Text.Encoding.Default.GetString(bytes).Trim();
    }

    public static Hashtable HttpGetInfo(string info)
    {
        if (string.IsNullOrEmpty(info))
        {
            return null;
        }

        Hashtable table = new Hashtable();
        string[] paramList = info.Split('&');
        for (int i = 0; i < paramList.Length; i++)
        {
            string[] keyAndValue = paramList[i].Split('=');
            if (keyAndValue.Length >= 2)
            {
                if (!table.ContainsKey(keyAndValue[0]))
                {
                    table.Add(keyAndValue[0], keyAndValue[1]);
                }
            }
        }

        return table;
    }
}
