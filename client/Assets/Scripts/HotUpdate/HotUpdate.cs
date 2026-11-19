using Assets.Scripts.Manager;
using Assets.Scripts.UI.Common;
using Assets.Scripts.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace Assets.Scripts.HotUpdate
{
    public class FileListStruct
    {
        public int schemaVersion;
        public int resourceVersion;

        public string platform;

        public Dictionary<string, AssetBundleInfo> bundles;
        public Dictionary<string, string> assets;

    }

    public class AssetBundleInfo
    {
        public long size;
        public string sha256;
        public string[] dependencies;
    }

    internal class DownloadFileInfo
    {
        public string url;
        public string fileName;
        public DownloadHandler fileData;
        public long downloadedBytes;

        public DownloadFileInfo(string url, string fileName)
        {
            this.url = url;
            this.fileName = fileName;
            //this.fileData = fileData;
        }
    }
    public class HotUpdate : MonoBehaviour
    {
        [SerializeField] private int m_maxRetryTime = 5;
        [SerializeField, Min(1)] private int m_maxDownloadCount = 2;
        [SerializeField] private UIProgressBox originProgressBox;
        [SerializeField] private UIProgressBox updateProgressBox;

        private long m_needDownloadBytes;
        private readonly List<DownloadFileInfo> m_downloadFiles = new List<DownloadFileInfo>();

        private byte[] m_downloadData;

        private int m_downloadNum;
        private int m_downloadFailNum;
        private int m_needDownloadNum;
        private int m_activeDownloadCount;

        private string TargetFilePath = "";

        private FileListStruct m_fileList;

        private void Awake()
        {
            
        }
        IEnumerator Start()
        {
            TargetFilePath = Path.Combine(PathUtil.LocalFilePath, LoadingService.Instance.platform);

            if (AssetBundleManager.instance.assetLoadMode == AssetLoadMode.AssetBundle)
            {
                bool isFirstEnter = IsFirstEnterGame();
                if (isFirstEnter)
                {
                    originProgressBox?.ShowDescription();
                    originProgressBox?.SetDescription("正在释放资源，此过程不消耗流量");
                    ReleaseResources();
                    yield break;
                }

                OnFileReleasedFinish();
                yield break;
            }
            yield return null;
            EnterGame();
        }
        IEnumerator DownloadFile(DownloadFileInfo info, Action<DownloadFileInfo> Complete, Action Fail = null)
        {
            int retryCount = 1;

            if (!info.url.Contains("://"))
                info.url = new Uri(info.url).AbsoluteUri;

            while (retryCount <= m_maxRetryTime)
            {
                while(m_activeDownloadCount >= m_maxDownloadCount)
                {
                    yield return null;
                }
                UnityWebRequest request = UnityWebRequest.Get(info.url);

                info.downloadedBytes = 0;
                m_activeDownloadCount++;
                //request.timeout = 60;
                request.SendWebRequest();

                long previousDownload = 0;
                bool hasTimed = false;
                float timer = 0f;

                while (!request.isDone)
                {
                    info.downloadedBytes = (long)request.downloadedBytes;

                    if(info.downloadedBytes == previousDownload)
                    {
                        if(!hasTimed)
                        {
                            hasTimed = true;
                            timer = Time.realtimeSinceStartup;
                        }
                    }
                    else
                    {
                        hasTimed = false;
                    }

                    if (Time.realtimeSinceStartup - timer > 10f && hasTimed)
                    {
                        //断开
                        Debug.LogWarning($"下载停滞10秒，准备重试：{info.url}");

                        request.Abort();

                        while (!request.isDone)
                            yield return null;

                        break;
                    }

                    previousDownload = info.downloadedBytes;
                    yield return null;
                }

                info.downloadedBytes = (long)request.downloadedBytes;
                m_activeDownloadCount--;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    info.fileData = request.downloadHandler;
                    Complete?.Invoke(info);
                    request.Dispose();
                    yield break;
                }

                info.downloadedBytes = 0;
                //Debug.LogWarning($"下载失败 {info.url} Retry:{retryCount}/{m_maxRetryTime}");

                Debug.LogWarning($"下载失败 {info.url} Retry:{retryCount}/{m_maxRetryTime} " + 
                        $"Result:{request.result} HTTP:{request.responseCode} " +
                        $"Error:{request.error} Bytes:{request.downloadedBytes}");

                request.Dispose();

                if(retryCount == m_maxRetryTime)
                {
                    Fail?.Invoke();
                    yield break;
                }
                yield return new WaitForSecondsRealtime(2 * retryCount);

                retryCount++;
            }
        }
        private bool IsFirstEnterGame()
        {
            bool isFilelistExistInWriteablePath = File.Exists($"{PathUtil.LocalFilePath}/{LoadingService.Instance.platform}/{PathUtil.FileListName}");
            return !isFilelistExistInWriteablePath;
        }
        /// <summary>
        /// 释放内置配置清单
        /// </summary>
        private void ReleaseResources()
        {
            string url = $"{PathUtil.ReadOnlyFilePath}/{LoadingService.Instance.platform}/{PathUtil.FileListName}";
            var info = new DownloadFileInfo(url, null);

            StartCoroutine(DownloadFile(info, OnFilelistReleased, () =>
            {
                Debug.LogError("读取内置资源清单失败");
                originProgressBox?.SetDescription("内置资源加载失败，请退出重试");
            }));
        }
        /// <summary>
        /// 释放内置清单完毕
        /// </summary>
        /// <param name="info"></param>
        private void OnFilelistReleased(DownloadFileInfo info)
        {
            m_downloadData = info.fileData.data;

            string json = info.fileData.text.TrimStart('\uFEFF');
            m_fileList = JsonConvert.DeserializeObject<FileListStruct>(json);

            m_downloadNum = 0;
            m_downloadFailNum = 0;
            m_needDownloadNum = m_fileList.bundles.Count;

            foreach (var entry in m_fileList.bundles)
            {
                AssetBundleInfo expected = entry.Value;

                string fileName = $"{Path.GetFileNameWithoutExtension(entry.Key)}_{expected.sha256}.ab";
                string url = $"{PathUtil.ReadOnlyFilePath}/{LoadingService.Instance.platform}/bundles/{fileName}";

                var file = new DownloadFileInfo(url, fileName);
                StartCoroutine(DownloadFile(file, result => OnDownloadAssetBundleComplete(result, expected), () => FinishBundle(false)));
            }

            StartCoroutine(WaitDownloadFinish(OnFileReleasedFinish));
        }
        /// <summary>
        /// 所有内置文件释放完毕
        /// </summary>
        private void OnFileReleasedFinish()
        {
            if (m_downloadFailNum > 0)
            {
                Debug.LogError("内置资源释放失败");
                originProgressBox?.SetDescription("内置资源加载失败，请退出重试");
                return;
            }

            if (IsFirstEnterGame())
            {
                originProgressBox?.CloseDescription();

                string fileListPath = Path.Combine(TargetFilePath, PathUtil.FileListName);

                if (!PathUtil.WriteFile(fileListPath, m_downloadData))
                    return;
            }
            m_fileList = GetLocalFileList();
            if (m_fileList == null)
                return;

            DeleteOldFiles();
            AssetBundleManager.instance.InitAssetBundleInfos(m_fileList);
            MessageBox.Init(()=>ProgressBox.Init(DownloadFileList));
        }
        private List<string> CheckFileList(FileListStruct newFilelist)
        {
            if (newFilelist == null || newFilelist.schemaVersion != 1 || newFilelist.platform != LoadingService.Instance.platform || newFilelist.bundles == null)
            {
                //throw new InvalidDataException("清单格式或目标平台不匹配。");
                MessageBox.Show(MessageType.Error, "客户端版本过低，资源文件无法读取，请更新", ()=>
                {
                    Application.Quit();
                });
            }
            string bundlesDir = Path.Combine(TargetFilePath, "bundles");

            var localBundles = GetLocalFileList()?.bundles;

            var list = new List<string>();  

            foreach(var bundle in newFilelist.bundles)
            {
                string bundleName = bundle.Key;
                AssetBundleInfo bundleInfo = bundle.Value;

                if (!Regex.IsMatch(bundleName, @"\A[a-z0-9_]+\.ab\z") || bundleInfo == null || bundleInfo.size <= 0 || 
                    string.IsNullOrEmpty(bundleInfo.sha256) || !Regex.IsMatch(bundleInfo.sha256, @"\A[0-9a-f]{64}\z"))
                {
                    throw new InvalidDataException($"AB 清单条目无效：{bundleName}");
                }
                string stem = Path.GetFileNameWithoutExtension(bundleName);
                string fileName = $"{stem}_{bundleInfo.sha256}.ab";
                var localFile = new FileInfo(Path.Combine(bundlesDir, fileName));

                AssetBundleInfo oldInfo = null;
                localBundles?.TryGetValue(bundleName, out oldInfo);

                if(!IsBundleFileVaild(bundleInfo, oldInfo) || !localFile.Exists || localFile.Length != bundleInfo.size)
                {
                    list.Add(bundleName);
                }
            }
            return list;
        }
        private bool IsBundleFileVaild(AssetBundleInfo newInfo, AssetBundleInfo oldInfo)
        {
            bool hasLocal = (oldInfo != null);
            bool vertify = false;
            if(hasLocal)
            {
                vertify = newInfo.sha256 == oldInfo.sha256 && newInfo.size == oldInfo.size;
            }

            return hasLocal && vertify;
        }
        /// <summary>
        /// 获取本地文件清单
        /// </summary>
        private FileListStruct GetLocalFileList()
        {
            string path = Path.Combine(TargetFilePath, PathUtil.FileListName);
            if(!File.Exists(path))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                var local = JsonConvert.DeserializeObject<FileListStruct>(json);

                if (local == null || local.schemaVersion != 1 || local.platform != LoadingService.Instance.platform || local.bundles == null)
                {
                    return null;
                }

                return local;
            }
            catch(Exception e)
            {
                Debug.LogError($"读取本地文件清单失败:{e.Message}");
                return null;
            }
        }
        /// <summary>
        /// 下载服务器文件清单
        /// </summary>
        private void DownloadFileList()
        {
            string baseUrl = LoadingService.Instance.onlineHotUpdate ? PathUtil.ServerBundleDownloadPath : PathUtil.LocalBundleDownloadPath;

            string url = $"{baseUrl.TrimEnd('/')}/{LoadingService.Instance.platform}/{PathUtil.FileListName}";
            var info = new DownloadFileInfo(url, "FileList");

            updateProgressBox = ProgressBox.GetProgressBox();

            updateProgressBox.ShowDescription();
            updateProgressBox.SetDescription("正在检查资源更新");

            StartCoroutine(StartLoadFileList(info));
        }
        IEnumerator StartLoadFileList(DownloadFileInfo info)
        {
            yield return new WaitForSecondsRealtime(2f);
            
            yield return DownloadFile(info, OnDownloadFileListComplete, OnDownloadFileListFail);
        }
        /// <summary>
        /// 服务器清单下载完毕回调
        /// </summary>
        private void OnDownloadFileListComplete(DownloadFileInfo info)
        {
            Debug.Log($"{info.url}已下载完成");
            m_downloadData = info.fileData.data;
            try
            {
                string json = info.fileData.text.TrimStart('\uFEFF');
                var filelist =  JsonConvert.DeserializeObject<FileListStruct>(json);

                var pendingList = CheckFileList(filelist);
                m_fileList = filelist;

                m_downloadFailNum = 0;
                m_downloadNum = 0;

                m_needDownloadNum = pendingList.Count;
                m_downloadFiles.Clear();
                m_needDownloadBytes = pendingList.Sum(name => filelist.bundles[name].size);

                if (pendingList.Count > 0)
                {
                    updateProgressBox.SetDescription("检测到新资源，等待确认");

                    //有更新，配置需要重新加载防止错过热更新内容
                    DataManager.Instance.isInit = false;
                    //有更新，对象池清空防止对象池内对象未更新
                    PoolManager.Instance.ClearAll();
                    BattleCardManager.Instance?.Restart();

                    MessageBox.Show(MessageType.Warning, $"检测到新资源（{FormatSize(m_needDownloadBytes)}），是否进行更新？\n（点击取消退出游戏）", ()=>
                    {
                        StartCoroutine(WaitDownloadFinish());

                        updateProgressBox.ShowProgress();

                        foreach (var assetBundleName in pendingList)
                        {
                            //TODO:下载待下载文件
                            DownloadAssetBundle(assetBundleName, filelist.bundles[assetBundleName]);
                        }
                    }, ()=>
                    {
                        Application.Quit();
                    });
                }
                else
                {
                    OnAllFileDownloadFinish();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"检查资源清单失败 {info.url}：{ex.Message}");
                OnDownloadFileListFail();
            }
        }
        /// <summary>
        /// 文件下载失败
        /// </summary>
        private void OnDownloadFileListFail()
        {
            MessageBox.Show(UI.Common.MessageType.Error, "资源文件下载失败，请重试或退出", () =>
            {
                updateProgressBox?.CloseUI();
                DownloadFileList();
            },()=>
            {
                Application.Quit();
            });
        }
        /// <summary>
        /// 下载AB包
        /// </summary>
        /// <param name="bundleFileName">AB包名称</param>
        private void DownloadAssetBundle(string bundleName, AssetBundleInfo expected)
        {
            string bundleFileName =$"{Path.GetFileNameWithoutExtension(bundleName)}_{expected.sha256}.ab";

            string baseUrl = LoadingService.Instance.onlineHotUpdate ? PathUtil.ServerBundleDownloadPath : PathUtil.LocalBundleDownloadPath;
            string url = $"{baseUrl.TrimEnd('/')}/{LoadingService.Instance.platform}/bundles/{bundleFileName}";

            var info = new DownloadFileInfo(url, bundleFileName);
            m_downloadFiles.Add(info);

            StartCoroutine(DownloadFile(info, result=>
                OnDownloadAssetBundleComplete(result, expected),
                () => FinishBundle(false)));
        }
        /// <summary>
        /// AB包下载完成
        /// </summary>
        private void OnDownloadAssetBundleComplete(DownloadFileInfo info, AssetBundleInfo expected)
        {
            string targetPath = Path.Combine(TargetFilePath, "bundles", info.fileName);
            Debug.Log($"AssetBundle {info.fileName} 已下载完成");
            byte[] content = info.fileData.data;

            if(content == null || content.LongLength != expected.size)
            {
                Debug.LogError($"AB 大小校验失败：{info.fileName}");
                FinishBundle(false);
                return;
            }

            using(var sha256 = SHA256.Create())
            {
                string newHash = BitConverter.ToString(sha256.ComputeHash(content)).Replace("-","");
                if (!string.Equals(newHash, expected.sha256, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"AB SHA-256 校验失败：{info.fileName}");
                    FinishBundle(false);
                    return;
                }
            }

            FinishBundle(PathUtil.WriteFile(targetPath, content));

        }
        private void FinishBundle(bool success)
        {
            if(success)
            {
                m_downloadNum++;
            }
            else
            {
                m_downloadFailNum++;
            }
        }

        IEnumerator WaitDownloadFinish(Action onComplete = null)
        {
            while(m_needDownloadNum > m_downloadNum + m_downloadFailNum)
            {
                UpdateDownloadProgress();
                yield return null;
            }
            UpdateDownloadProgress();

            yield return new WaitForSecondsRealtime(1f);

            if (onComplete != null)
                onComplete();
            else
                OnAllFileDownloadFinish();
        }
        private void UpdateDownloadProgress()
        {
            long downloadedBytes = m_downloadFiles.Sum(file => file.downloadedBytes);

            updateProgressBox?.SetDescription( $"正在下载资源（{FormatSize(downloadedBytes)} / {FormatSize(m_needDownloadBytes)}）");

            updateProgressBox?.UpdateProgress(downloadedBytes, m_needDownloadBytes);
        }
        private void OnAllFileDownloadFinish()
        {
            if(m_downloadFailNum > 0)
            {
                //TODO:重新下载流程
                MessageBox.Show(UI.Common.MessageType.Error, "资源下载错误，请重试或退出游戏", () =>
                {
                    DownloadFileList();
                }, () =>
                {
                    Application.Quit();
                });
                return;
            }
            string fileListPath = Path.Combine(TargetFilePath, PathUtil.FileListName);

            if (!PathUtil.WriteFile(fileListPath, m_downloadData))
                return;

            updateProgressBox.SetDescription("资源已为最新版");

            updateProgressBox.CloseUI();
            updateProgressBox = null;

            AssetBundleManager.instance.InitAssetBundleInfos(GetLocalFileList());
            DeleteOldFiles();
            EnterGame();
        }
        private void DeleteOldFiles()
        {
            string bundlesDir = Path.Combine(TargetFilePath, "bundles");
            if (!Directory.Exists(bundlesDir))
                return;

            var keepNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in m_fileList.bundles)
            {
                string stem = Path.GetFileNameWithoutExtension(entry.Key);
                keepNames.Add($"{stem}_{entry.Value.sha256}.ab");
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(bundlesDir, "*.ab", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.LogWarning($"无法枚举本地 AB：{ex.Message}");
                return;
            }

            foreach (string path in files)
            {
                string name = Path.GetFileName(path);
                if (keepNames.Contains(name) ||
                    !Regex.IsMatch(name, @"\A[a-z0-9_]+_[0-9a-f]{64}\.ab\z"))
                {
                    continue;
                }

                try
                {
                    File.Delete(path);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                {
                    Debug.LogWarning($"清理旧 AB 失败 {name}：{ex.Message}");
                }
            }
        }

        private void EnterGame()
        {
            MessageBox.Init();
            InputFieldBox.Init();
            ProgressBox.Init(()=>
            {
                updateProgressBox = ProgressBox.GetProgressBox();
                updateProgressBox.ShowDescription();

                StartCoroutine(LoadData());
            });

        }
        IEnumerator LoadData()
        {
            DataManager.Instance.Init();
            updateProgressBox.SetDescription("正在加载配置文件");
            yield return new WaitForSecondsRealtime(1f);

            while (!DataManager.Instance.isInit)
            {
                yield return null;
            }
            updateProgressBox.SetDescription("加载完毕，即将进入游戏");
            yield return new WaitForSecondsRealtime(2f);

            MySceneManager.Instance.EnterScene("StartScene");
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:F2} KB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / (1024d * 1024):F2} MB";
            return $"{bytes / (1024d * 1024 * 1024):F2} GB";
        }
    }
}
