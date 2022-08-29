using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//BUGS
//If Unit dies while selected, selectedUnit becomes null. Solution: deselect Units when they die

public class PlayerController : UnitController
{
    #region Variables

    public static PlayerController Instance { get; private set; }   //Singleton Instance of the PlayerController

    //Variables
    [SerializeField] private Unit selectedUnit;     //Stores the currently selected unit
    [SerializeField] private LayerMask unitMask;    //The layer mask for selected units
    public event EventHandler OnSelectedUnitChanged;    //An event that is invoked whenever a new unit is selected

    private Tile selectedTile;
    [SerializeField] private LayerMask tileMask;    //The layer mask for selecting tiles

    [SerializeField] private LayerMask interactableMask;

    public event EventHandler OnSelectedActionChanged;    //An event that is invoked whenever a new action is selected
    private BaseAction selectedAction;

    public event EventHandler<bool> OnBusyChanged;  //
    private bool isBusy;

    [SerializeField] private PathRenderer pathRenderer;
    [SerializeField] private PathRenderer arcRenderer;

    private List<Destructible> destructiblesToHighlight = new List<Destructible>();

    #endregion //end Variables

    #region Unity Control Methods

    protected override void Awake()
    {
        //If Instance does not exist yet, this instance should be the Instance
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one PlayerController in the scene " + transform + " - " + Instance);
        }

        base.Awake();

