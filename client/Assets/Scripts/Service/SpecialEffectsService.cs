using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Manager;
using Assets.Scripts.Util;
using Common;
using Common.Network;
using JetBrains.Annotations;
using NetMessage;
using UnityEngine;

namespace Assets.Scripts.Service
{
    public class SpecialEffectsService : Singleton<SpecialEffectsService>
    {
        public void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserGetSpecialEffectListResponse>(OnRecieveSpecialLists);
            MessageDispatcher.Instance.Subscribe<UserEquipSpecialEffectResponse>(OnRecieveEquip);
            MessageDispatcher.Instance.Subscribe<UserUnEquipSpecialEffectResponse>(OnRecieveUnEquip);
        }
        public void SendGetSpecialEffectLists()
        {
            Message message = new Message();
            message.Request = new RequestMessage();

            UserGetSpecialEffectListRequest request = new UserGetSpecialEffectListRequest();
            message.Request.getSpecialEffectList = request;

            NetClient.Instance.Send(message);
        }
        public void SendEquipEffect(string effect, string character, int slot)
        {
            Message message = new Message();
            message.Request = new RequestMessage();

            UserEquipSpecialEffectRequest request = new UserEquipSpecialEffectRequest();
            request.specialEffectDataID = effect;
            request.characterDataID = character;
            request.slotIndex = slot;

            message.Request.equipSpecialEffect = request;
            NetClient.Instance.Send(message);
        }
        public void SendUnEquipEffect(string effect)
        {
            Message message = new Message();
            message.Request = new RequestMessage();

            UserUnEquipSpecialEffectRequest request = new UserUnEquipSpecialEffectRequest();
            request.specialEffectDataID = effect;

            message.Request.unEquipSpecialEffect = request;
            NetClient.Instance.Send(message);
        }
        private void OnRecieveSpecialLists(ISession session, UserGetSpecialEffectListResponse response)
        {
            if(session == null | response == null) return;
            
            if(!response.Success)
            {
                MessageBox.Show(Assets.Scripts.UI.Common.MessageType.Error, response.Msg);
                return;
            }
            foreach(var info in response.specialEffects)
            {
                SpecialEffectsManager.Instance.SetData(info);
            }

        }
        private void OnRecieveEquip(ISession session, UserEquipSpecialEffectResponse response)
        {
            if(session == null | response == null) return;

            if(!response.Success)
            {
                MessageBox.Show(Assets.Scripts.UI.Common.MessageType.Error, response.Msg);
                return;
            }

            foreach(var info in response.specialEffects)
            {
                SpecialEffectsManager.Instance.SetData(info);
            }

        }
        private void OnRecieveUnEquip(ISession session, UserUnEquipSpecialEffectResponse response)
        {
            if(session == null | response == null) return;

            if(!response.Success)
            {
                MessageBox.Show(Assets.Scripts.UI.Common.MessageType.Error, response.Msg);
                return;
            }

            foreach(var info in response.specialEffects)
            {
                SpecialEffectsManager.Instance.SetData(info);
            }
        }
    }

}
