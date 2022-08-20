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
    private int turnNumber = 0;
    private bool isPlayerTurn;

    private List<Unit> unitList;
    private List<Unit> friendlyUnitList;
    private List<Unit> enemyUnitList;

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
        friendlyUnitList = new List<Unit>();
        enemyUnitList = new List<Unit>();

        isPlayerTurn = true;

        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        turnSystemUI.SetTurnOwnerText("YOUR TURN");

        Time.timeScale = 1f;

        //
        if (PersistantManager.Instance == null)
        {
            //
            SceneManager.LoadSceneAsync("PersistantScene", LoadSceneMode.Additive);
        }
    }//end Start

    // Update is called once per frame
    void Update()
    {
        

    }//end Update

    private void OnDestroy()
    {
        Unit.OnAnyUnitSpawned -= Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
        BaseAction.OnAnyActionCompleted -= BaseAction_OnAnyActionCompleted;
    }

    #endregion //end Unity Control Methods

    #region


    public void NextTurn()
    {
        TileManager.Instance.ResetTilemapPathfinding();

        turnNumber++;

        isPlayerTurn = !isPlayerTurn;

        if(isPlayerTurn)
        {
            turnSystemUI.SetTurnOwnerText("YOUR TURN");
            if(friendlyUnitList.Count <= 0)
            {
                Debug.LogWarning("No more friendly units!");
                NextTurn();
            }
            else
            {
                PlayerController.Instance.SetSelectedUnit(friendlyUnitList[0]);
            }
        }
        else
        {
            turnSystemUI.SetTurnOwnerText("ENEMY TURN");
        }

        //If the event has subscribers (is not null), invoke it
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }


    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }


    public void EndGame(bool playerWon)
    {
        //
        GameOver = true;

        //
        PauseMenu.Instance.DisplayEndScreen(playerWon);
    }


    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        if (friendlyUnitList == null || friendlyUnitList.Count <= 0)
        {
            EndGame(false);
        }
        else if (enemyUnitList == null || enemyUnitList.Count <= 0)
        {
            EndGame(true);
        }
    }


    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        //
        Unit unit = sender as Unit;

        //
        unitList.Add(unit);

        //
        if (unit.IsEnemy())
        {
            enemyUnitList.Add(unit);
        }
        //
        else
        {
            friendlyUnitList.Add(unit);
        }
    }


    private void Unit_OnAnyUnitDied(object sender, EventArgs e)
    {
        //
        Unit unit = sender as Unit;

        //
        unitList.Remove(unit);

        //
        if (unit.IsEnemy())
        {
            enemyUnitList.Remove(unit);
        }
        //
        else
        {
            friendlyUnitList.Remove(unit);
        }
    }


    public List<Unit> GetUnitList()
    {
        return unitList;
    }


    public List<Unit> GetFriendlyUnitList()
    {
        return friendlyUnitList;
    }


    public List<Unit> GetEnemyUnitList()
    {
        return enemyUnitList;
    }

    #endregion
}
