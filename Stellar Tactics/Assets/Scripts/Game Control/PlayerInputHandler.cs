using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static Vector2 mousePosition;
    public static bool leftClickFlag;
    public static bool rightClickFlag;
    public static Vector2 moveInput;
    public static float lookInput;
    public static float zoomInput;

    public static bool pauseInput;

    public void CollectMousePosition(InputAction.CallbackContext context)
    {
        //
        if (PauseMenu.IsPaused)
        {
            return;
        }

        mousePosition = context.ReadValue<Vector2>();
    }//end CollectMousePosition

    public void CollectMouseLeftClick(InputAction.CallbackContext context)
    {
        //
        if (PauseMenu.IsPaused)
        {
            leftClickFlag = false;
            return;
        }

        if (context.started)
        {
            leftClickFlag = true;
        }
    }//end CollectMouseLeftClick

    public void CollectMouseRightClick(InputAction.CallbackContext context)
    {
        //
        if (PauseMenu.IsPaused)
        {
            return;
        }

        if (context.started)
        {
            rightClickFlag = true;
        }
    }//end CollectMouseRightClick

    public void CollectMoveInput(InputAction.CallbackContext context)
    {
        //
        if (PauseMenu.IsPaused)
        {
            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }//end CollectMoveInput

    public void CollectLookInput(InputAction.CallbackContext context)
    {
        //
        if(PauseMenu.IsPaused)
        {
            return;
        }

        lookInput = context.ReadValue<float>();
    }//end CollectLookInput

    public void CollectZoomInput(InputAction.CallbackContext context)
    {
        //
        if (PauseMenu.IsPaused)
        {
            return;
        }

        zoomInput = context.ReadValue<float>();
    }//end CollectZoomInput

    public void CollectPauseInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            pauseInput = true;
        }
    }//end CollectPauseInput
}
