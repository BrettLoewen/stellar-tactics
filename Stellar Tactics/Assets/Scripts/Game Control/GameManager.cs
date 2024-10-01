using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Variables

    public static GameManager Instance { get; private set; }   //Singleton Instance of the GameManager

    public static bool GameOver { get; private set; }

    public event EventHandler OnTurnChanged;
    [SerializeField] private TurnSystemUI turnSystemUI;
    private int turnIndex;

    private List<Unit> unitList;
    [SerializeField] private List<UnitController> unitControllers = new List<UnitController>();

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    private void Awake()
    {
        //If Instance does not exist yet, this instance should be the Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one GameManager in the scene " + transform + " - " + Instance);
        }

        GameOver = false;

        unitList = new List<Unit>();

        turnIndex = 0;

        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        turnSystemUI.SetTurnOwnerText("YOUR TURN");

        //Ensure that the time scale is correct
        Time.timeScale = 1f;

        //If there is no persistant scene loaded (the game started in the game scene)
        if (PersistantManager.Instance == null)
        {
            //Load the persistant scene
            SceneManager.LoadSceneAsync("PersistantScene", LoadSceneMode.Additive);
        }
    }//end Start

    // Update is called once per frame
    void Update()
    {
        

    }//end Update

    // OnDestroy is called when the object is destroyed (or the scene unloads)
    private void OnDestroy()
    {
        //Remove any subscriptions to static events to prevent errors
        Unit.OnAnyUnitSpawned -= Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;
    }

    #endregion //end Unity Control Methods

    #region

    /// <summary>
    /// Continue in the cycle of turns and update the game accordingly
    /// </summary>
    public void NextTurn()
    {
        //Reset any Tilemap pathfinding and visuals
        TileManager.Instance.ResetTilemapPathfinding();

        //Increment the turn counter
        turnIndex++;

        //If the turnIndex increased past the number of unit controllers
        if(turnIndex >= unitControllers.Count)
        {
            //Overflow the turnIndex to 0
            turnIndex = 0;
        }

        //If the game is switching to the player's turn
        if (turnIndex == 0)
        {
            //Display that it is currently the player's turn
            turnSystemUI.SetTurnOwnerText("YOUR TURN");

            //If the player does not have any Units, end the game with a loss
            if(PlayerController.Instance.GetUnits().Count <= 0)
            {
                EndGame(false);
            }
            //If the player does still have Units, set the first one as the selected one
            else
            {
                PlayerController.Instance.SetSelectedUnit(PlayerController.Instance.GetUnits()[0]);
            }
        }
        //If the game is switching to an enemies turn
        else
        {
            //Display which enemy is currently the turn owner
            turnSystemUI.SetTurnOwnerText("ENEMY TURN");
        }

        //If the event has subscribers (is not null), invoke it
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }//end NextTurn

    /// <summary>
    /// Returns whether or not it is currently the player's turn
    /// </summary>
    /// <returns>Return true if it is the player's turn, false otherwise</returns>
    /*public bool IsPlayerTurn()
    {
        return turnIndex == 0;
    }*///end IsPlayerTurn

    /// <summary>
    /// Returns whether or not if the passed teamID matches the UnitController whose turn it is
    /// </summary>
    /// <param name="teamID">The teamID to check if it is the turn owner</param>
    /// <returns>Return true if the passed teamID matches the UnitController whose turn it is, false otherwise</returns>
    public bool IsMyTurn(int teamID)
    {
        return unitControllers[turnIndex].SameTeam(teamID);
    }//end IsMyTurn

    /// <summary>
    /// End the game with either a victory or defeat prompt depending on whether or not the player won
    /// </summary>
    /// <param name="playerWon">True if the player won, false if the player lost</param>
    public void EndGame(bool playerWon)
    {
        //Mark that the game has ended
        GameOver = true;

        //Tell the pause menu to display the game over screen
        PauseMenu.Instance.DisplayEndScreen(playerWon);
    }//end EndGame

    /// <summary>
    /// Called when any Action finished performing itself
    /// </summary>
    /// <param name="sender">The object triggering the event</param>
    /// <param name="e">Additional information about the event (if it gets used)</param>
    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        //Loop through each of the UnitControllers
        foreach (UnitController controller in unitControllers)
        {
            //If the UnitController has no more Units, it has lost and the game should end
            if (controller.OutOfUnits())
            {
                //Get the UnitController as a PlayerController
                //PlayerController player = (PlayerController)controller;

                ////If the UnitController was the player, the player lost
                //if (player != null)
                //{
                //    EndGame(false);
                //}
                ////If the UnitController was not the player, the player won
                //else
                //{
                //    EndGame(true);
                //}

                //If the UnitController was the player, the player lost
                if (controller is PlayerController)
                {
                    EndGame(false);
                }
                //If the UnitController was not the player, the player won
                else
                {
                    EndGame(true);
                }
            }
        }
    }//end BaseAction_OnAnyActionCompleted

    /// <summary>
    /// Called when any Unit spawns so it can be added to the list of all Units
    /// </summary>
    /// <param name="sender">The object triggering the event</param>
    /// <param name="e">Additional information about the event (if it gets used)</param>
    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        //Get the Unit that triggered the event
        Unit unit = sender as Unit;

        //Add the Unit that just spawned to the list of all Units
        unitList.Add(unit);
    }//end Unit_OnAnyUnitSpawned

    /// <summary>
    /// Called when any Unit dies to remove the Unit from the list of all Units
    /// </summary>
    /// <param name="sender">The object triggering the event</param>
    /// <param name="e">Additional information about the event (if it gets used)</param>
    private void Unit_OnAnyUnitDied(object sender, EventArgs e)
    {
        //Get the Unit that triggered the event
        Unit unit = sender as Unit;

        //Remove the Unit that just died from the list of all Units
        unitList.Remove(unit);
    }//end Unit_OnAnyUnitDied

    /// <summary>
    /// Return the stored list of all Units in the game
    /// </summary>
    /// <returns>Return the stored list of all Units in the game</returns>
    public List<Unit> GetUnitList()
    {
        return unitList;
    }//end GetUnitList

    /// <summary>
    /// Find all of the Units whose teamID matches the passed teamID and return a list of them
    /// </summary>
    /// <param name="teamID">The teamID used to find friendly Units</param>
    /// <returns>A list of Units who are on the passed teamID</returns>
    public List<Unit> GetFriendlyUnitList(int teamID)
    {
        //Create a list to hold the friendly Units
        List<Unit> friendlyUnits = new List<Unit>();

        //Loop through each of the UnitControllers
        foreach (UnitController controller in unitControllers)
        {
            //If the UnitController's teamID matches the passed teamID (the controller is a friend to the caller)
            if (controller.SameTeam(teamID))
            {
                //Add each Unit to the list of friendly Unit
                foreach (Unit unit in controller.GetUnits())
                {
                    friendlyUnits.Add(unit);
                }
            }
        }

        //Return the list of friendly Units
        return friendlyUnits;
    }//end GetFriendlyUnitList

    /// <summary>
    /// Find all of the Units whose teamID does not match the passed teamID and return a list of them
    /// </summary>
    /// <param name="teamID">The teamID used to find enemy Units</param>
    /// <returns>A list of Units who are not on the passed teamID</returns>

    public List<Unit> GetEnemyUnitList(int teamID)
    {
        //Create a list to hold the enemy Units
        List<Unit> enemyUnits = new List<Unit>();

        //Loop through each of the UnitControllers
        foreach (UnitController controller in unitControllers)
        {
            //If the UnitController's teamID does not match the passed teamID (the controller is an enemy to the caller)
            if (controller.SameTeam(teamID) == false)
            {
                //Add each Unit to the list of enemy Unit
                foreach (Unit unit in controller.GetUnits())
                {
                    enemyUnits.Add(unit);
                }
            }
        }

        //Return the list of friendly Units
        return enemyUnits;
    }//end GetEnemyUnitList

    #endregion
}
