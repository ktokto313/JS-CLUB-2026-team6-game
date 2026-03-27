using UnityEngine;

namespace _Game.Scripts.Core
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")] 
        [SerializeField] private AudioClip music;

        [Header("SFX Clips (EventManager)")]
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip playerHitClip;
        [SerializeField] private AudioClip deadClip;
        [SerializeField] private AudioClip playerDeadClip;
        [SerializeField] private AudioClip playerAttackClip;
        [SerializeField] private AudioClip playerHealthUpdateClip;
        [SerializeField] private AudioClip pointUpdateClip;

        [Header("SFX Volume")]
        [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
        
        private const string MusicVolumeKey = "MusicVolume";
        private const string SFXVolumeKey = "SFXVolume";
        private const float DefaultVolume = 1.0f;

        private void Start()
        {
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, DefaultVolume);
            sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, DefaultVolume);
            
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            sfxSource.volume = musicVolume;
            PlayMusic();
            
            if (EventManager.current == null) return;

            EventManager.current.onHitAction += HandleHit;
            EventManager.current.onPlayerHitAction += HandlePlayerHit;
            EventManager.current.onDeadAction += HandleDead;
            EventManager.current.onPlayerDeadAction += HandlePlayerDead;
            EventManager.current.onPlayerAttackAction += HandlePlayerAttack;
            EventManager.current.onPlayerHealthUpdateAction += HandlePlayerHealthUpdate;
            EventManager.current.onPointUpdateAction += HandlePointUpdate;
            EventManager.current.onMusicVolumeSliderUpdateAction += HandleMusicVolumeSliderUpdate;
            EventManager.current.onSFXVolumeSliderUpdateAction += HandleSFXVolumeSliderUpdate;
        }

        private void HandleHit(Vector3 hitPosition)
        {
            PlayAtPoint(hitClip, hitPosition);
        }

        private void HandlePlayerHit(Vector3 hitPosition)
        {
            PlayAtPoint(playerHitClip, hitPosition);
        }

        private void HandleDead()
        {
            PlayOneShot(deadClip);
        }

        private void HandlePlayerDead()
        {
            PlayOneShot(playerDeadClip);
        }

        private void HandlePlayerAttack()
        {
            PlayOneShot(playerAttackClip);
        }

        private void HandlePlayerHealthUpdate(int _)
        {
            PlayOneShot(playerHealthUpdateClip);
        }

        private void HandlePointUpdate(int _)
        {
            PlayOneShot(pointUpdateClip);
        }

        private void HandleMusicVolumeSliderUpdate(float volume)
        {
            musicVolume = volume;
            musicSource.volume = musicVolume;
        }

        private void HandleSFXVolumeSliderUpdate(float volume)
        {
            sfxVolume = volume;
            sfxSource.volume = sfxVolume;
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (!clip|| !sfxSource) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private void PlayMusic()
        {
            if (!musicSource || !music) return;
            musicSource.clip = music;
            musicSource.Play();
        }

        private void PlayAtPoint(AudioClip clip, Vector3 position)
        {
            if (!clip) return;
            AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
        }
    }
}
