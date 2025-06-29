using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;

namespace InfinitePenanceRL
{
    // Менеджер фоновой музыки (поддерживает WAV файлы)
    public class MusicManager
    {
        private List<string> _musicFiles = new List<string>();
        private int _currentTrackIndex = 0;
        private bool _isPlaying = false;
        private bool _isEnabled = true;
        private System.Windows.Forms.Timer _fadeTimer = new System.Windows.Forms.Timer();
        private float _volume = 1.0f;
        private WaveOutEvent _waveOut;
        private WaveFileReader _currentWaveStream;

        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set 
            { 
                _isEnabled = value; 
                if (!value) StopMusic();
            } 
        }

        public float Volume 
        { 
            get => _volume; 
            set => _volume = Math.Clamp(value, 0.0f, 1.0f); 
        }

        public MusicManager()
        {
            _waveOut = new WaveOutEvent();
            LoadMusicFiles();
            SetupFadeTimer();
        }

        // Загружаем все WAV файлы из папки music
        private void LoadMusicFiles()
        {
            string musicFolder = "music";
            if (!Directory.Exists(musicFolder))
            {
                Directory.CreateDirectory(musicFolder);
                LogThrottler.Log("Создана папка music", "music");
                LogThrottler.Log("Поместите WAV файлы в папку music для фоновой музыки", "music");
                return;
            }

            try
            {
                var wavFiles = Directory.GetFiles(musicFolder, "*.wav", SearchOption.TopDirectoryOnly);
                _musicFiles = wavFiles.ToList();
                
                LogThrottler.Log($"Загружено {_musicFiles.Count} музыкальных файлов", "music");
                
                if (_musicFiles.Count == 0)
                {
                    LogThrottler.Log("В папке music нет WAV файлов. Поместите WAV файлы для фоновой музыки.", "music");
                }
                
                foreach (var file in _musicFiles)
                {
                    LogThrottler.Log($"Музыкальный файл: {Path.GetFileName(file)}", "music");
                }
            }
            catch (Exception ex)
            {
                LogThrottler.Log($"Ошибка загрузки музыки: {ex.Message}", "music");
            }
        }

        // Настраиваем таймер для плавного перехода между треками
        private void SetupFadeTimer()
        {
            _fadeTimer.Interval = 1000; // Проверяем каждую секунду
            _fadeTimer.Tick += (sender, e) =>
            {
                // Проверяем, закончился ли текущий трек
                if (_isPlaying && _waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    PlayNextTrack();
                }
            };
        }

        // Начинаем воспроизведение музыки
        public void StartMusic()
        {
            if (!_isEnabled || _musicFiles.Count == 0) 
            {
                return;
            }

            try
            {
                StopMusic(); // Останавливаем предыдущий трек
                
                if (_currentTrackIndex >= _musicFiles.Count)
                    _currentTrackIndex = 0;

                string currentFile = _musicFiles[_currentTrackIndex];
                
                // Используем NAudio для воспроизведения музыки
                _currentWaveStream = new WaveFileReader(currentFile);
                _waveOut.Init(_currentWaveStream);
                _waveOut.Play();
                _isPlaying = true;
                _fadeTimer.Start();
            }
            catch (Exception ex)
            {
                LogThrottler.Log($"Ошибка воспроизведения музыки: {ex.Message}", "music");
                PlayNextTrack(); // Пробуем следующий трек
            }
        }

        // Останавливаем музыку
        public void StopMusic()
        {
            _waveOut.Stop();
            _isPlaying = false;
            _fadeTimer.Stop();
            
            _currentWaveStream?.Dispose();
            _currentWaveStream = null;
        }

        // Переходим к следующему треку
        public void PlayNextTrack()
        {
            _currentTrackIndex++;
            if (_currentTrackIndex >= _musicFiles.Count)
                _currentTrackIndex = 0;

            StartMusic();
        }

        // Переходим к предыдущему треку
        public void PlayPreviousTrack()
        {
            _currentTrackIndex--;
            if (_currentTrackIndex < 0)
                _currentTrackIndex = _musicFiles.Count - 1;

            StartMusic();
        }

        // Пауза/возобновление
        public void TogglePause()
        {
            if (_isPlaying)
            {
                if (_waveOut.PlaybackState == PlaybackState.Playing)
                {
                    _waveOut.Pause();
                }
                else
                {
                    _waveOut.Play();
                }
            }
            else
            {
                StartMusic();
            }
        }

        // Освобождаем ресурсы
        public void Dispose()
        {
            StopMusic();
            _waveOut?.Dispose();
            _fadeTimer?.Dispose();
        }
    }
} 