using Common;
using Common.Network;
using GameServer.DataBase.Service;
using GameServer.Network;
using NetMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Manager
{
    public class SpecialEffectManager : Singleton<SpecialEffectManager>
    {
        private UserSpecialEffectService service = new UserSpecialEffectService();

        private Dictionary<(string, string, int), string> characterSlotEffects = new Dictionary<(string, string, int), string>();
        public async void Init()
        {
            MessageDispatcher.Instance.Subscribe<UserGetSpecialEffectListRequest>(OnUserGetSpecialEffectInfos);
            MessageDispatcher.Instance.Subscribe<UserEquipSpecialEffectRequest>(OnUserEquipSpecialEffect);
            MessageDispatcher.Instance.Subscribe<UserUnEquipSpecialEffectRequest>(OnUserUnEquipSpecialEffect);

            var result = await service.InitManagerData();
        }
        /// <summary>
        /// 为用户解锁所有被动效果
        /// </summary>
        /// <param name="userID">需解锁用户ID</param>
        public async void UnlockAllEffects(string userID)
        {
            await service.UnlockAllSpecialEffects(userID);
        }

        public void SetSlotEffect((string, string, int) key, string value)
        {
            characterSlotEffects[key] = value;
        }
        public void RemoveSlotEffect((string, string, int) key, string effectID)
        {
            if (characterSlotEffects.TryGetValue(key, out var current) && current == effectID)
            {
                characterSlotEffects.Remove(key);
            }
        }
        public bool GetSlotEffect((string, string, int) key, out string value)
        {
            return characterSlotEffects.TryGetValue(key, out value);
        }

        private async void OnUserGetSpecialEffectInfos(ISession session, UserGetSpecialEffectListRequest request)
        {
            if (session == null) return;

            ClientSession client = (ClientSession)session;
            if (client.IsClosed || client.player == null) return;

            Message message = new Message();
            message.Response = new ResponseMessage();

            var result = await service.GetListAsync(client.player.id);

            UserGetSpecialEffectListResponse response = new UserGetSpecialEffectListResponse();
            response.Success = result.success;
            response.Msg = result.msg;
            if(result.success)
            {
                response.specialEffects.AddRange(result.effects);
            }

            message.Response.getSpecialEffectList = response;

            session.Send(message);
        }
        private async void OnUserEquipSpecialEffect(ISession session, UserEquipSpecialEffectRequest request)
        {
            if (session == null) return;

            ClientSession client = (ClientSession)session;
            if (client.IsClosed || client.player == null) return;

            Message message = new Message();
            message.Response = new ResponseMessage();

            var result = await service.ChangeSpeicalEffectStatus(SpecialEffectHandle.Equip, client.player.id, request.specialEffectDataID,
                                       request.characterDataID, request.slotIndex);

            UserEquipSpecialEffectResponse response = new UserEquipSpecialEffectResponse();
            response.Success = result.success;
            response.Msg = result.msg;
            if(result.success)
            {
                var infos = await service.GetListAsync(client.player.id);
                if(!infos.success)
                {
                    response.Success = infos.success;
                    response.Msg = infos.msg;
                }
                else
                {
                    response.specialEffects.AddRange(infos.effects);
                }
            }
            message.Response.equipSpecialEffect = response;
            session.Send(message);
        }
        private async void OnUserUnEquipSpecialEffect(ISession session, UserUnEquipSpecialEffectRequest request)
        {
            if (session == null) return;

            ClientSession client = (ClientSession)session;
            if (client.IsClosed || client.player == null) return;

            Message message = new Message();
            message.Response = new ResponseMessage();

            var result = await service.ChangeSpeicalEffectStatus(SpecialEffectHandle.UnEquip, client.player.id, request.specialEffectDataID, null, null); 

            UserUnEquipSpecialEffectResponse response = new UserUnEquipSpecialEffectResponse();
            response.Success = result.success;
            response.Msg = result.msg;
            if(result.success)
            {
                var infos = await service.GetListAsync(client.player.id);
                if (!infos.success)
                {
                    response.Success = infos.success;
                    response.Msg = infos.msg;
                }
                else
                {
                    response.specialEffects.AddRange(infos.effects);
                }
            }
            message.Response.unEquipSpecialEffect = response;
            session.Send(message);
        }
    }
}
