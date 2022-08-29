using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileLinkType { Walk, Wall, Ladder, Vault, LongVault }

[System.Serializable]
public class TileLink
{
    #region Variables

    [SerializeField] private Tile destinationTile;  //The tile that this TileLink links to
    [SerializeField] private TileLinkType linkType; //The type of this TileLink
    [SerializeField] private float linkDistance;    //The distance needed to walk through this TileLink

    #endregion //end Variables


    public TileLink(Tile destination, TileLinkType type, float distance)
    {
        destinationTile = destination;
        linkType = type;
        linkDistance = distance;
    }//end Constructor

    #region Getters


    public Tile GetDestinationTile()
    {
        return destinationTile;
    }//end GetDestinationTile


    public TileLinkType GetTileLinkType()
    {
        return linkType;
    }//end GetTileLinkType


    public float GetLinkDistance()
    {
        return linkDistance;
    }//end GetLinkDistance

    #endregion //end Getters
}
