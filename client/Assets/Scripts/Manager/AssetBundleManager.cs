using Assets.Scripts.HotUpdate;
using Assets.Scripts.Model;
using Assets.Scripts.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class BundleData
    {
        public AssetBundle assetBundle;
        public AssetBundleCreateRequest loadingRequest;
        public int Count;
        public double UnloadTime;

        public BundleData(AssetBundle assetBundle)
        {
            this.assetBundle = assetBundle;
            this.Count = 1;
        }
    }

}
namespace Assets.Scripts.Manager
{
    public enum AssetLoadMode
    {
        Editor,
        AssetBundle
    }

    public class AssetBundleManager : MonoBehaviour
    {
        [SerializeField] public AssetLoadMode assetLoadMode;
        private FileListStruct m_fileList = null;
        private Dictionary<string, BundleData> m_AssetBundles = new Dictionary<string, BundleData>();

        private List<string> m_pendingAssetBundles = new List<string>();

        public static AssetBundleManager instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        public void InitAssetBundleInfos(FileListStruct filelist)
        {
            foreach (var bundle in m_AssetBundles.Values)
            {
                if (bundle.assetBundle != null)
                    bundle.assetBundle.Unload(false);
            }

            m_AssetBundles.Clear();
            m_pendingAssetBundles.Clear();
            m_fileList = filelist;
        }
        private void Update()
        {
            double now = Time.unscaledTimeAsDouble;

            for (int i = m_pendingAssetBundles.Count - 1; i >= 0; i--)
            {
                string name = m_pendingAssetBundles[i];

                if (!m_AssetBundles.TryGetValue(name, out var data) || data.Count != 0)
                {
                    m_pendingAssetBundles.RemoveAt(i);
                    continue;
                }

                if (now < data.UnloadTime || data.loadingRequest != null || HasCachedParent(name))
                {
                    continue;
                }

                if (data.assetBundle != null)
                    data.assetBundle.Unload(true);

                m_AssetBundles.Remove(name);
                m_pendingAssetBundles.RemoveAt(i);
            }
        }
        private bool HasCachedParent(string assetBundleName)
        {
            foreach (var entry in m_AssetBundles)
            {
                var dependencies = m_fileList.bundles[entry.Key].dependencies;

                if (Array.IndexOf(dependencies ?? Array.Empty<string>(),assetBundleName) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAssetAsync<T>(string assetName, Action<T> callback) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            if(assetLoadMode == AssetLoadMode.Editor)
            {
                string extension = ".asset";
                //StartCoroutine(MyResourceLoader.LoadResources<T>(assetName, callback));
                bool wantsComponent = typeof(Component).IsAssignableFrom(typeof(T));
                Type loadType = wantsComponent ? typeof(GameObject) : typeof(T);

                if (loadType == typeof(GameObject))
                    extension = ".prefab";
                else if (loadType == typeof(TextAsset))
                    extension = ".json";
                else if (loadType == typeof(UnityEditor.SceneAsset))
                    extension = ".unity";

                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath($"{PathUtil.BuildResourcesPath}/{assetName}{extension}", loadType);

                T asset = obj as T;
                if (wantsComponent && obj is GameObject go)
                    asset = go.GetComponent<T>();

                if (asset == null)
                    Debug.LogError($"未找到资源或组件：{assetName}，类型：{typeof(T).Name}");

                callback?.Invoke(asset);

                return;
            }
#endif
            if(!m_fileList.assets.TryGetValue(assetName,out var assetBundleName))
            {
                callback?.Invoke(null);
                return;
            }
            StartCoroutine(LoadBundle<T>(assetBundleName, assetName, callback));
        }
        public IEnumerator LoadBundle<T>(string assetBundleName, string assetName = null ,Action<T> callback = null, Action ReadyCallback = null) where T : UnityEngine.Object
        {
            AssetBundleInfo bundleInfo = m_fileList.bundles[assetBundleName];
            if(bundleInfo == null)
            {
                callback?.Invoke(null);
                yield break;
            }

            if(!m_AssetBundles.TryGetValue(assetBundleName, out var bundle))
            {
                string bundleName = $"{Path.GetFileNameWithoutExtension(assetBundleName)}_{bundleInfo.sha256}.ab";
                string bundlePath = Path.Combine(PathUtil.LocalFilePath, LoadingService.Instance.platform, "bundles", bundleName);

                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromFileAsync(bundlePath);

                bundle = new BundleData(null);
                bundle.loadingRequest = createRequest;

                m_AssetBundles.Add(assetBundleName, bundle);
            }
            else
            {
                bundle.Count++;
            }

            //不同协程统一加载相同的AB包。
            var loadingRequest = bundle.loadingRequest;
            if (loadingRequest != null)
            {
                while (!loadingRequest.isDone)
                {
                    yield return null;
                }

                if (bundle.loadingRequest != null)
                {
                    bundle.assetBundle = bundle.loadingRequest.assetBundle;
                    bundle.loadingRequest = null;

                    if(bundle.assetBundle == null)
                    {
                        m_AssetBundles.Remove(assetBundleName);
                    }
                }

            }

            if(bundle.assetBundle == null)
            {
                callback?.Invoke(null);
                yield break;
            }


            if (bundleInfo.dependencies != null)
            {
                for(int i = 0;i < bundleInfo.dependencies.Length; i++)
                {
                    bool ready = false;
                    var denpendency = bundleInfo.dependencies[i];

                    yield return LoadBundle<T>(denpendency, ReadyCallback:()=> ready = true);

                    if (ready)
                        continue;

                    DecreaseAssetBundleCount(assetBundleName);

                    for (int j = 0; j < i; j++)
                        DecreaseAllAssetBundleCount(bundleInfo.dependencies[j]);

                    callback?.Invoke(null);
                    yield break;
                }
            }
            ReadyCallback?.Invoke();

            if (callback == null || assetName == null)//依赖资源不需要加载具体资源以及回调
            {
                yield break;
            }

            if (typeof(T) == typeof(AssetBundle))//场景
            {
                callback.Invoke(bundle.assetBundle as T);
                yield break;
            }

            bool wantsComponent = typeof(Component).IsAssignableFrom(typeof(T));
            Type loadType = wantsComponent? typeof(GameObject) : typeof(T);

            var request = bundle.assetBundle.LoadAssetAsync(assetName, loadType);

            yield return request;

            var loadAsset = request.asset;
            T result = loadAsset as T;

            if(wantsComponent && loadAsset is GameObject go)
            {
                result = go.GetComponent<T>();
            }

            if (result == null)
                DecreaseAllAssetBundleCount(assetBundleName);

            callback?.Invoke(result);

        }
        public void ReleaseAsset(string assetName)
        {
#if UNITY_EDITOR
            if (assetLoadMode == AssetLoadMode.Editor)
                return;
#endif
            if (m_fileList.assets.TryGetValue(assetName, out var assetBundleName))
                DecreaseAllAssetBundleCount(assetBundleName);
        }

        private void DecreaseAllAssetBundleCount(string assetBundleName)
        {
            DecreaseAssetBundleCount(assetBundleName);

            var denpendencies = m_fileList.bundles[assetBundleName].dependencies;
            foreach(var denpendency in denpendencies ?? Array.Empty<string>())
            {
                DecreaseAllAssetBundleCount(denpendency);
            }

        }
        private void DecreaseAssetBundleCount(string assetBundleName)
        {
            if(m_AssetBundles.TryGetValue(assetBundleName, out var data))
            {
                data.Count--;
                Debug.Log($"Bundle: {assetBundleName} 计数减一 count:{data.Count}");
                if (data.Count == 0)
                {
                    //TODO:清理AB包

                    data.UnloadTime = Time.unscaledTimeAsDouble + 60d;

                    if (!m_pendingAssetBundles.Contains(assetBundleName))
                        m_pendingAssetBundles.Add(assetBundleName);
                }
            }
        }

        

    }

}
