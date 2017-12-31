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
        private AssetImporter m_asset_importer = null;
        private bool m_is_file = false;
        private DirectoryInfo m_dir_info = null;
        private FileInfo m_file_info = null;
        
        public AssetBundleImporter(AssetImporter asset_import)
        {
            m_asset_importer = asset_import;
            if (m_asset_importer != null)
            {
                DirectoryInfo dir_info = new DirectoryInfo(m_asset_importer.assetPath);
                FileInfo file_info = new FileInfo(m_asset_importer.assetPath);
                if (dir_info.Exists)
                {
                    m_is_file = false;
                    m_dir_info = dir_info;
                }
                if (file_info.Exists)
                {
                    m_is_file = true;
                    m_file_info = file_info;
                }
            }
        }
        
        public static AssetBundleImporter GetByAssetImporterInstance(AssetImporter asset_import)
        {
            return new AssetBundleImporter(asset_import);
        }
        
        public static AssetBundleImporter GetAtPath(string asset_path)
        {
            if (string.IsNullOrEmpty(asset_path))
            {
                return null;
            }

            AssetImporter asset_importer = AssetImporter.GetAtPath(asset_path);
            if (asset_importer == null)
            {
                return null;
            }
            else
            {
                return new AssetBundleImporter(asset_importer);
            }
        }
        
        public bool IsValid
        {
            get
            {
                if (m_asset_importer == null)
                {
                    return false;
                }

                if (m_is_file && (m_file_info == null || !m_file_info.Exists))
                {
                    return false;
                }

                if (!m_is_file && (m_dir_info == null || !m_dir_info.Exists))
                {
                    return false;
                }

                return true;
            }
        }
        
        /// </summary>
        public bool IsFile
        {
            get
            {
                return m_is_file;
            }
        }

        public AssetImporter AssetImporter
        {
            get
            {
                return m_asset_importer;
            }
        }

        public string assetBundleName 
        {
            get
            {
                if (IsValid)
                {
                    return m_asset_importer.assetBundleName;
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                    return null;
                }
            }
            set
            {
                if (IsValid)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        //remove root "Assets/"
                        value = value.Replace("Assets/", "");
                        //no " "
                        value = value.Replace(" ", "");
                        //there should not be any '.' in the assetbundle name
                        //otherwise the variant handling in client may go wrong
                        value = value.Replace(".", "_");
                        //add after suffix ".assetbundle" to the end
                        value = value + AssetBundleConfig.AssetBundleSuffix;
                        m_asset_importer.assetBundleName = value;
                    }
                    else
                    {
                        m_asset_importer.assetBundleName = null;
                    }
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                }
            }
        }

        public string assetBundleVariant 
        {
            get {
                if (IsValid)
                {
                    return m_asset_importer.assetBundleVariant;
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                    return null;
                }
            }
            set {
                if (IsValid)
                {
                    //must firstly set assetBundleName,then set assetBundleVariant
                    if (!string.IsNullOrEmpty(m_asset_importer.assetBundleName))
                    {
                        m_asset_importer.assetBundleVariant = value;
                    }
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                }
            }
        }

        public string assetPath { 
            get {
                if (IsValid)
                {
                    return m_asset_importer.assetPath;
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                    return null;
                }
            } 
        }
        public ulong assetTimeStamp { 
            get {
                if (IsValid)
                {
                    return m_asset_importer.assetTimeStamp;
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                    return 0L;
                }
            }
        }

        public string userData
        {
            get {
                if (IsValid)
                {
                    return m_asset_importer.userData;
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                    return null;
                }
            }
            set {
                if (IsValid)
                {
                    m_asset_importer.userData = value;
                }
                else
                {
                    Debug.LogError("AssetBundlesImporter is not valid!");
                }
            }
        }

        public void SaveAndReimport() { m_asset_importer.SaveAndReimport(); }
        
        private string FullPathToAssetPath(string full_path)
        {
            string ret_path = GameUtility.FullPathToAssetPath(full_path);
            if (ret_path.Equals(GameUtility.AssetsFolderName))
            {
                return null;
            }
            else
            {
                return ret_path;
            }
        }
        
        private string GetParentAssetPath()
        {
            if (!IsValid)
            {
                return null;
            }

            if (m_is_file)
            {
                return FullPathToAssetPath(m_file_info.Directory.FullName);
            }
            else
            {
                return FullPathToAssetPath(m_dir_info.Parent.FullName);
            }
        }
        
        public AssetBundleImporter GetParent()
        {
            string parent_path = GetParentAssetPath();
            return GetAtPath(parent_path);
        }
        
        public List<AssetBundleImporter> GetChildren()
        {
            List<AssetBundleImporter> ret_arr = new List<AssetBundleImporter>();
            if (!IsValid || m_is_file)
            {
                return ret_arr;
            }
            
            DirectoryInfo[] dirs = m_dir_info.GetDirectories();
            FileInfo[] files = m_dir_info.GetFiles();
            int length = dirs.Length + files.Length;
            if(length == 0)
            {
                return ret_arr;
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
                    ret_arr.Add(child);
                }
            }
            return ret_arr;
        }
        
    }
}
