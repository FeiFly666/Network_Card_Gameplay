using Common.Data;
using Common.Model;
using GameServer.Model;
using GameServer.Utils;
using NetMessage;
using System.Reflection;

namespace GameServer.Manager
{
    public class BattleCardManager
    {
        private BattleRoom room;
        private Dictionary<int, Card> m_cards = new Dictionary<int, Card>();

        private Dictionary<string, CardData> m_allCards = new Dictionary<string, CardData>();

        private Dictionary<int, List<string>> m_qualityCards = new Dictionary<int, List<string>>();
        private List<string> m_characterCards = new List<string>();

        private Random random = new Random();

        private Dictionary<string, int> m_nextPlayerSendCardNum = new Dictionary<string, int>();

        private Dictionary<int, int> qualityWeight = new Dictionary<int, int>
        {
            { 1, 70 },
            { 2, 30 },
            { 3, 30 }
        };

        private int maxQuality = 1;

        public BattleCardManager(BattleRoom battleRoom)
        {
            room = battleRoom;

            foreach(var card in DataManager.Instance.m_cards.Values)
            {
                int quality = card.Quality;

                if (!m_qualityCards.ContainsKey(quality))
                {
                    m_qualityCards[quality] = new List<string>();
                }
                if (card.Type == CardType.Character) m_characterCards.Add(card.DataID);

                m_qualityCards[quality].Add(card.DataID);

                maxQuality = Math.Max(maxQuality, quality);
                
                m_allCards.Add(card.DataID, card);
            }
            
        }
        public void InitPlayerSendCardNum()
        {
            foreach (var player in room.players.Keys)
            {
                m_nextPlayerSendCardNum.Add(player, 4);
            }
        }
        public BattleEvent GiveCard(Player player, string cardData)
        {
            Card card = BattleUtil.CreateNewCard(cardData);
            if (card == null)
            {
                return null;
            }
            card.ID = room.currentFreeID++;
            if (player.battlePlayer.cards.ContainsKey(card.ID))
            {
                player.battlePlayer.cards[card.ID] = card;
            }
            else
            {
                player.battlePlayer.cards.Add(card.ID, card);
            }

            m_cards[card.ID] = card;

            BattleEvent e = new BattleEvent();

            e.Type = BattleEvent.EventType.EventDrawCard;
            e.Id = card.ID;
            e.dataID = cardData;
            e.actionID = -1;
            //添加消息
            return e;
        }
        public BattleEvent GiveFakeCard(Player player)
        {
            BattleEvent e = new BattleEvent();

            e.Type = BattleEvent.EventType.EventDrawCard;
            e.Id = -1;
            e.dataID = player.id;
            e.actionID = -1;
            //添加消息
            return e;
        }
        public void RemoveCard(string playerID, int cardID, int actionID, bool isDrop = false)
        {
            if(m_cards.ContainsKey(cardID))
            {
               m_cards.Remove(cardID);
            }
            room.players[playerID].battlePlayer.cards.Remove(cardID);

            BattleEvent e = new BattleEvent();
            e.actionID = actionID;
            e.Type = BattleEvent.EventType.EventDropCard;
            e.Id = cardID;

            e.Param = isDrop ? 1 : 0;

            room.messageSystem.BroadCastEvent(e);
        }
        public void DrawCardDemo()
        {
            //分发消息
            Message message = new Message();

            foreach (var playerID in room.loadedPlayers)
            {
                Player player = room.players[playerID];

                message.Response = new ResponseMessage();
                message.Response.battleEvent = new BattleEventBroadcast();

                int drawNum = 0;

                if (player.battlePlayer.cards.Count < 6)
                {
                    message.Response.battleEvent.Events.Add(room.cardManager.GiveCard(player, "card7"));

                    drawNum = 1;
                }

                //给获得卡牌的玩家具体数据
                room.messageSystem.SendToPlayer(player, message);

                message.Response = new ResponseMessage();
                message.Response.battleEvent = new BattleEventBroadcast();
                for (int i = 0; i < drawNum; i++)
                {
                    message.Response.battleEvent.Events.Add(room.cardManager.GiveFakeCard(player));
                }

                //给其他玩家假数据，主要目的是让其他玩家客户端更新该玩家的卡牌数量
                room.messageSystem.BroadCast(message, player);
            }

        }

