using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Destructible
{
    #region Variables

    [Header("Unit")]
    [SerializeField] private UnitAnimator unitAnimator;     //Controls the Unit's animations

    [SerializeField] private GameObject selectionVisual;    //Displays whether or not the Unit is selected

    [SerializeField] private UnitWorldUI worldUI;           //A reference to the Unit's world UI class

    [SerializeField] private Tile standingTile;             //The tile that the Unit is currently standing on
    [SerializeField] private Transform tileDetectPoint;     //The point to check for the tile that the Unit is standing on
    private float tileDetectRadius = 0.3f;                  //The radius to check for the tile that the Unit is standing on
    [SerializeField] private LayerMask tileDetectMask;      //The layer mask to check for the tile that the Unit is standing on

    private MoveAction moveAction;          //The Unit's movement action
    private SpinAction spinAction;          //The Unit's spin action
    private ShootAction shootAction;        //The Unit's shoot action
    private GrenadeAction grenadeAction;
    private MeleeAction meleeAction;
    private InteractAction interactAction;
    private BaseAction[] baseActionArray;   //An array holding all of the Unit's actions

    private const int maxActionPoints = 3;          //The number of action points the Unit will start each turn with
    private int actionPoints = maxActionPoints;     //The number of action points the Unit currently has to work with

    [SerializeField] private bool isEnemy;  //Stores whether or not the Unit is on the player's team or the enemy's team

    [SerializeField] private Transform ragdollPrefab;   //The prefab that will be spawned when the Unit dies
    [SerializeField] private Transform rootBone;        //The root bone of the Unit's character graphic

    public Transform cameraShoulderPoint;   //The transform that will be used to position the third person action camera

    [SerializeField] private Transform grenadeThrowPoint;

    public static event EventHandler OnAnyUnitSpawned;  //An event that is triggered whenever a Unit is spawned
    public static event EventHandler OnAnyUnitDied;     //An event that is triggered whenever a Unit dies
    public event EventHandler OnTakeDamage;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before the first frame start
    protected override void Awake()
    {
        //
        base.Awake();

        //Enable this unit to know when the player selects a unit
        PlayerController.Instance.OnSelectedUnitChanged += PlayerController_OnSelectedUnitChanged;
        GameManager.Instance.OnTurnChanged += GameManager_OnTurnChanged;

        //Tell the UnitAnimator that it corresponds to this Unit
        unitAnimator.SetUnit(this);

        //Ensure the Unit has the correct number of action points at the start of the game
        SetActionPoints(maxActionPoints);

        //Get the various action references
        moveAction = GetComponent<MoveAction>();
        spinAction = GetComponent<SpinAction>();
        shootAction = GetComponent<ShootAction>();
        grenadeAction = GetComponent<GrenadeAction>();
        meleeAction = GetComponent<MeleeAction>();
        interactAction = GetComponent<InteractAction>();
        baseActionArray = GetComponents<BaseAction>();
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        //Update the Unit's selection visual
        UpdateSelectionVisual();

        //Invoke the event saying that a Unit has spawned if there is at least one subscriber
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region

    /// <summary>
    /// Calculate the tile that this Unit is currently standing on
    /// </summary>
    public void FindStandingTile()
    {
        //Get the colliders that could possibly be the tile this unit is standing on
        Collider[] colliders = Physics.OverlapSphere(tileDetectPoint.position, tileDetectRadius, tileDetectMask);

        //If there is at least one valid collider
        if(colliders != null && colliders.Length > 0)
        {
            //Check the first collider found for a tile component
            if(colliders[0].TryGetComponent(out Tile tile))
            {
                //Set this tile as the standing tile
                standingTile = tile;
                standingTile.SetTileState(TileState.Walkable);
            }
        }
    }//end FindStandingTile

    /// <summary>
    /// Enable or diable the selection visual based on whether or not this unit is currently selected
    /// </summary>
    private void UpdateSelectionVisual()
    {
        //If the unit that was selected was this unit, enable the selection visual
        if (PlayerController.Instance.GetSelectedUnit().Equals(this))
        {
            selectionVisual.SetActive(true);
        }
        else
        {
            selectionVisual.SetActive(false);
        }
    }//end UpdateSelectionVisual

    /// <summary>
    /// Subscribes to the PlayerController's OnUnitSelectedChanged event (when a unit is selected, the event
    /// is invoked, and this method is called)
    /// </summary>
    /// <param name="sender">The object that invoked the event this method subscribes to</param>
    /// <param name="e">Any arguments sent by the event</param>
    private void PlayerController_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        //If the selected unit is this unit
        if (PlayerController.Instance.GetSelectedUnit().Equals(this))
        {
            PlayerController.Instance.SetSelectedAction(moveAction);
        }

        //Update this unit's selection visual now that a new unit has been selected
        UpdateSelectionVisual();
    }//end PlayerController_OnSelectedUnitChanged

    /// <summary>
    /// Subscribes to the GameManager's OnTurnChanged event (when a turn ends and a new one begins, the event
    /// is invoked, and this method is called)
    /// </summary>
    /// <param name="sender">The object that invoked the event this method subscribes to</param>
    /// <param name="e">Any arguments sent by the event</param>
    private void GameManager_OnTurnChanged(object sender, EventArgs e)
    {
        //If the turn changed to the owner of this unit
        if(IsEnemy() == !GameManager.Instance.IsPlayerTurn())
        {
            //The Unit should reset its action points to full
            SetActionPoints(maxActionPoints);
        }
    }//end GameManager_OnTurnChanged

    /// <summary>
    /// Returns whether or not this Unit is the same as the passed Unit
    /// </summary>
    /// <param name="other">The Unit to compare this one against</param>
    /// <returns>Returns whether or not this Unit is the same as the passed Unit</returns>
    public bool Equals(Unit other)
    {
        return this == other;
    }//end Equals


    public UnitAnimator GetUnitAnimator()
    {
        return unitAnimator;
    }//end GetUnitAnimator


    public Tile GetStandingTile()
    {
        if(standingTile == null)
        {
            FindStandingTile();
        }

        return standingTile;
    }//end GetStandingTile


    public Transform GetGrenadeThrowPoint()
    {
        return grenadeThrowPoint;
    }


    public MoveAction GetMoveAction()
    {
        return moveAction;
    }//end GetMoveAction

    public SpinAction GetSpinAction()
    {
        return spinAction;
    }//end GetSpinAction

    public ShootAction GetShootAction()
    {
        return shootAction;
    }//end GetShootAction

    public GrenadeAction GetGrenadeAction()
    {
        return grenadeAction;
    }

    public MeleeAction GetMeleeAction()
    {
        return meleeAction;
    }

    public InteractAction GetInteractAction()
    {
        return interactAction;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }//end GetBaseActionArray

    public int GetActionPoints()
    {
        return actionPoints;
    }//end GetActionPoints

    public void SetActionPoints(int newActionPoints)
    {
        //
        actionPoints = newActionPoints;

        //
        worldUI.UpdateActionPointsText();
    }//end SetActionPoints

    public bool CanPerformAction(BaseAction action)
    {
        return actionPoints >= action.GetActionPointCost();
    }//end CanPerformAction

    private void SpendActionPoints(int amount)
    {
        SetActionPoints(actionPoints - amount);
    }//end SpendActionPoints

    public bool TryPerformAction(BaseAction action)
    {
        //
        bool canPerformAction = false;

        //
        if(CanPerformAction(action))
        {
            SpendActionPoints(action.GetActionPointCost());
            canPerformAction = true;
        }

        //
        return canPerformAction;
    }//end TryPerformAction

    public bool IsEnemy()
    {
        return isEnemy;
    }//end IsEnemy


    public override void TakeDamage(int amount, Vector3 impactPoint)
    {
        //
        base.TakeDamage(amount, impactPoint);

        //
        OnTakeDamage?.Invoke(this, EventArgs.Empty);

        //
        worldUI.UpdateHealthBar();
    }//end TakeDamage


    protected override void Die(Vector3 impactPoint)
    {
        //Unsubscribe from all events to prevent errors
        PlayerController.Instance.OnSelectedUnitChanged -= PlayerController_OnSelectedUnitChanged;
        GameManager.Instance.OnTurnChanged -= GameManager_OnTurnChanged;
        
        //
        UnitRagdoll ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation).GetComponent<UnitRagdoll>();
        ragdoll.Setup(rootBone, impactPoint);

        //
        OnAnyUnitDied?.Invoke(this, EventArgs.Empty);

        //
        base.Die(impactPoint);
    }//end Die

    #endregion
}
