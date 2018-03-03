using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// added by wsh @ 2017.12.24
/// 功能：Assetbundle相关的Editor帮助类，build相关的在BuildScript脚本
/// </summary>

namespace AssetBundles
{
    public class AssetBundleEditorHelper
    {
        public class AssetEntry
        {
            public string path;
            public List<Object> objs = new List<Object>();

            public bool Contains(Object obj)
            {
                return objs.Contains(obj);
            }
            
            public bool ContainsType(System.Type tpye)
            {
                foreach (Object obj in objs)
                {
                    if (obj.GetType().Equals(tpye))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public enum REMOVE_TYPE
        {
            ALL,
            CHILDREN_FILES,
            CHILDREN_DIR,
        }
        
        public static bool HasValidSelection()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                Debug.LogWarning("You must select an object first");
                return false;
            }
            return true;
        }
        
        public static Object[] GetDependencisObjs(Object[] objects, bool reverse)
        {
            return reverse ? EditorUtility.CollectDeepHierarchy(objects) : EditorUtility.CollectDependencies(objects);
        }
        
        public static List<AssetEntry> GetDependencyList(Object[] objects, bool reverse)
        {
            Object[] deps = GetDependencisObjs(objects, reverse);

            List<AssetEntry> list = new List<AssetEntry>();

            foreach (Object obj in deps)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (!string.IsNullOrEmpty(path))
                {
                    bool found = false;

                    foreach (AssetEntry ent in list)
                    {
                        if (ent.path.Equals(path))
                        {
                            if (!ent.ContainsType(obj.GetType())) ent.objs.Add(obj);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        AssetEntry ent = new AssetEntry();
                        ent.path = path;
                        ent.objs.Add(obj);
                        list.Add(ent);
                    }
                }
            }

            deps = null;
            objects = null;
            return list;
        }
        
        public static string RemovePrefix(string text)
        {
            text = text.Replace("UnityEngine.", "");
            text = text.Replace("UnityEditor.", "");
            return text;
        }
        
        public static string GetDependencyText(Object[] objects, bool reverse)
        {
            List<AssetEntry> dependencies = GetDependencyList(objects, reverse);
            List<string> list = new List<string>();
            string text = "";

            foreach (AssetEntry ae in dependencies)
            {
                text = ae.path;

                if (ae.objs.Count > 1)
                {
                    text += " (" + RemovePrefix(ae.objs[0].GetType().ToString());

                    for (int i = 1; i < ae.objs.Count; ++i)
                    {
                        text += ", " + RemovePrefix(ae.objs[i].GetType().ToString());
                    }

                    text += ")";
                }
                list.Add(text);
            }

            list.Sort();

            text = "";
            foreach (string s in list) text += s + "\n";
            list.Clear();
            list = null;

            dependencies.Clear();
            dependencies = null;
            return text;
        }
        
        public static void SelectDependency(Object[] objects, bool reverse)
        {
            List<AssetEntry> dependencies = GetDependencyList(objects, reverse);
            List<Object> list = new List<Object>();

            foreach (AssetEntry ae in dependencies)
            {
                if (!ae.path.StartsWith("Assets/"))
                {
                    // 不是Assets资源
                    continue;
                }
                if (ae.objs != null & ae.objs.Count > 0)
                {
                    bool isFind = false;
                    foreach (Object obj in objects)
                    {
                        if (ae.Contains(obj))
                        {
                            list.Add(obj);
                            isFind = true;
                        }
                    }
                    if (!isFind)
                    {
                        // 未找到，则默认添加第一个object
                        list.Add(ae.objs[0]);
                    }
                }
            }

            dependencies.Clear();
            dependencies = null;
            Selection.objects = list.ToArray();
        }
        
        public static string[] AssetbundleNameToAssetPath(string assetbundleName)
        {
            string[] allNames = AssetDatabase.GetAllAssetBundleNames();
            if (allNames == null)
            {
                return null;
            }

            int idx = System.Array.IndexOf(allNames, assetbundleName);
            if (idx == -1)
            {
                //not found
                Debug.LogError(string.Format("Assetbundle({0}) not found! check the name is correct with it's variant."));
                return null;
            }
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetbundleName);
        }
        
        public static string AssetPathToAssetbundleName(string assetPath)
        {
            string[] allNames = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in allNames)
            {
                string[] allPath = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                if (allPath == null)
                {
                    continue;
                }

                int idx = System.Array.IndexOf(allPath, assetPath);
                if (idx == -1)
                {
                    //not found
                    continue;
                }

                return name;
            }

            return null;
        }
        
        public static List<string> GetSelectionAssetbundleNames()
        {
            if (!HasValidSelection())
            {
                return null;
            }
            else
            {
                return GetObjectsAssetbundleNames(Selection.objects);
            }
        }
        
        public static List<string> GetObjectsAssetbundleNames(Object[] objects)
        {
            List<string> ret = new List<string>();
            foreach (Object obj in objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                string toName = AssetPathToAssetbundleName(assetPath);
                if (!string.IsNullOrEmpty(toName) && !ret.Contains(toName))
                {
                    ret.Add(toName);
                }
            }
            return ret;
        }
        
