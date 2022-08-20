using System;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    #region Variables

    [SerializeField] private bool isOpen;

    private float timeToOpen = 1f;
    private float timer;

    private bool isActive;

    private TileNavExtension doorNavExtension;

    private Animator animator;
    private string openStateString = "isOpen";

    private Action onInteractComplete;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        doorNavExtension = GetComponent<TileNavExtension>();
        animator = GetComponent<Animator>();
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        //
        if (isOpen)
        {
            OpenDoor();
        }
        //
        else
        {
            CloseDoor();
        }
    }//end Start

    // Update is called once per frame
    void Update()
    {
        //
        if(!isActive)
        {
            return;
        }

        //
        timer -= Time.deltaTime;

        //
        if(timer <= 0f)
        {
            //
            isActive = true;

            //
            onInteractComplete();
        }
    }//end Update

    #endregion //end Unity Control Methods

    #region


    public void Interact(Action onInteractComplete)
    {
        //
        this.onInteractComplete = onInteractComplete;

        //
        timer = timeToOpen;

        //
        isActive = true;

        //
        AudioManager.Instance.PlaySound("Door");

        //
        if (isOpen)
        {
            CloseDoor();
        }
        //
        else
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        //
        isOpen = true;

        //
        doorNavExtension.enabled = false;

        //
        animator.SetBool(openStateString, true);

        //
        TileManager.Instance.QueueRebakeTilemap();
    }

    private void CloseDoor()
    {
        //
        isOpen = false;

        //
        doorNavExtension.enabled = true;

        //
        animator.SetBool(openStateString, false);

        //
        TileManager.Instance.QueueRebakeTilemap();
    }

    #endregion
}
