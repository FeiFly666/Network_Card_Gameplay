using Common;
using Common.Model;
using NetMessage;
using System;
using UnityEditor;
using UnityEngine;


namespace Assets.Scripts.Manager
{
    public class RoomManager :Singleton<RoomManager>
    {
        private Room currentRoom;
        public System.Action<Room> OnRoomInfoChanged;

        public void RefreshRoom(RoomInfo roomInfo)
        {
            if(roomInfo == null)
            {
                OnRoomInfoChanged?.Invoke(null);
                return;
            }
            currentRoom = new Room();
            currentRoom.ID = roomInfo.roomID;

            bool isSelfIn = false;

            foreach(var playerInfo in roomInfo.Players)
            {
                Player player = new Player();
                player.ready = playerInfo.Ready;
                player.id = playerInfo.userID;
                player.mainCharcterID = playerInfo.playerData.mainCharacterID;
                player.nickname = playerInfo.playerData.nickName;
                if(player.id == PlayerInfoManager.Instance.userID) { isSelfIn = true; };
                currentRoom.Players.Add(player);
            }
            if (isSelfIn)
                OnRoomInfoChanged?.Invoke(currentRoom);
            else
                OnRoomInfoChanged?.Invoke(null);


        }
        public int GetPlayerRoomPos()
        {
            if (currentRoom == null || currentRoom.Players == null) return -1;
            for(int i = 0; i< currentRoom.Players.Count; i++)
            {
                Player p = currentRoom.Players[i];
                if(p.id == PlayerInfoManager.Instance.userID)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
