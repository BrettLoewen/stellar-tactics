/* Destructible
 * Author:      Brett Loewen
 * Date:        August 19, 2022
 * Purpose:     Provide a class (or base class) to allow objects to have health and be damaged/destroyed by other scripts
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    #region Variables

    [Header("Destructible")]

    [SerializeField] private int maxHealth = 10;    //The maximum health for the Destructible
    private int currentHealth;                      //The current health for the Destructible

    [Tooltip("Whether or not the Destructible can take damage")]
    [SerializeField] private bool isDestructible = false;   //If true, the object can take damage and be destroyed

    [SerializeField] private Transform destroyedPrefab;     //Will be spawned when this object is destroyed (if null, nothing happens)

    [SerializeField] private Renderer destructibleRenderer;             //The main Renderer of the object's graphics
    [SerializeField] private Renderer secondDestructibleRenderer;       //A secondary Renderer of the object's graphics (incase it is needed)
    [SerializeField] private Material destructibleHightlightMaterial;   //A special material which the Renderers will switch to when being highlighted
    private Material normalMaterial;                                    //Stores the normal material so it can be switched back to after highlighting completes

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    protected virtual void Awake()
    {
        //Start with max health
        currentHealth = maxHealth;

        //If the main Renderer is not null, get the normal material from it
        if(destructibleRenderer != null)
        {
            normalMaterial = destructibleRenderer.material;
        }
    }//end Awake

    #endregion //end Unity Control Methods

    #region Manage Highlight

    /// <summary>
    /// Make all valid Renderers switch to the highlight material
    /// </summary>
    public void ShowDestructibleHighlight()
    {
        //If there is a highlight material to switch to
        if(destructibleHightlightMaterial != null)
        {
            //If the main Renderer exists
            if(destructibleRenderer != null)
            {
                //Make the main Renderer switch to the highlight material
                destructibleRenderer.material = destructibleHightlightMaterial;
            }

            //If the secondary Renderer exists
            if(secondDestructibleRenderer != null)
            {
                //Make the secondary Renderer switch to the highlight material
                secondDestructibleRenderer.material = destructibleHightlightMaterial;
            }
        }
    }//end ShowDestructibleHighlight

    /// <summary>
    /// Make all valid Renderers switch to the normal material
    /// </summary>
    public void HideDestructibleHighlight()
    {
        //If the main Renderer exists
        if (destructibleRenderer != null)
        {
            //Make the main Renderer switch to the normal material
            destructibleRenderer.material = normalMaterial;
        }

        //If the secondary Renderer exists
        if (secondDestructibleRenderer != null)
        {
            //Make the secondary Renderer switch to the normal material
            secondDestructibleRenderer.material = normalMaterial;
        }
    }//end HideDestructibleHighlight

    #endregion //end Manage Highlight

    #region Manage Health, Damage, Death

    /// <summary>
    /// Returns the Destructible's current health
    /// </summary>
    /// <returns>Returns the Destructible's current health</returns>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }//end GetCurrentHealth

    /// <summary>
    /// Returns the Destructible's maximum health
    /// </summary>
    /// <returns>Returns the Destructible's maximum health</returns>
    public int GetMaxHealth()
    {
        return maxHealth;
    }//end GetMaxHealth

    /// <summary>
    /// Returns the health of the Destructible as a float between 0 and 1 (0 = 0 health, 1 = max health)
    /// </summary>
    /// <returns>Returns the health as a float between 0 and 1</returns>
    public float GetHealthNormalized()
    {
        return (float)currentHealth / maxHealth;
    }//end GetHealthNormalized

    /// <summary>
    /// A method which allows other scripts to decrease the health of Destructibles
    /// </summary>
    /// <param name="amount">The amount of health the Destructible should lose</param>
    /// <param name="impactPoint">The location that the damage was applied at (not always necessary)</param>
    public virtual void TakeDamage(int amount, Vector3 impactPoint)
    {
        //If the object is NOT destructible, return and do nothing
        if(isDestructible == false)
        {
            return;
        }

        //Decrease the health by the passed amount of damage
        currentHealth -= amount;

        //If the health is below 0 (if the destructible is dead)
        if (currentHealth <= 0)
        {
            //Ensure the health does not go below 0
            currentHealth = 0;

            //The destructible has died, so run the die code
            Die(impactPoint);
        }
    }//end TakeDamage

    /// <summary>
    /// Called when the Destructible's health reaches 0. Should hold any necessary cleanup code for the object
    /// </summary>
    /// <param name="impactPoint">The location where the killing damage was dealt at</param>
    protected virtual void Die(Vector3 impactPoint)
    {
        //If there is a destroyed version of this object to spawn in
        if(destroyedPrefab != null)
        {
            //Spawn the destroyed version and get a reference to its transform
            Transform destroyedRoot = Instantiate(destroyedPrefab, transform.position, transform.rotation);

            //Apply an explosion to its transform and its children
            ApplyExplosion(destroyedRoot, 300f, impactPoint, 10f);
        }

        //Delete the gameobject from the scene
        Destroy(gameObject);
    }//end Die

    /// <summary>
    /// Recursively add an explosion force (defined by arguments) to the passed transform and its children
    /// </summary>
    /// <param name="root">Its children will be checked and if possible, have an explosion force applied to them</param>
    /// <param name="explosionForce">The force of the explosion</param>
    /// <param name="explosionPoint">The epicenter of the explosion</param>
    /// <param name="explosionRange">The size of the explosion</param>
    private void ApplyExplosion(Transform root, float explosionForce, Vector3 explosionPoint, float explosionRange)
    {
        //Loop through each child bone of the passed transform
        foreach (Transform childBone in root)
        {
            //Get the Rigidbody component of the child bone if one exists
            if (childBone.TryGetComponent(out Rigidbody boneRigidbody))
            {
                //Add the passed explosion force to the Rigidbody
                boneRigidbody.AddExplosionForce(explosionForce, explosionPoint, explosionRange);
            }

            //Apply the passed explosion force to any children of this child bone
            ApplyExplosion(childBone, explosionForce, explosionPoint, explosionRange);
        }
    }//end ApplyExplosion

    #endregion //end Manage Health, Damage, Death
}
