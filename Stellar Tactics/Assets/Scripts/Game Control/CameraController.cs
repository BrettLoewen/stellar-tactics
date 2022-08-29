using System;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    # region Variables

    public static CameraController Instance { get; private set; }   //Singleton Instance of the CameraController

    public static  Camera mainCamera;               //The main camera used by the game
    private static Vector3 mousePosition;           //Stores the last valid position found at the mouse position
    private static float raycastDistance = 1000f;   //A constant defining the length raycasts can travel from the camera

    [SerializeField] private float moveSpeed = 10f;         //The speed at which the camera will move around the world
    [SerializeField] private float rotationSpeed = 100f;    //The speed at which the camera will move around the world

    [SerializeField] private Vector3 minBounds = new Vector3(-20f, 0f, -20f);
    [SerializeField] private Vector3 maxBounds = new Vector3(20f, 0f, 20f);

    [SerializeField] private float zoomSpeed = 2f;
    private float zoomPercent = 0.5f;
    [SerializeField] private Animator cameraAnimator;

    [SerializeField] private GameObject actionCamera;
    private CinemachineImpulseSource cinemachineImpulseSource;

    private Transform focusTransform;

    [SerializeField] private int obstructionSamplePoints;
    [SerializeField] private float obstructionDetectRadius;
    [SerializeField] private LayerMask obstructionMask;
    private bool isPerformingAction;

    #endregion //end Variables

    #region Unity Control Methods

    private void Awake()
    {
        //If Instance does not exist yet, this instance should be the Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one CameraController in the scene " + transform + " - " + Instance);
        }

        mainCamera = GetComponentInChildren<Camera>();
        cinemachineImpulseSource = GetComponent<CinemachineImpulseSource>();

        focusTransform = null;

        isPerformingAction = false;
    }//end Awake

    private void Start()
    {
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

        HideActionCamera();
    }

    private void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleZoom();

        HideObstructions();
    }//end Update

    private void OnDestroy()
    {
        BaseAction.OnAnyActionStarted -= BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;
    }

    #endregion //end Unity Control Methods

    #region Handle Controls

    private void HandleMovement()
    {
        //
        Vector2 moveInput = PlayerInputHandler.moveInput;

        //
        if (moveInput.magnitude >= 0.1f)
        {
            //
            focusTransform = null;

            //Get the vector that the camera should move in
            Vector3 moveVector = transform.forward * PlayerInputHandler.moveInput.y + transform.right * PlayerInputHandler.moveInput.x;

            //
            Vector3 clampedPosition = transform.position;

            //Move the camera by the movement vector
            clampedPosition += moveVector * moveSpeed * Time.deltaTime;

            //
            float clampedX = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
            float clampedY = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
            float clampedZ = Mathf.Clamp(clampedPosition.z, minBounds.z, maxBounds.z);

            //
            transform.position = new Vector3(clampedX, clampedY, clampedZ);
        }

        //
        else if(focusTransform != null)
        {
            transform.position = Vector3.Slerp(transform.position, focusTransform.position, 0.05f);
        }
    }//end HandleMovement

    private void HandleRotation()
    {
        //Rotate the camera controller on the y-axis by a calculated amount
        transform.eulerAngles += Vector3.up * (PlayerInputHandler.lookInput * rotationSpeed * Time.deltaTime);
    }//end HandleRotation

    private void HandleZoom()
    {
        //
        zoomPercent -= PlayerInputHandler.zoomInput * zoomSpeed * Time.deltaTime;

        //
        zoomPercent = Mathf.Clamp(zoomPercent, 0f, 1f);

        //
        cameraAnimator.SetFloat("zoomPercent", zoomPercent);
    }//end HandleZoom

    
    public void SetFocus(Transform focus)
    {
        focusTransform = focus;
    }

    #endregion Handle Controls

    #region Getters

    /// <summary>
    /// Returns the point in the world that corresponds to the mouse position on the screen
    /// </summary>
    /// <param name="mouseDetectionMask">The layer mask which controls which physics layers will be interacted with while trying
    /// to find the mouse world position</param>
    /// <returns>Returns the point in the world that corresponds to the mouse position on the screen</returns>
    public static Vector3 GetMousePosition(LayerMask mouseDetectionMask)
    {
        Ray ray = mainCamera.ScreenPointToRay(PlayerInputHandler.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, mouseDetectionMask))
        {
            mousePosition = hitInfo.point;
        }

        return mousePosition;
    }//end GetMousePosition

    public static Collider GetColliderAtMousePosition(LayerMask mouseDetectionMask)
    {
        Collider collider = null;

        Ray ray = mainCamera.ScreenPointToRay(PlayerInputHandler.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, raycastDistance, mouseDetectionMask))
        {
            collider = hitInfo.collider;
        }

        return collider;
    }//end GetColliderAtMousePosition

    #endregion Getters

    #region Camera Effects

    public void Shake(float intensity = 1f)
    {
        cinemachineImpulseSource.GenerateImpulse(intensity);
    }

    private void ShowActionCamera()
    {
        actionCamera.SetActive(true);
    }

    private void HideActionCamera()
    {
        actionCamera.SetActive(false);
    }

    private void HideObstructions()
    {
        //Create the start position (The position of the camera)
        Vector3 currentPosition = mainCamera.transform.position;

        //Calculate the total distance from the start position (the camera) to the end position (the controller)
        float totalDistance = Vector3.Distance(currentPosition, transform.position);

        //Calculate the distance between sample points (as a percentage from 0 to 1, ~0: lots of points, ~1: not many points)
        float segmentLength = 1f / (obstructionSamplePoints + 1f);

        //Calculate the movement direction from the camera to the controller
        Vector3 moveDirection = (transform.position - currentPosition).normalized;

        //Loop to calculate the position of all sample points
        for (int i = 0; i <= obstructionSamplePoints; i++)
        {
            //Update the position using the move direction and length of a segment
            currentPosition += moveDirection * segmentLength * totalDistance;

            //Get the colliders that could be Obstructions which are near the current position
            Collider[] colliders = Physics.OverlapSphere(currentPosition, obstructionDetectRadius, obstructionMask);

            //Get the distance between the Camera and the controller
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

            //Loop through each collider
            foreach (Collider collider in colliders)
            {
                //Create a variable to hold the collider's Obstruction componenet if it exists
                Obstruction obstruction = null;

                //Try to get the Obstruction component from the collider if it exists
                if(collider.TryGetComponent(out obstruction) == false)
                {
                    //If the collider did not have an Obstruction component, try to get it from the collider's parent
                    collider.transform.parent.TryGetComponent(out obstruction);
                }

                //If an Obstruction was found
                if(obstruction != null)
                {
                    //If the Obstruction is closer to the Camera than the controller is
                    if (Vector3.Distance(mainCamera.transform.position, obstruction.transform.position) <= distance)
                    {
                        //Hide the Obstruction
                        obstruction.HideObstruction(true, isPerformingAction == false);
                    }
                }
            }
        }
    }

    #endregion Camera Effects

    #region Event Subscriptions

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
        isPerformingAction = true;

        switch(sender)
        {
            case ShootAction shootAction:
                //
                Unit shooterUnit = shootAction.GetUnit();
                Unit targetUnit = shootAction.GetTargetUnit();

                //
                Vector3 cameraCharacterHeight = Vector3.up * 1.7f;

                //
                Vector3 shootDirection = (targetUnit.transform.position - shooterUnit.transform.position).normalized;

                //
                float shoulderOffsetAmount = 0.5f;
                Vector3 shoulderOffset = Quaternion.Euler(0f, 90f, 0f) * shootDirection * shoulderOffsetAmount;

                //
                Vector3 actionCameraPosition = shooterUnit.cameraShoulderPoint.position + shoulderOffset + (shootDirection * -1f);
                
                //
                actionCamera.transform.position = actionCameraPosition;

                //
                actionCamera.transform.LookAt(targetUnit.transform.position + cameraCharacterHeight);
                
                //
                ShowActionCamera();
                break;
        }
    }

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        isPerformingAction = false;

        switch (sender)
        {
            case ShootAction shootAction:
                //
                HideActionCamera();
                break;
        }
    }

    #endregion Event Subscriptions
}
