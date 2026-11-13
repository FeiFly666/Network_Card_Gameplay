using Assets.Scripts.Manager;
using Assets.Scripts.Service;
using NetMessage;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Friend
{
    public enum CurrentPanel
    {
        Friend,
        Request,
        Search
    }
    public class UIFriendPanel : UIWindow
    {
        [Header("预制体")]
        [SerializeField] private UIFriendInfoItem friendInfoItemPrefab;
        [SerializeField] private UIFriendRequestInfoItem friendRequestInfoItemPrefab;
        [SerializeField] private UISearchPlayerInfoItem searchPlayerInfoItemPrefab;

        [Header("UI配置")]
        [SerializeField] private CurrentPanel currentPanel = CurrentPanel.Search;
        [SerializeField] private Color buttonUnSetColor;
        [SerializeField] private Color buttonSetColor;
        [SerializeField] private Image[] panelButtonImgs;
        [SerializeField] private Transform[] panelContents;
        [SerializeField] private GameObject[] panels;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField] private Button searchButton;

        private Dictionary<string, UIFriendInfoItem> friendInfoItems = new Dictionary<string, UIFriendInfoItem>();
        private Dictionary<long, UIFriendRequestInfoItem> requestInfoItems = new Dictionary<long, UIFriendRequestInfoItem>();
        private Dictionary<string, UISearchPlayerInfoItem> searchInfoItems = new Dictionary<string, UISearchPlayerInfoItem>();

        private readonly List<UIFriendInfoItem> sortBuffer = new List<UIFriendInfoItem>();
        private void Awake()
        {
            searchButton.onClick.RemoveAllListeners();
            searchButton.onClick.AddListener(SendSearchRequest);
        }

        private void OnEnable()
        {
            FriendManager.Instance.OnFriendChange += ChangeFriend;
            FriendManager.Instance.OnRemoveFriend += RemoveFriend;
            FriendManager.Instance.OnRequestChange += ChangeRequest;
            FriendManager.Instance.OnRemoveRequest += RemoveRequest;
            FriendManager.Instance.OnUpdateSearchedInfo += UpdateSearchInfo;

            FriendService.Instance.OnSearchFinished += ShowSearchInfo;
            

            FriendManager.Instance.CallActionForCurrentData();
            currentPanel = CurrentPanel.Search;
            ChangePanel(0);
        }
        public void ChangePanel(int num)
        {
            CurrentPanel newPanel = (CurrentPanel)num;
            if (newPanel == currentPanel) { return; }
            currentPanel = newPanel;

            for(int i = 0; i< panels.Length; i++)
            {
                panels[i].SetActive(i == num);
                panelButtonImgs[i].color = i == num ? buttonUnSetColor : buttonSetColor;
            }
        }
        private void SendSearchRequest()
        {
            FriendService.Instance.SendSearchRequest(searchInput.text);
        }
        private void ChangeFriend(FriendInfo info)
        {
            Debug.Log("生成/修改好友信息");
            if (!friendInfoItems.TryGetValue(info.Id, out var item))
            {
                item = GameObject.Instantiate(friendInfoItemPrefab, panelContents[(int)CurrentPanel.Friend]);
                friendInfoItems.Add(info.Id, item);
            }

            item.SetInfo(info.Id, info.Nickname, info.Online, info.mainCharacterID);
            RefreshFriendOrder();
        }
        private void RemoveFriend(string id)
        {
            if(friendInfoItems.TryGetValue(id, out var item))
            {
                Destroy(item.gameObject);
                friendInfoItems.Remove(id);
            }
            RefreshFriendOrder();
        }
        private void RefreshFriendOrder()
        {
            sortBuffer.Clear();
            foreach(var item in friendInfoItems)
            {
                sortBuffer.Add(item.Value);
            }

            sortBuffer.Sort(CompareFriendItem);

            for (int i = 0; i < sortBuffer.Count; i++)
            {
                sortBuffer[i].transform.SetSiblingIndex(i);
            }
        }
        private int CompareFriendItem(UIFriendInfoItem a, UIFriendInfoItem b)
        {
            if (a.IsOnline != b.IsOnline)
            {
                return a.IsOnline ? -1 : 1;
            }
            int lengthCompare = a.UserID.Length.CompareTo(b.UserID.Length);
            if (lengthCompare != 0)
            {
                return lengthCompare;
            }
            return string.CompareOrdinal(a.UserID, b.UserID);
        }
        private void ChangeRequest(FriendRequestInfo info)
        {
            Debug.Log("生成/修改好友请求");
            if(!requestInfoItems.TryGetValue(info.Id,out var item))
            {
                item = Instantiate(friendRequestInfoItemPrefab, panelContents[(int)CurrentPanel.Request]);
                requestInfoItems.Add(info.Id, item);
            }
            item.SetInfo(info.Id, info.requesterID, info.requesterNickname, info.createTime, info.requestMainCharacterID);
        }
        private void RemoveRequest(long id)
        {
            if(requestInfoItems.TryGetValue(id,out var item))
            {
                Destroy(item.gameObject);
                requestInfoItems.Remove(id);
            }
        }
        private void ShowSearchInfo(List<PlayerSearchInfo> infos)
        {
            //Debug.Log($"生成玩家查询结果");
            ClearSearchInfos();
            foreach (var info in infos)
            {
                if (info.Relation == FriendRelationState.FriendRelationSelf) continue;
                UISearchPlayerInfoItem item = Instantiate(searchPlayerInfoItemPrefab, panelContents[(int)CurrentPanel.Search]);
                searchInfoItems.Add(info.userID, item);

                item.SetInfo(info);
            }
        }
        private void UpdateSearchInfo(PlayerSearchInfo info)
        {
            if(searchInfoItems.TryGetValue(info.userID, out var item))
            {
                item.SetInfo(info);
            }
        }
        private void ClearSearchInfos()
        {
            foreach (var item in searchInfoItems)
            {
                Destroy(item.Value.gameObject);
            }
            searchInfoItems.Clear();
        }
        private void OnDisable()
        {
            FriendManager.Instance.OnFriendChange -= ChangeFriend;
            FriendManager.Instance.OnRemoveFriend -= RemoveFriend;
            FriendManager.Instance.OnRequestChange -= ChangeRequest;
            FriendManager.Instance.OnRemoveRequest -= RemoveRequest;
            FriendManager.Instance.OnUpdateSearchedInfo -= UpdateSearchInfo;

            FriendService.Instance.OnSearchFinished -= ShowSearchInfo;

            searchInput.text = string.Empty;
            ClearSearchInfos();
        }
    }
}