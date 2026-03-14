using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Video;

namespace History
{
    public class HistoryCache
    {
        public static Dictionary<string, (object asset, int staleIndex)> loadedAssets = new Dictionary<string, (object asset, int staleIndex)>();
    
        public static T TryLoadObject<T>(string key)
        {
            object resource = null;

            if(loadedAssets.ContainsKey(key))
            {
                resource = (T)loadedAssets[key].asset;
            }
            else
            {
                resource = Resources.Load(key);

                if (resource != null)
                {
                    loadedAssets[key] = (resource, 0);
                }
            }

            if(resource != null)
            {
                if (resource is T)
                {
                    return (T)resource;
                }
                else
                {
                    Debug.LogWarning($"캐시(Cache)에서 '{key}' 오브젝트를 로드했지만, 타입이 일치하지 않습니다. 기대한 타입: {typeof(T)}, 실제 타입: {resource.GetType()}");
                }
            }
            Debug.LogWarning($"캐시(Cache)에서 '{key}' 오브젝트를 로드할 수 없습니다.\"");
            return default(T);
        }

        public static TMP_FontAsset LoadFont(string key) => TryLoadObject<TMP_FontAsset>(key);
        public static AudioClip LoadAudio(string key) => TryLoadObject<AudioClip>(key);
        public static Texture2D LoadImage(string key) => TryLoadObject<Texture2D>(key);
        public static VideoClip LoadVideos(string key) => TryLoadObject<VideoClip>(key);



    }
}