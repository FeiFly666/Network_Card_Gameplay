using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using Common.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Main
{
    public class UICharacterShow : MonoBehaviour
    {
        private Dictionary<string, GameObject> m_characterPrefabs = new Dictionary<string, GameObject>();
        [SerializeField] private GameObject m_camera;
        private void Awake()
        {
            ClearPreviewTexture();
            foreach (var cardData in DataManager.Instance.cardData)
            {
                if (cardData.Value.Type == CardType.Character)
                {
                    GameObject newCharacterPrefab = Instantiate(DataManager.Instance.characterPrefabs[cardData.Value.TargetID[0]], this.transform, false).gameObject;
                    m_characterPrefabs[cardData.Value.TargetID[0]] = newCharacterPrefab;

                    newCharacterPrefab.GetComponent<BattleCharacter>().enabled = false;
                    newCharacterPrefab.SetActive(false);
                }
            }
        }
        public void ChangeShowPrefab(string cardCharacterID)
        {
            foreach (var characterPrefab in m_characterPrefabs)
            {
                //Debug.Log(characterPrefab.Key + ' ' + cardCharacterID);
                characterPrefab.Value.SetActive(characterPrefab.Key == cardCharacterID);
            }
            if(!m_camera.activeSelf)
            {
                m_camera.SetActive(true);
            }
        }
        public void HideAllPrefabs()
        {
            foreach (var characterPrefab in m_characterPrefabs)
            {
                if(characterPrefab.Value != null)
                    characterPrefab.Value.SetActive(false);
            }
            ClearPreviewTexture();
        }

        private void ClearPreviewTexture()
        {
            if (m_camera == null || !m_camera.TryGetComponent<Camera>(out var camera))
                return;
            m_camera?.SetActive(false);
            var texture = camera.targetTexture;
            if (texture == null)
                return;

            var previous = RenderTexture.active;
            try
            {
                RenderTexture.active = texture;
                GL.Clear(true, true, Color.clear);
            }
            finally
            {
                RenderTexture.active = previous;
            }
        }
    }
}