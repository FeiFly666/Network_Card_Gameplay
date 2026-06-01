using Common;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Assets.Scripts.Manager
{
    public class MySceneManager : MonoBehaviour
    {
        public static MySceneManager Instance;
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
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            StartCoroutine(Enter(op));
            
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
