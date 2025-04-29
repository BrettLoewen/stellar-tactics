using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private bool canControl;

    [SerializeField] private TextMeshProUGUI versionNumberText;

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject creditsPanel;


    // Awake is called before Start before the first frame update
    void Awake()
    {
        canControl = true;
        Main();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Make sure the persistent scene is loaded
        if (PersistantManager.Instance == null)
        {
            SceneManager.LoadSceneAsync("PersistantScene", LoadSceneMode.Additive);
        }

        // Setup the version text
        versionNumberText.text = "Ver. " + Application.version;
    }//end Start


    public void Play()
    {
        // If the player can't control the game, then they can't start playing it, so return
        if (canControl == false)
        {
            return;
        }

        // Only continue if the persistent scene and manager are setup
        if(PersistantManager.Instance != null)
        {
            // Don't let the player do anything after they've started the play process
            canControl = false;
            
            // Load the game
            PersistantManager.Instance.LoadGameScene();
        }
    }

    public void Quit()
    {
        // If the player can't control the game, then they can't quit, so return
        if (canControl == false)
        {
            return;
        }

        // Don't let the player do anything after they've started the quit process
        canControl = false;

        // Close the game
        Application.Quit();
    }

    public void HowToPlay()
    {
        // If the player can't control the game, then they can't quit, so return
        if (canControl == false)
        {
            return;
        }

        mainPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void Credits()
    {
        // If the player can't control the game, then they can't quit, so return
        if (canControl == false)
        {
            return;
        }

        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void Main()
    {
        // If the player can't control the game, then they can't quit, so return
        if (canControl == false)
        {
            return;
        }

        mainPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }
}
