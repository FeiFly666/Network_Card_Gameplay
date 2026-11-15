using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using Common;
using Common.Data;
using Common.Model;
using Common.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Service
{
    public class BattleService : Singleton<BattleService>
    {
        private Dictionary<int, Action<bool>> m_pendings = new Dictionary<int, Action<bool>>();
        private int currentAvailableActionID = 0;

        private Message battleActionMessage;
        public void Init()
        {
            InitBattleActionMessage();
            MessageDispatcher.Instance.Subscribe<BattleEnter>(OnRecieveEnterBattle);
            MessageDispatcher.Instance.Subscribe<UserEnterBattleResponse>(OnRecieveEnterResponse);
            MessageDispatcher.Instance.Subscribe<UserLeaveBattleResponse>(OnRecieveExitResponse);
            MessageDispatcher.Instance.Subscribe<BattleRoundStartBroadcast>(OnNewRoundStart);
            MessageDispatcher.Instance.Subscribe<BattleStateSync>(OnBattleInfoSync);
            MessageDispatcher.Instance.Subscribe<BattleEventBroadcast>(OnBattleEventRecieve);
            MessageDispatcher.Instance.Subscribe<BattleActionResponse>(OnRecieveBattleEventRespose);
            MessageDispatcher.Instance.Subscribe<BattleTimedoutResponse>(OnBattleRoomDisbanded);
            MessageDispatcher.Instance.Subscribe<BattleGameOverResponse>(OnBattleEnd);
            MessageDispatcher.Instance.Subscribe<BattleReconnectSnapshot>(OnReciceveBattleSnapShot);
        }
        public void Reset()
        {
            //currentAvailableActionID = 0;
            m_pendings.Clear();
        }
        private void InitBattleActionMessage()
        {
            battleActionMessage = new Message();
            battleActionMessage.Request = new RequestMessage();
            battleActionMessage.Request.battleAction = new BattleActionRequest();
        }
        private void OnRecieveEnterBattle(ISession session, BattleEnter message)
        {
            if (message == null) return;
            if (message.Success)
            {
                MySceneManager.Instance.EnterScene("BattleScene");
            }
        }
        public void SendEnterRequest()
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.userEnterBattle = new UserEnterBattleRequest();

            NetClient.Instance.Send(message);
        }
        private void OnRecieveEnterResponse(ISession session, UserEnterBattleResponse response)
        {

        }
        public void SendExitBattleRequest()
        {
            Message message = new Message();
            message.Request = new RequestMessage();
            message.Request.userLeaveBattle = new UserLeaveBattleRequest();

            NetClient.Instance.Send(message);
        }
        private void OnRecieveExitResponse(ISession sesion, UserLeaveBattleResponse response)
        {
            if (response == null) return;
            MySceneManager.Instance.EnterScene("MainScene");
        }
        private void OnNewRoundStart(ISession session, BattleRoundStartBroadcast message)
        {
            if (message == null) return;

            BattleManager.Instance.SetRoundInfo(message);
        }

        private void OnBattleInfoSync(ISession session, BattleStateSync message)
        {
            if (message == null) return;
            BattleManager.Instance.SetRoundInfo(message);
        }
        public void SendStateSyncBack()
        {
            Message back = new Message();
            back.Request = new RequestMessage();
            back.Request.Sync = new BattleStateSyncReq();

            NetClient.Instance.Send(back);
        }
        public bool SendDropCard(Card card, Action<bool> callback)
        {
            BattleAction action = new BattleAction();

            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeDropCard;

            action.dropCardInfo = new DropCardInfo();
            action.dropCardInfo.Id = card.ID;
            action.dropCardInfo.dataID = card.Data.DataID;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);

            return true;
        }
        public bool SendSpawnCharacter(int cardID, string targetID, Action<bool> callback)
        {
            BattleAction action = new BattleAction();

            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypePlaceCharacter;

            action.Source = new ActionTarget();
            action.Source.Id = cardID;
            action.Target = new ActionTarget();
            action.Target.dataID = targetID;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);

            return true;
        }

        public bool SendCharacterAttributeChangeRequest(int cardID, int characterID, Action<bool> callback)
        {
            BattleAction action = new BattleAction();
            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeChangeAttribute;

            action.Source = new ActionTarget();
            action.Source.Id = cardID;

            action.Target = new ActionTarget();
            action.Target.Id = characterID;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);

            return true;
        }
        public void SendRemoveCharacter(int characterID, Action<bool> callback)
        {
            BattleAction action = new BattleAction();

            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeCharacterRemove;

            action.Source = new ActionTarget();
            action.Source.Id = characterID;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);
        }
        public void SendCharacterDefend(int characterID, Action<bool> callback)
        {
            BattleAction action = new BattleAction();

            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeCharacterAction;

            action.Source = new ActionTarget();
            action.Source.Id = characterID;
            action.chaActionType = CharacterActionType.ChaActionTypeDefend;

            action.Target = new ActionTarget();
            action.Target.Id = characterID;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);

        }
        public void SendCharacterAttack(int attackID, int targetID, Action<bool> callback)
        {
            BattleAction action = new BattleAction();

            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeCharacterAction;

            action.Source = new ActionTarget();
            action.Source.Id = attackID;
            action.chaActionType = CharacterActionType.ChaActionTypeAttack;

            action.Target = new ActionTarget();
            action.Target.Id = targetID;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);
        }
        public void SendCharacterSkill(int attackID, Action<bool> callback, int targetID = -1)
        {
            BattleAction action = new BattleAction();

            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeCharacterAction;

            action.Source = new ActionTarget();
            action.Source.Id = attackID;
            action.chaActionType = CharacterActionType.ChaActionTypeSkill;

            if(targetID != -1)
            {
                action.Target = new ActionTarget();
                action.Target.Id = targetID;
            }

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);
            battleActionMessage.Request.battleAction = new BattleActionRequest();

            m_pendings.Add(action.actionID, callback);
        }
        
        public void SendCharacterCancleAction(int characterID, Action<bool> callback)
        {
            BattleAction action = new BattleAction();
            action.actionID = currentAvailableActionID++;
            action.Type = BattleActionType.TypeCharacterAction;

            action.Source = new ActionTarget();
            action.Source.Id = characterID;

            action.Target = new ActionTarget();
            action.Target.Id = characterID;

            action.chaActionType = CharacterActionType.ChaActionTypeNone;

            battleActionMessage.Request.battleAction.Action = action;

            NetClient.Instance.Send(battleActionMessage);

            m_pendings.Add(action.actionID, callback);


        }
        private void OnRecieveBattleEventRespose(ISession sesion, BattleActionResponse response)
        {
            int actionID = response.actionID;
            if (!m_pendings.TryGetValue(actionID, out var callback))
            {
                return;
            }
            if (!response.Success)
            {
                MessageBox.Show(UI.Common.MessageType.Warning, response.Msg);
            }
            m_pendings.Remove(actionID);
            callback?.Invoke(response.Success);
        }
        private void OnBattleEventRecieve(ISession session, BattleEventBroadcast broadcast)
        {
            if (broadcast == null) return;

            if (BattleManager.Instance.CurrentPhase == BattlePhase.PhaseAction)
            {
                BattleManager.Instance.RegistBattlingEvent(broadcast.Events);
                return;
            }

            foreach (BattleEvent broadcastEvent in broadcast.Events)
            {
                BattleEventHandler.HandleBattleEvent(broadcastEvent);
            }
        }
        private void OnBattleRoomDisbanded(ISession session, BattleTimedoutResponse response)
        {
            if (response == null) return;

            MessageBox.Show(UI.Common.MessageType.Normal, "对方长时间未响应，游戏结束", () =>
            {
                MySceneManager.Instance.EnterScene("MainScene");
            });
        }

        private void OnBattleEnd(ISession session, BattleGameOverResponse response)
        {
            if (response == null) return;

            string msg = string.Empty;
            if (response.Result == GameResult.GameResultDraw) msg = "双方角色全部阵亡，平局";
            else if (response.Result == GameResult.GameResultWin) msg = "敌方角色全部阵亡，战斗胜利";
            else if (response.Result == GameResult.GameResultLose) msg = "我方角色全部阵亡，战斗失败";

            MessageBox.Show(UI.Common.MessageType.Normal, msg, () =>
            {
                MySceneManager.Instance.EnterScene("MainScene");
            });
        }
        private void OnReciceveBattleSnapShot(ISession session, BattleReconnectSnapshot snapshot)
        {
            if (snapshot == null) return;

            Reset();
            BattleManager.Instance.ApplyReconnectSnapshot(snapshot);
        }
    }
}
