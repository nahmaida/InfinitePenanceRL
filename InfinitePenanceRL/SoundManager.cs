using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using NAudio.Wave;

namespace InfinitePenanceRL
{
    // Менеджер звуковых эффектов с поддержкой одновременного воспроизведения
    public class SoundManager
    {
        private Dictionary<string, List<byte[]>> _soundCategories = new Dictionary<string, List<byte[]>>();
        private Random _random = new Random();
        private bool _isEnabled = true;
        private float _volume = 1.0f;

        public bool IsEnabled 
        { 
            get => _isEnabled; 
            set => _isEnabled = value; 
        }

        public float Volume 
        { 
            get => _volume; 
            set => _volume = Math.Clamp(value, 0.0f, 1.0f); 
        }

        public SoundManager()
        {
            LoadSoundEffects();
        }

        // Загружаем все звуковые эффекты из папок в память
        private void LoadSoundEffects()
        {
            string sfxFolder = "sfx";
            if (!Directory.Exists(sfxFolder))
            {
                Directory.CreateDirectory(sfxFolder);
                LogThrottler.Log("Создана папка sfx", "sound");
                return;
            }

            try
            {
                // Загружаем звуки атаки
                LoadSoundCategory("battle", Path.Combine(sfxFolder, "battle"));
                
                // Загружаем звуки интерфейса
                LoadSoundCategory("interface", Path.Combine(sfxFolder, "interface"));
                
                // Загружаем звуки ворчания врагов
                LoadSoundCategory("grunt", Path.Combine(sfxFolder, "grunt"));
                
                // Загружаем звуки боли
                LoadSoundCategory("pain", Path.Combine(sfxFolder, "pain"));
                
                // Загружаем звуки смерти
                LoadSoundCategory("death", Path.Combine(sfxFolder, "death"));

                LogThrottler.Log("Звуковые эффекты загружены в память", "sound");
                foreach (var category in _soundCategories)
                {
                    LogThrottler.Log($"Категория {category.Key}: {category.Value.Count} звуков", "sound");
                }
            }
            catch (Exception ex)
            {
                LogThrottler.Log($"Ошибка загрузки звуковых эффектов: {ex.Message}", "sound");
            }
        }

        // Загружаем звуки из конкретной категории в память
        private void LoadSoundCategory(string categoryName, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                LogThrottler.Log($"Папка {folderPath} не найдена", "sound");
                _soundCategories[categoryName] = new List<byte[]>();
                return;
            }

            var wavFiles = Directory.GetFiles(folderPath, "*.wav", SearchOption.TopDirectoryOnly);
            var soundData = new List<byte[]>();
            
            foreach (var file in wavFiles)
            {
                try
                {
                    byte[] data = File.ReadAllBytes(file);
                    soundData.Add(data);
                }
                catch (Exception ex)
                {
                    LogThrottler.Log($"Ошибка загрузки файла {file}: {ex.Message}", "sound");
                }
            }
            
            _soundCategories[categoryName] = soundData;
            LogThrottler.Log($"Загружено {soundData.Count} звуков в категории {categoryName}", "sound");
        }

        // Воспроизводим случайный звук из категории
        public void PlayRandomSound(string category)
        {
            if (!_isEnabled || !_soundCategories.ContainsKey(category) || _soundCategories[category].Count == 0)
                return;

            try
            {
                var sounds = _soundCategories[category];
                byte[] randomSound = sounds[_random.Next(sounds.Count)];
                PlaySoundData(randomSound);
            }
            catch (Exception ex)
            {
                LogThrottler.Log($"Ошибка воспроизведения звука {category}: {ex.Message}", "sound");
            }
        }

        // Воспроизводим звук из данных в памяти
        public void PlaySoundData(byte[] soundData)
        {
            if (!_isEnabled || soundData == null)
                return;

            // Запускаем звук в отдельном потоке чтобы не мешать музыке
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (var stream = new MemoryStream(soundData))
                    using (var waveStream = new WaveFileReader(stream))
                    {
                        var waveOut = new WaveOutEvent();
                        waveOut.Init(waveStream);
                        waveOut.Play();
                        
                        // Ждём окончания воспроизведения
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(10);
                        }
                        
                        waveOut.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    LogThrottler.Log($"Ошибка воспроизведения звука: {ex.Message}", "sound");
                }
            });
        }

        // Воспроизводим звук атаки (случайный из 3)
        public void PlayAttackSound()
        {
            PlayRandomSound("battle");
        }

        // Воспроизводим звук клика кнопки
        public void PlayButtonClick()
        {
            if (_soundCategories.ContainsKey("interface") && _soundCategories["interface"].Count > 0)
            {
                // Ищем interface1.wav (первый звук в списке)
                var interfaceSounds = _soundCategories["interface"];
                if (interfaceSounds.Count > 0)
                {
                    PlaySoundData(interfaceSounds[0]);
                }
                else
                {
                    // Если interface1.wav не найден, играем случайный звук интерфейса
                    PlayRandomSound("interface");
                }
            }
        }

        // Воспроизводим случайный звук ворчания врага
        public void PlayEnemyGrunt()
        {
            PlayRandomSound("grunt");
        }

        // Воспроизводим случайный звук боли врага
        public void PlayEnemyPain()
        {
            PlayRandomSound("pain");
        }

        // Воспроизводим случайный звук смерти врага
        public void PlayEnemyDeath()
        {
            PlayRandomSound("death");
        }
    }
} 