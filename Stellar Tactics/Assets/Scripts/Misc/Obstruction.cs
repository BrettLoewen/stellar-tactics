using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstruction : MonoBehaviour
{
    #region Variables

    private float timeToUnhide = 0.1f;              //
    private float timer;                            //

    private bool hideObstruction;                   //

    [SerializeField] private Renderer[] renderers;  //The Renderers of the object's graphics
    [SerializeField] private Collider[] colliders;  //

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        hideObstruction = false;
        timer = 0f;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        //
        if(hideObstruction == false)
        {
            return;
        }

        //
        timer -= Time.deltaTime;

        //
        if(timer <= 0f)
        {
            //
            hideObstruction = false;

            //
            ShowRenderers();

            //
            ShowColliders();
        }
    }//end Update

    #endregion //end Unity Control Methods

    /// <summary>
    /// 
    /// </summary>
    public void HideObstruction(bool hideRenderers = true, bool hideColliders = false)
    {
        //
        if(hideRenderers)
        {
            //
            HideRenderers();
        }

        //
        if(hideColliders)
        {
            //
            HideColliders();
        }

        //
        hideObstruction = true;

        //
        timer = timeToUnhide;
    }//end HideObstruction

    #region Manage Renderers

    /// <summary>
    /// Enable all Renderers
    /// </summary>
    private void ShowRenderers()
    {
        //If the array of Renderers exists
        if (renderers != null)
        {
            //Loop through the array of Renderers
            for (int i = 0; i < renderers.Length; i++)
            {
                //
                if (renderers[i] != null)
                {
                    //Enable the Renderer
                    renderers[i].enabled = true;
                }
            }
        }
    }//end ShowRenderers

    /// <summary>
    /// Disable all Renderers
    /// </summary>
    private void HideRenderers()
    {
        //If the array of Renderers exists
        if (renderers != null)
        {
            //Loop through the array of Renderers
            for (int i = 0; i < renderers.Length; i++)
            {
                //
                if (renderers[i] != null)
                {
                    //Disable the Renderer
                    renderers[i].enabled = false;
                }
            }
        }
    }//end HideRenderers

    #endregion //end Manage Renderers


    #region Manage Colliders

    /// <summary>
    /// Enable all Colliders
    /// </summary>
    private void ShowColliders()
    {
        //If the array of Colliders exists
        if (colliders != null)
        {
            //Loop through the array of Colliders
            for (int i = 0; i < colliders.Length; i++)
            {
                //
                if (colliders[i] != null)
                {
                    //Enable the Collider
                    colliders[i].enabled = true;
                }
            }
        }
    }//end ShowColliders

    /// <summary>
    /// Disable all Colliders
    /// </summary>
    private void HideColliders()
    {
        //If the array of Colliders exists
        if (colliders != null)
        {
            //Loop through the array of Colliders
            for (int i = 0; i < colliders.Length; i++)
            {
                //
                if (colliders[i] != null)
                {
                    //Disable the Collider
                    colliders[i].enabled = false;
                }
            }
        }
    }//end HideColliders

    #endregion //end Manage Colliders
}
