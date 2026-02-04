using UnityEngine;
using System.Collections.Generic;

namespace Managers
{
    /// <summary>
    /// AudioManager : Gestion centralisée de tous les sons du jeu
    /// Pattern Singleton pour un accès global
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudio();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Audio Sources

        [Header("Sources Audio")] [SerializeField]
        private AudioSource musicSource;

        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource engineSource; // Pour le moteur du chariot
        [SerializeField] private AudioSource ambientSource;

        #endregion

        #region Audio Clips

        [Header("Musiques")] [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;

        [Header("SFX - Chariot")] [SerializeField]
        private AudioClip engineIdle;

        [SerializeField] private AudioClip engineRunning;
        [SerializeField] private AudioClip hornSound;
        [SerializeField] private AudioClip forkLift;
        [SerializeField] private AudioClip forkLower;
        [SerializeField] private AudioClip reverseBeep;

        [Header("SFX - Gameplay")] [SerializeField]
        private AudioClip palletPickup;

        [SerializeField] private AudioClip palletDrop;
        [SerializeField] private AudioClip deliverySuccess;
        [SerializeField] private AudioClip violationWarning;
        [SerializeField] private AudioClip accidentCrash;

        [Header("SFX - UI")] [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip menuSelect;

        #endregion

        #region Settings

        [Header("Réglages")] [Range(0f, 1f)] [SerializeField]
        private float masterVolume = 1f;

        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

        // Dictionnaire pour accès rapide aux clips par nom
        private Dictionary<string, AudioClip> sfxClips = new();

        #endregion

        #region Initialization

        private void InitializeAudio()
        {
            // Crée les sources si manquantes
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }

            if (engineSource == null)
            {
                engineSource = gameObject.AddComponent<AudioSource>();
                engineSource.loop = true;
            }

            // Remplit le dictionnaire de SFX
            RegisterSFX("horn", hornSound);
            RegisterSFX("fork_lift", forkLift);
            RegisterSFX("fork_lower", forkLower);
            RegisterSFX("reverse_beep", reverseBeep);
            RegisterSFX("pallet_pickup", palletPickup);
            RegisterSFX("pallet_drop", palletDrop);
            RegisterSFX("delivery_success", deliverySuccess);
            RegisterSFX("violation_warning", violationWarning);
            RegisterSFX("accident", accidentCrash);
            RegisterSFX("button", buttonClick);

            Debug.Log("[AudioManager] Initialisé");
        }

        private void RegisterSFX(string name, AudioClip clip)
        {
            if (clip != null && !sfxClips.ContainsKey(name))
            {
                sfxClips[name] = clip;
            }
        }

        #endregion

        #region Music Control

        public void PlayMusic(string musicName)
        {
            AudioClip clip = musicName switch
            {
                "menu" => menuMusic,
                "gameplay" => gameplayMusic,
                "victory" => victoryMusic,
                "defeat" => defeatMusic,
                _ => null
            };

            if (clip != null && musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            musicSource?.Stop();
        }

        public void PauseMusic()
        {
            musicSource?.Pause();
        }

        public void ResumeMusic()
        {
            musicSource?.UnPause();
        }

        #endregion

        #region SFX Control

        public void PlaySFX(string sfxName)
        {
            if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
            {
                PlaySFX(clip);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] SFX non trouvé: {sfxName}");
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
            }
        }

        /// <summary>
        /// Joue le klaxon du chariot
        /// </summary>
        public void PlayHorn()
        {
            PlaySFX("horn");

            // Notifie le SafetyManager
            SafetyManager.Instance?.OnHornUsed();
        }

        #endregion

        #region Engine Sound

        /// <summary>
        /// Met à jour le son du moteur en fonction de la vitesse
        /// </summary>
        public void UpdateEngineSound(float speedNormalized, bool isReversing)
        {
            if (engineSource == null) return;

            // Ajuste le pitch en fonction de la vitesse
            engineSource.pitch = Mathf.Lerp(0.8f, 1.5f, Mathf.Abs(speedNormalized));
            engineSource.volume = Mathf.Lerp(0.3f, 0.8f, Mathf.Abs(speedNormalized)) * sfxVolume * masterVolume;

            if (!engineSource.isPlaying)
            {
                engineSource.clip = engineIdle;
                engineSource.Play();
            }
        }

        /// <summary>
        /// Active/désactive le bip de recul
        /// </summary>
        public void SetReverseBeep(bool active)
        {
            // À implémenter avec une source dédiée ou une coroutine
            if (active)
            {
                // Joue le bip en boucle
            }
        }

        #endregion

        #region Volume Control

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        private void UpdateAllVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
            if (engineSource != null)
                engineSource.volume *= masterVolume;
        }

        #endregion
    }
}