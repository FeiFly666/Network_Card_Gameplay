using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Card
{
    public class UICardContainer : UIWindow
    {
        [SerializeField] private Transform enemyCardRoot;
        [SerializeField] private Transform playerCardRoot;
        private Transform dragRoot;

        [SerializeField] private List<UICard> enemyCards = new List<UICard>();

        private Dictionary<int, UICard> playerCards = new Dictionary<int, UICard>();
        // Use this for initialization
        public override void OnOpen()
        {
            dragRoot = UIManager.Instance.dragRoot;
            InitUI();
            BattleCardManager.Instance.OnAddNewCard += HandleSetNewCard;
            BattleCardManager.Instance.OnRemoveCard += HandleRemoveCard;
            base.OnOpen();
        }
        private void InitUI()
        {
            List<UICard> cards = BattleCardManager.Instance.GetAllPlayerCardInfo();
            int enemyCardNum = BattleCardManager.Instance.GetEnemyCardNum();

            foreach (var card in cards)
            {
                HandleSetNewCard(card);
            }
            for(int i = 0; i < enemyCardNum; i++)
            {
                UICard newCard = BattleCardManager.Instance.GenerateFakeCard();
                HandleSetNewCard(newCard);
            }
        }
        
        private void HandleSetNewCard(UICard newCard)
        {
            if(newCard.card == null)//对手的
            {
                enemyCards.Add(newCard);
                newCard.transform.SetParent(enemyCardRoot, false);
                return;
            }
            if(playerCards.ContainsKey(newCard.card.ID))
                playerCards[newCard.card.ID] = newCard;
            else
                playerCards.Add(newCard.card.ID, newCard);
            newCard.transform.SetParent(playerCardRoot, false);
            newCard.dragRoot = this.dragRoot;
        }
        private void HandleRemoveCard(bool isPlayer, int cardID, bool isDrop)
        {
            //Debug.LogError("UI删除卡片");
            UICard deleteCard = null;
            bool findCard = false;
            if (!isPlayer)
            {
                if(enemyCards.Count > 0)
                {
                    findCard = true;
                    deleteCard = enemyCards[enemyCards.Count - 1];
                    enemyCards.RemoveAt(enemyCards.Count - 1);
                }
            }
            else
            {
                if(playerCards.TryGetValue(cardID, out deleteCard))
                {
                    findCard = true;
                    playerCards.Remove(cardID);
                }
            }
            if(findCard)
            {
                //Debug.LogError("UI删除卡片成功");
                deleteCard.RemoveCard(isDrop);
            }
        }
        public override void OnClose()
        {
            base.OnClose();
            BattleCardManager.Instance.OnAddNewCard -= HandleSetNewCard;
            BattleCardManager.Instance.OnRemoveCard -= HandleRemoveCard;
        }

    }
}