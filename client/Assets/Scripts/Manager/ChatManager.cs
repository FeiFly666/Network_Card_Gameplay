using Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Service
{
    public class ChatManager : Singleton<ChatManager>
    {
        public System.Action<string,string> OnNewChatRecieve;

        public void Init()
        {

        }
        public void NewChatRecieve(string id, string message)
        {
            OnNewChatRecieve?.Invoke(id,message);
        }
    }


}
