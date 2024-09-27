/* UnitData
 * Author:      Brett Loewen
 * Date:        August 20, 2022
 * Purpose:     Provide a data file which will hold information about Units
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Data/Unit")]
public class UnitData : ScriptableObject
{
    #region Variables

    public string unitName; //The name used to identify this Unit
    public Unit unitPrefab; //The prefab that will be spawned in for this Unit

    #endregion //end Variables
}
