using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionBarUI : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform actionButtonParent;      //
    [SerializeField] private Transform actionButtonPrefab;      //
    private List<ActionButtonUI> actionButtons = new List<ActionButtonUI>();

    [SerializeField] private TextMeshProUGUI actionNameText;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        PlayerController.Instance.OnSelectedUnitChanged += PlayerController_OnSelectedUnitChanged;
        PlayerController.Instance.OnSelectedActionChanged += PlayerController_OnSelectedActionChanged;

        CreateActionButtons();
        UpdateSelectedVisual();
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region


    private void CreateActionButtons()
    {
        //
        foreach(Transform button in actionButtonParent)
        {
            Destroy(button.gameObject);
        }

        //
        actionButtons.Clear();

        //
        Unit selectedUnit = PlayerController.Instance.GetSelectedUnit();

        //
        foreach(BaseAction action in selectedUnit.GetBaseActionArray())
        {
            Transform button = Instantiate(actionButtonPrefab, actionButtonParent);
            ActionButtonUI actionButton = button.GetComponent<ActionButtonUI>();

            actionButton.SetAction(action);
            actionButtons.Add(actionButton);
        }
    }//end CreateActionButtons


    private void UpdateSelectedVisual()
    {
        //
        foreach (ActionButtonUI actionButton in actionButtons)
        {
            actionButton.UpdateSelectedVisual();
        }

        //
        actionNameText.text = PlayerController.Instance.GetSelectedAction().GetActionName();
    }


    private void PlayerController_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        CreateActionButtons();
        UpdateSelectedVisual();
    }//end PlayerController_OnSelectedUnitChanged

    private void PlayerController_OnSelectedActionChanged(object sender, EventArgs e)
    {
        UpdateSelectedVisual();
    }//end PlayerController_OnSelectedActionChanged

    #endregion
}
