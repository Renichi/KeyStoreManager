namespace KSM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class KeyStoreManager : EditorWindow
    {
        [SerializeField] private KSMPermanentData data;
        private Dictionary<string, Tuple<string, string, string, string>> keystoreInfo = new Dictionary<string, Tuple<string, string, string, string>>();
        private string[] popupNames = new String[0];
        private string password = "";
        private string alias = "";
        private string password2 = "";
        private string memo = "";

        [MenuItem("Extra/Android/KeyStoreManager")]
        public static void Create()
        {
            EditorWindow wnd = GetWindow<KeyStoreManager>();
            wnd.titleContent = new GUIContent("KeyStoreManager");
        }

        private void OnGUI()
        {
            if (this.data == null) { this.data = LoadData(); }

            EditorGUI.BeginChangeCheck();

            string path = this.data.Folderpath;
            int selector = this.data.Selector;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Folder Path");
            path = EditorGUILayout.TextField(path);
            string infopath = path + "info.text";
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (Directory.Exists(path) && path.Substring(path.Length - 1) == "/")
            {
                popupNames = Directory.GetFiles(path, "*.keystore");
                
                if (selector > popupNames.Length)
                {
                    selector = 0;
                }

                if (popupNames.Length > 0)
                {
                    LoadInfoFile(infopath);
                }
            }
            else
            {
                popupNames = new string[0];
                selector = 0;
            }
            if (popupNames.Length == 0)
            {
                EditorGUILayout.LabelField("Nothig KeyStore");

                if (EditorGUI.EndChangeCheck())
                {
                    this.data.Folderpath = path;
                    this.data.Selector = selector;
                    EditorUtility.SetDirty(this.data);
                }
            }
            else
            {
                for (int i = 0; i < popupNames.Length; i++) popupNames[i] = popupNames[i].Replace(path, "");
                selector = EditorGUILayout.Popup("KeyStore", selector, popupNames);
                if (!keystoreInfo.ContainsKey(popupNames[selector]))
                {
                    CreateInfoFile(popupNames[selector]);
                }
                Tuple<string, string, string, string> val = keystoreInfo[popupNames[selector]];
                password = val.Item1;
                alias = val.Item2;
                password2 = val.Item3;
                memo = val.Item4;
                password = EditorGUILayout.TextField("Password", password);
                alias = EditorGUILayout.TextField("Alias", alias);
                password2 = EditorGUILayout.TextField("Password", password2);
                memo = EditorGUILayout.TextField("memo", memo);

                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = path + popupNames[selector];
                PlayerSettings.Android.keystorePass = password;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = password2;

                if (EditorGUI.EndChangeCheck())
                {
                    this.data.Folderpath = path;
                    this.data.Selector = selector;
                    string savedata = "";
                    keystoreInfo[popupNames[selector]] = new Tuple<string, string, string, string>(password, alias, password2, memo);
                    foreach (KeyValuePair<string, Tuple<string, string, string, string>> kvp in keystoreInfo)
                    {
                        savedata += string.Format("{0}-{1}-{2}-{3}-{4},", kvp.Key, kvp.Value.Item1, kvp.Value.Item2, kvp.Value.Item3, kvp.Value.Item4);
                    }
                    StreamWriter wr = new StreamWriter(infopath, false);
                    wr.WriteLine(savedata);
                    wr.Close();
                    EditorUtility.SetDirty(this.data);
                }
            }

            
        }

        static KSMPermanentData LoadData()
        {
            return (KSMPermanentData)AssetDatabase.FindAssets("t:ScriptableObject")
               .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
               .Select(path => AssetDatabase.LoadAssetAtPath(path, typeof(KSMPermanentData)))
               .Where(obj => obj != null)
               .FirstOrDefault();
        }

        private void LoadInfoFile( string path )
        {
            keystoreInfo.Clear();
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            
            StreamReader sr = new StreamReader(path);
            string data = sr.ReadToEnd();
            sr.Close();
            string[] infos = data.Split(",");
            if (infos[ 0 ] != "")
            {
                for (int i = 0; i < infos.Length - 1; i++)
                {
                    string[] keystoredata = infos[i].Split("-");
                    Tuple<string, string, string, string> val = new Tuple<string, string, string, string>(keystoredata[1], keystoredata[2], keystoredata[3], keystoredata[4]);
                    keystoreInfo.Add(keystoredata[0], val);
                }
            }
        }

        private void CreateInfoFile(string key)
        {
            Tuple<string, string, string, string> val = new Tuple<string, string, string, string>("", "", "", "");
            keystoreInfo.Add(key, val);
        }
    }
}

