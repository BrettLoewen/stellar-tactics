using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTarget
{
    public Unit targetUnit;                     //The Unit that this ActionTarget is targeting
    public Tile targetTile;                     //The Tile that this ActionTarget is targeting
    public IInteractable targetInteractable;    //The Interactable that this ActionTarget is targeting

    /// <summary>
    /// A constructor to make a new ActionTarget with the passed Unit
    /// </summary>
    /// <param name="unit">Used as the target Unit for this ActionTarget</param>
    public ActionTarget(Unit unit)
    {
        //Set the ActionTarget's target Unit using the passed Unit
        targetUnit = unit;

        //Set everything else to null
        targetTile = null;
        targetInteractable = null;
    }//end Unit Constructor

    /// <summary>
    /// A constructor to make a new ActionTarget with the passed Tile
    /// </summary>
    /// <param name="tile">Used as the target Tile for this ActionTarget</param>
    public ActionTarget(Tile tile)
    {
        //Set the ActionTarget's target Tile using the passed Tile
        targetTile = tile;

        //Set everything else to null
        targetUnit = null;
        targetInteractable = null;
    }//end Tile Constructor

    /// <summary>
    /// A constructor to make a new ActionTarget with the passed Interactable
    /// </summary>
    /// <param name="interactable">Used as the target Interactable for this ActionTarget</param>
    public ActionTarget(IInteractable interactable)
    {
        //Set the ActionTarget's target Interactable using the passed Interactable
        targetInteractable = interactable;

        //Set everything else to null
        targetUnit = null;
        targetTile = null;
    }//end Interactable Constructor
}

public abstract class BaseAction : MonoBehaviour
{
    #region Variables

    public static event EventHandler OnAnyActionStarted;    //An event to be triggered when any Unit starts an Action
    public static event EventHandler OnAnyActionCompleted;  //An event to be triggered when any Unit completes an Action

    protected Unit unit;                    //The Unit that owns this Action
    protected UnitAnimator unitAnimator;    //Controls the unit's animations

    protected bool isActive;            //Stores whether or not the Action is currently performing
    protected Action onActionComplete;  //Stores a callback to be called when the Action completes

    [Header("Base Action")]
    [SerializeField] protected string actionName;       //The name of the Action
    [SerializeField] protected Sprite actionIcon;       //The icon for the Action to be displayed in the player's HUD
    [SerializeField] protected int actionPointCost = 1; //The number of action points needed to perform this Action

    protected List<ActionTarget> targets = new List<ActionTarget>();    //The valid ActionTargets (options) for this Action
    protected ActionTarget target;                                      //The ActionTarget that is being performed

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected virtual void Awake()
    {
        //Get the Unit and UnitAnimator components
        unit = GetComponent<Unit>();
        unitAnimator = unit.GetUnitAnimator();
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

    /// <summary>
    /// Return the Action's Unit
    /// </summary>
    /// <returns>Return the owner Unit for this Action</returns>
    public Unit GetUnit()
    {
        //Return the Unit
        return unit;
    }//end GetUnit

    /// <summary>
    /// Calculates and returns the best possible decision for this Action
    /// </summary>
    /// <returns>Returns the best possible decision for this Action</returns>
    public abstract EnemyAIAction GetBestEnemyAIAction();

    /// <summary>
    /// Evaluates the passed ActionTarget option and return the evaluation
    /// </summary>
    /// <param name="target">The valid ActionTarget to evaluate</param>
    /// <returns>Returns the evaluation of the passed ActionTarget</returns>
    public abstract EnemyAIAction GetEnemyAIAction(ActionTarget target);

    /// <summary>
    /// Attempt to perform the passed ActionTarget option. If the Action can be performed, start performing it and return true.
    /// Otherwise, return false do nothing.
    /// </summary>
    /// <param name="target">The ActionTarget option which should be checked and maybe started</param>
    /// <param name="onActionComplete">The callback to be stored for when the ActionTarget completes</param>
    /// <returns>Returns whether or not the Action can be performed with the passed ActionTarget</returns>
    public abstract bool TryTakeAction(ActionTarget target, Action onActionComplete);

    /// <summary>
    /// Calculate the possible valid ActionTargets for this Action
    /// </summary>
    public abstract void CalculateActionTargets();

    /// <summary>
    /// Return the Action's name
    /// </summary>
    /// <returns>Return the Action's name</returns>
    public string GetActionName()
    {
        //Return the Action's name
        return actionName;
    }//end GetActionName

    /// <summary>
    /// Return the Actions's icon sprite
    /// </summary>
    /// <returns>Return the Actions's icon sprite</returns>
    public Sprite GetActionIcon()
    {
        //Return the Actions's icon sprite
        return actionIcon;
    }//end GetActionIcon

    /// <summary>
    /// Return the action point cost of the Action
    /// </summary>
    /// <returns>Return the action point cost of the Action</returns>
    public virtual int GetActionPointCost()
    {
        //Return the action point cost of the Action
        return actionPointCost;
    }//end GetActionPointCost

    /// <summary>
    /// Sets the Action as active, stores the completion callback, and trigger the action start event
    /// </summary>
    /// <param name="onActionComplete">The callback to be stored</param>
    protected void StartAction(Action onActionComplete)
    {
        //Mark the Action as active
        isActive = true;

        //Store the action completion callback
        this.onActionComplete = onActionComplete;

        //Call the event for when any Action starts
        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }//end StartAction

    /// <summary>
    /// Sets the Action as inactive, invokes the completion callback, and trigger the action completion event
    /// </summary>
    protected void CompleteAction()
    {
        //Mark the Aciton as inactive
        isActive = false;

        //Invoke the action completion callback
        onActionComplete();

        //Call the event for when any Action completes
        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }//end CompleteAction

    /// <summary>
    /// Check and return whether or not the Action could perform the passed ActionTarget
    /// </summary>
    /// <param name="targetToCheck">The ActionTarget to validate</param>
    /// <returns>Return true if this is a valid ActionTarget and false otherwise</returns>
    public bool HasActionTarget(ActionTarget targetToCheck)
    {
        //Start returning false
        bool hasActionTarget = false;

        //Loop through each valid ActionTarget
        foreach (ActionTarget t in targets)
        {
            //Check if the two ActionTargets both have targetUnits
            if(t.targetUnit != null && targetToCheck.targetUnit != null)
            {
                //Check if the targetUnits of the two ActionTargets match
                if (t.targetUnit.Equals(targetToCheck.targetUnit))
                {
                    //Return true because the targetUnits matched
                    hasActionTarget = true;
                }
            }

            //Check if the two ActionTargets both have targetTiles
            if (t.targetTile != null && targetToCheck.targetTile != null)
            {
                //Check if the targetTiles of the two ActionTargets match
                if (t.targetTile.Equals(targetToCheck.targetTile))
                {
                    //Return true because the targetTiles matched
                    hasActionTarget = true;
                }
            }

            //Check if the two ActionTargets both have interactables
            if (t.targetInteractable != null && targetToCheck.targetInteractable != null)
            {
                //Check if the interactables of the two ActionTargets match
                if (t.targetInteractable.Equals(targetToCheck.targetInteractable))
                {
                    //Return true because the interactables matched
                    hasActionTarget = true;
                }
            }
        }

        //Return the stored validation
        return hasActionTarget;
    }//end HasActionTarget

    #endregion
}
