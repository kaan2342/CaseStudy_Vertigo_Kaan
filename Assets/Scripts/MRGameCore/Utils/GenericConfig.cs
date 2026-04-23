using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MRGameCore
{
    namespace Utils
    {
        /// <summary>
        /// Generic Configuration class
        /// Use this base class to create game configuration scriptable objects
        /// 
        /// This script will automatically create an instance and save it to the specified path in editor mode
        /// 
        /// Usage Example:
        /// <code>
        /// public class GameplayConfig : GenericConfig<T>
        /// {
        ///		public float someFloat;
        /// }
        /// </code>
        /// 
        /// To get an instance of the GameplayConfig use:
        /// <code>
        /// GameplayConfig.Current;
        /// </code>
        /// 
        /// </summary>

        public abstract class GenericConfig<T> : ScriptableObject where T : GenericConfig<T>
        {
            private const string CONFIG_LOCATION_OLD = "Configs";

            private static T s_instance;

            public static T Instance
            {
                get
                {
                    if (s_instance == null)
                    {
                        Load();

                        // failed to find a config, creating one - only in editor mode
                        if (s_instance == null)
                        {
                            CreateConfig();
                        }
                    }
                    return s_instance;
                }
                set
                {
                    s_instance = value;
                }
            }

            private static void Load()
            {
                s_instance = Resources.Load<T>(typeof(T).ToString());
                if (s_instance == null)
                {
                    s_instance = Resources.Load<T>(CONFIG_LOCATION_OLD + "/" + typeof(T).ToString());
                }
            }

            private static void CreateConfig()
            {
#if UNITY_EDITOR
                string typeName = typeof(T).ToString();
                GenericConfig<T> asset = ScriptableObject.CreateInstance<T>();
                string savePath = "Assets/Data/Resources/";
                string assetName = string.Format("{0}.asset", typeName);
                ValidateDirectory(savePath);

                AssetDatabase.CreateAsset(asset, savePath + assetName);
                AssetDatabase.SaveAssets();

                s_instance = (T)asset;

                Debug.LogWarning("Failed to find " + typeName + " - created a new one");
#endif
            }

            private static void ValidateDirectory(string directory)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }
    }
}