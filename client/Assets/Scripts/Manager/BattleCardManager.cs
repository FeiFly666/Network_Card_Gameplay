using Common;
using Common.Model;
using Assets.Scripts.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Service;
using NetMessage;

namespace Assets.Scripts.Manager
{
    public class BattleCardManager : MonoBehaviour
    {
        public static BattleCardManager Instance;
        //存自己的卡牌
        private Dictionary<int, UICard> m_cards = new Dictionary<int, UICard>();
        private Dictionary<int,UICard>m_lockedCards = new Dictionary<int, UICard>();

        //存其他玩家的卡牌数量
        [SerializeField]private int m_otherCardNum = 0;

        [SerializeField] private UICard cCardPrefab;
        [SerializeField] private UICard bCardPrefab;
        [SerializeField] private UICard eCardPrefab;
        [SerializeField] private UICard fCardPrefab;

        public Action<UICard> OnAddNewCard;
        public Action<bool, int, bool> OnRemoveCard;

        public Action OnStartDragCard;
        public Action OnStopDragCard;

        public bool HasDraggingCard = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
            }
        }

        //用于新对局清理残留信息
        public void Init()
        {
            m_cards.Clear();
            m_lockedCards.Clear();
            m_otherCardNum = 0;
        }
        public List<UICard> GetAllPlayerCardInfo()
        {
            return m_cards.Values.ToList();
        }
        public int GetEnemyCardNum()
        {
            return m_otherCardNum;
        }
        public void SetNewCard(string playerID, Card newCard = null)//生成UICard
        {
            UICard newUICard = null;
            if(newCard == null)//生成对方假卡
            {
                newUICard = GenerateFakeCard();
               
                OnAddNewCard?.Invoke(newUICard);
                return;
            }

            switch(newCard.Data.Type)
            {
                case Common.Data.CardType.Character:
                    newUICard = GameObject.Instantiate(cCardPrefab); 
                    break;
                case Common.Data.CardType.Buff:
                    newUICard = GameObject.Instantiate(bCardPrefab);
                    break;
                case Common.Data.CardType.Gain:
                    newUICard = GameObject.Instantiate(eCardPrefab);
                    break;
            }

            if (newUICard == null) return;

            newUICard.card = newCard;

            if(m_cards.ContainsKey(newCard.ID))
                m_cards[newCard.ID] = newUICard;
            else
                m_cards.Add(newCard.ID, newUICard);

            newUICard.SetCardInfo();

            OnAddNewCard?.Invoke(newUICard);
        }
        public UICard GenerateFakeCard()
        {
            UICard newUICard = GameObject.Instantiate(fCardPrefab);

            m_otherCardNum++;

            return newUICard;
        }
        public void RemoveCard(int cardID, bool isDrop)
        {

            bool isPlayer = true;

            if(!m_cards.TryGetValue(cardID, out var card))
            {
                isPlayer = false;
                m_otherCardNum--;

            }
            else
            {
                m_cards.Remove(cardID);
            }

            OnRemoveCard?.Invoke(isPlayer, cardID, isDrop);

        }


        public bool UpdateUsingCard(UICard card)
        {
            //if(m_lockedCards.ContainsKey(card.card.ID)) return false;

            if(card == null)
            {
                OnStopDragCard?.Invoke();

                return true;
            }

            if (!m_cards.ContainsKey(card.card.ID)) return false;

            OnStartDragCard?.Invoke();

            return true;
        }

        public bool SendDiscardRequest(UICard request)
        {
            bool result = BattleService.Instance.SendDropCard(request.card, 
                (success)=>
                    {
                        m_lockedCards.Remove(request.card.ID);
                        OnRecieveActionResponse(success, request);
                    }
            );
            if(result) m_lockedCards[request.card.ID] = request;

            return result;
        }
        public bool SendSpawnChaRequest(UICard request)
        {
            bool result = BattleCharacterManager.Instance.SendSpawnCharacterRequest(request,
                (success) =>
                {
                    m_lockedCards.Remove(request.card.ID);
                    OnRecieveActionResponse(success, request);
                });
            if(result) m_lockedCards[request.card.ID] = request;
            return result;
        }
        public bool SendAttributeChangeRequest(UICard request, BattleCharacter targetCharacter)
        {
            bool result = BattleService.Instance.SendCharacterAttributeChangeRequest(request.card.ID, targetCharacter.character.ID,(success) =>
            {
                m_lockedCards.Remove(request.card.ID);
                OnRecieveActionResponse(success, request);
            });
            if(result) m_lockedCards[request.card.ID] = request;
            return result;
        }
        public void OnRecieveActionResponse(bool result, UICard card)
        {
            Debug.Log($"card manager : recieve response , unlock card {card.card.ID}");

            //m_lockedCards.Remove(card.card.ID);

            if (card != null)
            {
                if(!result)
                {
                    Debug.Log($"card manager : action fail , {card.card.ID}");
                    card.ReturnToOrigin();
                }
            }
            OnStopDragCard?.Invoke();
        }
        public bool CanDragThisCard(UICard card)
        {
            return !m_lockedCards.ContainsKey((card.card.ID)) && !HasDraggingCard && BattleManager.Instance.CurrentPhase == BattlePhase.PhasePrepare;
        }
        public void SetDraggingCard(bool b)
        {
            HasDraggingCard = b;
        }
        public void ApplySnapshot(BattleReconnectSnapshot snapshot)
        {
            foreach (var c in snapshot.selfCards)
            {
                AddCardFormSnapShot(c);
            }

            for(int i = 0; i< snapshot.enemyCardCount; i++)
            {
                SetNewCard("enemy", null);
            }
        }
        private void AddCardFormSnapShot(BattleCardSnapshot cardData)
        {
            Card card = new Card();
            card.ID = cardData.Id;
            card.Data = DataManager.Instance.cardData[cardData.dataID];

            SetNewCard(PlayerInfoManager.Instance.userID, card);
        }
    }
}
