using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAction : BaseAction
{
    private enum State { Aiming, Shooting, Ending }


    public event EventHandler<OnShootEventArgs> OnShoot;
    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Tile targetTile;
        public Unit shootingUnit;
    }

    [Header("Shoot Action")]
    [SerializeField] private float shootRange = 7f;
    [SerializeField] private int damage = 6;

    [SerializeField] private LayerMask tileMask;
    [SerializeField] private LayerMask sightMask;

    private State state;
    private float stateTimer;


    // Awake is called before Start before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        //
        if(!isActive)
        {
            return;
        }

        //
        stateTimer -= Time.deltaTime;

        //
        switch(state)
        {
            case State.Aiming:
                //
                float turnSpeed = 10f;

                //Get the direction the unit needs to shoot in
                Vector3 aimDirection = (target.targetTile.transform.position - transform.position).normalized;

                //Tell the transform to point in the direction of shooting
                transform.forward = Vector3.Slerp(transform.forward, aimDirection, turnSpeed * Time.deltaTime);
                break;
            case State.Shooting:
                //
                PerformShoot();
                break;
            case State.Ending:
                
                break;
        }

        //
        if (stateTimer <= 0f)
        {
            NextState();
        }
    }


    #region


    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;

                float shootingStateTime = 0f;
                stateTimer = shootingStateTime;

                break;
            case State.Shooting:
                state = State.Ending;

                float endingStateTime = 0.5f;
                stateTimer = endingStateTime;

                break;
            case State.Ending:
                PlayerController.Instance.SetSelectedAction(this);
                CompleteAction();
                break;
        }
    }


    private void PerformShoot()
    {
        //
        Unit targetUnit = target.targetUnit;

        //Tell the UnitAnimator to play a shoot animation
        OnShoot?.Invoke(this, new OnShootEventArgs {
            targetUnit = targetUnit,
            targetTile = target.targetTile,
            shootingUnit = unit,
        });

        //
        CameraController.Instance.Shake();

        //
        Vector3 impactPoint = target.targetTile.GetSightPoint().position + (unit.transform.position - targetUnit.transform.position).normalized;

        //
        targetUnit.TakeDamage(damage, impactPoint);
    }


    public override bool TryTakeAction(ActionTarget target, Action onActionComplete)
    {
        //
        bool canShoot = false;

        //
        if (HasActionTarget(target) && unit.TryPerformAction(this))
        {
            //If the Unit is not null, check if it is what makes the ActionTarget valid and act appropriately
            if(target.targetUnit != null)
            {
                //If the target unit is not on the same team as the attacker, then the Unit makes the ActionTarget valid, so get the Unit's Tile
                if (GameManager.Instance.IsMyTurn(target.targetUnit.GetTeamID()) == false)
                {
                    target.targetTile = target.targetUnit.GetStandingTile();
                }
                //If the target unit is on the same team as the attacker, then the Tile makes the ActionTarget valid, so get the Tile's Unit
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
            state = State.Aiming;
            float aimingStateTime = 1f;
            stateTimer = aimingStateTime;

            //
            canShoot = true;

            //
            StartAction(onActionComplete);
        }

        //
        return canShoot;
    }


    public override void CalculateActionTargets()
    {
        FindShootableTargetsFromTile(unit.GetStandingTile());
    }

    public List<ActionTarget> CalculateShootableTargets(Tile startingTile, float shootRange, Unit shooter)
    {
        //Reset the breadth-first-search pathfinding variables on the tilemap
        TileManager.Instance.ResetTilemapPathfinding();

        //Create a List of Tiles to stores the Tiles that have valid targets on them
        List<ActionTarget> targets = new List<ActionTarget>();

        //Get the sight position of the shooter's Tile
        Vector3 sightPoint = startingTile.GetSightPoint().position;

        //Get all of the Tiles in the shoot area (+ 2f to ensure we get everything)
        Collider[] colliders = Physics.OverlapSphere(sightPoint, (shootRange + 2f) * 2f, tileMask);

        //Loop through the found Tile colliders
        foreach (Collider collider in colliders)
        {
            //Ensure that the colliders are Tiles and get their Tile components
            if (collider.TryGetComponent(out Tile tile))
            {
                //Get this Tile's sight position
                Vector3 targetPoint = tile.GetSightPoint().position;

                //Calculate the distance between the two sight positions (the distance the bullet would need to travel)
                float distance = Vector3.Distance(sightPoint, targetPoint);

                //If the distance is less than or equal to the shooting range (If the shooter could shoot to this Tile)
                if (distance <= (shootRange * 2f))
                {
                    //Get the direction the bullet would need to shoot
                    Vector3 sightDirection = (targetPoint - sightPoint).normalized;

                    //Do a raycast to see if the bullet would hit anything
                    //If the raycast returns false, nothing intercepted it, so there is a clear line of sight
                    if (Physics.Raycast(sightPoint, sightDirection, distance, sightMask) == false)
                    {
                        //Mark the Tile as shootable for visuals
                        tile.shootable = true;

                        //Get the Unit standing on the Tile if one exists
                        Unit unit = tile.GetUnit();

                        //If there is a Unit standing on the Tile
                        if (unit != null)
                        {
                            //If the Unit is on a different team than the shooter
                            if (shooter.IsEnemy(unit.GetTeamID()))
                            {
                                //Mark the Tile as a target for visuals
                                tile.target = true;

                                //
                                ActionTarget newTarget = new ActionTarget(tile);
                                newTarget.targetUnit = unit;

                                //Add the Tile to the List of valid targets
                                targets.Add(newTarget);
                            }
                        }
                    }
                }
            }
        }

        //Return the List of Tiles which have shootable targets on them
        return targets;
    }//end CalculateShootableTargets


    public int CalculateShootableTargetsCount(Tile shootFromTile)
    {
        //
        FindShootableTargetsFromTile(shootFromTile);

        //
        return targets.Count;
    }


    private void FindShootableTargetsFromTile(Tile shootFromTile)
    {
        targets = CalculateShootableTargets(shootFromTile, shootRange, unit);
    }


    public Unit GetTargetUnit()
    {
        return target.targetUnit;
    }


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
        if(validActions.Count > 0)
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
        //Get the unit this action would shoot
        Unit targetUnit = target.targetUnit;

        //Get the target's health as a value between 0 and 1
        float healthPercent = targetUnit.GetHealthNormalized();

        //
        int multiplier = 100 * damage;

        //Create a new EnemyAIAction with the correct properties for the ShootAction
        return new EnemyAIAction
        {
            target = target,

            //Determine the action value (higher values for targets with lower health)
            actionValue = multiplier + Mathf.RoundToInt((1f - healthPercent) * multiplier),
        };
    }//end GetEnemyAIAction

    #endregion
}
