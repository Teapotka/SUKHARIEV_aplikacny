using System.Collections.Generic;
using UnityEngine;

namespace BA.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlaylist : MonoBehaviour
    {
        [SerializeField] private List<AudioClip> playlist = new();
        [SerializeField] private bool shuffle = false;
        [SerializeField] private bool persistAcrossScenes = true;

        private AudioSource _src;
        private int _index;

        private static MusicPlaylist _instance;

        private void Awake()
        {
            if (persistAcrossScenes)
            {
                if (_instance != null && _instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

            _src = GetComponent<AudioSource>();
            _src.spatialBlend = 0f;
            _src.loop = false;
            _src.playOnAwake = false;
        }

        private void Start()
        {
            if (playlist == null || playlist.Count == 0) return;
            PlayCurrent();
        }

        private void Update()
        {
            if (playlist == null || playlist.Count == 0) return;

            // If nothing is playing and we have no pause, go next
            if (!_src.isPlaying && _src.clip != null && _src.time <= 0.01f)
            {
                Next();
            }
        }

        private void PlayCurrent()
        {
            _index = Mathf.Clamp(_index, 0, playlist.Count - 1);
            _src.clip = playlist[_index];
            _src.Play();
        }

        private void Next()
        {
            if (shuffle)
            {
                _index = Random.Range(0, playlist.Count);
            }
            else
            {
                _index = (_index + 1) % playlist.Count;
            }
            PlayCurrent();
        }
    }
}