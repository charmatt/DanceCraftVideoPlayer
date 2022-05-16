/**
 * Copyright 2021, Charm Games Inc, All rights reserved.
 */

//------------------------------------------------------------------------------
// Using directives 
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Video;

namespace CharmGames.Core {

//------------------------------------------------------------------------------
// Class definition 
//------------------------------------------------------------------------------

[RequireComponent(typeof(VideoPlayer))]
public class AddressablesVideoPlayerAdapter : MonoBehaviour
{
    [SerializeField] private AssetReference[] videoClipRefs = null;
    
    private VideoPlayer videoPlayer;

    private AsyncOperationHandle<VideoClip> asyncOperationHandle;

    private bool videoPlaying;

    public bool loading;

    public UnityEvent FinishedPlaying;

    private int playingIdx = -1;

    private Dictionary<int, AsyncOperationHandle<VideoClip>> asyncOperationHandles =
        new Dictionary<int, AsyncOperationHandle<VideoClip>>();

    //------------------------------------------------------------------------------
    
    public void PlayVideo(int index)
    {
        if (index == playingIdx) {
            videoPlayer.Play();
            return;
        }
        StartCoroutine(PlayVideoInternal(index));
    }
    
    //------------------------------------------------------------------------------

    public void SetPause(bool pause)
    {
        if (videoPlaying && pause) {
            videoPlaying = false;
            videoPlayer.Pause();
        } else if (!videoPlaying && !pause) {
            videoPlaying = true;
            videoPlayer.Play();
        }
        
    }
    
    //------------------------------------------------------------------------------

    public void ReleaseVideo()
    {
        videoPlayer.clip = null;
        Addressables.Release(asyncOperationHandle);
        videoPlaying = false;
        playingIdx = -1;
    }
    
    //------------------------------------------------------------------------------

    private IEnumerator PlayVideoInternal(int index)
    {
        try {
            loading = true;
            if (asyncOperationHandle.IsValid()) {
                ReleaseVideo();
            }
            asyncOperationHandle = videoClipRefs[index].LoadAssetAsync<VideoClip>();
            yield return asyncOperationHandle;
            videoPlayer.clip = asyncOperationHandle.Result;
            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);
            videoPlayer.Play();
            yield return new WaitUntil(() => videoPlayer.isPlaying);
            videoPlaying = true;
            playingIdx = index;
        } finally {
            loading = false;
        }
        
    }

    //------------------------------------------------------------------------------
    
    void Awake()
    {
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(60f);
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.targetTexture.DiscardContents();
    }
    
    //------------------------------------------------------------------------------

    private void Update()
    {
        if (videoPlaying && !videoPlayer.isPlaying) {
            ReleaseVideo();
            FinishedPlaying.Invoke();
        }
    }

}

}
