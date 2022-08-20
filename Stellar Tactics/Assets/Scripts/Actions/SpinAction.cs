using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{
    #region Variables

    private float totalSpinAmount;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        if(!isActive)
        {
            totalSpinAmount = 0f;
            return;
        }

        float spinAddAmount = 360f * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);

        totalSpinAmount += spinAddAmount;
        if(totalSpinAmount >= 360f)
        {
            PlayerController.Instance.SetSelectedAction(this);
            CompleteAction();
        }
    }//end Update

    #endregion //end Unity Control Methods

    #region


    public override bool TryTakeAction(ActionTarget target, Action onActionComplete)
    {
        //Store whether or not the spin action is performed, start false
        bool canSpin = false;

        //Tell the unit to attempt to perform a spin
        if (unit.TryPerformAction(this))
        {
            //Reset the spin amount so the spin will execute properly
            totalSpinAmount = 0f;

            //Store that a spin was performed
            canSpin = true;

            //Trigger the spin to start
            StartAction(onActionComplete);
        }

        //Return whether or not the spin action is performed
        return canSpin;
    }


    public override void CalculateActionTargets()
    {
        return;
    }


    public override EnemyAIAction GetBestEnemyAIAction()
    {
        //Calculate and return the appropriate EnemyAIAction for the SpinAction
        return GetEnemyAIAction(target);
    }//end GetBestEnemyAIAction


    public override EnemyAIAction GetEnemyAIAction(ActionTarget target)
    {
        //Make a new EnemyAIAction variable with minimal priority
        return new EnemyAIAction
        {
            target = target,
            actionValue = 1,
        };
    }//end GetEnemyAIAction

    #endregion
}
