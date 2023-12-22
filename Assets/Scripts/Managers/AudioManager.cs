using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class AudioManager : MonoBehaviour
    {
        [Header("AUDIO SOURCES")]
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private AudioSource musicSource;

        [Space, Header("AUDIO CLIPS")]
        [Header("UI Audio")]
        [SerializeField] private AudioClip uiButtonAudio;
        [Header("MUSIC")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip[] gameplayMusic;


        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            // Ensure only one instance of AudioManager exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void PlaySound(AudioClip soundClip, float volume = 1f)
        {
            if (!PlayerPrefsUtil.GetSoundsEnabled())
                return;

            soundSource.volume = volume;
            soundSource.PlayOneShot(soundClip);
        }

        public void PlayMusic(AudioClip musicClip, float volume = 1f)
        {
            musicSource.clip = musicClip;
            musicSource.volume = volume;
            musicSource.Play();
            MuteMusic(PlayerPrefsUtil.GetMusicEnabled());
        }

        public void MuteMusic(bool mute)
        {
            musicSource.mute = !mute;
        }

        public void PlayUIButtonSound()
        {
            PlaySound(uiButtonAudio);
        }

        public void PlayMainMenuMusic()
        {
            if (musicSource.clip == mainMenuMusic)
                return;

            PlayMusic(mainMenuMusic);
        }

        public void PlayGameplayMusic()
        {
            int index = Random.Range(0, gameplayMusic.Length);
            PlayMusic(gameplayMusic[index], 0.3f);
        }
    }
}