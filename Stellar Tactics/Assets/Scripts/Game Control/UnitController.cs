using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    #region Variables

    [SerializeField] protected TeamData teamData;
    [SerializeField] protected int teamID;

    [SerializeField] protected Transform[] spawnPoints;
    [SerializeField] protected UnitData[] unitDatas;

    protected List<Unit> units;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected virtual void Awake()
    {
        Unit.OnAnyUnitDied += Unit_OnAnyUnitDied;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    protected virtual void OnDestroy()
    {
        Unit.OnAnyUnitDied -= Unit_OnAnyUnitDied;
    }

    #endregion //end Unity Control Methods

    #region Unit Management


    public void SpawnUnits()
    {
        //
        units = new List<Unit>();

        //
        for (int i = 0; i < unitDatas.Length; i++)
        {
            //
            if(i < spawnPoints.Length)
            {
                //
                Unit unit = Instantiate(unitDatas[i].unitPrefab, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);

                //
                unit.Setup(teamID, teamData, unitDatas[i]);

                //
                units.Add(unit);
            }
            //
            else
            {
                Debug.LogWarning("More Units than Spawn Points!");
            }
        }
    }//end SpawnUnits


    public List<Unit> GetUnits()
    {
        return units;
    }


    public bool OutOfUnits()
    {
        return units.Count <= 0;
    }


    private void Unit_OnAnyUnitDied(object sender, EventArgs e)
    {
        //
        Unit unit = sender as Unit;

        //
        if(units.Contains(unit))
        {
            //
            units.Remove(unit);
        }
    }

    #endregion //end Unit Management

    #region Team Management


    public bool SameTeam(int otherTeamID)
    {
        return teamID == otherTeamID;
    }


    public int GetTeamID()
    {
        return teamID;
    }

    #endregion
}
