using Assets.Scripts.Manager;
using Assets.Scripts.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBattleCharacterPanel : MonoBehaviour
{
    [SerializeField] private Button characterAttack;
    [SerializeField] private Button characterDefend;
    [SerializeField] private Button characterRemove;
    [SerializeField] private Button characterCancleRemove;
    [SerializeField] private GameObject arrow;
    private BattleCharacter currentCharacter;

    private RectTransform rTransform;

    [SerializeField]private Vector2 originPos = new Vector2(2500,400);

    private bool isSelectingEnemy = false;
    private void Awake()
    {
        rTransform = this.GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        BattleCharacterManager.Instance.OnCharacterClicked += HandleNewCharacterClicked;
        BattleCharacterManager.Instance.OnCharacterStateChanged += HandleCharacterStateChange;
        BattleCharacterManager.Instance.OnCharacterRemoved += CheckRemovedCharacter;
        BattleManager.Instance.OnSubmitPhaseChanged += HidePanel;
    }
    void Start()
    {
        characterRemove.onClick.RemoveAllListeners();
        characterRemove.onClick.AddListener(SendRemoveCurrentCharacter);

        characterDefend.onClick.RemoveAllListeners();
        characterDefend.onClick.AddListener(SendCurrentCharacterDefend);
        characterDefend.onClick.AddListener(HidePanel);

        characterAttack.onClick.RemoveAllListeners();
        characterAttack.onClick.AddListener(CharacterSelectTarget);

        characterCancleRemove.onClick.RemoveAllListeners();
        characterCancleRemove.onClick.AddListener(SendCharacterCancleRemove);
        characterCancleRemove.onClick.AddListener(HidePanel);
    }

    private void HandleNewCharacterClicked(BattleCharacter character)
    {
        if(character ==  null)//点空消失
        {
            HidePanel();
            return;
        }
        //Debug.Log("处理点击角色");
        if (currentCharacter == null)
        {
            if(character.ownerID == PlayerInfoManager.Instance.userID)
            {
                currentCharacter = character;
                this.ShowPanel();
            }
            return;
        }
        //没有在选择敌人阶段不能点击敌人||没有在选择敌人阶段不能点击自己人
        if (currentCharacter.character.data.CharacterType == Common.Data.CharacterType.Attack)
        {
            if (!isSelectingEnemy && character.ownerID != PlayerInfoManager.Instance.userID) { return; }
        }
        else if(currentCharacter.character.data.CharacterType == Common.Data.CharacterType.Heal)
        {
            if (!isSelectingEnemy && character.ownerID == PlayerInfoManager.Instance.userID && character != currentCharacter) { return; }
        }
        //点击角色两次关闭
        if(character == currentCharacter)
        {
            this.HidePanel();
            return;
        }
        else
        {
            if(currentCharacter.character.data.CharacterType == Common.Data.CharacterType.Attack)
            {
                //点击队友切换当前角色
                if (currentCharacter.ownerID == character.ownerID)
                {
                    currentCharacter = character;
                    isSelectingEnemy = false;
                    this.ShowPanel();
                    return;
                }

                //点击敌人则上传攻击请求
                if (currentCharacter.ownerID != character.ownerID)
                {
                    if (character.character.isMain)
                    {
                        if (BattleCharacterManager.Instance.IsEnemyHasCharacters())
                        {
                            return;
                        }
                    }
                    SendCharacterAttackRequest(character.character.ID);
                    ShowArrow(character);
                    characterCancleRemove.gameObject.transform.parent.gameObject.SetActive(currentCharacter.state != BattleCharacterState.None);
                }
            }
            else
            {
                //点击敌人关闭
                if (currentCharacter.ownerID != character.ownerID)
                {
                    isSelectingEnemy = false;
                    HidePanel();
                    return;
                }

                //点击自己人则上传攻击请求
                if (currentCharacter.ownerID == character.ownerID)
                {
                    SendCharacterAttackRequest(character.character.ID);
                    ShowArrow(character);
                    characterCancleRemove.gameObject.transform.parent.gameObject.SetActive(currentCharacter.state != BattleCharacterState.None);
                }
            }
        }
    }
    private void CheckRemovedCharacter(BattleCharacter removed)
    {
        if(removed ==  currentCharacter)
        {
            HidePanel();
        }
    }
    private void CharacterSelectTarget()
    {
        isSelectingEnemy = true;
    }
    private void SendCharacterAttackRequest(int targetID)
    {
        if (currentCharacter != null)
            BattleCharacterManager.Instance.SendCharacterAttackRequest(currentCharacter, targetID);
    }
    private void SendCurrentCharacterDefend()
    {
        if (currentCharacter != null)
            BattleCharacterManager.Instance.SendCharacterDefendRequest(currentCharacter);
    }
    private void SendRemoveCurrentCharacter()
    {
        if (currentCharacter != null)
            BattleCharacterManager.Instance.SendRemoveCharacterRequest(currentCharacter.character.ID);
    }
    private void SendCharacterCancleRemove()
    {
        if(currentCharacter != null)
            BattleCharacterManager.Instance.SendCharacterCancleAction(currentCharacter);
    }
    private void HidePanel()
    {
        //this.gameObject.SetActive(false);
        this.gameObject.transform.position = originPos;
        currentCharacter = null;
        isSelectingEnemy = false;

        HideArrow();
    }
    private void ShowPanel()
    {
        Vector2 pos = Camera.main.WorldToScreenPoint(currentCharacter.transform.position);

        rTransform.position = pos;

        if(currentCharacter.targetID != -1)
        {
            if (BattleCharacterManager.Instance.TryGetCharacter(currentCharacter.targetID, out var enemy))
            {
                ShowArrow(enemy);
            }
            else
            {
                HideArrow();
            }
        }
        else
        {
            HideArrow();
        }
        characterDefend.interactable = currentCharacter.canDefend;
        characterAttack.interactable = currentCharacter.canAttack;
        characterRemove.interactable = !currentCharacter.character.isMain;
        characterCancleRemove.gameObject.transform.parent.gameObject.SetActive(currentCharacter.state != BattleCharacterState.None);
    }
    private void ShowArrow(BattleCharacter enemy)
    {
        arrow.gameObject.SetActive(true);

        arrow.transform.position = enemy.transform.position;
    }
    private void HandleCharacterStateChange(BattleCharacter character)
    {
        if (character != currentCharacter) return;
        characterCancleRemove.gameObject.transform.parent.gameObject.SetActive(currentCharacter.state != BattleCharacterState.None);
    }
    private void HideArrow()
    {
        arrow.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        BattleCharacterManager.Instance.OnCharacterClicked -= HandleNewCharacterClicked;
        BattleCharacterManager.Instance.OnCharacterStateChanged -= HandleCharacterStateChange;
        BattleCharacterManager.Instance.OnCharacterRemoved -= CheckRemovedCharacter;
        BattleManager.Instance.OnSubmitPhaseChanged -= HidePanel;
    }
}
