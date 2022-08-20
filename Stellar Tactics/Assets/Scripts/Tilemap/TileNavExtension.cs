using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNavExtension : MonoBehaviour
{
    #region Variables

    [System.Serializable]
    private class TileExtensionLink
    {
        public Transform startPoint;
        public Transform endPoint;
        public TileLinkType type;
        public float distance;
    }

    [SerializeField] private TileExtensionLink[] extensionLinks;

    [SerializeField] private float tileDetectRadius;
    [SerializeField] private LayerMask tileDetectMask;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    private void OnDestroy()
    {
        TileManager.Instance.QueueRebakeTilemap();
    }//end OnDestroy

    #endregion //end Unity Control Methods

    #region


    public void BakeExtensionLinks()
    {
        //
        foreach (TileExtensionLink link in extensionLinks)
        {
            //
            Collider[] colliders = Physics.OverlapSphere(link.startPoint.position, tileDetectRadius, tileDetectMask);

            //
            if(colliders == null || colliders.Length == 0)
            {
                continue;
            }

            //
            if(colliders[0].TryGetComponent(out Tile startTile) == false)
            {
                continue;
            }

            //
            colliders = Physics.OverlapSphere(link.endPoint.position, tileDetectRadius, tileDetectMask);

            //
            if (colliders == null || colliders.Length == 0)
            {
                continue;
            }

            //
            if (colliders[0].TryGetComponent(out Tile endTile) == false)
            {
                continue;
            }

            //
            startTile.AddLinkToTile(new TileLink(endTile, link.type, link.distance));

            //
            //endTile.AddLinkToTile(new TileLink(startTile, link.type, link.distance));
        }
    }

    #endregion
}
