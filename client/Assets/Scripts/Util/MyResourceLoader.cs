using System;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public static class MyResourceLoader
    {
        public static IEnumerator LoadResources<T>(string resourcePath, Action<T> onLoaded) where T: UnityEngine.Object
        {
            var request = Resources.LoadAsync<T>(resourcePath);

            yield return request;

            T asset = request.asset as T;

            if (asset == null)
            {
                Debug.LogWarning($"资源加载失败：{resourcePath}，类型：{typeof(T).Name}");
            }

            // 加载失败时也回调，结果为 null
            onLoaded?.Invoke(asset);
        }
        
    }
}