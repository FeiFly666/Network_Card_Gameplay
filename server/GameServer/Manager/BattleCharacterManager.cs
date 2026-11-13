using Common.Data;
using Common.Model;
using GameServer.Model;
using GameServer.Model.Battle;
using GameServer.Utils;
using NetMessage;
using System;


namespace GameServer.Manager
{
    public class BattleCharacterManager
    {
        private BattleRoom room;
        private Dictionary<int,BattleCharacter> m_characters = new Dictionary<int, BattleCharacter>();
        public Dictionary<int,string> characterOwner = new Dictionary<int, string>();

        private Dictionary<int, int> characterDefendRound = new Dictionary<int, int>();
        private Dictionary<int, int> characterCantAttackStartRound = new Dictionary<int, int>();
        private Dictionary<int, int> characterCantSkillStartRound = new Dictionary<int, int>();
        public BattleCharacterManager(BattleRoom battleRoom)
        {
            this.room = battleRoom;
        }

        public BattleEvent AddCharacter(Player player, string characterData, bool isMain = false)
        {
            Character character = BattleUtil.CreateNewCharacter(characterData);
            if(character == null)
            {
                return null;
            }

            character.ID = room.currentFreeID++;
            character.CurMaxHP = character.data.HP;
            character.CurHP = character.data.HP;
            character.CurMP = 0;
            character.CurATK = character.data.ATK;
            character.CurDEF = character.data.DEF;
            character.CurSpeed = character.data.Speed;
            character.isMain = isMain;
            if(isMain)
            {
                character.CurMaxHP *= 2;
                character.CurHP *= 2;
            }

            player.battlePlayer.characters[character.ID] = character;

            characterOwner[character.ID] = player.id;

            BattleCharacter battleCha = new BattleCharacter();
            battleCha.InitBattleCharacter(player, character, room.round);

            m_characters[character.ID] = battleCha;

            BattleEvent e = new BattleEvent();
            e.Type = BattleEvent.EventType.EventSpawnCharacter;
            e.actionID = -1;
            e.dataID = characterData;
            e.Id = character.ID;
            e.paramS = player.id;

            return e;
        }
        public void SpawnCharacter(string playerID,BattleAction action)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.battleEvent = new BattleEventBroadcast();

            string characterData = action.Target.dataID;

            BattleEvent e = AddCharacter(room.players[playerID], characterData);

            e.actionID = action.actionID;

            message.Response.battleEvent.Events.Add(e);

            room.messageSystem.BroadCast(message);

        }
        public void SpawnMainCharacter(Player player, string characterData)
        {
            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.battleEvent = new BattleEventBroadcast();

            BattleEvent e = AddCharacter(player, characterData, true);

            player.battlePlayer.mainCharacterID = e.Id;

            e.actionID = -1;
            e.Param = 1;

            message.Response.battleEvent.Events.Add(e);

            room.messageSystem.BroadCast(message);
        }

        public void InitMainCharacter(Player player)
        {
            SpawnMainCharacter(player, player.mainCharcterID);
        }

        public void RemoveCharacter(string playerID,int characterID, int actionID, bool isDead)
        {
            m_characters.Remove(characterID);
            characterOwner.Remove(characterID);

            room.players[playerID].battlePlayer.characters.Remove(characterID);

            Message message = new Message();
            message.Response = new ResponseMessage();
            message.Response.battleEvent = new BattleEventBroadcast();

            BattleEvent e = new BattleEvent();

            e.actionID = actionID;

            e.Type = BattleEvent.EventType.EventCharacterDie;
            e.Id = characterID;
            e.Param = isDead ? 1 : 0;//1表示角色死亡，0表示角色是删除
            e.paramS = playerID;

            message.Response.battleEvent.Events.Add(e);

            room.messageSystem.BroadCast(message);

            if(room.pendingBattleActions.ContainsKey((playerID, characterID)))
            {
                room.pendingBattleActions.Remove((playerID, characterID));
            }
        }
        public void RemoveCharacter(string playerID, int characterID)
        {
            m_characters.Remove(characterID);
            characterOwner.Remove(characterID);
            room.players[playerID].battlePlayer.characters.Remove(characterID);
        }
        public bool GetBattleCharacter(int charactetID, out BattleCharacter character)
        {
            return m_characters.TryGetValue(charactetID, out character);
        }
        public bool GetCharacterOwner(int characterID, out string player)
        {
            return characterOwner.TryGetValue(characterID, out player);
        }
        public bool GetCharacter(int characterID, out Character? character)
        {
            bool res = m_characters.TryGetValue(characterID, out var battleCha);

            character = res ? battleCha.character : null;

            return res;
        }
        public bool GetCharacterData(int characterID, out CharacterData data)
        {
            data = null;
            if(m_characters.TryGetValue(characterID, out var battleCharacter))
            {
                data = battleCharacter.character.data;
                return true;
            }
            return false;
        }

        public IReadOnlyList<Character> GetPlayerCharacters(int characterID)
        {
            if(!GetCharacterOwner(characterID,out var playerID))
            {
                return null;
            }
            if(!room.players.TryGetValue(playerID, out var player))
            {
                return null;
            }

            var allies = new List<Character>();
            foreach(var ally in player.battlePlayer.characters.Values)
            {
                allies.Add(ally);
            }
            return allies;
        }

        public bool IsCharacterExist(int characterID)
        {
            return m_characters.ContainsKey(characterID);
        }
        //在每回合结尾调用
        public bool CanCharacterDefend(int characterID)
        {
            if(characterDefendRound.TryGetValue(characterID,out var lastDefendRound))
            {
                return room.round - lastDefendRound > 1;
            }
            else
            {
                return true;
            }
        }
        public bool CanCharacterAttack(int characterID)
        {
            if(characterCantAttackStartRound.TryGetValue(characterID, out var startRound))
            {
                return room.round - startRound >= 1;
            }
            else
            {
                return true;
            }
        }
        public bool CanCharacterUseSkill(int characterID)
        {
            if (!m_characters.TryGetValue(characterID, out var battleCharacter))
            {
                return false;
            }

            string skillID = battleCharacter.character.data.SkillID;

            if (!DataManager.Instance.m_Skills.TryGetValue(skillID, out var skill))
            {
                return false;
            }

            bool isMPEnough = battleCharacter.character.CurMP >= skill.Cost;
            
            if (characterCantSkillStartRound.TryGetValue(characterID, out var startRound))
            {
                return room.round - startRound >= 1 && isMPEnough;
            }

            return isMPEnough;
           
        }
        public void CharacterDefend(int characterID)
        {
            characterDefendRound[characterID] = room.round;
        }
        public void CharacterCantAttack(int characterID)
        {
            characterCantAttackStartRound[characterID] = room.round;
        }
        public void CharacterCantSkill(int characterID)
        {
            characterCantSkillStartRound[characterID] = room.round;
        }

    }
}
