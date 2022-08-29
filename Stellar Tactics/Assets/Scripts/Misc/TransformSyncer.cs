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

    [Tooltip("Adds an offset to the target's position when syncing position")]
    [SerializeField] private Vector3 positionOffset;

    [Tooltip("Controls whether or not to sync rotation between transforms")]
    [SerializeField] private bool syncRotation;

    [Tooltip("Adds an offset to the target's rotation when syncing rotation")]
    [SerializeField] private Vector3 rotationOffset;

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
        //If there is a target transform to sync to
        if(target != null)
        {
            //If syncPosition is on, match position with the target
            if (syncPosition)
            {
                transform.position = target.position + positionOffset;
            }

            //If syncRotaton is on, match rotation with the target
            if (syncRotation)
            {
                //Use the target's rotation and the rotation offset to get the x, y, and z values for the rotation
                float xRot = target.rotation.x + rotationOffset.x;
                float yRot = target.rotation.y + rotationOffset.y;
                float zRot = target.rotation.z + rotationOffset.z;

                //Create a quaternion using the above calculated angles and use it to set the rotation
                //transform.rotation = Quaternion.Euler(xRot, yRot, zRot);

                transform.rotation = target.rotation * Quaternion.Euler(rotationOffset.x, rotationOffset.y, rotationOffset.z);
            }
        }
    }//end SyncToTarget
}
