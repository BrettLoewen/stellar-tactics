using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAction : BaseAction
{
    private enum State { BeforeHit, AfterHit }

    #region Variables

    public event EventHandler OnMeleeStart;
    public event EventHandler OnMeleeEnd;

    [SerializeField] private int damage = 10;

    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private LayerMask tileMask;

    //private List<Tile> targets = new List<Tile>();
    //private Tile target;

    private State state;
    private float stateTimer;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }//end Awake

    // Update is called once per frame
    void Update()
    {
        //
        if (!isActive)
        {
            return;
        }

        //
        stateTimer -= Time.deltaTime;

        //
        switch (state)
        {
            case State.BeforeHit:
                //
                float turnSpeed = 10f;

                //Get the direction the unit needs to attack in
                Vector3 aimDirection = (target.targetTile.transform.position - transform.position).normalized;

                //Tell the transform to point in the direction of attacking
                transform.forward = Vector3.Slerp(transform.forward, aimDirection, turnSpeed * Time.deltaTime);
                break;
            case State.AfterHit:
                //
                break;
        }

        //
        if (stateTimer <= 0f)
        {
            NextState();
        }
    }//end Update

    #endregion //end Unity Control Methods

    #region


    private void NextState()
    {
        switch (state)
        {
            //
            case State.BeforeHit:
                //
                state = State.AfterHit;

                //
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;

                //
                PerformMelee();

                break;
            //
            case State.AfterHit:
                //
                OnMeleeEnd?.Invoke(this, EventArgs.Empty);

                //
                PlayerController.Instance.SetSelectedAction(this);
                CompleteAction();
                break;
        }
    }

    //
    private void PerformMelee()
    {
        //
        Unit targetUnit = target.targetUnit;

        //
        CameraController.Instance.Shake();

        //
        AudioManager.Instance.PlaySound("Sword");

        //
        Vector3 impactPoint = target.targetTile.GetSightPoint().position + (unit.transform.position - targetUnit.transform.position).normalized;

        //
        targetUnit.TakeDamage(damage, impactPoint);
    }


    public override bool TryTakeAction(ActionTarget target, Action onActionComplete)
    {
        //Store whether or not the melee action is performed, start false
        bool canMelee = false;

        //Tell the unit to attempt a melee attack
        if (HasActionTarget(target) && unit.TryPerformAction(this))
        {
            //If the Unit is not null, check if it is what makes the ActionTarget valid and act appropriately
            if (target.targetUnit != null)
            {
                //If the target unit is on the same team as the attacker, then the Unit makes the ActionTarget valid, so get the Unit's Tile
                if (target.targetUnit.IsEnemy() == GameManager.Instance.IsPlayerTurn())
                {
                    target.targetTile = target.targetUnit.GetStandingTile();
                }
                //If the target unit is not on the same as the attacker, then the Tile makes the ActionTarget valid, so get the Tile's Unit
                else
                {
                    target.targetUnit = target.targetTile.GetUnit();
                }
            }
            //If the target Tile is not null, then the Tile makes the ActionTarget valid, so get the Tile's Unit
            else if (target.targetTile != null)
            {
                target.targetUnit = target.targetTile.GetUnit();
            }

            //
            this.target = target;

            //
            state = State.BeforeHit;
            float beforeHitStateTime = 0.7f;
            stateTimer = beforeHitStateTime;

            //
            canMelee = true;

            //
            OnMeleeStart?.Invoke(this, EventArgs.Empty);

            //Trigger the melee to start
            StartAction(onActionComplete);
        }

        //Return whether or not the melee action is performed
        return canMelee;
    }


    public override void CalculateActionTargets()
    {
        targets = CalculateMeleeTargets(unit.GetStandingTile(), meleeRange, unit);
    }

    //
    private List<ActionTarget> CalculateMeleeTargets(Tile startingTile, float meleeRange, Unit attacker)
    {
        //Reset the breadth-first-search pathfinding variables on the tilemap
        TileManager.Instance.ResetTilemapPathfinding();

        //Create a List of Tiles to stores the Tiles to which a melee attack could reach
        List<ActionTarget> targets = new List<ActionTarget>();

        //Get all of the Tiles in the melee area (+ 2f to ensure we get everything)
        Collider[] colliders = Physics.OverlapSphere(startingTile.transform.position, (meleeRange + 2f) * 2f, tileMask);

        //Loop through the found Tile colliders
        foreach (Collider collider in colliders)
        {
            //Ensure that the colliders are Tiles and get their Tile components
            if (collider.TryGetComponent(out Tile tile))
            {
                //Ensure that the Tile is not occupied by an Obstacle
                if (tile.IsObstructed() == false || tile.GetUnit() != null)
                {
                    //Calculate the distance between the two Tile positions (the distance the Unit can attack across)
                    float distance = Vector3.Distance(startingTile.transform.position, tile.transform.position);

                    //If the distance is less than or equal to the melee range (If the attacker could melee to this Tile)
                    if (distance <= (meleeRange * 2f))
                    {
                        //Mark the Tile as shootable for visuals
                        tile.shootable = true;

                        //Get the Unit standing on the Tile if one exists
                        Unit unit = tile.GetUnit();

                        //If there is a Unit standing on the Tile
                        if (unit != null)
                        {
                            //If the Unit is on a different team than the attacker
                            if (unit.IsEnemy() != attacker.IsEnemy())
                            {
                                //Mark the Tile as a target for visuals
                                tile.target = true;

                                //Add the Tile to the List of valid targets
                                targets.Add(new ActionTarget(tile) { targetUnit = unit, });
                            }
                        }
                    }
                }
            }
        }

        //Return the List of Tiles to which a melee attack could reach
        return targets;
    }

    //
    public override EnemyAIAction GetBestEnemyAIAction()
    {
        //
        List<EnemyAIAction> validActions = new List<EnemyAIAction>();

        //
        CalculateActionTargets();

        //
        foreach (ActionTarget target in targets)
        {
            //
            EnemyAIAction action = GetEnemyAIAction(target);
            validActions.Add(action);
        }

        //
        if (validActions.Count > 0)
        {
            //
            validActions.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);

            //
            return validActions[0];
        }
        //
        else
        {
            return null;
        }
    }//end GetBestEnemyAIAction


    public override EnemyAIAction GetEnemyAIAction(ActionTarget target)
    {
        //Get the unit this action would melee
        Unit targetUnit = target.targetUnit;

        //Get the target's health as a value between 0 and 1
        float healthPercent = targetUnit.GetHealthNormalized();

        //
        int multiplier = 100 * damage;

        //Create a new EnemyAIAction with the correct properties for the MeleeAction
        return new EnemyAIAction
        {
            target = target,

            //Determine the action value (higher values for targets with lower health)
            actionValue = multiplier + Mathf.RoundToInt((1f - healthPercent) * multiplier),
        };
    }//end GetEnemyAIAction

    #endregion
}
