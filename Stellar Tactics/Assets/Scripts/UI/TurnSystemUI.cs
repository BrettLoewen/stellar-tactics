using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnSystemUI : MonoBehaviour
{
    #region Variables

    [SerializeField] private Button endTurnButton;
    [SerializeField] private TextMeshProUGUI turnOwnerText;
    [SerializeField] private GameObject enemyTurnIndicator;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    private void Awake()
    {
        GameManager.Instance.OnTurnChanged += GameManager_OnTurnChanged;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        UpdateEnemyTurnIndicator();
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region


    public void NextTurn()
    {
        GameManager.Instance.NextTurn();
        PlayerInputHandler.leftClickFlag = false;
    }


    public void SetTurnOwnerText(string text)
    {
        turnOwnerText.text = text;
    }

    private void GameManager_OnTurnChanged(object sender, EventArgs e)
    {
        UpdateEnemyTurnIndicator();
        UpdateEndTurnButtonVisibility();
    }


    private void UpdateEnemyTurnIndicator()
    {
        enemyTurnIndicator.SetActive(!GameManager.Instance.IsPlayerTurn());
    }


    private void UpdateEndTurnButtonVisibility()
    {
        endTurnButton.gameObject.SetActive(GameManager.Instance.IsPlayerTurn());
    }

    #endregion
}
