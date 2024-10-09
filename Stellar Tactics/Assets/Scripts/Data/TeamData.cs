/* TeamData
 * Author:      Brett Loewen
 * Date:        August 20, 2022
 * Purpose:     Provide a data file which will hold information about Teams
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Team Data", menuName = "Data/Team")]
public class TeamData : ScriptableObject
{
    #region Variables

    public Color teamColor; //This team's Color

    public Material rangerMaterial;     //The material used for the Ranger Unit when it is on this team
    public Material ravagerMaterial;    //The material used for the Ravager Unit when it is on this team
    public Material officerMaterial;    //The material used for the Officer Unit when it is on this team
    public Material captainMaterial;    //The material used for the Captain Unit when it is on this team

    #endregion //end Variables

    /// <summary>
    /// Determine the material that matches the passed UnitData and return it
    /// </summary>
    /// <param name="unitData">The unit data which determines which material should be returned</param>
    /// <returns>Returns the material from this TeamData that matches the passed UnitData</returns>
    public Material GetUnitMaterial(UnitData unitData)
    {
        //Create a variable to store the correct material when it is found (default to rangerMaterial)
        Material unitMaterial = rangerMaterial;

        //Based on the passed UnitData's unitName decide which material to retun
        switch(unitData.unitName)
        {
            //If the UnitData was the Ranger UnitData, return the Ranger's material
            case "Ranger":
                unitMaterial = rangerMaterial;
                break;
            //If the UnitData was the Ravager UnitData, return the Ravager's material
            case "Ravager":
                unitMaterial = ravagerMaterial;
                break;
            case "Officer":
                unitMaterial = officerMaterial;
                break;
            case "Captain":
                unitMaterial = captainMaterial;
                break;
        }

        //Return the stored material for the passde UnitData
        return unitMaterial;
    }//end GetUnitMaterial
}