        public void DrawCard()
        {
            //分发消息
            Message message = new Message();

            foreach (var playerID in room.loadedPlayers)
            {
                Player player = room.players[playerID];

                message.Response = new ResponseMessage();
                message.Response.battleEvent = new BattleEventBroadcast();

                int maxDrawNum = m_nextPlayerSendCardNum[playerID];
                m_nextPlayerSendCardNum[playerID] = 3;

                int drawNum = 0;

                while (player.battlePlayer.cards.Count < 8 && drawNum < maxDrawNum )
                {
                    if(room.round == 3)//第三回合给没有角色卡的一方保底一张
                    {
                        if(player.battlePlayer.characters.Count == 1 && !HasCharacterCard(player))
                        {
                            string characterCardID = m_characterCards[random.Next(m_characterCards.Count)];
                            message.Response.battleEvent.Events.Add(room.cardManager.GiveCard(player, characterCardID));
                            drawNum++;
                            continue;
                        }
                    }
                    //TODO：随机发牌
                    int randQuality = RandomQuality();

                    string cardID = m_qualityCards[randQuality][random.Next(m_qualityCards[randQuality].Count)];

                    if(m_characterCards.Contains(cardID))//是角色卡判断是否可以发给角色
                    {
                        int removeRoll = random.Next(100);
                        if(removeRoll < GetCharacterRemoveWeight())
                        {
                            continue;
                        }
                    }

                    message.Response.battleEvent.Events.Add(room.cardManager.GiveCard(player, cardID));

                    drawNum++;
                }

                //给获得卡牌的玩家具体数据
                room.messageSystem.SendToPlayer(player, message);

                message.Response = new ResponseMessage();
                message.Response.battleEvent = new BattleEventBroadcast();
                for (int i = 0; i < drawNum; i++)
                {
                    message.Response.battleEvent.Events.Add(room.cardManager.GiveFakeCard(player));
                }

                //给其他玩家假数据，主要目的是让其他玩家客户端更新该玩家的卡牌数量
                room.messageSystem.BroadCast(message, player);
            }

        }
        private bool HasCharacterCard(Player player)
        {
            foreach(var card in player.battlePlayer.cards)
            {
                if (card.Value.Data.Type == CardType.Character) return true;
            }
            return false;
        }
        private int GetCharacterRemoveWeight()
        {
            if (room.round > 17) return 80;
            if (room.round > 11) return 40;
            if (room.round > 5) return 10;
            return 0;
        }
        private int RandomQuality()
        {
            int totalWeight = 0;
            foreach(var qualityKV in qualityWeight)
            {
                int quality = qualityKV.Key;
                int weight = qualityKV.Value;
                if (!m_qualityCards.ContainsKey(quality)) continue;
                if (m_qualityCards[quality].Count == 0) continue;

                totalWeight += weight;

            }
            if (totalWeight <= 0) return -1;
            int roll = random.Next(0, totalWeight);

            int currentWeight = 0;
            foreach(var qualityKV in qualityWeight)
            {
                int quality = qualityKV.Key;
                int weight = qualityKV.Value;
                if (!m_qualityCards.ContainsKey(quality)) continue;
                if (m_qualityCards[quality].Count == 0) continue;

                currentWeight += weight;

                if (roll < currentWeight) return quality;
            }
            return -1;
        }
        public void AddPlayerSendCardNum(string playerID, int num)
        {
            m_nextPlayerSendCardNum[playerID] = Math.Min(m_nextPlayerSendCardNum[playerID] + num, 6);
        }
    }
}
