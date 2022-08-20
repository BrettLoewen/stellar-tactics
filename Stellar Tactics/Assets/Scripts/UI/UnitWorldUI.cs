using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnitWorldUI : MonoBehaviour
{
    #region Variables

    [SerializeField] private Unit unit;

    [SerializeField] private TextMeshProUGUI actionPointsText;

    [SerializeField] private Image healthBarFill;

    private Transform cameraTransform;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = CameraController.mainCamera.transform;

        UpdateActionPointsText();
        UpdateHealthBar();
    }//end Start

    // LateUpdate is called once per frame after Update
    void LateUpdate()
    {
        Vector3 directionToCamera = (cameraTransform.position - transform.position).normalized;
        //transform.LookAt(transform.position + (directionToCamera * -1f));
        transform.LookAt(cameraTransform.position);
    }//end Update


    #endregion //end Unity Control Methods

    #region

    public void UpdateActionPointsText()
    {
        actionPointsText.text = unit.GetActionPoints().ToString();
    }

    public void UpdateHealthBar()
    {
        healthBarFill.fillAmount = unit.GetHealthNormalized();
    }

    #endregion
}