        SpawnUnits();
    }//end Awake

    private void Start()
    {
        //
        pathRenderer.Disable();

        //
        SetSelectedUnit(units[0]);
    }//end Start

    private void Update()
    {
        if(isBusy)
        {
            PlayerInputHandler.leftClickFlag = false;
            PlayerInputHandler.rightClickFlag = false;

            pathRenderer.Disable();
            arcRenderer.Disable();

            return;
        }

        //If it is not my turn, do nothing
        if(GameManager.Instance.IsMyTurn(teamID) == false)
        {
            return;
        }

        //
        HandleActionVisuals();

        //


        //If the mouse is over UI, do not try to select things
        if(EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //If a new unit was selected, do not try to perform any actions
        if (TryUnitSelection() == true)
        {
            return;
        }

        //Get the tile that the mouse is currently over
        TryTileSelection();

        //See if any actions should be performed
        HandleSelectedAction();
    }//end Update


    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    #endregion //end Unity Control Methods

    #region Unit Selection

    /// <summary>
    /// Try to select a unit that the mouse is pointing at
    /// </summary>
    /// <returns>Returns true if a unit was found and selected and false otherwise</returns>
    private bool TryUnitSelection()
    {
        //Start false for the return value
        bool didSelectUnit = false;

        //
        if (PlayerInputHandler.rightClickFlag)
        {
            PlayerInputHandler.rightClickFlag = false;

            //Get the (unit) collider that the mouse is point at
            Collider collider = CameraController.GetColliderAtMousePosition(unitMask);

            //Ensure a (unit) collider was found
            if (collider != null)
            {
                //Try to get the Unit component from the collider
                if (collider.TryGetComponent(out Unit unit))
                {
                    //Can only select a unit that is on the player's team
                    if(unit.GetTeamID() == teamID)
                    {
                        //Can only select a new unit
                        if (unit.Equals(selectedUnit) == false)
                        {
                            //Set that unit as the newly selected unit
                            SetSelectedUnit(unit);

                            //Store true for the return value
                            didSelectUnit = true;
                        }
                    }
                }
            }
        }        

        //Return the stored return value
        return didSelectUnit;
    }//end HandleUnitSelection

    /// <summary>
    /// Takes a unit, sets it as the selected unit, and invokes an event which says that the selected unit changed
    /// </summary>
    /// <param name="unit">The newly selected unit</param>
    public void SetSelectedUnit(Unit unit)
    {
        //Set the selected unit to be the passed unit
        selectedUnit = unit;

        //
        SetSelectedAction(selectedUnit.GetMoveAction());

        //
        CameraController.Instance.SetFocus(unit.transform);

        //If the event has subscribers (is not null), invoke it
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }//end SetSelectedUnit

    /// <summary>
    /// Returns the currently selected unit
    /// </summary>
    /// <returns>Returns the currently selected unit</returns>
    public Unit GetSelectedUnit()
    {
        //Return the selected unit
        return selectedUnit;
    }//end GetSelectedUnit

    #endregion //end Unit Selection

    #region Action Management

    private void SetBusy()
    {
        isBusy = true;

        //If the event has subscribers (is not null), invoke it
        OnBusyChanged?.Invoke(this, isBusy);
    }//end SetBusy

    private void ClearBusy()
    {
        isBusy = false;

        //If the event has subscribers (is not null), invoke it
        OnBusyChanged?.Invoke(this, isBusy);
    }//end ClearBusy

    public void SetSelectedAction(BaseAction action)
    {
        //
        selectedAction = action;

        //
        PlayerInputHandler.leftClickFlag = false;

        //
        selectedAction.CalculateActionTargets();

        //If the event has subscribers (is not null), invoke it
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }//end SetSelectedAction


    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }


    private void HandleSelectedAction()
    {
        //
        if (PlayerInputHandler.leftClickFlag)
        {
            PlayerInputHandler.leftClickFlag = false;

            //
            ActionTarget actionTarget = GenerateActionTarget();

            //
            if(selectedAction.TryTakeAction(actionTarget, ClearBusy))
            {
                SetBusy();
            }
        }
    }//end HandleSelectedAction


    private ActionTarget GenerateActionTarget()
    {
        //
        TryTileSelection();

        //
        ActionTarget actionTarget = new ActionTarget(selectedTile);

        //
        Unit hoveredUnit = null;

        //Get the (Unit) collider that the mouse is point at
        Collider collider = CameraController.GetColliderAtMousePosition(unitMask);

        //Ensure a (Unit) collider was found
        if (collider != null)
        {
            //Try to get the Unit component from the collider
            collider.TryGetComponent(out hoveredUnit);
        }

        //
        actionTarget.targetUnit = hoveredUnit;

        //
        IInteractable interactable = null;

        //Get the (Interactable) collider that the mouse is point at
        collider = CameraController.GetColliderAtMousePosition(interactableMask);

        //Ensure a (Interactable) collider was found
        if (collider != null)
        {
            //Try to get the Interactable component from the collider
            collider.TryGetComponent(out interactable);
        }

        //
        actionTarget.targetInteractable = interactable;

        //
        return actionTarget;
    }

    
    private void HandleActionVisuals()
    {   
        //
        pathRenderer.Disable();
        arcRenderer.Disable();

        //
        foreach (Destructible d in destructiblesToHighlight)
        {
            d.HideDestructibleHighlight();
        }

        //
        destructiblesToHighlight.Clear();

        //
        switch (selectedAction)
        {
            //
            case MoveAction moveAction:
                //
                if (moveAction.GetWalkableTiles().Contains(selectedTile))
                {
                    //
                    pathRenderer.DisplayPath(moveAction.CalculatePathToTile(selectedTile));
                }

                break;
            //
            case GrenadeAction grenadeAction:
                //
                List<ActionTarget> targets = grenadeAction.CalculateGrenadableTiles(selectedUnit.GetStandingTile(), grenadeAction.GetThrowRange());
                
                //
                if(grenadeAction.HasActionTarget(new ActionTarget(selectedTile)))
                {
                    //
                    arcRenderer.DisplayArc(selectedUnit.GetGrenadeThrowPoint().position, selectedTile.transform.position, selectedUnit.GetGrenadeAction().GetArcYCurve(), selectedUnit.GetGrenadeAction().GetArcSamplePoints());

                    //
                    float explosionRadius = grenadeAction.GetExplosionRadius();

                    //
                    foreach (Destructible d in destructiblesToHighlight)
                    {
                        d.HideDestructibleHighlight();
                    }
                    destructiblesToHighlight.Clear();

                    //
                    Collider[] colliders = Physics.OverlapSphere(selectedTile.transform.position, (explosionRadius * 2f) + 2f, tileMask);

                    //
                    foreach (Collider collider in colliders)
                    {
                        //
                        if (collider.TryGetComponent(out Tile tile) && Vector3.Distance(selectedTile.transform.position, collider.transform.position) <= (explosionRadius * 2f))
                        {
                            //
                            tile.grenadable = true;
                        }
                    }

                    //
                    colliders = Physics.OverlapSphere(selectedTile.transform.position, (explosionRadius * 2f), grenadeAction.GetDestructibleMask());

                    //
                    foreach(Collider collider in colliders)
                    {
                        //
                        if (collider.TryGetComponent(out Destructible destructible))
                        {
                            //
                            destructible.ShowDestructibleHighlight();

                            //
                            destructiblesToHighlight.Add(destructible);
                        }
                    }
                }

                break;
        }
    }

    #endregion //end Action Management

    /// <summary>
    /// Try to select a tile that the mouse is pointing at
    /// </summary>
    /// <returns>Returns true if a tile was found and selected and false otherwise</returns>
    private bool TryTileSelection()
    {
        //Start false for the return value
        bool didFindTile = false;

        //Get the (tile) collider that the mouse is point at
        Collider collider = CameraController.GetColliderAtMousePosition(tileMask);

        //Ensure a (tile) collider was found
        if (collider != null)
        {
            //Try to get the Tile component from the collider
            if (collider.TryGetComponent(out Tile tile))
            {
                //Set that tile as the newly selected tile
                SetSelectedTile(tile);

                //Store true for the return value
                didFindTile = true;
            }
        }

        //Return the stored return value
        return didFindTile;
    }


    private void SetSelectedTile(Tile tile)
    {
        if(selectedTile != null && tile.Equals(selectedTile) == false)
        {
            selectedTile.selected = false;
        }

        selectedTile = tile;
        selectedTile.selected = true;
        tile.SetTileState(TileState.Selected);
    }
}
