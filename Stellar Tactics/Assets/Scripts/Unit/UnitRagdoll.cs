using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitRagdoll : MonoBehaviour
{
    #region Variables

    [SerializeField] private Transform ragdollRootBone;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region

    public void Setup(Transform unitRootBone, Vector3 impactPoint)
    {
        MatchAllChildTransforms(ragdollRootBone, unitRootBone);

        ApplyExplosion(ragdollRootBone, 300f, impactPoint, 10f);
    }


    private void MatchAllChildTransforms(Transform ragdollRoot, Transform unitRoot)
    {
        //
        foreach (Transform ragdollBone in ragdollRoot)
        {
            //
            Transform unitBone = unitRoot.Find(ragdollBone.name);

            //
            if (unitBone != null)
            {
                //
                ragdollBone.position = unitBone.position;
                ragdollBone.rotation = unitBone.rotation;

                //
                MatchAllChildTransforms(ragdollBone, unitBone);
            }
        }
    }


    private void ApplyExplosion(Transform root, float explosionForce, Vector3 explosionPoint, float explosionRange)
    {
        //
        foreach (Transform ragdollBone in root)
        {
            //
            if(ragdollBone.TryGetComponent(out Rigidbody boneRigidbody))
            {
                boneRigidbody.AddExplosionForce(explosionForce, explosionPoint, explosionRange);
            }

            //
            ApplyExplosion(ragdollBone, explosionForce, explosionPoint, explosionRange);
        }
    }

    #endregion
}
