/* FrameRate
 * Author:      Brett Loewen
 * Date:        August 20, 2022
 * Purpose:     Writes the current frame rate to a text UI object
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRate : MonoBehaviour
{
    #region Variables

    [SerializeField] private TextMeshProUGUI frameRateText;

    #endregion //end Variables

    #region Unity Control Methods

    // Update is called once per frame
    void Update()
    {
        frameRateText.text = Mathf.RoundToInt(1f / Time.deltaTime).ToString();
    }//end Update

    #endregion //end Unity Control Methods
}
