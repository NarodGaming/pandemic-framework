using AlkalineThunder.Pandemic.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace AlkalineThunder.Pandemic.Audio
{
    /// <summary>
    /// Provides Pandemic Framework with the ability to play audio.
    /// </summary>
    [RequiresModule(typeof(SettingsService))]
    public class AudioSystem : EngineModule
    {
        private SoundEffectInstance _bgm;
        private SoundEffectInstance _area;
        private SoundEffectInstance _bgmFade;
        private bool _areaLoop;
        private float _fadeDuration; 
        private float _fadePercentage;
        private float _masterVolume = 1;
        private float _sfxVolume = 1;
        private float _bgmVolume = 1;

        [Exec("audio.setMasterVolume")]
        public void SetMasterVolume(float value)
        {
            _masterVolume = MathHelper.Clamp(value, 0, 1);
        }
        
        [Exec("audio.setBgmVolume")]
        public void SetBgmVolume(float value)
        {
            _bgmVolume = MathHelper.Clamp(value, 0, 1);
        }
        
        [Exec("audio.setSfxVolume")]
        public void SetSfxVolume(float value)
        {
            _sfxVolume = MathHelper.Clamp(value, 0, 1);
        }

        
        [Exec("audio.playSong")]
        public void PlaySong(string path, float fade = 0)
        {
            var soundEffect = GameLoop.Content.Load<SoundEffect>(path);

            if (fade > 0)
            {
                if (_bgmFade != null)
                {
                    if (_bgm != null)
                    {
                        _bgm.Stop();
                        _bgm.Dispose();
                    }

                    _bgm = _bgmFade;
                }

                _bgmFade = soundEffect.CreateInstance();
                _bgmFade.IsLooped = true;
                _bgmFade.Play();

                _fadePercentage = 0;
            }
            else
            {
                if (_bgm != null)
                {
                    _bgm.Stop();
                    _bgm.Dispose();
                }

                _bgm = soundEffect.CreateInstance();
                _bgm.IsLooped = true;
                _bgm.Play();
                _fadePercentage = 0;
            }

            _fadeDuration = fade;
        }
        
        [Exec("audio.playAreaSong")]
        public void PlayAreaMusic(string path, bool looped = false)
        {
            var soundEffect = GameLoop.Content.Load<SoundEffect>(path);

            _areaLoop = looped;
            _area = soundEffect.CreateInstance();
            _area.Play();
        }

        [Exec("audio.stopAreaSong")]
        public void StopAreaMusic()
        {
            if (_area != null)
            {
                _area.Stop();
                StopAreaMusicInternal();
            }
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            if (_area != null)
            {
                PauseBgm();
                CheckAreaState();
            }
            else
            {
                ProcessBgmFade(gameTime.ElapsedGameTime.TotalSeconds);
            }
            
            SetTrackVolumes();
            
            base.OnUpdate(gameTime);
        }

        private void ProcessBgmFade(double frameTime)
        {
            if (_bgmFade != null)
            {
                if (_fadePercentage < 1)
                {
                    if (_fadeDuration <= 0)
                    {
                        _fadePercentage = 1;
                    }
                    else
                    {
                        _fadePercentage = MathHelper.Clamp(_fadePercentage + ((float) frameTime / _fadeDuration), 0, 1);
                    }
                }
                else
                {
                    StopBgmInternal();
                    _bgm = _bgmFade;
                    _bgmFade = null;
                    _fadePercentage = 0;
                    _fadeDuration = 0;
                }
            }
        }

        private void StopBgmInternal()
        {
            _bgm?.Stop();
            _bgm?.Dispose();
            _bgm = null;
        }
        
        private void SetTrackVolumes()
        {
            float bgmVolume = (_masterVolume * _bgmVolume) * (1 - _fadePercentage);
            float fadeBgmVolume = (_masterVolume * _bgmVolume) * _fadePercentage;

            if (_bgm != null && _bgm.State == SoundState.Playing)
            {
                _bgm.Volume = bgmVolume;
            }
            
            if (_bgmFade != null && _bgmFade.State == SoundState.Playing)
            {
                _bgmFade.Volume = fadeBgmVolume;
            }
        }

        protected override void OnInitialize()
        {
            GameUtils.Log("Audio system is alive.");
            base.OnInitialize();
        }

        protected override void OnUnload()
        {
            StopAllAudio();
            
            GameUtils.Log("Audio system has been killed.");
            base.OnUnload();
        }

        private void StopAllAudio()
        {
            _bgm?.Stop();
            _area?.Stop();
            _bgmFade?.Stop();
            
            _bgm?.Dispose();
            _area?.Dispose();
            _bgmFade?.Dispose();

            _bgm = null;
            _area = null;
            _bgmFade = null;
        }

        private void PauseBgm()
        {
            if (_bgm != null && _bgm.State == SoundState.Playing)
            {
                _bgm.Pause();
            }
            
            if (_bgmFade != null && _bgmFade.State == SoundState.Playing)
            {
                _bgmFade.Pause();
            }
        }

        private void CheckAreaState()
        {
            if (_area != null && _area.State == SoundState.Stopped)
            {
                if (_areaLoop)
                {
                    _area.Play();
                }
                else
                {
                    StopAreaMusicInternal();
                }
            }
        }

        private void StopAreaMusicInternal()
        {
            _area?.Dispose();
            _area = null;

            _bgm?.Play();
            _bgmFade?.Play();

            _areaLoop = false;
        }
    }
}