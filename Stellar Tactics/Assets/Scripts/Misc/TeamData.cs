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

    #endregion //end Variables
}
