using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";

    public int channelIndex { get; private set; }

    public Transform trackContainer { get; private set; } = null;
    
    public AudioTrack activeTrack { get; private set; } = null;
    private List<AudioTrack> tracks = new List<AudioTrack>();

    bool isLevelingVolume => co_volumeLeveling != null;
    Coroutine co_volumeLeveling = null;

    public AudioChannel(int channel)
    {
        channelIndex = channel;

        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channel)).transform;
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, string filePath)
    {
        if (TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            if (!existingTrack.isPlaying())
            {
                existingTrack.Play();
            }
            activeTrack = existingTrack;
            return existingTrack;
        }

        AudioTrack track = new AudioTrack(clip, loop, startingVolume, volumeCap, this, AudioManager.instance.musicMixer);
        track.Play();
        
        activeTrack = track;

        return track;
    }

    public bool TryGetTrack(string trackName, out AudioTrack value)
    {
        trackName = trackName.ToLower();

        foreach (var track in tracks)
        {
            if (track.name.ToLower() == trackName)
            {
                value = track;
                return true;
            }
        }

        value = null;
        return false;

    }

    private void TryStartVolumeLeveling()
    {
        if(!isLevelingVolume)
        {
            co_volumeLeveling = AudioManager.instance.StartCoroutine(VolumeLeveling());
        }
    }

    private IEnumerator VolumeLeveling()
    {
        while(tracks.Count > 1|| activeTrack.volume != activeTrack.volumeCap)
        {

        }

        return null;
    }

}
