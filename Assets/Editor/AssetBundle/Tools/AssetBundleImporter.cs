using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// added by wsh @ 2017.12.25
/// 功能：Assetbundle相关的AssetImporter扩展类，结合AssetImporter与文件系统功能，方便对父、子节点进行操作
/// 注意：
/// 1、所有对assetbundle属性的操作，都应该在此类中得到封装
/// 2、采用了组合方式，是因为继承AssetImporter时，静态函数GetAtPath无法处理，new出来的AssetBundlesImporter为空
/// </summary>

namespace AssetBundles
{
    public class AssetBundleImporter
    {
        private AssetImporter assetImporter = null;
        private bool isFile = false;
        private DirectoryInfo dirInfo = null;
        private FileInfo fileInfo = null;
        
        public AssetBundleImporter(AssetImporter assetImporter)
        {
            this.assetImporter = assetImporter;
            if (this.assetImporter != null)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(this.assetImporter.assetPath);
                FileInfo fileInfo = new FileInfo(this.assetImporter.assetPath);
                if (dirInfo.Exists)
                {
                    isFile = false;
                    this.dirInfo = dirInfo;
                }
                if (fileInfo.Exists)
                {
                    isFile = true;
                    this.fileInfo = fileInfo;
                }
            }
        }
        
        public static AssetBundleImporter GetAtPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }
            
            AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);
            if (assetImporter == null)
            {
                return null;
            }
            else
            {
                return new AssetBundleImporter(assetImporter);
            }
        }
        
        public bool IsValid
        {
            get
            {
                bool isValid = true;
                if (assetImporter == null)
                {
                    isValid = false;
                }

                if (isFile && (fileInfo == null || !fileInfo.Exists))
                {
                    isValid = false;
                }

                if (!isFile && (dirInfo == null || !dirInfo.Exists))
                {
                    isValid = false;
                }

                if (!isValid)
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                }
                return isValid;
            }
        }
        
        
        public bool IsFile
        {
            get
            {
                return isFile;
            }
        }

        public AssetImporter AssetImporter
        {
            get
            {
                return assetImporter;
            }
        }

        public string assetBundleName 
        {
            get
            {
                return IsValid ? assetImporter.assetBundleName : null;
            }
            set
            {
                if (IsValid)
                {
                    assetImporter.assetBundleName = AssetBundleUtility.AssetBundlePathToAssetBundleName(value);
                }
            }
        }

        public string assetBundleVariant 
        {
            get
            {
                return IsValid ? assetImporter.assetBundleVariant : null;
            }
            set
            {
                //must firstly set assetBundleName,then set assetBundleVariant
                if (IsValid && !string.IsNullOrEmpty(assetImporter.assetBundleName))
                {
                    assetImporter.assetBundleVariant = value;
                }
            }
        }

        public string assetPath
        { 
            get
            {
                return IsValid ? assetImporter.assetPath : null;
            } 
        }

        public string packagePath
        {
            get
            {
                return IsValid ? AssetBundleUtility.AssetsPathToPackagePath(assetPath) : null;
            }
        }

        public ulong assetTimeStamp { 
            get
            {
                return IsValid ? assetImporter.assetTimeStamp : 0L;
            }
        }

        public string userData
        {
            get
            {
                return IsValid ? assetImporter.userData : null;
            }
            set
            {
                if (IsValid)
                {
                    assetImporter.userData = value;
                }
            }
        }
        
        public void SaveAndReimport()
        {
            if (IsValid)
            {
                assetImporter.SaveAndReimport();
            }
        }
        
        private string FullPathToAssetPath(string fullPath)
        {
            string retPath = GameUtility.FullPathToAssetPath(fullPath);
            if (retPath.Equals(GameUtility.AssetsFolderName))
            {
                return null;
            }
            else
            {
                return retPath;
            }
        }
        
        private string GetParentAssetPath()
        {
            if (!IsValid)
            {
                return null;
            }

            if (isFile)
            {
                return FullPathToAssetPath(fileInfo.Directory.FullName);
            }
            else
            {
                return FullPathToAssetPath(dirInfo.Parent.FullName);
            }
        }
        
        public AssetBundleImporter GetParent()
        {
            string parentPath = GetParentAssetPath();
            return GetAtPath(parentPath);
        }
        
        public List<AssetBundleImporter> GetChildren()
        {
            List<AssetBundleImporter> arr = new List<AssetBundleImporter>();
            if (!IsValid || isFile)
            {
                return arr;
            }
            
            DirectoryInfo[] dirs = dirInfo.GetDirectories();
            FileInfo[] files = dirInfo.GetFiles();
            int length = dirs.Length + files.Length;
            if(length == 0)
            {
                return arr;
            }

            for (int i = 0; i < length; i++)
            {
                AssetBundleImporter child = null;
                if (i < dirs.Length)
                {
                    child = GetAtPath(FullPathToAssetPath(dirs[i].FullName));
                }
                else
                {
                    child = GetAtPath(FullPathToAssetPath(files[i - dirs.Length].FullName));
                }

                if (child != null && child.IsValid)
                {
                    //说明：文件系统目录下的.cs，.meta文件是无法创建AssetImporter的，这里会自动进行过滤
                    arr.Add(child);
                }
            }
            return arr;
        }
        
    }
}
