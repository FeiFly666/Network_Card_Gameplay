using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public enum UILayer
    {
        Static,
        Window,
        ToolTip,
        MessageBox,
        HUD
    }
    [Serializable]
    public class SceneStaticUIData
    {
        public string tag = "";
        public GameObject root;
    }
    [Serializable]
    public class LayerRoot
    {
        public UILayer layer;
        public Transform root;
    }


    public class UIManager : MonoBehaviour
    {
        private Dictionary<string, UIWindow> m_UI = new Dictionary<string, UIWindow>();

        private readonly Dictionary<string, (bool, Func<UIWindow, bool>)> m_loadingUIs = new Dictionary<string, (bool, Func<UIWindow, bool>)>();

        private Dictionary<UILayer, Stack<UIWindow>> m_layerStack = new Dictionary<UILayer, Stack<UIWindow>>();

        private Dictionary<UILayer, Transform> m_roots = new Dictionary<UILayer, Transform>();

        private readonly Dictionary<string, double> m_pendingUIs = new Dictionary<string, double>();
        private List<string> m_deadUIs = new List<string>();

        public static UIManager Instance;
        private string UIPath = "UI/";
        [SerializeField] private List<SceneStaticUIData> m_sceneStaticUIs = new List<SceneStaticUIData>();
        [SerializeField] private List<LayerRoot> m_layerRoots = new List<LayerRoot>();
        [SerializeField] public Transform dragRoot;
        [SerializeField] public Transform uiChaValRoot;
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }
            DontDestroyOnLoad(this.gameObject);
            InitUIRoots();
            InitStack();
        }
        private void InitUIRoots()
        {
            for(int i = 0; i < m_layerRoots.Count; i++)
            {
                LayerRoot root = m_layerRoots[i];
                m_roots.Add(root.layer, root.root);
            }
        }
        private void InitStack()
        {
            m_layerStack[UILayer.Static] = new Stack<UIWindow>();
            m_layerStack[UILayer.Window] = new Stack<UIWindow>();
            m_layerStack[UILayer.ToolTip] = new Stack<UIWindow>();
            m_layerStack[UILayer.MessageBox] = new Stack<UIWindow>();
            m_layerStack[UILayer.HUD] = new Stack<UIWindow>();
        }
        private void Update()
        {
            if (m_pendingUIs.Count == 0) return;

            double now = Time.unscaledTimeAsDouble;
            m_deadUIs.Clear();

            foreach (var entry in m_pendingUIs)
            {
                if (entry.Value <= now)
                    m_deadUIs.Add(entry.Key);
            }

            foreach (string id in m_deadUIs)
            {
                m_pendingUIs.Remove(id);

                if (!m_UI.TryGetValue(id, out var ui)) continue;

                m_UI.Remove(id);
                if (ui != null)
                    Destroy(ui.gameObject);
            }
        }
        /// <summary>
        /// 开启静态UI
        /// </summary>
        /// <param name="tag">场景标签</param>
        public void SetSceneStaticUI(string tag)
        {
            foreach(var data in m_sceneStaticUIs)
            {
                string key = data.tag;
                GameObject obj = data.root;

                obj.SetActive(key == tag); 
            }
        }
        /// <summary>
        /// 获取指定UI窗口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool GetUIWindow(string id,out UIWindow ui)
        {
            return m_UI.TryGetValue(id,out ui);
        }
        /// <summary>
        /// 生成UI预制体
        /// </summary>
        /// <param name="ui_id"></param>
        private void GenerateUIPrefab(string ui_id, Func<UIWindow, bool> callback = null)
        {
            UIWindow ui = null;
            if(m_UI.TryGetValue(ui_id, out ui))
            {
                if (!m_pendingUIs.Remove(ui_id)) return;

                ui.OnOpen();
                if (callback != null && !callback(ui))
                {
                    HideUIPrefab(ui);
                    return;
                }

                PushWindow(ui);
                return;
            }
            else
            {
                if (m_loadingUIs.ContainsKey(ui_id))
                {
                    m_loadingUIs[ui_id] = (true, callback);
                    return;
                }

                m_loadingUIs.Add(ui_id, (true, callback));

                StartCoroutine(MyResourceLoader.LoadResources<UIWindow>(UIPath + ui_id, (uiPrefab) =>
                {
                    var (shouldShow, callback) = m_loadingUIs[ui_id];
                    m_loadingUIs.Remove(ui_id);

                    if (!shouldShow || uiPrefab == null)
                        return;

                    UIWindow ui = GameObject.Instantiate(uiPrefab, m_roots[uiPrefab.layer], false);
                    ui.Init();

                    ui.OnOpen();

                    if (callback != null && !callback(ui))
                    {
                        ui.OnClose();
                        Destroy(ui.gameObject);
                        return;
                    }
                    PushWindow(ui);
                    m_UI.Add(ui_id, ui);
                }));
            }
        }
        /// <summary>
        /// 隐藏UI预制体
        /// </summary>
        private void HideUIPrefab(string ui_id)
        {
            if (m_loadingUIs.ContainsKey(ui_id))
            {
                m_loadingUIs[ui_id] = (false, null);
                return;
            }

            if (!m_UI.TryGetValue(ui_id, out var ui))
            {
                return;
            }
            HideUIPrefab(ui);
        }
        
        private void HideUIPrefab(UIWindow ui)
        {
            if (ui == null || m_pendingUIs.ContainsKey(ui.id)) return;

            m_pendingUIs.Add(ui.id, Time.unscaledTimeAsDouble + Mathf.Max(0f, ui.sleepTime));

            var stack = m_layerStack[ui.layer];

            bool wasTop = stack.Count > 0 && stack.Peek() == ui;
            if (wasTop)
            {
                stack.Pop();
            }
            else
            {
                var windows = stack.ToArray();
                stack.Clear();

                for (int i = windows.Length - 1; i >= 0; i--)
                {
                    if (windows[i] != null && windows[i] != ui)
                        stack.Push(windows[i]);
                }
            }
            bool controlsInteraction = ui.layer == UILayer.Window;

            if (controlsInteraction)
                ui.DeAcviveWindow();

            ui.OnClose();

            if (controlsInteraction && wasTop && stack.Count > 0 && stack.Peek() != null)
            {
                stack.Peek().ActiveWindow();
            }
        }
        private void PushWindow(UIWindow ui)
        {
            var stack = m_layerStack[ui.layer];

            if (ui.layer == UILayer.Window)
            {
                if (stack.Count > 0 && stack.Peek() != null)
                    stack.Peek().DeAcviveWindow();

                ui.transform.SetAsLastSibling();
                ui.ActiveWindow();
            }

            stack.Push(ui);
        }
        /// <summary>
        /// 生成UI
        /// </summary>
        public void ShowUI(string ui_id)
        {
            GenerateUIPrefab(ui_id);
        }
        public void ShowUI<T>(string id, Action<T> bind) where T : UIWindow
        {
            GenerateUIPrefab(id, window =>
            {
                if (window is T typedWindow)
                {
                    bind(typedWindow);
                    return true;
                }

                Debug.LogError($"UI {id} 的根组件应为 {typeof(T).Name}");
                return false;
            });
        }
        /// <summary>
        ///  关闭UI
        /// </summary>
        public void CloseUI(string ui_id)
        {
            HideUIPrefab(ui_id);
        }
        /// <summary>
        /// 关闭全部活跃UI，目前用于切换场景后重置UI
        /// </summary>
        public void CloseAllUI()
        {
            // 切场景时，旧的加载请求完成后跳过显示。
            foreach (var id in new List<string>(m_loadingUIs.Keys))
                m_loadingUIs[id] = (false, null);


            foreach (var stack in m_layerStack.Values)
            {
                while (stack.Count > 0)
                {
                    var ui = stack.Peek();
                    if (ui == null || !ui.gameObject.activeSelf)
                    {
                        stack.Pop();
                        continue;
                    }

                    HideUIPrefab(ui);
                }
            }
        }
    }
}
