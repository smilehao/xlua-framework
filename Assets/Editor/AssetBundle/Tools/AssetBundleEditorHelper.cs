using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// add by wsh @ 2017.12.24
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
                    bool is_find = false;
                    foreach (Object obj in objects)
                    {
                        if (ae.Contains(obj))
                        {
                            list.Add(obj);
                            is_find = true;
                        }
                    }
                    if (!is_find)
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
        
        public static string[] AssetbundleNameToAssetPath(string assetbundle_name)
        {
            string[] all_names = AssetDatabase.GetAllAssetBundleNames();
            if (all_names == null)
            {
                return null;
            }

            int idx = System.Array.IndexOf(all_names, assetbundle_name);
            if (idx == -1)
            {
                //not found
                Debug.LogError(string.Format("Assetbundle({0}) not found! check the name is correct with it's variant."));
                return null;
            }
            return AssetDatabase.GetAssetPathsFromAssetBundle(assetbundle_name);
        }
        
        public static string AssetPathToAssetbundleName(string asset_path)
        {
            string[] all_names = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in all_names)
            {
                string[] all_path = AssetDatabase.GetAssetPathsFromAssetBundle(name);
                if (all_path == null)
                {
                    continue;
                }

                int idx = System.Array.IndexOf(all_path, asset_path);
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
                string asset_path = AssetDatabase.GetAssetPath(obj);
                string to_name = AssetPathToAssetbundleName(asset_path);
                if (!string.IsNullOrEmpty(to_name) && !ret.Contains(to_name))
                {
                    ret.Add(to_name);
                }
            }
            return ret;
        }
        
        public static string GetDependancisTextFormLocal(string manifest_path, Object[] objects,bool is_all = true)
        {
            AssetBundleManifest manifest = AssetBundleUtility.GetManifestFormLocal(manifest_path);
            if (manifest == null)
            {
                return null;
            }

            List<string> names = GetObjectsAssetbundleNames(objects);
            if (names == null || names.Count == 0)
            {
                return null;
            }

            List<string> str_list = new List<string>();
            foreach (string name in names)
            {
                string[] deps = null;
                if (is_all)
                {
                    deps = manifest.GetAllDependencies(name);
                }
                else
                {
                    deps = manifest.GetDirectDependencies(name);
                }
                foreach (string dep in deps)
                {
                    if (!str_list.Contains(dep))
                    {
                        //if nod in ret_str, add to it
                        str_list.Add(dep);
                    }
                }
            }

            str_list.Sort();

            string ret_str = string.Empty;
            int i = 0;
            foreach (string str in str_list)
            {
                ret_str += string.Format("[{0}]{1}\n", ++i, str);
            }
            return ret_str;
        }
        
        public static void CreateAssetbundleForCurrent(string asset_path, string variant_name = null, bool keep_variant_when_null = true)
        {
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(asset_path);
            if (importer == null)
            {
                Debug.LogError(string.Format("importer null! make sure object at path({0}) is a valid assetbundle!", asset_path));
                return;
            }

            CreateAssetbundleForCurrent(importer, variant_name, keep_variant_when_null);
        }
        
        public static void CreateAssetbundleForCurrent(AssetBundleImporter importer, string variant_name = null, bool keep_variant_when_null = true)
        {
            if (importer == null || !importer.IsValid)
            {
                Debug.LogError("importer null or not valid!");
                return;
            }

            //set the asset_path as assetbundle name
            importer.assetBundleName = importer.assetPath;
            if (string.IsNullOrEmpty(variant_name))
            {
                if (!keep_variant_when_null)
                {
                    importer.assetBundleVariant = null;
                }
            }
            else
            {
                importer.assetBundleVariant = variant_name;
            }
        }
        
        public static void CreateAssetbundleForChildrenFiles(string asset_path, string variant_name = null, bool keep_variant_when_null = true)
        {
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(asset_path);
            if (importer == null)
            {
                Debug.LogError(string.Format("importer null! make sure object at path({0}) is a valid assetbundle!", asset_path));
                return;
            }

            CreateAssetbundleForChildrenFiles(importer, variant_name, keep_variant_when_null);
        }
        
        public static void CreateAssetbundleForChildrenFiles(AssetBundleImporter importer, string variant_name = null, bool keep_variant_when_null = true)
        {
            if (importer == null || !importer.IsValid)
            {
                Debug.LogError("importer null or not valid!");
                return;
            }

            List<AssetBundleImporter> child_importer = importer.GetChildren();
            foreach (AssetBundleImporter child in child_importer)
            {
                if (!child.IsValid)
                {
                    continue;
                }

                if (child.IsValid)
                {
                    CreateAssetbundleForCurrent(child, variant_name, keep_variant_when_null);
                }
                if (!child.IsFile)
                {
                    CreateAssetbundleForChildrenFiles(child.assetPath);
                }
            }
        }
        
        public static List<string> RemoveAssetBundleInParents(string asset_path)
        {
            List<string> remove_list = new List<string>();
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(asset_path);
            if (importer == null)
            {
                return remove_list;
            }

            AssetBundleImporter parent_importer = importer.GetParent();
            while (parent_importer != null)
            {
                if (!string.IsNullOrEmpty(parent_importer.assetBundleName))
                {
                    remove_list.Add(string.Format("{0}{1}", parent_importer.assetPath,
                        string.IsNullOrEmpty(parent_importer.assetBundleVariant) ? null : string.Format("({0})", parent_importer.assetBundleVariant)));
                    parent_importer.assetBundleName = null;
                }
                parent_importer = parent_importer.GetParent();
            }
            return remove_list;
        }
        
        public static List<string> RemoveAssetbundleInParents(Object[] objects)
        {
            List<string> remove_list = new List<string>();
            foreach (Object obj in objects)
            {
                string asset_path = AssetDatabase.GetAssetPath(obj);
                remove_list.AddRange(RemoveAssetBundleInParents(asset_path));
            }
            return remove_list;
        }
        
        public static List<string> RemoveAssetBundleInChildren(string asset_path, bool contains_self = false, REMOVE_TYPE remove_type = REMOVE_TYPE.ALL)
        {
            List<string> remove_list = new List<string>();
            AssetBundleImporter importer = AssetBundleImporter.GetAtPath(asset_path);
            if (importer == null)
            {
                return remove_list;
            }

            //是否删除自身
            if (contains_self && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                remove_list.Add(string.Format("{0}{1}",importer.assetPath,
                    string.IsNullOrEmpty(importer.assetBundleVariant) ? null : string.Format("({0})", importer.assetBundleVariant)));
                importer.assetBundleName = null;
            }

            List<AssetBundleImporter> child_importer = importer.GetChildren();
            foreach (AssetBundleImporter child in child_importer)
            {
                if (!child.IsValid)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(child.assetBundleName))
                {
                    if (remove_type == REMOVE_TYPE.ALL ||
                        (remove_type == REMOVE_TYPE.CHILDREN_DIR && child.IsFile == false) ||
                        (remove_type == REMOVE_TYPE.CHILDREN_FILES && child.IsFile == true)
                        )
                    {
                        remove_list.Add(string.Format("{0}{1}", child.assetPath,
                            string.IsNullOrEmpty(child.assetBundleVariant) ? null : string.Format("({0})", child.assetBundleVariant)));
                        child.assetBundleName = null;
                    }
                }
                if (!child.IsFile)
                {
                    // 为目录，递归：递归时contains_self设置为false，是否删除本身由remove_type确定
                    remove_list.AddRange(RemoveAssetBundleInChildren(child.assetPath, false, remove_type));
                }
            }
            return remove_list;
        }
        
        public static List<string> RemoveAssetbundleInChildren(Object[] objects,bool countains_self = false, REMOVE_TYPE remove_type = REMOVE_TYPE.ALL)
        {
            List<string> remove_list = new List<string>();
            foreach (Object obj in objects)
            {
                string asset_path = AssetDatabase.GetAssetPath(obj);
                remove_list.AddRange(RemoveAssetBundleInChildren(asset_path, countains_self, remove_type));
            }
            return remove_list;
        }
        
        public static void CreateAssetbundleForCurrent(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                string asset_path = AssetDatabase.GetAssetPath(obj);
                CreateAssetbundleForCurrent(asset_path);
            }
        }
        
        public static void CreateAssetbundleForChildrenFiles(Object[] objects)
        {
            foreach (Object obj in objects)
            {
                string asset_path = AssetDatabase.GetAssetPath(obj);
                CreateAssetbundleForChildrenFiles(asset_path);
            }
        }
    }
}