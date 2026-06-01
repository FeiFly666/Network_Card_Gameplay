using Common;
using Common.Data;
using Common.Model;
using GameServer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    public class DataManager :Singleton<DataManager>
    {
        public Dictionary<string, CardData> m_cards = new Dictionary<string, CardData>();
        public Dictionary<string, CharacterData> m_characters = new Dictionary<string, CharacterData>();
        public Dictionary<string, BuffData> m_effects = new Dictionary<string,BuffData>();
        public Dictionary<string, PlayerData> m_playerInfos = new Dictionary<string, PlayerData>();

        private readonly string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\");
        private readonly string infoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Info\\");
        public void Init()
        {
            Console.WriteLine("data manager : start init data");

            string json = File.ReadAllText(this.configPath + "Card.json");
            m_cards = JsonConvert.DeserializeObject<Dictionary<string, CardData>>(json);

            Console.WriteLine("data manager : card data finish");

            json = File.ReadAllText(this.configPath + "Character.json");
            m_characters = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(json);

            Console.WriteLine("data manager : character data finish");

            json = File.ReadAllText(this.configPath + "Effect.json");
            m_effects = JsonConvert.DeserializeObject<Dictionary<string, BuffData>>(json);

            Console.WriteLine("data manager : buff data finish");

            json = File.ReadAllText(this.infoPath + "Player.json");
            m_playerInfos = JsonConvert.DeserializeObject<Dictionary<string, PlayerData>>(json);

            if(m_playerInfos == null)m_playerInfos = new Dictionary<string, PlayerData>();

            Console.WriteLine("data manager : player data finish");

            Console.WriteLine("data manager : data init finish");
        }
        public void SavePlayerInfo(Player player)
        {
            m_playerInfos[player.id] = new PlayerData();
            m_playerInfos[player.id].Name = player.id;
            m_playerInfos[player.id].mainCharacterID = player.mainCharcterID;

            string save = JsonConvert.SerializeObject(m_playerInfos, Formatting.Indented);

            File.WriteAllText(infoPath + "Player.json", save);
        }
    }
}
