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

        private readonly List<UICard> m_enemyCards = new List<UICard>();

        [SerializeField] private UICard cCardPrefab;
        [SerializeField] private UICard bCardPrefab;
        [SerializeField] private UICard eCardPrefab;
        [SerializeField] private UICard fCardPrefab;

        public event Action<UICard> OnAddNewCard;
        public event Action<UICard, bool> OnRemoveCard;

        public Action OnStartDragCard;
        public Action OnStopDragCard;

        public bool HasDraggingCard = false;

        private readonly HashSet<UICard> m_rentedCards = new HashSet<UICard>();

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
            foreach (var ui in m_rentedCards.ToList())
                ReleaseCard(ui);

            m_cards.Clear();
            m_lockedCards.Clear();
            m_enemyCards.Clear();
            HasDraggingCard = false;

            OnStopDragCard?.Invoke();
        }
        public List<UICard> GetAllPlayerCardInfo()
        {
            return m_cards.Values.ToList();
        }
        public IReadOnlyList<UICard> GetEnemyCards()
        {
            return m_enemyCards;
        }
        public void SetNewCard(string playerID, Card newCard = null)//生成UICard
        {
            UICard newUICard = null;
            if(newCard == null)//生成对方假卡
            {
                newUICard = GenerateCard(fCardPrefab);
                m_enemyCards.Add(newUICard);
                OnAddNewCard?.Invoke(newUICard);
                return;
            }

            switch(newCard.Data.Type)
            {
                case Common.Data.CardType.Character:
                    newUICard = GenerateCard(cCardPrefab);
                    break;
                case Common.Data.CardType.Buff:
                    newUICard = GenerateCard(bCardPrefab);
                    break;
                case Common.Data.CardType.Gain:
                    newUICard = GenerateCard(eCardPrefab);
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
        private UICard GenerateCard(UICard Prefab)
        {
            var pool = PoolManager.Instance;
            pool.CreateGameObjectPool<UICard>(Prefab, maxNum: 8, preWarmNum: 2);

            var ui = pool.GetGameObject<UICard>(Prefab.gameObject.GetInstanceID());
            m_rentedCards.Add(ui);

            return ui;
        }

        public void ReleaseCard(UICard ui)
        {
            if (m_rentedCards.Remove(ui) && ui != null)
                PoolManager.Instance.ReleaseGameObject<UICard>(ui);
        }

        public void RemoveCard(int cardID, bool isDrop)
        {
            if (m_cards.TryGetValue(cardID, out var card))
            {
                m_cards.Remove(cardID);
                m_lockedCards.Remove(cardID);
            }
            else
            {
                if (m_enemyCards.Count == 0) return;

                int lastIndex = m_enemyCards.Count - 1;
                card = m_enemyCards[lastIndex];
                m_enemyCards.RemoveAt(lastIndex);
            }

            if (OnRemoveCard == null)
                ReleaseCard(card);
            else
                OnRemoveCard.Invoke(card, isDrop);
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
            /*bool result = BattleService.Instance.SendDropCard(request.card, 
                (success)=>
                    {
                        m_lockedCards.Remove(request.card.ID);
                        OnRecieveActionResponse(success, request);
                    }
            );
            if(result) m_lockedCards[request.card.ID] = request;

            return result;*/
            return SendCardRequest(request, callback => BattleService.Instance.SendDropCard(request.card, callback));
        }
        public bool SendSpawnChaRequest(UICard request)
        {
          /*  bool result = BattleCharacterManager.Instance.SendSpawnCharacterRequest(request,
                (success) =>
                {
                    m_lockedCards.Remove(request.card.ID);
                    OnRecieveActionResponse(success, request);
                });
            if (result) m_lockedCards[request.card.ID] = request;
            return result;*/
            return SendCardRequest(request, callback =>BattleCharacterManager.Instance.SendSpawnCharacterRequest(request, callback));
        }
        public bool SendAttributeChangeRequest(UICard request, BattleCharacter targetCharacter)
        {
          /*bool result = BattleService.Instance.SendCharacterAttributeChangeRequest(request.card.ID, targetCharacter.character.ID,(success) =>
            {
                m_lockedCards.Remove(request.card.ID);
                OnRecieveActionResponse(success, request);
            });
            if(result) m_lockedCards[request.card.ID] = request;
            return result;*/

            return SendCardRequest(request, callback => BattleService.Instance.SendCharacterAttributeChangeRequest(request.card.ID, targetCharacter.character.ID, callback));
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
        private bool SendCardRequest(UICard ui, Func<Action<bool>, bool> send)
        {
            Card submittedCard = ui.card;
            int cardId = submittedCard.ID;

            if (!send(success => HandleCardResponse(success, cardId, submittedCard, ui)))
                return false;

            m_lockedCards[cardId] = ui;
            return true;
        }

        private void HandleCardResponse(bool success, int cardId, Card submittedCard, UICard ui)
        {
            //仅处理失败回包，且 UI 卡绑定卡与请求卡相同。
            if (success || ui == null || !ReferenceEquals(ui.card, submittedCard))
                return;

            if (!m_cards.TryGetValue(cardId, out var currentUI) || currentUI != ui)
                return;

            if (!m_lockedCards.TryGetValue(cardId, out var lockedUI) || lockedUI != ui)
                return;

            m_lockedCards.Remove(cardId);
            ui.ReturnToOrigin();
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
