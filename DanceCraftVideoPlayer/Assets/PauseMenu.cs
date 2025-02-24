using System;
using System.Collections;
using System.Collections.Generic;
using CharmGames.Core;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    

    public GameObject menuPanel;

    public GameObject loadingPanel;

    public GameObject pointer;

    public GameObject playerHead;

    public AddressablesVideoPlayerAdapter videoPlayerAdapter;
    
    //------------------------------------------------------------------------------
    
    private bool isPaused;

    private bool videoLoaded;

    private bool showMenuWhenLoaded = true;

    //------------------------------------------------------------------------------
    
    private void ShowMenu(bool active, bool updateVideo = true)
    {
        if (updateVideo) {
            videoPlayerAdapter.SetPause(active);
        }
        if (active) {
            transform.forward = playerHead.transform.forward;
        }
        menuPanel.SetActive(active);
        pointer.SetActive(active);
        QualitySettings.antiAliasing = active ? 4 : 0;
    }
    
    //------------------------------------------------------------------------------

    private bool IsPausePressed()
    {
        return OVRInput.GetDown(OVRInput.Button.Start) ||
               OVRInput.GetDown(OVRInput.Button.One) ||
               OVRInput.GetDown(OVRInput.Button.Two) ||
               OVRInput.GetDown(OVRInput.Button.Three) ||
               OVRInput.GetDown(OVRInput.Button.Four);
    }
    
    //------------------------------------------------------------------------------

    public void LoadVideo(int videoIdx)
    {
        StartCoroutine(LoadVideoInternal(videoIdx));
    }
    
    //------------------------------------------------------------------------------

    public IEnumerator LoadVideoInternal(int videoIdx)
    {
        ShowMenu(false, false);
        if (videoIdx != 0) {
            loadingPanel.SetActive(true);
        }
        videoPlayerAdapter.PlayVideo(videoIdx, videoIdx == 0);
        yield return new WaitUntil(() => !videoPlayerAdapter.loading);
        loadingPanel.SetActive(false);
        if (showMenuWhenLoaded) {
            ShowMenu(true, false);
            showMenuWhenLoaded = false;
        }
        videoLoaded = true;
        isPaused = false;
    }
    
    //------------------------------------------------------------------------------

    private void Awake()
    {
        ShowMenu(false, false);
    }

    //------------------------------------------------------------------------------
    
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.antiAliasing = 4;
        videoPlayerAdapter.FinishedPlaying.AddListener(() =>
        {
            showMenuWhenLoaded = true;
            LoadVideo(0);
        });
        // LoadVideo(0);
        StartCoroutine(PlayMenuVideo());
    }
    
    //------------------------------------------------------------------------------

    private IEnumerator PlayMenuVideo()
    {
        yield return new WaitForSeconds(3);
        LoadVideo(0);
        yield break;
    }

    //------------------------------------------------------------------------------
    
    // Update is called once per frame
    void Update()
    {
        if (videoLoaded && 
            !videoPlayerAdapter.loading && 
            IsPausePressed()) {
            isPaused = !isPaused;
            ShowMenu(isPaused);
        }
    }
}
