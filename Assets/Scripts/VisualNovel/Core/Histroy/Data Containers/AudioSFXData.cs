using System.Collections.Generic;
using UnityEngine;

namespace History
{
    [System.Serializable]
    public class AudioSFXData
    {
        public string filePath;
        public string fileName;
        public float volume;
        public float pitch;

        public static List<AudioSFXData> Capture()
        {
            List<AudioSFXData> audioList = new List<AudioSFXData>();

            AudioSource[] sfx = AudioManager.instance.allSFX;

            foreach(var sound in sfx)
            {
                if(!sound.loop)
                {
                    continue;
                }

                AudioSFXData data = new AudioSFXData();
                data.volume = sound.volume;
                data.pitch = sound.pitch;
                data.fileName = sound.clip.name;

                string resourcesPath = sound.gameObject.name.Split(AudioManager.SFX_NAME_FORMAT_CONTAINERS)[1];

                data.filePath = resourcesPath;

                audioList.Add(data);
            }

            return audioList;
        }

        public static void Apply(List<AudioSFXData> sfx)
        {
            List<string> cache = new List<string>();

            foreach(var sound in sfx)
            {
                if(!AudioManager.instance.IsPlayingSoundEffect(sound.fileName))
                {
                    AudioManager.instance.PlaySoundEffect(sound.filePath, volume: sound.volume, pitch: sound.pitch, loop: true);
                }

                cache.Add(sound.fileName);
            }

            foreach (var source in AudioManager.instance.allSFX)
            {
                if (!cache.Contains(source.clip.name))
                {
                    AudioManager.instance.StopSoundEffect(source.clip);
                }
            }
        }
    }
}


