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
        [SerializeField, Range(0f, 1f)] private float positionalSfxVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float oneShotSfxVolume = 1f;

        private void Start()
        {
            
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

        public void ChangeMusicVolume(float volume)
        {
            musicVolume = volume;
        }

        public void ChangeSfxVolume(float volume)
        {
            oneShotSfxVolume = volume;
            positionalSfxVolume = volume;
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
            sfxSource.volume = oneShotSfxVolume * 0.005f;
            PlayOneShot(deadClip);
            sfxSource.volume = oneShotSfxVolume;
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
        }

        private void HandleSFXVolumeSliderUpdate(float volume)
        {
            oneShotSfxVolume = volume;
            positionalSfxVolume = volume;
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (!clip|| !sfxSource) return;
            sfxSource.PlayOneShot(clip, oneShotSfxVolume);
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
            AudioSource.PlayClipAtPoint(clip, position, positionalSfxVolume);
        }
    }
}
