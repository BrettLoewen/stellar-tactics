using System;
using System.Collections.Generic;
using UnityEngine;

public enum TileState { Inactive, Walkable, Runable, Selected, Shootable }

public class TileManager : MonoBehaviour
{
    #region Variables

    public static TileManager Instance { get; private set; }    //Singleton instance of the TileManager

    private Tile[] tilemap; //An array that holds all of the tiles that comprise the tilemap
    private TileNavExtension[] extensions;

    private bool rebakeTilemap;

    [SerializeField] private Color inactiveColor;
    [SerializeField] private Color walkableColor = Color.white;
    [SerializeField] private Color runableColor = Color.white;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color shootableColor = Color.white;
    [SerializeField] private Color targetColor = Color.white;
    [SerializeField] private Color grenadableColor = Color.white;
    [SerializeField] private Color interactableColor = Color.white;

    [SerializeField] private Transform tilePrefab;

    [SerializeField] private LayerMask tileMask;
    [SerializeField] private LayerMask sightMask;
    List<Vector3> points = new List<Vector3>();
    Vector3 start = Vector3.zero;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        //If Instance does not exist yet, this instance should be the Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one TileManager in the scene " + transform + " - " + Instance);
        }

        //Get the tilemap
        BakeTilemap();

        //
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {

    }//end Start

    private void LateUpdate()
    {
        if(rebakeTilemap)
        {
            BakeTilemap();
        }
    }

    private void OnDestroy()
    {
        BaseAction.OnAnyActionStarted -= BaseAction_OnAnyActionStarted;
    }

    #endregion //end Unity Control Methods

    #region Tilemap Management

    /// <summary>
    /// Finds every object relevant to the tilemap and pathfinding, stores them in appropriate arrays,
    /// and creates all appropriate links between tiles
    /// </summary>
    public void BakeTilemap()
    {
        //
        ClearTilemap();

        //Get all of the Tiles that make up the tilemap
        tilemap = FindObjectsOfType<Tile>();

        //
        extensions = FindObjectsOfType<TileNavExtension>();

        //
        foreach (TileNavExtension extension in extensions)
        {
            //
            if(extension.enabled)
            {
                //
                extension.BakeExtensionLinks();
            }
        }

        //Loop through every Tile in the tilemap
        foreach(Tile tile in tilemap)
        {
            //Tell each Tile to calculate its tile links
            tile.BakeStraightTileLinks();
        }

        //Loop through every Tile in the tilemap
        foreach (Tile tile in tilemap)
        {
            //Tell each Tile to calculate its tile links
            tile.BakeDiagonalTileLinks();
        }
    }//end BakeTilemap


    private void ClearTilemap()
    {
        //
        if(tilemap != null)
        {
            //
            foreach(Tile tile in tilemap)
            {
                //
                tile.ClearTileLinks();
            }
        }

        //
        tilemap = new Tile[0];

        //
        extensions = new TileNavExtension[0];
    }


    public void QueueRebakeTilemap()
    {
        rebakeTilemap = true;
    }

    [ContextMenu("Generate Tilemap")]
    private void GenerateTilemap()
    {
        foreach(Transform trans in transform)
        {
            DestroyImmediate(trans.gameObject);
        }

        for(int i = -20; i <= 20; i += 2)
        {
            for (int j = -20; j <= 20; j += 2)
            {
                Transform tile = Instantiate(tilePrefab, transform);
                tile.position = new Vector3(i, 0f, j);
                tile.name = (int)tile.position.x + " " + tile.position.y + " " + tile.position.z;
            }
        }
    }//end GenerateTilemap

    #endregion //end Tilemap Managment


    

    /// <summary>
    /// Reset the breadth-first-search (BFS) and A* pathfinding variables on the tilemap
    /// </summary>
    public void ResetTilemapPathfinding()
    {
        //
        ResetTilemapVisuals();

        //For every tile in the tilemap
        foreach (Tile tile in tilemap)
        {
            //Reset the tile's distance
            tile.distance = 0f;

            //Reset whether or not the tile has been visited by a pathfinding algorithm
            tile.visited = false;

            //Reset the tile's path to the starting tile
            tile.parent = null;

            //
            tile.SetTileState(TileState.Inactive);
        }
    }//end ResetTilemapPathfinding

    public void ResetTilemapVisuals()
    {
        //For every tile in the tilemap
        foreach (Tile tile in tilemap)
        {
            //
            tile.walkable = false;

            tile.shootable = false;
            tile.target = false;
            tile.grenadable = false;
            tile.interactable = false;
        }
    }


    public Color GetTileStateColor(TileState state)
    {
        Color returnColor = Color.black;

        switch (state)
        {
            case TileState.Inactive:
                returnColor = inactiveColor;
                break;
            case TileState.Walkable:
                returnColor = walkableColor;
                break;
            default:
                returnColor = selectedColor;
                break;
        }

        return returnColor;
    }


    public Color GetTileStateColor(bool selected, bool walkable, bool shootable, bool target, bool grenadable, bool interactable)
    {
        Color returnColor = inactiveColor;

        if(walkable)
        {
            returnColor = walkableColor;
        }

        if(shootable)
        {
            returnColor = shootableColor;
        }

        if(target)
        {
            returnColor = targetColor;
        }

        if(grenadable)
        {
            returnColor = grenadableColor;
        }

        if(interactable)
        {
            returnColor = interactableColor;
        }

        if(selected)
        {
            returnColor = selectedColor;
        }

        return returnColor;
    }

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        ResetTilemapVisuals();
    }

    private void OnDrawGizmos()
    {
        if(start != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.DrawLine(start, points[i]);
                Gizmos.DrawWireSphere(points[i], 0.25f);
            }
        }
    }
}
