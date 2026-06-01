using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Model
{
    public class MainCharacter : MonoBehaviour
    {
        private BattleCharacter character;

        private void Awake()
        {
            character = GetComponent<BattleCharacter>();
            if(character != null )
            {
                character.OnCharacterDead += SendPlayerLose;
            }
        }

        private void SendPlayerLose()
        {

        }
        private void OnDisable()
        {
            if(character !=null)
            {
                character.OnCharacterDead -= SendPlayerLose;
            }
        }
    }
}
