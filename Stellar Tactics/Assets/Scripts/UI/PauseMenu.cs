using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    #region Variables

    public static PauseMenu Instance { get; private set; }   //Singleton Instance of the PauseMenu

    public static bool IsPaused;

    private Animator animator;
    private string pauseStateString = "isPaused";

    [SerializeField] private TextMeshProUGUI mainText;
    [SerializeField] private TextMeshProUGUI playButtonText;

    private string mainPauseText = "PAUSED";
    private string mainWinText = "VICTORY";
    private string mainLoseText = "DEFEAT";

    private string playPauseText = "RESUME";
    private string playEndText = "RESTART";

    #endregion //end Variables

    #region Unity Control Methods

    // Awake is called before Start before the first frame update
    void Awake()
    {
        //If Instance does not exist yet, this instance should be the Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one PauseMenu in the scene " + transform + " - " + Instance);
        }

        //
        animator = GetComponent<Animator>();

        //
        Resume();
    }//end Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }//end Start

    // Update is called once per frame
    void Update()
    {
        //
        HandlePauseInput();
    }//end Update

    #endregion //end Unity Control Methods

    #region

    private void HandlePauseInput()
    {
        //
        if(PlayerInputHandler.pauseInput)
        {
            //
            PlayerInputHandler.pauseInput = false;

            //
            if(IsPaused)
            {
                Resume();
            }
            //
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        //
        IsPaused = false;

        //
        animator.SetBool(pauseStateString, IsPaused);
    }

    public void Pause()
    {
        //
        IsPaused = true;

        //
        mainText.text = mainPauseText;

        //
        playButtonText.text = playPauseText;

        //
        animator.SetBool(pauseStateString, IsPaused);
    }

    public void DisplayEndScreen(bool playerWon)
    {
        //
        IsPaused = true;

        //
        animator.SetBool(pauseStateString, IsPaused);

        //
        playButtonText.text = playEndText;

        //
        Time.timeScale = 0f;

        //
        if (playerWon)
        {
            //
            mainText.text = mainWinText;
        }
        //
        else
        {
            //
            mainText.text = mainLoseText;
        }
    }

    public void Play()
    {
        IsPaused = false;
        //
        Time.timeScale = 1f;

        //
        if(GameManager.GameOver)
        {
            //
            if (PersistantManager.Instance != null)
            {
                //
                PersistantManager.Instance.ReloadGameScene();
            }
        }
        else
        {
            Resume();
        }    
    }

    public void QuitToMenu()
    {
        //
        Time.timeScale = 1f;

        //
        if(PersistantManager.Instance != null)
        {
            //
            PersistantManager.Instance.LoadMenuScene();
        }
    }

    public void QuitGame()
    {
        //
        Time.timeScale = 1f;

        //
        Application.Quit();
    }

    #endregion
}
