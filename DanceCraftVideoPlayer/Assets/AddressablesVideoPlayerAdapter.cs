/**
 * Copyright 2021, Charm Games Inc, All rights reserved.
 */

//------------------------------------------------------------------------------
// Using directives 
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    [SerializeField]
    private OVRCameraRig rig;
    
    // [SerializeField] private AssetReference[] videoClipRefs = null;

    [SerializeField]
    private string[] clipNames;
    
    [SerializeField]
    private AudioSource[] videoAudioOverrides = null;

    private AudioSource PlayingAudioSource;
    
    [SerializeField]
    private int[] videoClipRotationOffsets;
    
    private VideoPlayer videoPlayer;

    private AsyncOperationHandle<VideoClip> asyncOperationHandle;

    private bool videoPlaying;

    public bool loading;

    public UnityEvent FinishedPlaying;

    private int playingIdx = -1;

    private Dictionary<int, AsyncOperationHandle<VideoClip>> asyncOperationHandles =
        new Dictionary<int, AsyncOperationHandle<VideoClip>>();

    //------------------------------------------------------------------------------
    
    public void PlayVideo(int index, bool loop = false)
    {
        videoPlayer.isLooping = loop;
        if (index == playingIdx) {
            // videoPlayer.Play();
            SetPause(false);
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
            if (PlayingAudioSource != null) {
                PlayingAudioSource.Pause();
            }
        } else if (!videoPlaying && !pause) {
            videoPlaying = true;
            videoPlayer.Play();
            if (PlayingAudioSource != null) {
                PlayingAudioSource.Play();
            }
        }
        
    }
    
    //------------------------------------------------------------------------------

    public void ReleaseVideo()
    {
        videoPlayer.clip = null;
        // Addressables.Release(asyncOperationHandle);
        videoPlaying = false;
        playingIdx = -1;
    }
    
    //------------------------------------------------------------------------------

    private IEnumerator PlayVideoInternal(int index)
    {
        try {
            loading = true;
            videoPlaying = false;
            // if (asyncOperationHandle.IsValid()) {
            //     ReleaseVideo();
            // }
            // asyncOperationHandle = videoClipRefs[index].LoadAssetAsync<VideoClip>();
            // yield return asyncOperationHandle;
            // videoPlayer.clip = asyncOperationHandle.Result;
            string externalAssetPath = "/sdcard/Android/obb/com.MattAustin.DanceCraft";
#if UNITY_EDITOR
            externalAssetPath = Path.GetDirectoryName(Application.dataPath) + "/Assets";
#endif
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = externalAssetPath + $"/{clipNames[index]}";
            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared);
            if (videoAudioOverrides.Length > index && videoAudioOverrides[index] != null) {
                PlayingAudioSource = videoAudioOverrides[index];
                PlayingAudioSource.Play();
                // videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                videoPlayer.SetDirectAudioMute(0, true);
            } else {
                if (PlayingAudioSource != null) {
                    PlayingAudioSource.Stop();
                    PlayingAudioSource = null;
                }
                videoPlayer.SetDirectAudioMute(0, false);
                // videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            }
            videoPlayer.Play();
            yield return new WaitUntil(() => videoPlayer.isPlaying);
            videoPlaying = true;
            playingIdx = index;
        } finally {
            loading = false;
            if (videoClipRotationOffsets.Length > index) {
                rig.transform.Rotate(new Vector3(0, videoClipRotationOffsets[index], 0));
            }
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

    private void Start()
    {
        PlayVideo(0, true);
    }

    //------------------------------------------------------------------------------

    private void Update()
    {
        if (videoPlaying && !videoPlayer.isPlaying && !videoPlayer.isLooping) {
            ReleaseVideo();
            FinishedPlaying.Invoke();
        }
    }

}

}
