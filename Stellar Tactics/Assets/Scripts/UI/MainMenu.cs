using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    #region Variables

    private bool canControl;

    [SerializeField] private TextMeshProUGUI versionNumberText;

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        //
        canControl = true;
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        //
        if (PersistantManager.Instance == null)
        {
            //
            SceneManager.LoadSceneAsync("PersistantScene", LoadSceneMode.Additive);
        }

        //
        versionNumberText.text = "Ver. " + Application.version;
    }//end Start

    // Update is called once per frame
    void Update()
    {
        
    }//end Update

    #endregion //end Unity Control Methods

    #region

    public void Play()
    {
        //
        if(canControl == false)
        {
            return;
        }

        //
        if(PersistantManager.Instance != null)
        {
            //
            canControl = false;
            
            //
            PersistantManager.Instance.LoadGameScene();
        }
    }

    public void Quit()
    {
        //
        if (canControl == false)
        {
            return;
        }

        //
        canControl = false;

        //
        Application.Quit();
    }

    #endregion
}
