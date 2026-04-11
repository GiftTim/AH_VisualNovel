using System.Collections.Generic;
using UnityEngine;

namespace History
{
    [System.Serializable]
    public class AudioTrackData
    {
        public int channel = 0;
        public string trackName;
        public string trackPath;
        public float trackVolume;
        public float trackPitch;
        public bool loop;

        public AudioTrackData(AudioChannel channel)
        {
            this.channel = channel.channelIndex;
            if (channel.activeTrack == null)
            {
                return;
            }

            var track = channel.activeTrack;
            trackName = track.name;
            trackPath = track.path;
            trackVolume = track.volumeCap;
            trackPitch = track.pitch;
            loop = track.loop();
        }

        public static List<AudioTrackData> Capture()
        {
            List<AudioTrackData> audioChannels = new List<AudioTrackData>();

            foreach ( var channel in AudioManager.instance.channels)
            {
                if (channel.Value.activeTrack == null)
                {
                    continue;
                }

                AudioTrackData data = new AudioTrackData(channel.Value);
                audioChannels.Add(data);
            }

            return audioChannels;
        }

        public static void Apply(List<AudioTrackData> track)
        {
            List<int> cache = new List<int>();

            foreach (var channelData in track)
            {
                AudioChannel channel = AudioManager.instance.TryGetChannel(channelData.channel, createIfNotExists: true);
                if (channel.activeTrack == null || channel.activeTrack.name != channelData.trackName)
                {
                    AudioClip clip = HistoryCache.LoadAudio(channelData.trackPath);
                    if (clip != null)
                    {
                        channel.StopTrack(immediate: true);
                        channel.PlayTrack(clip, channelData.loop, channelData.trackVolume, channelData.trackVolume, channelData.trackPitch, channelData.trackPath);
                    }
                    else
                    {
                        Debug.LogWarning($"History State: Could not load audio track '{channelData.trackPath}'");
                    }
                }

                cache.Add(channelData.channel);
            }

            foreach (var channel in AudioManager.instance.channels)
            {
                if (!cache.Contains(channel.Value.channelIndex))
                {
                    channel.Value.StopTrack(immediate: true);
                }
            }
        }
    }
}