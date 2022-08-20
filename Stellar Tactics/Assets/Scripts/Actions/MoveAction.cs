using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{
    #region Variables

    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [Header("Move Action")]
    [SerializeField] private float moveAmount = 5f; //The distance the unit can move in a single action
    [SerializeField] private float moveSpeed = 4f;  //The speed the unit can move at

    //private List<Tile> walkableTiles = new List<Tile>();
    private Stack<Tile> path = new Stack<Tile>();
    private Vector3 nextTilePosition;
    private Vector3 endTilePosition;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected override void Awake()
    {
        //
        base.Awake();

        //
        endTilePosition = transform.position;
    }//end Awake

    // Update is called once per frame
    void Update()
    {
        if(!isActive)
        {
            endTilePosition = transform.position;
            return;
        }

        float stoppingDistance = 0.1f;
        float turnSpeed = 10f;

        if (Vector3.Distance(nextTilePosition, transform.position) > stoppingDistance)
        {
            //Get the direction the unit needs to move in
            Vector3 moveDirection = (nextTilePosition - transform.position).normalized;

            //Move the unit in the desired direction
            transform.position += moveDirection * Time.deltaTime * moveSpeed;

            //Tell the transform to point in the direction of movement
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, turnSpeed * Time.deltaTime);

            //Tell the animator that the unit is moving
            OnStartMoving?.Invoke(this, EventArgs.Empty);
        }
        else if(Vector3.Distance(endTilePosition, transform.position) > stoppingDistance)
        {
            nextTilePosition = path.Pop().transform.position;
        }
        else
        {
            //Tell the animator that the unit is not moving
            OnStopMoving?.Invoke(this, EventArgs.Empty);

            PlayerController.Instance.SetSelectedAction(this);

            CompleteAction();
        }
    }//end Update

    #endregion //end Unity Control Methods

    #region


    public override bool TryTakeAction(ActionTarget target, Action onActionComplete)
    {
        //Ensure the pathfinding to the target tile is correct
        CalculateWalkableTiles(unit.GetStandingTile(), moveAmount);

        //
        bool canMove = false;

        //
        if (HasActionTarget(target) && unit.TryPerformAction(this))
        {
            //
            path = CalculatePathToTile(target.targetTile);

            //
            nextTilePosition = path.Pop().transform.position;

            //
            endTilePosition = target.targetTile.transform.position;

            //
            TileManager.Instance.ResetTilemapPathfinding();

            //
            canMove = true;

            //
            StartAction(onActionComplete);
        }

        //
        return canMove;
    }


    public override void CalculateActionTargets()
    {
        //Ensure that the unit knows which tile it is currently standing on
        unit.FindStandingTile();

        //Get all of the tiles that the unit can walk to
        targets = CalculateWalkableTiles(unit.GetStandingTile(), moveAmount);

        //Loop through each walkable tile
        foreach (Tile tile in GetWalkableTiles())
        {
            //Tell those tiles that they are walkable
            tile.SetTileState(TileState.Walkable);
            tile.walkable = true;
        }
    }


    public List<Tile> GetWalkableTiles()
    {
        //
        List<Tile> walkableTiles = new List<Tile>();

        //
        foreach(ActionTarget target in targets)
        {
            //
            walkableTiles.Add(target.targetTile);
        }

        //
        return walkableTiles;
    }
    
    /// <summary>
     /// 
     /// </summary>
     /// <param name="startingTile"></param>
     /// <param name="walkDistance"></param>
     /// <returns></returns>
    public List<ActionTarget> CalculateWalkableTiles(Tile startingTile, float walkDistance)
    {
        //Reset the breadth-first-search pathfinding variables on the tilemap
        TileManager.Instance.ResetTilemapPathfinding();

        //Create a list to store the valid walkable tiles that are found
        List<ActionTarget> walkableTiles = new List<ActionTarget>();

        //Create a queue which new valid tiles will be added to in before being fully checked
        Queue<Tile> tilesToCheck = new Queue<Tile>();

        //Add the starting tile to the walkable tiles list
        tilesToCheck.Enqueue(startingTile);

        //Mark the starting tile as visited by the algorithm
        startingTile.visited = true;

        //While there are still tiles that need to be checked
        while (tilesToCheck.Count > 0)
        {
            //Get the tile that has been in the queue the longest
            Tile tileToCheck = tilesToCheck.Dequeue();

            //If the tile's distance is within the walking distance of the unit AND either the tile is not obstructed OR the tile is the starting tile
            //If the tile can be walked to and can be stood on
            if (tileToCheck.distance <= walkDistance && (tileToCheck.IsObstructed() == false || tileToCheck.Equals(startingTile) == true))
            {
                //The tile being checked is valid, so add it to the list of walkable tiles
                walkableTiles.Add(new ActionTarget(tileToCheck));

                //Get the valid tile's links to other tiles to check them
                foreach (TileLink link in tileToCheck.GetTileLinks())
                {
                    //Get the walking distance to the tile this link leads to
                    float distance = tileToCheck.distance + link.GetLinkDistance();

                    //
                    if (link.GetTileLinkType() != TileLinkType.Wall)
                    {
                        //If the linked tile has not already been visited by the algorithm OR the new path for this tile is faster than the old one
                        if (link.GetDestinationTile().visited == false || link.GetDestinationTile().distance > distance)
                        {
                            //Give the linked tile the path that leads to it
                            link.GetDestinationTile().parent = tileToCheck;

                            //Mark the linked tile as visited by the algorithm
                            link.GetDestinationTile().visited = true;

                            //Record the walking distance to the linked tile 
                            link.GetDestinationTile().distance = distance;

                            //Add the linked tile to the queue for further validation later
                            tilesToCheck.Enqueue(link.GetDestinationTile());
                        }
                    }
                }
            }
        }

        //Remove the starting tile from the list of walkable tiles
        walkableTiles.RemoveAt(0);

        //Return a list containing all of the valid tiles that could be walked to
        return walkableTiles;
    }//end CalculateWalkableTiles

    public Stack<Tile> CalculatePathToTile(Tile endingTile)
    {
        //
        Stack<Tile> path = new Stack<Tile>();

        //
        path.Push(endingTile);

        //
        Tile current = endingTile;

        //
        while (current.parent != null)
        {
            //
            path.Push(current.parent);

            //
            current = current.parent;
        }

        //
        return path;
    }//end CalculatePathToTile


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
        validActions.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);

        //
        return validActions[0];
    }//end GetBestEnemyAIAction


    public override EnemyAIAction GetEnemyAIAction(ActionTarget target)
    {
        //
        int targetCountAtTile = unit.GetShootAction().CalculateShootableTargetsCount(target.targetTile);

        //
        return new EnemyAIAction
        {
            target = target,
            actionValue = targetCountAtTile * 10,
        };
    }//end GetEnemyAIAction

    #endregion
}
