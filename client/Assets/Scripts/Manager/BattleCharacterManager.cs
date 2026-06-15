using Assets.Scripts.Model;
using Assets.Scripts.Service;
using Common;
using Common.Data;
using Common.Model;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Manager
{
    public class BattleCharacterManager:MonoBehaviour
    {
        public static BattleCharacterManager Instance;
        private Dictionary<int, BattleCharacter> m_playerCharacters = new Dictionary<int, BattleCharacter>();
        private Dictionary<int, BattleCharacter> m_enemyCharacters = new Dictionary<int, BattleCharacter>();

        private Dictionary<int, int>m_characterRank = new Dictionary<int, int>();
        private List<BattleCharacter> m_allCharacters = new List<BattleCharacter>();

        private HashSet<int> m_lockedCharacters = new HashSet<int>();

        public Action<BattleCharacter> OnCharacterClicked;
        public Action<BattleCharacter> OnCharacterAttributeChanged;

        public Action<BattleCharacter> OnPointerEnterCharacter;
        public Action<BattleCharacter> OnPointerExitCharacter;

        public Action<BattleCharacter> OnNewCharacterSpawn;
        public Action<BattleCharacter> OnCharacterRemoved;
        public Action<BattleCharacter> OnCharacterStateChanged;

        public bool CanAddPlayerCha => m_playerCharacters.Count < 4;

        public Vector2 playerCenter;
        public Vector2 enemyCenter;
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else if(Instance != this)
            {
                Destroy(this);
            }
        }
        public void Init()
        {
            m_playerCharacters.Clear();
            m_enemyCharacters.Clear();
            m_allCharacters.Clear();
            m_characterRank.Clear();
            m_lockedCharacters.Clear();
            
        }
        public void HandleCharacterClicked(BattleCharacter character)
        {
            if(BattleManager.Instance.CurrentPhase == BattlePhase.PhasePrepare)
                OnCharacterClicked?.Invoke(character);
        }
        public void HandleCharacterDefendCoolDownSync(int characterID, bool canDefend)
        {
            if(m_playerCharacters.TryGetValue(characterID, out BattleCharacter character))
            {
                character.canDefend = canDefend;
            }
        }
        public void HandleCharacterAttackCoolDownSync(int characterID, bool canAttack)
        {
            if (m_playerCharacters.TryGetValue(characterID, out BattleCharacter character))
            {
                character.canAttack = canAttack;
            }
        }
        public void HandleCharacterSkillCoolDownSync(int characterID, bool canUseSkill)
        {
            if (m_playerCharacters.TryGetValue(characterID, out BattleCharacter character))
            {
                character.canUseSkill = canUseSkill;
            }
        }
        public void HandleCharacterAttributeChange(int characterID, EffectType effect,int param)
        {
            if (TryGetCharacter(characterID, out BattleCharacter character))
            {
                character.ChangeValue(effect, param);

                if(effect == EffectType.Speed)
                {
                    CalculateRank();
                }

                OnCharacterAttributeChanged?.Invoke(character);
            }
        }
        public void HandlePointerEnterCharacter(BattleCharacter character)
        {
            OnPointerEnterCharacter?.Invoke(character);
        }
        public void HandlePointerExitCharacter(BattleCharacter character)
        {
            OnPointerExitCharacter?.Invoke(character);
        }
        public bool IsEnemyHasCharacters()
        {
            if(m_enemyCharacters.Count >1) return true;

            if (m_enemyCharacters.Count == 0) return false;

            BattleCharacter enemyFirstEnemy = null;

            foreach(var character in m_enemyCharacters)
            {
                enemyFirstEnemy = character.Value;
            }

            return !enemyFirstEnemy.character.isMain;
        }
        public bool TryGetPlayerCharacter(int characterId, out BattleCharacter character)
        {
            return m_playerCharacters.TryGetValue(characterId, out character);
        }
        public bool TryGetEnemyCharacter(int characterId, out BattleCharacter character)
        {
            return m_enemyCharacters.TryGetValue(characterId, out character);
        }

        public bool TryGetCharacter(int characterId, out BattleCharacter character)
        {
            if(m_playerCharacters.TryGetValue(characterId, out character))
            {
                return true;
            }

            return m_enemyCharacters.TryGetValue(characterId, out character);
        }

        public bool SendSpawnCharacterRequest(UICard requester, Action<bool> callback)
        {
            bool result = BattleService.Instance.SendSpawnCharacter(requester.card.ID, requester.card.Data.TargetID[0],callback);
            return result;
        }

        public void SendRemoveCharacterRequest(int characterID)
        {
            if (m_lockedCharacters.Contains(characterID)) return;
            BattleService.Instance.SendRemoveCharacter(characterID,(success)=>
            {
                OnRecieveCharacterRemoveResponse(characterID, success);
            });

            m_lockedCharacters.Add(characterID);
        }
        public void SendCharacterDefendRequest(BattleCharacter character)
        {
            if (m_lockedCharacters.Contains(character.character.ID)) return;
            BattleService.Instance.SendCharacterDefend(character.character.ID,(success)=>
            {
                OnRecieveCharacterDefendResponse(character, success);
            });

            m_lockedCharacters.Add(character.character.ID);
        }
        public void SendCharacterAttackRequest(BattleCharacter character, int targetID)
        {
            if (m_lockedCharacters.Contains(character.character.ID)) return;

            BattleService.Instance.SendCharacterAttack(character.character.ID, targetID, (success) =>
            {
                OnRecieveCharacterAttackResponse(character, targetID, success);
            });

            m_lockedCharacters.Add(character.character.ID);
        }
        public void SendCharacterSkillRequest(BattleCharacter character, int targetID = -1)
        {
            if (m_lockedCharacters.Contains(character.character.ID)) return;
            BattleService.Instance.SendCharacterSkill(character.character.ID, (success) =>
            {
                OnRecieveCharacterSkillResponse(character, targetID, success);
            }, targetID);
        }
        public void SendCharacterCancleAction(BattleCharacter character)
        {
            if (character.state == BattleCharacterState.None) return;

            if (m_lockedCharacters.Contains(character.character.ID)) return;

            BattleService.Instance.SendCharacterCancleAction(character.character.ID, (success) =>
            {
                OnRecieveCharacterCancleResponse(character, success);
            });


        }
        public void AddNewCharacter(Character character, BattleEvent e)
        {
            string characterName = character.data.DataID;

            string ownerID = e.paramS;

            bool isMain = e.Param == 1;

            if(!DataManager.Instance.characterPrefabs.TryGetValue(characterName, out var characterPrefab))
            {
                Debug.LogError($"character manager : can not find character prefab {character.data.DataID}");
                return;
            }


            BattleCharacter newCharacter = GameObject.Instantiate(characterPrefab);
            newCharacter.enabled = true;

            newCharacter.character = character;

            if (isMain)
            {
                newCharacter.character.isMain = true;
                newCharacter.AddComponent<MainCharacter>();
            }

            newCharacter.SetCharacterOwner(ownerID);
            newCharacter.canDefend = true;

            if (ownerID == PlayerInfoManager.Instance.userID)
                m_playerCharacters[character.ID] = newCharacter;
            else
                m_enemyCharacters[character.ID] = newCharacter;

            m_allCharacters.Add(newCharacter);

            CalculateRank();

            OnNewCharacterSpawn?.Invoke(newCharacter);
        }
        public void RemoveCharacter(int characterID, string ownerID, bool isDead)
        {
            Debug.Log($"character manager : remove character {characterID}");
            BattleCharacter cha = null;
            if(ownerID == PlayerInfoManager.Instance.userID)
            {
                if(!m_playerCharacters.TryGetValue(characterID, out cha))
                {
                    return;
                }
                m_playerCharacters.Remove(characterID);
            }
            else
            {
                if (!m_enemyCharacters.TryGetValue(characterID, out cha))
                {
                    return;
                }
                m_enemyCharacters.Remove(characterID);
            }
            m_allCharacters.Remove(cha);

            CalculateRank();

            OnCharacterRemoved?.Invoke(cha);

            cha.RemoveThisCharacter(isDead);

        }
        private void CalculateRank()
        {
            m_allCharacters.Sort((a,b)=>
            {
                int Aid = a.character.ID;
                int Bid = b.character.ID;

                int Aspeed = a.character.CurSpeed;
                int Bspeed = b.character.CurSpeed;

                if(Aspeed != Bspeed)
                {
                    return Bspeed.CompareTo(Aspeed);
                }

                return Aid.CompareTo(Bid);
            });
            for(int i = 0; i< m_allCharacters.Count; i++)
            {
                m_characterRank[m_allCharacters[i].character.ID] = i+1;
            }
        }
        public int GetCharacterRank(int characterID)
        {
            if(m_characterRank.ContainsKey(characterID))
                return m_characterRank[characterID];
            return -1;
        }
        private void OnRecieveCharacterDefendResponse(BattleCharacter character, bool success)
        {
            if(m_lockedCharacters.Contains(character.character.ID))
            {
                m_lockedCharacters.Remove(character.character.ID);
            }
            if(success)
            {
                character.ChangeState(BattleCharacterState.Defend);
            }
        }
        private void OnRecieveCharacterRemoveResponse(int characterID, bool success)
        {
            if (m_lockedCharacters.Contains(characterID))
            {
                m_lockedCharacters.Remove(characterID);
            }
        }

        private void OnRecieveCharacterAttackResponse(BattleCharacter character, int targetID, bool success)
        {
            if (m_lockedCharacters.Contains(character.character.ID))
            {
                m_lockedCharacters.Remove(character.character.ID);
            }
            if (success)
            {
                character.targetID = targetID;
                character.ChangeState(BattleCharacterState.Attack);
                OnCharacterStateChanged?.Invoke(character);
            }
        }
        private void OnRecieveCharacterSkillResponse(BattleCharacter character, int targetID, bool success)
        {
            if (m_lockedCharacters.Contains(character.character.ID))
            {
                m_lockedCharacters.Remove(character.character.ID);
            }
            if (success)
            {
                character.targetID = targetID;
                character.ChangeState(BattleCharacterState.Skill);
                OnCharacterStateChanged?.Invoke(character);
            }
        }
        private void OnRecieveCharacterCancleResponse(BattleCharacter character, bool success)
        {
            if(m_lockedCharacters.Contains(character.character.ID))
            {
                m_lockedCharacters.Remove(character.character.ID);
            }
            if(success)
            {
                character.ForceClearState();
                //OnCharacterStateChanged?.Invoke(character);
            }
        }
        public void ApplySnapshot(BattleReconnectSnapshot snapshot)
        {
            foreach (var c in snapshot.Characters)
            {
                AddCharacterFromSnapshot(c);
            }

            foreach (var p in snapshot.pendingActions)
            {
                if (!TryGetPlayerCharacter(p.characterID, out var cha)) continue;

                if (p.actionType == CharacterActionType.ChaActionTypeDefend)
                {
                    cha.ChangeState(BattleCharacterState.Defend);
                    cha.targetID = -1;
                    if(BattleManager.Instance.CurrentPhase == BattlePhase.PhaseAction)
                    {
                        cha.ShowDefend();
                    }
                }
                else if (p.actionType == CharacterActionType.ChaActionTypeAttack)
                {
                    cha.targetID = p.targetID;
                    cha.ChangeState(BattleCharacterState.Attack);
                    OnCharacterStateChanged?.Invoke(cha);
                }
                else if (p.actionType == CharacterActionType.ChaActionTypeSkill)
                {
                    cha.targetID = p.targetID;
                    cha.ChangeState(BattleCharacterState.Skill);
                    OnCharacterStateChanged?.Invoke(cha);
                }
            }
        }
        private void AddCharacterFromSnapshot(BattleCharacterSnapshot s)
        {
            Character cha = new Character();
            cha.ID = s.Id;
            cha.data = DataManager.Instance.characterData[s.dataID];
            cha.isMain = s.isMain;

            BattleEvent e = new BattleEvent();
            e.Id = s.Id;
            e.dataID = s.dataID;
            e.paramS = s.ownerID;
            e.Param = s.isMain ? 1 : 0;

            AddNewCharacter(cha, e);

            if (TryGetCharacter(s.Id, out var uiCha))
            {
                uiCha.character.CurHP = s.Hp;
                uiCha.character.CurMaxHP = s.maxHp;
                uiCha.character.CurATK = s.Atk;
                uiCha.character.CurDEF = s.Def;
                uiCha.character.CurSpeed = s.Speed;
                uiCha.character.CurMP = s.Mp;
                uiCha.canDefend = s.canDefend;
                uiCha.canAttack = s.canAttack;
                uiCha.canUseSkill = s.canUseSkill;
                

                uiCha.OnCharacterValueChanged?.Invoke();
            }
        }

        
    }
}
