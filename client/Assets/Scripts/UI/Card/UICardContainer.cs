using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using UnityEngine;

namespace Assets.Scripts.UI.Card
{
    public class UICardContainer : UIWindow
    {
        [SerializeField] private Transform enemyCardRoot;
        [SerializeField] private Transform playerCardRoot;
        private Transform dragRoot;

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
            var cardManager = BattleCardManager.Instance;
            foreach (var card in cardManager.GetAllPlayerCardInfo())
                HandleSetNewCard(card);

            foreach (var card in cardManager.GetEnemyCards())
                HandleSetNewCard(card);
        }

        private void HandleSetNewCard(UICard newCard)
        {
            if (newCard.card == null)
            {
                newCard.transform.SetParent(enemyCardRoot, false);
                return;
            }

            newCard.transform.SetParent(playerCardRoot, false);
            newCard.dragRoot = dragRoot;
        }

        private void HandleRemoveCard(UICard card, bool isDrop)
        {
            card.RemoveCard(isDrop);
        }

        public override void OnClose()
        {
            base.OnClose();
            BattleCardManager.Instance.OnAddNewCard -= HandleSetNewCard;
            BattleCardManager.Instance.OnRemoveCard -= HandleRemoveCard;
        }

    }
}