using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private Unit unit;

    [SerializeField] private Transform bulletPrefab;
    [SerializeField] private Transform shootPoint;

    private string takeDamageString = "takeDamage";
    private string walkingStateString = "isWalking";
    private string shootTriggerString = "shoot";
    private string grenadeTriggerString = "throwGrenade";
    private string meleeTriggerString = "melee";

    [SerializeField] private Transform gunPrimary;
    [SerializeField] private Transform meleeWeapon;

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    private void Awake()
    {
        
    }//end Awake

    // Start is called before the first frame update
    private void Start()
    {
        unit.OnTakeDamage += Unit_OnTakeDamage;
        unit.GetMoveAction().OnStartMoving += MoveAction_OnStartMoving;
        unit.GetMoveAction().OnStopMoving += MoveAction_OnStopMoving;
        unit.GetShootAction().OnShoot += ShootAction_OnShoot;
        if(unit.GetGrenadeAction() != null)
        {
            unit.GetGrenadeAction().OnStartThrowGrenade += GrenadeAction_OnStartThrowGrenade;
            unit.GetGrenadeAction().OnCompleteThrowGrenade += GrenadeAction_OnCompleteThrowGrenade;
        }
        unit.GetMeleeAction().OnMeleeStart += MeleeAction_OnMeleeStart;
        unit.GetMeleeAction().OnMeleeEnd += MeleeAction_OnMeleeEnd;

        //
        SwitchToGunPrimary();
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    public void SetUnit(Unit unit)
    {
        this.unit = unit;
    }

    private void TriggerHitReaction()
    {
        //
        animator.SetTrigger(takeDamageString);
    }

    private void SetIsWalking(bool isWalking)
    {
        animator.SetBool(walkingStateString, isWalking);
    }//end SetIsWalking

    private void TriggerShoot(Tile targetTile)
    {
        animator.SetTrigger(shootTriggerString);

        Bullet bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity).GetComponent<Bullet>();

        bullet.Setup(targetTile.GetSightPoint().position);

        AudioManager.Instance.PlaySound("Gun");
    }

    private void TriggerGrenadeThrow()
    {
        animator.SetTrigger(grenadeTriggerString);
    }

    private void TriggerMelee()
    {
        animator.SetTrigger(meleeTriggerString);
    }

    private void SwitchOffAllWeapons()
    {
        gunPrimary.gameObject.SetActive(false);
        meleeWeapon.gameObject.SetActive(false);
    }

    private void SwitchToGunPrimary()
    {
        gunPrimary.gameObject.SetActive(true);
        meleeWeapon.gameObject.SetActive(false);
    }

    private void SwitchToMelee()
    {
        gunPrimary.gameObject.SetActive(false);
        meleeWeapon.gameObject.SetActive(true);
    }


    #region Event Subscribers

    private void Unit_OnTakeDamage(object sender, EventArgs e)
    {
        TriggerHitReaction();
    }

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        SetIsWalking(true);
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        SetIsWalking(false);
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        TriggerShoot(e.targetTile);
    }

    private void GrenadeAction_OnStartThrowGrenade(object sender, EventArgs e)
    {
        SwitchOffAllWeapons();
        TriggerGrenadeThrow();
    }

    private void GrenadeAction_OnCompleteThrowGrenade(object sender, EventArgs e)
    {
        SwitchToGunPrimary();
    }

    private void MeleeAction_OnMeleeStart(object sender, EventArgs e)
    {
        //
        SwitchToMelee();
        
        //
        TriggerMelee();
    }

    private void MeleeAction_OnMeleeEnd(object sender, EventArgs e)
    {
        //
        SwitchToGunPrimary();
    }

    #endregion //end Event Subscribers
}
