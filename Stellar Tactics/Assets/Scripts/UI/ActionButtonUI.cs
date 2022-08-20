using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    #region Variables

    [SerializeField] private Image iconImage;   //
    [SerializeField] private Button button;     //

    [SerializeField] private Outline outline;                   //
    [SerializeField] private Color unselectedOutlineColor;      //
    [SerializeField] private Color selectedOutlineColor;        //

    private BaseAction action;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region


    public void SetAction(BaseAction action)
    {
        this.action = action;

        iconImage.sprite = action.GetActionIcon();

        button.onClick.AddListener(() =>
        {
            PlayerController.Instance.SetSelectedAction(action);
        });
    }


    public void UpdateSelectedVisual()
    {
        bool isSelected = action.Equals(PlayerController.Instance.GetSelectedAction());

        if(isSelected)
        {
            outline.effectColor = selectedOutlineColor;
        }
        else
        {
            outline.effectColor = unselectedOutlineColor;
        }
    }

    #endregion
}
