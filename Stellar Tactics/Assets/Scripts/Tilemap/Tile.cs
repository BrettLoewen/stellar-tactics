using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : Destructible
{
    #region Variables

    private TileManager tileManager;    //Used to store the reference to the TileManager

    [Header("Tile")]
    [SerializeField] private Transform[] tileDetectPoints;  //The transforms that will be used to find Tiles to link to
    private float tileDetectDistance = 100f;                //The vertical distance that will be used to find Tiles to link to
    [SerializeField] private LayerMask tileDetectMask;      //The LayerMask that will be used to find Tiles to link to

    [SerializeField] private List<TileLink> tileLinks = new List<TileLink>();   //The list of TileLinks for this Tile

    [SerializeField] private Transform obstacleDetectPoint; //The transform that will be used find obstacles (Obstacles and Units)
    private float obstacleDetectRadius = 0.5f;              //The radius that will be used find obstacles (Obstacles and Units)
    [SerializeField] private LayerMask obstacleDetectMask;  //The LayerMask that will be used find obstacles (Obstacles and Units)
    [SerializeField] private LayerMask unitDetectMask;      //The LayerMask that will be used find just Units

    [SerializeField] private Transform sightPoint;      //The transform that will be used to calculate lines of sight

    [SerializeField] private Renderer visualRenderer;   //The graphic used to reveal tile calculations like selection, pathfinding, and line of sight

    private TileState tileState;
    public bool selected;
    public bool walkable;
    public bool shootable;
    public bool target;
    public bool grenadable;
    public bool interactable;

    public float distance;
    public Tile parent;
    public bool visited;

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
        tileManager = TileManager.Instance;
    }//end Start

    // Update is called once per frame
    void Update()
    {
        visualRenderer.material.color = tileManager.GetTileStateColor(selected, walkable, shootable, target, grenadable, interactable);
    }//end Update

    private void OnDestroy()
    {
        TileManager.Instance.QueueRebakeTilemap();
    }//end OnDestroy

    #endregion //end Unity Control Methods

    #region Tile Link Management


    public void SetTileState(TileState state)
    {
        tileState = state;
    }

    /// <summary>
    /// Bake the straight TileLinks for this Tile where possible
    /// </summary>
    public void BakeStraightTileLinks()
    {
        //Bake the straight links
        BakeStraightLink(tileDetectPoints[0]);
        BakeStraightLink(tileDetectPoints[1]);
        BakeStraightLink(tileDetectPoints[2]);
        BakeStraightLink(tileDetectPoints[3]);
    }//end BakeTileLinks

    /// <summary>
    /// Bake the diagonal TileLinks for this Tile where possible
    /// </summary>
    public void BakeDiagonalTileLinks()
    {
        //Bake the straight links
        TileLink forward = FindStraightLink(tileDetectPoints[0]);
        TileLink right = FindStraightLink(tileDetectPoints[1]);
        TileLink backward = FindStraightLink(tileDetectPoints[2]);
        TileLink left = FindStraightLink(tileDetectPoints[3]);

        //Bake the diagonal links using the straight links to check diagonal requirements
        BakeDiagonalLink(tileDetectPoints[4], forward, right);
        BakeDiagonalLink(tileDetectPoints[5], right, backward);
        BakeDiagonalLink(tileDetectPoints[6], backward, left);
        BakeDiagonalLink(tileDetectPoints[7], left, forward);
    }//end BakeTileLinks

    /// <summary>
    /// Bake a TileLink which uses a cardinal direction and returns the TileLink created
    /// </summary>
    /// <param name="tileDetectPoint">A transform which represents the location to check for a tile to link to</param>
    /// <returns>Returns the TileLink created by this method</returns>
    private void BakeStraightLink(Transform tileDetectPoint)
    {
        //Send a ray downward from the passed point to see if a tile could be linked to
        if(Physics.Raycast(tileDetectPoint.position, Vector3.down, out RaycastHit hitInfo, tileDetectDistance, tileDetectMask))
        {
            //Get the collider that was hit
            Collider collider = hitInfo.collider;

            //Ensure that a collider was actually found
            if (collider != null)
            {
                //Try to get the Tile component from the collider
                if (collider.TryGetComponent(out Tile tile))
                {
                    //Ensure that this tile does not already have a link to this tile
                    if(HasLinkToTile(tile) == false)
                    {
                        //Create a new TileLink to the found tile and add the TileLink to this Tile
                        tileLinks.Add(new TileLink(tile, TileLinkType.Walk, 1f));
                    }
                }
            }
        }
    }//end BakeStraightLinks


    private TileLink FindStraightLink(Transform tileDetectPoint)
    {
        //Send a ray downward from the passed point to see if a tile could be linked to
        if (Physics.Raycast(tileDetectPoint.position, Vector3.down, out RaycastHit hitInfo, tileDetectDistance, tileDetectMask))
        {
            //Get the collider that was hit
            Collider collider = hitInfo.collider;

            //Ensure that a collider was actually found
            if (collider != null)
            {
                //Try to get the Tile component from the collider
                if (collider.TryGetComponent(out Tile tile))
                {
                    //
                    return GetLinkToTile(tile);
                }
            }
        }

        //Return null because no TileLink was made
        return null;
    }//end FindStraightLink

    /// <summary>
    /// Bake a TileLink which uses a diagonal direction
    /// </summary>
    /// <param name="tileDetectPoint">A transform which represents the location to check for a tile to link to</param>
    private void BakeDiagonalLink(Transform tileDetectPoint, TileLink link1, TileLink link2)
    {
        //Diagonal TileLink requirements
        //1: The TileLinks from the starting Tile to nearby Tiles must exist and be walkable
        //2: The destination Tile of the diagonal TileLink must exist and be walkable
        //3: The TileLinks from the nearby Tiles to the destination Tile must exist and be walkable

        //First, check the TileLinks from the starting Tile to nearby Tiles. 
        //Return if the requirements are not met, continue if they are met

        //If the first TileLink needed to allow a diagonal link does not exist or is not a walkable link, return
        if(link1 == null || link1.GetTileLinkType() != TileLinkType.Walk)
        {
            return;
        }

        //If the second TileLink needed to allow a diagonal link does not exist or is not a walkable link, return
        if (link2 == null || link2.GetTileLinkType() != TileLinkType.Walk)
        {
            return;
        }

        //Second, before checking the other relevant TileLinks, the destination Tile of this potential TileLink must be found
        //Return if the requirements are not met, continue if they are met

        //Create a variable to hold the destination Tile
        Tile tile = null;

        //Send a ray downward from the passed point to see if a tile could be linked to
        if (Physics.Raycast(tileDetectPoint.position, Vector3.down, out RaycastHit hitInfo, tileDetectDistance, tileDetectMask))
        {
            //Get the collider that was hit
            Collider collider = hitInfo.collider;

            //Ensure that a collider was actually found
            if (collider != null)
            {
                //Try to get the Tile component from the collider
                if (collider.TryGetComponent(out tile))
                {
                    //Ensure that this tile does not already have a link to this tile
                    if (HasLinkToTile(tile) != false)
                    {
                        tile = null;
                    }
                }
            }
        }

        //If the destination Tile of this potential TileLink was not found or is invalid, do not continue
        if(tile == null)
        {
            return;
        }

        //Third, now that the destination Tile has been found, check the TileLinks from nearby Tiles to the destination Tile
        //Return if the requirements are not met, continue if they are met

        //Get the third TileLink relevant to the requirements
        TileLink link3 = link1.GetDestinationTile().GetLinkToTile(tile);

        //If the third TileLink needed to allow a diagonal link does not exist or is not a walkable link, return
        if (link3 == null || link3.GetTileLinkType() != TileLinkType.Walk)
        {
            return;
        }

        //Get the fourth TileLink relevant to the requirements
        TileLink link4 = link2.GetDestinationTile().GetLinkToTile(tile);

        //If the fourth TileLink needed to allow a diagonal link does not exist or is not a walkable link, return
        if (link4 == null || link4.GetTileLinkType() != TileLinkType.Walk)
        {
            return;
        }

        //If this point was reached, all of the requirements for diagonal TileLinks were met

        //Create a new TileLink to the found tile
        tileLinks.Add(new TileLink(tile, TileLinkType.Walk, 1.5f));
    }//end BakeDiagonalLinks


    public void AddLinkToTile(TileLink link)
    {
        tileLinks.Add(link);
    }

    /// <summary>
    /// Check whether or not this Tile has a TileLink to the passed Tile
    /// </summary>
    /// <param name="destinationTile">The Tile against which TileLinks will be checked</param>
    /// <returns>Returns true if this Tile has a TileLink to the passed Tile and false otherwise</returns>
    public bool HasLinkToTile(Tile destinationTile)
    {
        //Start false
        bool hasLinkWithTile = false;

        //Loop through each TileLink
        foreach (TileLink link in tileLinks)
        {
            //If the link's destination tile is the passed tile, then we already have a link to that tile, so return true
            if(link.GetDestinationTile().Equals(destinationTile))
            {
                hasLinkWithTile = true;
            }
        }

        //Return the value determined above
        return hasLinkWithTile;
    }//end HasLinkToTile

    /// <summary>
    /// Return the list of TileLinks
    /// </summary>
    /// <returns>Return the list of TileLinks</returns>
    public List<TileLink> GetTileLinks()
    {
        return tileLinks;
    }//end GetTileLinks


    public TileLink GetLinkToTile(Tile destination)
    {
        //Loop through each TileLink
        foreach (TileLink link in tileLinks)
        {
            //If the link's destination tile is the passed tile, then we already have a link to that tile, so return true
            if (link.GetDestinationTile().Equals(destination))
            {
                return link;
            }
        }

        return null;
    }//end GetLinkToTile


    public void ClearTileLinks()
    {
        //
        tileLinks.Clear();
    }

    #endregion

    /// <summary>
    /// Checks if the the tile is obstructed by a Unit or Obstacle and returns the result
    /// </summary>
    /// <returns>Returns whether or not the tile is obstructed by an Obstacle or Unit</returns>
    public bool IsObstructed()
    {
        //Start false for the return value
        bool isObstructed = false;

        //Check if there is a unit standing on this tile
        Collider[] colliders = Physics.OverlapSphere(obstacleDetectPoint.position, obstacleDetectRadius, obstacleDetectMask);

        //If there is a unit standing on this tile
        if(colliders != null && colliders.Length > 0)
        {
            //Get the base class for the obstacle
            if(colliders[0].TryGetComponent(out Destructible obstacle))
            {
                //Store true for the return value
                isObstructed = true;
            }
        }

        //Return the stored return value
        return isObstructed;
    }//end IsObstructed

    /// <summary>
    /// Return the Unit that is standing on the tile and return null if no Unit is present
    /// </summary>
    /// <returns>Return the Unit that is standing on the tile and return null if no Unit is present</returns>
    public Unit GetUnit()
    {
        //Store the found unit, start null
        Unit unit = null;

        //Check if there is a unit standing on this tile
        Collider[] colliders = Physics.OverlapSphere(obstacleDetectPoint.position, obstacleDetectRadius, unitDetectMask);

        //If there is a unit standing on this tile
        if (colliders != null && colliders.Length > 0)
        {
            //If the collider has a unit component
            if(colliders[0].TryGetComponent(out Unit foundUnit))
            {
                //Store the found unit
                unit = foundUnit;
            }
        }

        //Return the found unit
        return unit;
    }//end GetUnit

    /// <summary>
    /// Returns the tile's sight point transform
    /// </summary>
    /// <returns>Returns the tile's sight point transform</returns>
    public Transform GetSightPoint()
    {
        return sightPoint;
    }//end GetSightPoint
}