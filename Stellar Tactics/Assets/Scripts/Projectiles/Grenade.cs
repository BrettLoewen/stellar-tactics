/* Grenade
 * Author:      Brett Loewen
 * Date:        August 19, 2022
 * Purpose:     Move to a passed position in an arc and will explode when the target is reached.
 *              Used to create a grenade effect
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    #region Variables

    [SerializeField] private float moveSpeed = 15;  //The speed that the Grenade will move at
    private float stopDistance = 0.25f;             //The distance to the target that must be reached before detonating
    private AnimationCurve arcYCurve;               //Defines the Y position for movement so the Grenade can travel in a controlled arc
    private float totalDistance;                    //Stores the total distance from the start point to the end point of travel
    private Vector3 positionXZ;                     //Stores the XZ position of the Grenade during travel

    [SerializeField] private Transform explosionVFX;    //The explosion particle effect that will spawned during detonation
    [SerializeField] private Transform trail;           //The trail object which provides a trail visual as the Grenade travels

    private Vector3 targetPosition;     //The position to travel towards
    private float explosionRadius;      //The radius of the explosion
    private LayerMask destructibleMask; //The LayerMask used to find Destructibles to damage during detonation
    private int damage;                 //The amount of damage to inflict to Destructibles during detonation

    private Action onDetonate;  //A callback used to end the GrenadeAction when the explosion occurs

    #endregion //end Variables

    #region Unity Control Methods

    // Update is called once per frame
    void Update()
    {
        //Get the direction the Grenade needs to travel in
        Vector3 moveDirection = (targetPosition - positionXZ).normalized;

        //Update the XZ position using the movement speed and direction
        positionXZ += moveDirection * moveSpeed * Time.deltaTime;

        //Calculate the distance (along the ground) from the Grenade to the target position
        float distance = Vector3.Distance(positionXZ, targetPosition);

        //Normalize the distance traveled against the total distance (0: just started, 1: reached destination)
        float distanceNormalized = 1 - (distance / totalDistance);

        //Get the Y position for the Grenade by sampling the ArcYCurve using the normalized distance
        float positionY = arcYCurve.Evaluate(distanceNormalized);

        //Set the Grenade to be at the calculated position
        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

        //If the distance to the target is close enough to detonate, DETONATE!
        if(distance <= stopDistance)
        {
            Detonate();
        }
    }//end Update

    #endregion //end Unity Control Methods

    /// <summary>
    /// Used to give the Grenade the information it needs to operate correctly
    /// </summary>
    /// <param name="targetPosition">The position the Grenade needs to move to</param>
    /// <param name="explosionRadius">The explosion radius used during detonation</param>
    /// <param name="destructibleMask">The LayerMask used to find Destructibles</param>
    /// <param name="damage">The amount of damage to be dealt to Destructibles</param>
    /// <param name="onDetonate">A callback which will be called when the Grenade detonates</param>
    /// <param name="arcYCurve">Used to detemine the Y position of the Grenade during travel</param>
    public void Setup(Vector3 targetPosition, float explosionRadius, LayerMask destructibleMask, int damage, Action onDetonate, AnimationCurve arcYCurve)
    {
        //Store the arguments so they can be used later
        this.targetPosition = targetPosition;
        this.explosionRadius = explosionRadius;
        this.destructibleMask = destructibleMask;
        this.damage = damage;
        this.onDetonate = onDetonate;
        this.arcYCurve = arcYCurve;

        //Set the starting XZ position
        positionXZ = transform.position;
        positionXZ.y = 0;

        //Calculate the total distance (along the ground) that the Grenade needs to travel
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }//end Setup

    /// <summary>
    /// Find any Destructibles in range and damage them, trigger the explosion effects, and destroy this object
    /// </summary>
    private void Detonate()
    {
        //Get the colliders of any Destructibles in the explosion area
        Collider[] colliders = Physics.OverlapSphere(targetPosition, explosionRadius, destructibleMask);

        //Loop through each collider
        foreach(Collider collider in colliders)
        {
            //Get the Destructible component from the collider if it exists
            if(collider.TryGetComponent(out Destructible destructible))
            {
                //Make the destructible take damage from the explosion point
                destructible.TakeDamage(damage, targetPosition);
            }
        }

        //Unparent the trail so it persists (it will destroy itself)
        trail.parent = null;

        //Play the Explosion sound effect
        AudioManager.Instance.PlaySound("Explosion");

        //Spawn the explosion particle effect
        Instantiate(explosionVFX, targetPosition, Quaternion.identity);

        //Call the callback signaling that the Grenade has finished
        onDetonate();

        //Destroy this gameobject
        Destroy(gameObject);
    }//end Detonate
}
