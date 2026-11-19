using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Setting
{
    public class UISettingPanel : UIWindow
    {
        [Header("分辨率下拉栏")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [Header("帧率下拉栏")]
        [SerializeField] private TMP_Dropdown fpsDropdown;
        [Header("按钮图片")]
        [SerializeField] private Sprite useSprite;
        [SerializeField] private Sprite unUseSprite;
        [Header("垂直同步按钮")]
        [SerializeField] private Button useVsync;
        [SerializeField] private Image useVsyncButtonImg;
        [Header("全屏按钮")]
        [SerializeField] private Button fullScreen;
        [SerializeField] private Image fullScreenButtonImg;
        [Header("应用按钮")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button logoutButton;
        private readonly List<Vector2Int> resolutionOptions = new List<Vector2Int>();
        private readonly int[] fpsCandidates = { 30, 60, 90, 120, 144, 165, 180, 240, 360 };
        private readonly List<int> fpsOptions = new List<int>();
        private Vector2Int pendingResolution = new Vector2Int(Screen.width, Screen.height);
        private int pendingFps = SettingManager.Instance.Settings.targetFps;
        private bool pendingSync = SettingManager.Instance.Settings.vSync;
        private bool pendingFullScreen = SettingManager.Instance.Settings.screenMode == FullScreenMode.ExclusiveFullScreen;

        private void Start()
        {
            InitResolutionDropdown();
            InitFpsDropdown();
            applyButton.onClick.RemoveAllListeners();
            applyButton.onClick.AddListener(ApplySettings);

            useVsync.onClick.RemoveAllListeners();
            useVsync.onClick.AddListener(ChangeUseVsync);

            fullScreen.onClick.RemoveAllListeners();
            fullScreen.onClick.AddListener(ChangeFullScreen);

            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(HandleLeaveButton);
        }
        public override void OnOpen()
        {
            base.OnOpen();
            ChangeVsyncButtonImg();
            ChangeFullScreenButtonImg();
        }
        private void InitResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();
            resolutionOptions.Clear();

            if (UnityEngine.Device.Application.isMobilePlatform)
            {
                resolutionDropdown.AddOptions(new List<string> { "自动（设备分辨率）" });
                resolutionDropdown.SetValueWithoutNotify(0);
                resolutionDropdown.RefreshShownValue();
                resolutionDropdown.interactable = false;

                pendingFullScreen = true;
                fullScreen.interactable = false;
                ChangeFullScreenButtonImg(); 
                return;
            }

            List<string> optionTexts = new List<string>();

            Resolution[] resolutions = Screen.resolutions;

            for (int i = 0; i < resolutions.Length; i++)
            {
                Resolution r = resolutions[i];
                if (!Is16By9(r.width, r.height))
                    continue;

                if (HasResolution(r.width, r.height))
                    continue;

                resolutionOptions.Add(new Vector2Int(r.width, r.height));
                optionTexts.Add($"{r.width} x {r.height}");
            }

            int currentIndex = 0;

            for (int i = 0; i < resolutionOptions.Count; i++)
            {
                if (resolutionOptions[i].x == Screen.width &&
                    resolutionOptions[i].y == Screen.height)
                {
                    currentIndex = i;
                    break;
                }
            }

            resolutionDropdown.AddOptions(optionTexts);
            resolutionDropdown.SetValueWithoutNotify(currentIndex);
            resolutionDropdown.RefreshShownValue();

            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        private bool HasResolution(int width, int height)
        {
            for (int i = 0; i < resolutionOptions.Count; i++)
            {
                if (resolutionOptions[i].x == width &&
                    resolutionOptions[i].y == height)
                {
                    return true;
                }
            }

            return false;
        }
        private bool Is16By9(int width, int height)
        {
            float ratio = (float)width / height;
            return Mathf.Abs(ratio - 16f / 9f) < 0.01f;
        }
        private void OnResolutionChanged(int index)
        {
            pendingResolution = resolutionOptions[index];
        }
        private void InitFpsDropdown()
        {
            fpsDropdown.ClearOptions();
            BuildFpsOptions();

            List<string> optionTexts = new List<string>();

            for (int i = 0; i< fpsOptions.Count; i++)
            {
                if (fpsOptions[i] != -1)
                    optionTexts.Add($"{fpsOptions[i]} FPS");
                else
                {
                    optionTexts.Add("无限制");
                }
            }
            int currentIndex = GetCurrentFpsIndex();
            fpsDropdown.AddOptions(optionTexts);
            fpsDropdown.SetValueWithoutNotify(currentIndex);
            fpsDropdown.RefreshShownValue();

            pendingFps = fpsOptions[currentIndex];

            fpsDropdown.onValueChanged.RemoveAllListeners();
            fpsDropdown.onValueChanged.AddListener(OnFpsChanged);
        }
        private void BuildFpsOptions()
        {
            fpsOptions.Clear();

            int maxRefreshRate = GetMaxRefreshRate();

            for (int i = 0; i < fpsCandidates.Length; i++)
            {
                if (fpsCandidates[i] <= maxRefreshRate + 1)
                {
                    fpsOptions.Add(fpsCandidates[i]);
                }
            }

            if (fpsOptions.Count == 0)
            {
                fpsOptions.Add(60);
            }

            if (!HasSimilarFpsOption(maxRefreshRate))
            {
                fpsOptions.Add(maxRefreshRate);
                fpsOptions.Sort();
            }

            fpsOptions.Add(-1);
        }
        private bool HasSimilarFpsOption(int fps)
        {
            for (int i = 0; i < fpsOptions.Count; i++)
            {
                if (Mathf.Abs(fpsOptions[i] - fps) <= 1)
                {
                    return true;
                }
            }

            return false;
        }
        private int GetCurrentFpsIndex()
        {
            int targetFps = SettingManager.Instance.Settings.targetFps;

            for (int i = 0; i < fpsOptions.Count; i++)
            {
                if (fpsOptions[i] == targetFps)
                {
                    return i;
                }
            }

            if (targetFps > 0)
            {
                int bestIndex = 0;
                for (int i = 0; i < fpsOptions.Count; i++)
                {
                    if (fpsOptions[i] == -1)
                    {
                        continue;
                    }

                    if (fpsOptions[i] <= targetFps)
                    {
                        bestIndex = i;
                    }
                }

                return bestIndex;
            }

            for (int i = 0; i < fpsOptions.Count; i++)
            {
                if (fpsOptions[i] == 60)
                {
                    return i;
                }
            }

            return 0;
        }
        private int GetMaxRefreshRate()
        {
            double maxRefreshRate = Screen.currentResolution.refreshRateRatio.value;
            Resolution[] resolutions = Screen.resolutions;

            for (int i = 0; i < resolutions.Length; i++)
            {
                double refreshRate = resolutions[i].refreshRateRatio.value;
                if (refreshRate > maxRefreshRate)
                    maxRefreshRate = refreshRate;
            }

            if (maxRefreshRate <= 0)
            {
                maxRefreshRate = 60;
            }

            return maxRefreshRate > 0 ? (int)System.Math.Round(maxRefreshRate) : 60;
        }
        private void HandleLeaveButton()
        {
            //已经登录则该按钮为退出登录并返回登陆页面
            //未登录为退出游戏
            var playerInfo = PlayerInfoManager.Instance;

            if(playerInfo.isLogin)
            {
                MessageBox.Show(Common.MessageType.Warning, "确定退出登录？", () =>
                {
                    NetClient.Instance.Close(true);
                    playerInfo.isLogin = false;
                    MySceneManager.Instance.EnterScene("HotUpdate");
                },
                () => { });
            }
            else
            {
                MessageBox.Show(Common.MessageType.Warning, "确定退出游戏？", () =>
                {
                    Application.Quit();
                },
                () => { });
            }
            
        }
        private void OnFpsChanged(int index)
        {
            pendingFps = fpsOptions[index];
        }
        private void ChangeUseVsync()
        {
            pendingSync = !pendingSync;
            ChangeVsyncButtonImg();
        }
        private void ChangeVsyncButtonImg()
        {
            if (pendingSync)
            {
                useVsyncButtonImg.overrideSprite = useSprite;
            }
            else
            {
                useVsyncButtonImg.overrideSprite = unUseSprite;
            }
        }
        private void ChangeFullScreen()
        {
            pendingFullScreen = ! pendingFullScreen;
            ChangeFullScreenButtonImg();

        }
        private void ChangeFullScreenButtonImg()
        {
            if (pendingFullScreen)
            {
                fullScreenButtonImg.overrideSprite = useSprite;
            }
            else
            {
                fullScreenButtonImg.overrideSprite = unUseSprite;
            }
        }
        private void ApplySettings()
        {
            var manager = SettingManager.Instance;
            var settings = manager.Settings;

            if (!UnityEngine.Device.Application.isMobilePlatform)
            {
                settings.width = pendingResolution.x;
                settings.height = pendingResolution.y;
                settings.screenMode = pendingFullScreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
            }

            settings.targetFps = pendingFps;
            settings.vSync = pendingSync;

            manager.Save();
            manager.Apply();
        }
    }
}
