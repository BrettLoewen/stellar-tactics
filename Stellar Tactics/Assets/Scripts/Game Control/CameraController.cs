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

    [SerializeField] private GameObject actionCamera;
    private CinemachineImpulseSource cinemachineImpulseSource;

    private Transform focusTransform;

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
    }//end Update

    private void OnDestroy()
    {
        BaseAction.OnAnyActionStarted -= BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;
    }

    #endregion //end Unity Control Methods

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

    
    public void SetFocus(Transform focus)
    {
        focusTransform = focus;
    }

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

    private void BaseAction_OnAnyActionStarted(object sender, EventArgs e)
    {
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
                //Vector3 actionCameraPosition = shooterUnit.transform.position + cameraCharacterHeight + shoulderOffset + (shootDirection * -2f);
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
        switch (sender)
        {
            case ShootAction shootAction:
                //
                HideActionCamera();
                break;
        }
    }
}
