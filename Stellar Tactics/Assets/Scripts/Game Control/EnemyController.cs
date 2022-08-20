using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region Variables

    private enum State { WaitingForTurn, TakingTurn, Busy }
    private State state;

    private float timer;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        GameManager.Instance.OnTurnChanged += GameManager_OnTurnChanged;

        state = State.WaitingForTurn;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance.IsPlayerTurn())
        {
            return;
        }

        switch(state)
        {
            case State.WaitingForTurn:
                break;
            case State.TakingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    if(TryPerformUnitAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    //No more units can act, so end the turn
                    else
                    {
                        GameManager.Instance.NextTurn();
                    }
                }
                break;
            case State.Busy:
                break;
        }
    }//end Update

    #endregion //end Unity Control Methods

    #region


    private void GameManager_OnTurnChanged(object sender, EventArgs e)
    {
        if(GameManager.Instance.IsPlayerTurn() == false)
        {
            state = State.TakingTurn;
            timer = 2f;
        }
    }


    private void SetStateTakingTurn()
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }


    private bool TryPerformUnitAction(Action onActionComplete)
    {
        //
        foreach(Unit enemyUnit in GameManager.Instance.GetEnemyUnitList())
        {
            if(TryPerformUnitAction(enemyUnit, onActionComplete))
            {
                //A unit was able to act, so don't check any other
                return true;
            }
        }

        //No units were able to act
        return false;
    }


    private bool TryPerformUnitAction(Unit enemyUnit, Action onActionComplete)
    {
        //
        CameraController.Instance.SetFocus(enemyUnit.transform);

        //
        bool canPerformAction = false;

        //
        EnemyAIAction bestAIAction = null;
        BaseAction bestAction = null;

        //
        foreach(BaseAction action in enemyUnit.GetBaseActionArray())
        {
            //If the unit can NOT perform this action, skip it
            if(enemyUnit.CanPerformAction(action) == false)
            {
                continue;
            }

            //
            EnemyAIAction enemyAIAction = action.GetBestEnemyAIAction();

            //
            if(bestAIAction == null)
            {
                bestAIAction = enemyAIAction;
                bestAction = action;
            }
            //
            else
            {
                //
                if(enemyAIAction != null && enemyAIAction.actionValue > bestAIAction.actionValue)
                {
                    bestAIAction = enemyAIAction;
                    bestAction = action;
                }
            }
        }

        //
        if (bestAIAction != null)
        {
            //
            ActionTarget target = bestAIAction.target;

            //
            if(bestAction.TryTakeAction(target, onActionComplete))
            {
                canPerformAction = true;
            }
        }

        //
        return canPerformAction;
    }

    #endregion
}
