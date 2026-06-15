using Common.Model;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model.Battle.States
{
    public class RoundEndState :IState
    {
        protected BattleRoom room;
        public BattleState stateType { get; }
        public float timeLeft { get; set; }

        private float maxLoadingTime = 2f;
        private Player losePlayer;

        private Message gameOverMessage;
        public RoundEndState(BattleRoom room)
        {
            stateType = BattleState.RoundEnd;
            this.room = room;

            gameOverMessage = new Message();
            gameOverMessage.Response = new ResponseMessage();
            gameOverMessage.Response.Gameover = new BattleGameOverResponse();
        }
        public void Enter()
        {
            timeLeft = maxLoadingTime;

            CheckGameOver();

            SyncCoolDowns();

        }
        private void CheckGameOver()
        {
            bool isAllLose = true;
            losePlayer = null;
            foreach(var player in room.players)
            {
                if(player.Value.battlePlayer.characters.Count <= 0)
                {
                    if (losePlayer == null)
                        losePlayer = player.Value;
                }
                else
                {
                    isAllLose = false;
                }
            }
            if (losePlayer == null) return;

            if(isAllLose)//平局
            {
                gameOverMessage.Response.Gameover.Result = GameResult.GameResultDraw;
                room.messageSystem.BroadCast(gameOverMessage);

                room.DisbandRoom();
                return;
            }
            else
            {
                //分别发送胜利失败消息
                foreach (var player in room.players)
                {
                    if (player.Value == losePlayer)
                    {
                        gameOverMessage.Response.Gameover.Result = GameResult.GameResultLose;
                        room.messageSystem.SendToPlayer(player.Value, gameOverMessage);
                    }
                    else
                    {
                        gameOverMessage.Response.Gameover.Result = GameResult.GameResultWin;
                        room.messageSystem.SendToPlayer(player.Value, gameOverMessage);
                    }
                }

                room.DisbandRoom();
            }
        }
        private void SyncCoolDowns()
        {
            SyncCharacterDefendCoolDown();
            SyncCharacterAttackCoolDown();
            SyncCharacterSkillCoolDown();
        }
        private void SyncCharacterDefendCoolDown()
        {
            Message sync = new Message();
            sync.Response = new ResponseMessage();
            sync.Response.battleEvent = new BattleEventBroadcast();
            foreach(var playerkv in room.players)
            {
                sync.Response.battleEvent.Events.Clear();
                if (room.loadedPlayers.Contains(playerkv.Key))
                {
                    foreach(var characterKv in playerkv.Value.battlePlayer.characters)
                    {
                        BattleEvent e = new BattleEvent();
                        e.Type = BattleEvent.EventType.EventCharacterActioncooldown;
                        e.actionCoolDownType = CharacterActionCoolDownType.ChaActionCooldownDefend;
                        e.Id = characterKv.Key;
                        e.Param = room.characterManager.CanCharacterDefend(characterKv.Key) ? 1 : 0;
                        e.actionID = -1;

                        sync.Response.battleEvent.Events.Add(e);
                    }

                    room.messageSystem.SendToPlayer(playerkv.Value, sync);
                }
            }
        }
        private void SyncCharacterAttackCoolDown()
        {
            Message sync = new Message();
            sync.Response = new ResponseMessage();
            sync.Response.battleEvent = new BattleEventBroadcast();
            foreach (var playerkv in room.players)
            {
                sync.Response.battleEvent.Events.Clear();
                if (room.loadedPlayers.Contains(playerkv.Key))
                {
                    foreach (var characterKv in playerkv.Value.battlePlayer.characters)
                    {
                        BattleEvent e = new BattleEvent();
                        e.Type = BattleEvent.EventType.EventCharacterActioncooldown;
                        e.actionCoolDownType = CharacterActionCoolDownType.ChaActionCooldownAttack;
                        e.Id = characterKv.Key;
                        e.Param = room.characterManager.CanCharacterAttack(characterKv.Key) ? 1 : 0;
                        e.actionID = -1;

                        sync.Response.battleEvent.Events.Add(e);
                    }

                    room.messageSystem.SendToPlayer(playerkv.Value, sync);
                }
            }
        }
        private void SyncCharacterSkillCoolDown()
        {
            Message sync = new Message();
            sync.Response = new ResponseMessage();
            sync.Response.battleEvent = new BattleEventBroadcast();

            foreach (var playerkv in room.players)
            {
                sync.Response.battleEvent.Events.Clear();
                if (room.loadedPlayers.Contains(playerkv.Key))
                {
                    foreach (var characterKv in playerkv.Value.battlePlayer.characters)
                    {
                        BattleEvent e = new BattleEvent();
                        e.Type = BattleEvent.EventType.EventCharacterActioncooldown;
                        e.actionCoolDownType = CharacterActionCoolDownType.ChaActionCooldownSkill;
                        e.Id = characterKv.Key;
                        e.Param = room.characterManager.CanCharacterUseSkill(characterKv.Key) ? 1 : 0;
                        e.actionID = -1;

                        sync.Response.battleEvent.Events.Add(e);
                    }

                    room.messageSystem.SendToPlayer(playerkv.Value, sync);
                }
            }
        }
        public void Update()
        {
            if (losePlayer != null) return;
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                room.NewRoundStart();
                return;
            }
        }
        public void Exit()
        {

        }
    }
}