        public static List<string> GetDependancisFormBuildManifest(string manifestPath, Object[] objects,bool isAll = true)
        {
            AssetBundleManifest manifest = PackageUtils.GetManifestFormLocal(manifestPath);
            if (manifest == null)
            {
                return null;
            }

            List<string> names = GetObjectsAssetbundleNames(objects);
            if (names == null || names.Count == 0)
            {
                return null;
            }

            List<string> strList = new List<string>();
            foreach (string name in names)
            {
                string[] deps = null;
                if (isAll)
                {
                    deps = manifest.GetAllDependencies(name);
                }
                else
                {
                    deps = manifest.GetDirectDependencies(name);
                }
                foreach (string dep in deps)
                {
                    if (!strList.Contains(dep))
                    {
                        strList.Add(dep);
                    }
                }
            }
            
            return strList;
        }
        
        public static void CreateAssetbundleForCurrent(string assetPath)
        {
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                Debug.LogError(string.Format("importer null! make sure object at path({0}) is a valid assetbundle!", assetPath));
                return;
            }

            CreateAssetbundleForCurrent(importer);
        }
        
        public static void CreateAssetbundleForCurrent(AssetBundleImporter importer)
        {
            if (importer == null || !importer.IsValid)
            {
                Debug.LogError("importer null or not valid!");
                return;
            }
            
            // TODO：处理variant
            importer.assetBundleName = importer.assetPath;
            //importer.assetBundleVariant = null;
        }
        
        public static void CreateAssetbundleForChildren(string assetPath)
        {
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                Debug.LogError(string.Format("importer null! make sure object at path({0}) is a valid assetbundle!", assetPath));
                return;
            }

            CreateAssetbundleForChildren(importer);
        }
        
        public static void CreateAssetbundleForChildren(AssetBundleImporter importer)
        {
            if (importer == null || !importer.IsValid)
            {
                Debug.LogError("importer null or not valid!");
                return;
            }

            List<AssetBundleImporter> childImporter = importer.GetChildren();
            foreach (AssetBundleImporter child in childImporter)
            {
                if (!child.IsValid)
                {
                    continue;
                }

                CreateAssetbundleForCurrent(child);
            }
        }
        
        public static List<string> RemoveAssetBundleInParents(string assetPath)
        {
            List<string> removeList = new List<string>();
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return removeList;
            }

            AssetBundleImporter parentImporter = importer.GetParent();
            while (parentImporter != null)
            {
                if (!string.IsNullOrEmpty(parentImporter.assetBundleName))
                {
                    removeList.Add(string.Format("{0}{1}", parentImporter.assetPath,
                        string.IsNullOrEmpty(parentImporter.assetBundleVariant) ? null : string.Format("({0})", parentImporter.assetBundleVariant)));
                    parentImporter.assetBundleName = null;
                }
                parentImporter = parentImporter.GetParent();
            }
            return removeList;
        }
        
        public static List<string> RemoveAssetbundleInParents(Object[] objects)
        {
            List<string> removeList = new List<string>();
            foreach (Object obj in objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                removeList.AddRange(RemoveAssetBundleInParents(assetPath));
            }
            return removeList;
        }
        
        public static List<string> RemoveAssetBundleInChildren(string assetPath, bool containsSelf, bool countainsDirectly, REMOVE_TYPE removeType)
        {
            List<string> removeList = new List<string>();
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                return removeList;
            }

            //是否删除自身
            if (containsSelf && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                removeList.Add(string.Format("{0}{1}",importer.assetPath,
                    string.IsNullOrEmpty(importer.assetBundleVariant) ? null : string.Format("({0})", importer.assetBundleVariant)));
                importer.assetBundleName = null;
            }

            List<AssetBundleImporter> childImporter = importer.GetChildren();
            foreach (AssetBundleImporter child in childImporter)
            {
                if (!child.IsValid)
                {
                    continue;
                }
                
                if (!string.IsNullOrEmpty(child.assetBundleName) && countainsDirectly)
                {
                    if (removeType == REMOVE_TYPE.ALL ||
                        (removeType == REMOVE_TYPE.CHILDREN_DIR && child.IsFile == false) ||
                        (removeType == REMOVE_TYPE.CHILDREN_FILES && child.IsFile == true)
                        )
                    {
                        removeList.Add(string.Format("{0}{1}", child.assetPath,
                            string.IsNullOrEmpty(child.assetBundleVariant) ? null : string.Format("({0})", child.assetBundleVariant)));
                        child.assetBundleName = null;
                    }
                }
                if (!child.IsFile)
                {
                    // 为目录，递归：递归时containsSelf设置为false，是否删除本身由removeType确定
                    removeList.AddRange(RemoveAssetBundleInChildren(child.assetPath, false, true, removeType));
                }
            }
            return removeList;
        }
        
        public static List<string> RemoveAssetbundleInChildren(Object[] objects, bool countainsSelf = false, bool countainsDirectly = true, REMOVE_TYPE removeType = REMOVE_TYPE.ALL)
        {
            List<string> removeList = new List<string>();
            foreach (Object obj in objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                removeList.AddRange(RemoveAssetBundleInChildren(assetPath, countainsSelf, countainsDirectly, removeType));
            }
            return removeList;
        }
        
        public static void CreateAssetbundleForCurrent(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                CreateAssetbundleForCurrent(assetPath);
            }
        }
        
        public static void CreateAssetbundleForChildren(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);
                CreateAssetbundleForChildren(assetPath);
            }
        }
    }
}