using Common;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Assets.Scripts.Manager
{
    public class MySceneManager : MonoBehaviour
    {
        public static MySceneManager Instance;
        private string ScenePath = "Scene/";
        private void Awake()
        {
            if(Instance == null)
                Instance = this;
            else if(Instance != this)
            {
                Destroy(this.gameObject);
            }
        }
        public void EnterScene(string sceneName)
        {

            if (sceneName == "HotUpdate")
            {
                StartCoroutine(Enter(SceneManager.LoadSceneAsync(sceneName)));
                return;
            }
#if UNITY_EDITOR
            if (AssetBundleManager.instance.assetLoadMode == AssetLoadMode.Editor)
            {
                AssetBundleManager.instance.LoadAssetAsync<UnityEditor.SceneAsset>(ScenePath + sceneName, sceneAsset =>
                    {
                        if (sceneAsset == null)
                            return;
                        string path = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);

                        var op = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
                        StartCoroutine(Enter(op));
                    });
                return;
            }
#endif
            AssetBundleManager.instance.LoadAssetAsync<AssetBundle>(ScenePath + sceneName, bundle =>
                {
                    if (bundle == null)
                        return;
                    string path = bundle.GetAllScenePaths()[0];
                    StartCoroutine(Enter(SceneManager.LoadSceneAsync(path, LoadSceneMode.Single)));
                });
        }
        IEnumerator Enter(AsyncOperation op)
        {
            op.allowSceneActivation = true;
            while (!op.isDone)
            {
                yield return null;
            }

        }
    }
}
