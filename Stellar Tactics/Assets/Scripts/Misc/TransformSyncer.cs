/* TransformSyncer
 * Author:      Brett Loewen
 * Date:        August 19, 2022
 * Purpose:     Syncs the transform this component is attached to with the values of another target transform
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode] //Allows this script to run in the editor (in edit mode)
public class TransformSyncer : MonoBehaviour
{
    [Tooltip("The transform to sync to")]
    [SerializeField] private Transform target;
    [Tooltip("Controls whether or not to sync position between transforms")]
    [SerializeField] private bool syncPosition;
    [Tooltip("Controls whether or not to sync rotation between transforms")]
    [SerializeField] private bool syncRotation;

    #region Unity Control Methods

    // Update is called once per frame
    void Update()
    {
        //Make this transform sync to its target according to its settings
        SyncToTarget();
    }//end Update

    #endregion //end Unity Control Methods

    /// <summary>
    /// Syncs this transform's position and rotation (as allowed) to the target's positiong and rotation
    /// </summary>
    [ContextMenu("Sync To Target")]
    private void SyncToTarget()
    {
        //If syncPosition is on, match position with the target
        if(syncPosition)
        {
            transform.position = target.position;
        }

        //If syncRotaton is on, match rotation with the target
        if(syncRotation)
        {
            transform.rotation = target.rotation;
        }
    }//end SyncToTarget
}
