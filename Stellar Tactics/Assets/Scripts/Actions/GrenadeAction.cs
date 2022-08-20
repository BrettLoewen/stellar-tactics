using System;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction
{
    #region Variables

    public event EventHandler OnStartThrowGrenade;      //An event to be triggered when this Action starts
    public event EventHandler OnCompleteThrowGrenade;   //An event to be triggered when this Action completes

    [Header("Grenade Action")]
    [SerializeField] private float throwDistance = 7f;  //The distance that grenades can be thrown
    [SerializeField] private AnimationCurve arcYCurve;  //The curve used to define the arc of the grenade
    [SerializeField] private int arcSamplePoints = 6;   //The amount of points along the arc used to draw the visual line
        
    [SerializeField] private float explosionRadius = 2.5f;  //The radius of the explosion
    [SerializeField] private LayerMask destructibleMask;    //The LayerMask used to find Destructibles to damage
    [SerializeField] private int damage = 3;                //The damage that the grenade explosion will do

    [SerializeField] private Grenade grenadePrefab; //The prefab for the grenade projectile

    private bool hasThrownGrenade;          //Store whether or not the grenade projectile has been spawned (action perform state machine)
    private float timeUntilThrow = 1.833f;  //Time during the animation before the grenade projectile is spawned
    private float timer;                    //Used to calculate time for spawning the grenade projectile

    [SerializeField] private LayerMask tileMask;    //The LayerMask used to find grenadable Tiles
    [SerializeField] private LayerMask sightMask;   //The LayerMask used to calculate line of sight

    //private Tile target;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected override void Awake()
    {
        //Call the Awake of BaseAction
        base.Awake();
    }//end Awake

    // Update is called once per frame
    void Update()
    {
        //Only run Update if the Action is active
        if (!isActive)
        {
            return;
        }

        //If the grenade projectile has not been thrown yet, count down until throw
        if(hasThrownGrenade == false)
        {
            //Count down the timer
            timer -= Time.deltaTime;

            //If the timer has ended
            if (timer <= 0f)
            {
                //Throw the grenade
                ThrowGrenade();
            }
        }

        //Set the turn speed used to rotate the Unit to face the throw direction
        float turnSpeed = 10f;

        //Get the direction the unit needs to throw at
        Vector3 aimDirection = (target.targetTile.transform.position - transform.position).normalized;

        //Tell the transform to point in the direction of throwing
        transform.forward = Vector3.Slerp(transform.forward, aimDirection, turnSpeed * Time.deltaTime);
    }//end Update

    #endregion //end Unity Control Methods

    #region

    /// <summary>
    /// Spawn the grenade projectile, set it up to throw correctly, mark that the grenade has been thrown
    /// </summary>
    private void ThrowGrenade()
    {
        //Spawn the grenade projectile at the throw point
        Grenade grenade = Instantiate(grenadePrefab, unit.GetGrenadeThrowPoint().position, Quaternion.identity);

        //Give the grenade projectile the information needed to behave correctly
        grenade.Setup(target.targetTile.transform.position, (explosionRadius * 2f), destructibleMask, damage, OnDetonate, arcYCurve);

        //Mark that the grenade has been thrown
        hasThrownGrenade = true;
    }//end ThrowGrenade

    /// <summary>
    /// A callback for when the grenade detonates
    /// </summary>
    private void OnDetonate()
    {
        //Trigger camera shake
        CameraController.Instance.Shake();

        //Trigger the event that the GrenadeAction has finished
        OnCompleteThrowGrenade?.Invoke(this, EventArgs.Empty);

        //Complete the Action
        CompleteAction();
    }//end OnDetonate

    //Throw a grenade if possible
    public override bool TryTakeAction(ActionTarget target, Action onActionComplete)
    {
        //Store whether or not the grenade action is performed, start false
        bool canThrowGrenade = false;

        //Tell the unit to attempt to throw a grenade
        if (HasActionTarget(target) && unit.TryPerformAction(this))
        {
            //Store to return true
            canThrowGrenade = true;

            //Start the grenade spawn countdown
            timer = timeUntilThrow;

            //Record that the grenade has not been thrown yet
            hasThrownGrenade = false;

            //Set this Action's stored ActionTarget to the passed ActionTarget
            this.target = target;

            //Trigger the event which says that the grenade throw has begun (to play the animation)
            OnStartThrowGrenade?.Invoke(this, EventArgs.Empty);

            //Trigger the throw to start
            StartAction(onActionComplete);
        }

        //Return whether or not the grenade action is performed
        return canThrowGrenade;
    }//end TryTakeAction

    //Calculate which Tiles are grenadable and fill a list of ActionTargets with them
    public override void CalculateActionTargets()
    {
        //Calculate which Tiles are grenadable and store them as a list of ActionTargets
        targets = CalculateGrenadableTiles(unit.GetStandingTile(), throwDistance);
    }//end CalculateActionTargets

    /// <summary>
    /// Find the Tiles in range. For each Tile in range, check if the thrower could see the Tile. If so, make an ActionTarget using it and add
    /// it to the list of ActionTargets to return. Also mark the Tiles as grenadable for visuals
    /// </summary>
    /// <param name="startingTile">The Tile the grenade will be thrown from</param>
    /// <param name="throwRange">The range (in Tiles not units (must be multiplied by 2)) the grenade can be thrown</param>
    /// <returns></returns>
    public List<ActionTarget> CalculateGrenadableTiles(Tile startingTile, float throwRange)
    {
        //Reset the breadth-first-search pathfinding variables on the tilemap
        TileManager.Instance.ResetTilemapPathfinding();

        //Create a List of Tiles to stores the Tiles to which a grenade could be thrown
        List<ActionTarget> targets = new List<ActionTarget>();

        //Get the sight position of the thrower's Tile
        Vector3 sightPoint = startingTile.GetSightPoint().position;

        //Get all of the Tiles in the throw area (+ 2f to ensure we get everything)
        Collider[] colliders = Physics.OverlapSphere(sightPoint, (throwRange + 2f) * 2f, tileMask);

        //Loop through the found Tile colliders
        foreach (Collider collider in colliders)
        {
            //Ensure that the colliders are Tiles and get their Tile components
            if (collider.TryGetComponent(out Tile tile))
            {
                //Ensure that the Tile is not occupied by an Obstacle or Unit
                if (tile.IsObstructed() == false)
                {
                    //Get this Tile's sight position
                    Vector3 targetPoint = tile.GetSightPoint().position;

                    //Calculate the distance between the two sight positions (the distance the grenade would need to travel)
                    float distance = Vector3.Distance(sightPoint, targetPoint);

                    //If the distance is less than or equal to the throwing range (If the thrower could throw to this Tile)
                    if (distance <= (throwRange * 2f))
                    {
                        //Get the direction the bullet would need to shoot
                        Vector3 sightDirection = (targetPoint - sightPoint).normalized;

                        //Do a raycast to see if the grenade would hit anything
                        //If the raycast returns false, nothing intercepted it, so there is a clear line of sight
                        if (Physics.Raycast(sightPoint, sightDirection, distance, sightMask) == false)
                        {
                            //Mark the Tile as shootable for visuals
                            tile.shootable = true;

                            //Add the Tile to the List of valid targets
                            targets.Add(new ActionTarget(tile));
                        }
                    }
                }
            }
        }

        //Return the List of Tiles to which a grenade could be thrown
        return targets;
    }//end CalculateGrenadableTiles

    //Evaluate the ActionTargets to find and return the option that deals the most net damage (damage to enemies - damage to allies)
    public override EnemyAIAction GetBestEnemyAIAction()
    {
        //Create a list to store the possible options that need to be evaluated
        List<EnemyAIAction> validActions = new List<EnemyAIAction>();

        //Calculate the possible ActionTargets for this Action
        CalculateActionTargets();

        //Loop through each possible ActionTarget
        foreach (ActionTarget target in targets)
        {
            //Evaluate the ActionTarget and add the evaluation to the list of options
            EnemyAIAction action = GetEnemyAIAction(target);
            validActions.Add(action);
        }

        //If there is at least one valid option
        if (validActions.Count > 0)
        {
            //Sort the valid options so the best decision (highest actionValue) is at index 0
            validActions.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);

            //Return the best valid option (which thanks to the above sorting is always at index 0)
            return validActions[0];
        }
        //If there are no valid options, return null
        else
        {
            return null;
        }
    }//end GetBestEnemyAIAction

    //Evaluate the passed ActionTarget and return the evaluation of it
    public override EnemyAIAction GetEnemyAIAction(ActionTarget target)
    {
        //Get the possible enemies to target (the Units we want to hit with a grenade)
        List<Unit> playerUnits = GameManager.Instance.GetFriendlyUnitList();

        //Get the Tile that this ActionTarget will throw a grenade to
        Tile targetTile = target.targetTile;

        //Start with an actionValue of 0
        int actionValue = 0;

        //Set the multiplier (more damage = better option)
        int multiplier = 100 * damage;

        //Loop through each possible enemy target
        foreach (Unit unit in playerUnits)
        {
            //Get the distance from the point of explosion to the target Unit
            float distExplToUnit = Vector3.Distance(unit.transform.position, targetTile.transform.position);

            //If the target Unit is within range of the explosino
            if (distExplToUnit <= explosionRadius * 2f)
            {
                //Get this Unit's standing Tile's sight position
                this.unit.FindStandingTile();
                Vector3 targetPoint = this.unit.GetStandingTile().GetSightPoint().position;

                //Get the target Unit's standing Tile's sight position
                unit.FindStandingTile();
                Vector3 sightPoint = unit.GetStandingTile().GetSightPoint().position;

                //Get the direction from the point of explosion to the target Unit
                Vector3 sightDirection = (targetPoint - sightPoint).normalized;

                //Get the distance from the thrower Unit to the target Unit
                float distUnitToUnit = Vector3.Distance(unit.transform.position, this.unit.transform.position);

                //Do a raycast to see if the grenade would hit anything
                //If the raycast returns false, nothing intercepted it, so there is a clear line of sight
                if (Physics.Raycast(sightPoint, sightDirection, distUnitToUnit, sightMask) == false)
                {
                    //Get the target's health as a value between 0 and 1
                    float healthPercent = unit.GetHealthNormalized();

                    //If the target Unit is on my team, reduce the actionValue (we don't want to hit teammates)
                    if (unit.IsEnemy() == this.unit.IsEnemy())
                    {
                        //Reduce the actionValue
                        actionValue -= multiplier + Mathf.RoundToInt((1f - healthPercent) * multiplier);
                    }
                    //If the target Unit is not on my team
                    else
                    {
                        //Increase the actionValue (actionValue == multipler: target has lots of health, actionValue == multiplier * 2: target has no health)
                        //Higher priority to options that hit targets with lower health to increase odds of kills
                        actionValue += multiplier + Mathf.RoundToInt((1f - healthPercent) * multiplier);
                    }
                }
            }
        }

        //Create a new EnemyAIAction with the correct properties for the GrenadeAction
        return new EnemyAIAction
        {
            //Use the passed ActionTarget to set this option's stored ActionTarget
            target = target,

            //Determine the action value using above calculations (higher values for targets with lower health)
            actionValue = actionValue,
        };
    }//end GetEnemyAIAction

    /// <summary>
    /// Return the distance a grenade can be thrown (the Action's attack range)
    /// </summary>
    /// <returns>Return the distance a grenade can be thrown (using Tile count instead of unit distance)</returns>
    public float GetThrowRange()
    {
        //Return the Action's throw range
        return throwDistance;
    }//end GetThrowRange

    /// <summary>
    /// Return the explosion radius of this Action (the distance from the grenade where Destructibles will take damage)
    /// </summary>
    /// <returns>Return the distance from the grenade where Destructibles will take damage (using Tile count instead of unit distance)</returns>
    public float GetExplosionRadius()
    {
        //Return the Action's explosion radius
        return explosionRadius;
    }//end GetExplosionRadius

    /// <summary>
    /// Return the Action's LayerMask for Destructibles
    /// </summary>
    /// <returns>Return the Action's LayerMask for Destructibles</returns>
    public LayerMask GetDestructibleMask()
    {
        //Return the Action's LayerMask for Destructibles
        return destructibleMask;
    }//end GetDestructibleMask

    /// <summary>
    /// Return the Action's AnimationCurve for the Y value of the grenade throw
    /// </summary>
    /// <returns>Return the Action's AnimationCurve for the Y value of the grenade throw</returns>
    public AnimationCurve GetArcYCurve()
    {
        //Return the Action's AnimationCurve for the Y value of the grenade throw
        return arcYCurve;
    }//end GetArcYCurve

    /// <summary>
    /// Return the number of points along the ArcYCurve should be sampled to draw the throw arc visual
    /// </summary>
    /// <returns>Return the number of points along the ArcYCurve should be sampled to draw the throw arc visual</returns>
    public int GetArcSamplePoints()
    {
        //Return the number of points along the ArcYCurve should be sampled to draw the throw arc visual
        return arcSamplePoints;
    }//end GetArcSamplePoints

    #endregion
}
