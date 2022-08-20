using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistantManager : MonoBehaviour
{
    #region Variables

    public static PersistantManager Instance { get; private set; }

    public event EventHandler OnLoadingStart;
    public event EventHandler OnLoadingComplete;

    private List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    [SerializeField] private Animator loadingScreenAnimator;
    private string loadingStateString = "isLoading";

    [SerializeField] private GameObject loadingScreenCamera;

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
            Debug.LogError("More than one PersistantManager in the scene " + transform + " - " + Instance);
        }

        loadingScreenCamera.SetActive(false);
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

    public void LoadGameScene()
    {
        StartCoroutine(StartLoadingScene("SampleScene", "MainMenu"));
    }

    public void ReloadGameScene()
    {
        StartCoroutine(StartLoadingScene("SampleScene", "SampleScene"));
    }

    public void LoadMenuScene()
    {
        StartCoroutine(StartLoadingScene("MainMenu", "SampleScene"));
    }

    private IEnumerator StartLoadingScene(string sceneToLoad, string sceneToUnload)
    {
        //
        OnLoadingStart?.Invoke(this, EventArgs.Empty);

        //
        scenesLoading.Clear();

        //
        loadingScreenAnimator.SetBool(loadingStateString, true);

        //
        loadingScreenCamera.SetActive(true);

        //
        float timer = 0.5f;

        //
        while(timer > 0f)
        {
            //
            Time.timeScale = 1f;

            //
            timer -= Time.deltaTime;

            //
            yield return null;
        }

        //
        scenesLoading.Add(SceneManager.UnloadSceneAsync(sceneToUnload));

        //
        scenesLoading.Add(SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive));

        //
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            //
            while (!scenesLoading[i].isDone)
            {
                //
                yield return null;
            }
        }

        //
        yield return null;

        //
        scenesLoading.Clear();

        //
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneToLoad));

        //
        loadingScreenAnimator.SetBool(loadingStateString, false);

        //
        loadingScreenCamera.SetActive(false);

        //
        OnLoadingComplete?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}

