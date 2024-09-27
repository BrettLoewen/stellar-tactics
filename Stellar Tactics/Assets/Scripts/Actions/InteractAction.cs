using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractAction : BaseAction
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableMask;


    // Awake is called before Start before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        //
        if (!isActive)
        {
            return;
        }
    }


    public override bool TryTakeAction(ActionTarget target, Action onActionComplete)
    {
        //Store whether or not the spin action is performed, start false
        bool canInteract = false;

        //Tell the unit to attempt to perform a spin
        if (HasActionTarget(target) && unit.TryPerformAction(this))
        {
            //Store that a spin was performed
            canInteract = true;

            //
            this.target = target;

            //
            target.targetInteractable.Interact(OnInteractComplete);

            //Trigger the spin to start
            StartAction(onActionComplete);
        }

        //Return whether or not the interact action is performed
        return canInteract;
    }


    private void OnInteractComplete()
    {
        onActionComplete();
    }


    public override void CalculateActionTargets()
    {
        targets = CalculateInteractTargets(unit.GetStandingTile(), interactRange);
    }

    //
    private List<ActionTarget> CalculateInteractTargets(Tile startingTile, float interactRange)
    {
        //Reset the breadth-first-search pathfinding variables on the tilemap
        TileManager.Instance.ResetTilemapPathfinding();

        //Create a List of ActionTargets to stores the Interactables which are in interact range
        List<ActionTarget> targets = new List<ActionTarget>();

        //Get all of the Interactables in the interact area (+ 2f to ensure we get everything)
        Collider[] colliders = Physics.OverlapSphere(startingTile.transform.position, (interactRange + 2f) * 2f, interactableMask);

        //Loop through the found Interactable colliders
        foreach (Collider collider in colliders)
        {
            //
            if (collider.TryGetComponent(out IInteractable interactable))
            {
                //
                if (Vector3.Distance(collider.transform.position, unit.transform.position) <= interactRange * 2f)
                {
                    //
                    targets.Add(new ActionTarget(interactable));
                }
            }
        }

        //Return the List of Interactables which are in interact range
        return targets;
    }


    public override EnemyAIAction GetBestEnemyAIAction()
    {
        //Calculate and return the appropriate EnemyAIAction for the InteractAction
        return GetEnemyAIAction(new ActionTarget(unit.GetStandingTile()));
    }


    public override EnemyAIAction GetEnemyAIAction(ActionTarget target)
    {
        //Make a new EnemyAIAction variable with minimal priority
        return new EnemyAIAction
        {
            target = target,
            actionValue = 1,
        };
    }
}
