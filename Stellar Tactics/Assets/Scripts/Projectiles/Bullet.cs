/* Bullet
 * Author:      Brett Loewen
 * Date:        August 19, 2022
 * Purpose:     Move to a passed position and will destroy itself when the target is reached.
 *              Used to create a bullet visual effect
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform trail;               //The child object which creates a trail effect as the Bullet moves
    [SerializeField] private Transform bulletHitVFXPrefab;  //This particle effect will be spawned when the Bullet reaches its target

    private float moveSpeed = 100f; //The speed at which the Bullet will move
    private Vector3 targetPosition; //The point that the Bullet will move toward

    #endregion //end Variables

    #region Unity Control Methods

    // Update is called once per frame
    void Update()
    {
        //Get the direction the Bullet needs to move in
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        //Get the distance between the Bullet and the target position before the Bullet moves
        float distanceBeforeMoving = Vector3.Distance(targetPosition, transform.position);

        //Move toward the target position at the assigned move speed
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        //Get the distance between the Bullet and the target position after the Bullet moves
        float distanceAfterMoving = Vector3.Distance(targetPosition, transform.position);

        //If the distance to the target before moving is less than the distance to the target after moving
        //If the Bullet was closer to the target before moving
        //If the Bullet reached its destination
        if(distanceBeforeMoving < distanceAfterMoving)
        {
            //Ensure the trail does not overshoot
            transform.position = targetPosition;

            //Unparent the trail so it despawns more naturally (it will self destruct when the trail's length is 0)
            trail.parent = null;

            //Spawn the impact effect
            Instantiate(bulletHitVFXPrefab, targetPosition, Quaternion.identity);

            //Destroy this gameobject to remove it from the scene
            Destroy(gameObject);
        }
    }//end Update

    #endregion //end Unity Control Methods

    /// <summary>
    /// Give the Bullet its target position
    /// </summary>
    /// <param name="targetPosition">The point that the Bullet will move towards</param>
    public void Setup(Vector3 targetPosition)
    {
        //Set the target position using the passed position
        this.targetPosition = targetPosition;
    }//end Setup
}
