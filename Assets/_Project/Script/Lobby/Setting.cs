using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace _2DTopDown
{
    public class Setting : MonoBehaviour
    {
        private const string ScreenModeKey = "Setting.ScreenMode";
        private const string ResolutionModeKey = "Setting.ResolutionMode";
        private const string BgmVolumeKey = "Setting.BGMVolume";
        private const string SfxVolumeKey = "Setting.SFXVolume";

        private static readonly Vector2Int[] ResolutionOptions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
        };

        public TMP_Dropdown ScreenMode;
        public TMP_Dropdown ResoultionMode;

        [Header("BGM 제어")]
        public AudioMixer BGM_AudioMixer;
        public Slider BGM_Slider;

        [Header("효과음 제어")]
        public AudioMixer SFX_AudioMixer;
        public Slider SFX_Slider;

        private void Start()
        {
            if (BGM_Slider != null)
            {
                BGM_Slider.onValueChanged.AddListener(SetBGMVolume);
            }

            if (SFX_Slider != null)
            {
                SFX_Slider.onValueChanged.AddListener(SetSFXVolume);
            }

            LoadSettings();
        }

        private void OnDestroy()
        {
            if (BGM_Slider != null)
            {
                BGM_Slider.onValueChanged.RemoveListener(SetBGMVolume);
            }

            if (SFX_Slider != null)
            {
                SFX_Slider.onValueChanged.RemoveListener(SetSFXVolume);
            }
        }

        private void LoadSettings()
        {
            int savedScreenMode = Mathf.Clamp(PlayerPrefs.GetInt(ScreenModeKey, 0), 0, ScreenMode.options.Count - 1);
            int savedResolutionMode = Mathf.Clamp(PlayerPrefs.GetInt(ResolutionModeKey, 2), 0, ResolutionOptions.Length - 1);
            float savedBgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, GetMixerVolumeAsSlider(BGM_AudioMixer, "BGMVolume")));
            float savedSfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, GetMixerVolumeAsSlider(SFX_AudioMixer, "SFXVolume")));

            ScreenMode.SetValueWithoutNotify(savedScreenMode);
            ResoultionMode.SetValueWithoutNotify(savedResolutionMode);
            BGM_Slider.SetValueWithoutNotify(savedBgmVolume);
            SFX_Slider.SetValueWithoutNotify(savedSfxVolume);

            ApplyScreenMode(savedScreenMode);
            ApplyResolution(savedResolutionMode, savedScreenMode);
            ApplyBGMVolume(savedBgmVolume);
            ApplySFXVolume(savedSfxVolume);
        }

        public void OnScreenModeChanged(int index)
        {
            int clampedIndex = Mathf.Clamp(index, 0, ScreenMode.options.Count - 1);

            ApplyScreenMode(clampedIndex);
            ApplyResolution(ResoultionMode.value, clampedIndex);

            PlayerPrefs.SetInt(ScreenModeKey, clampedIndex);
            PlayerPrefs.Save();
        }

        public void OnresoultionModeChanged(int index)
        {
            int clampedIndex = Mathf.Clamp(index, 0, ResolutionOptions.Length - 1);

            ApplyResolution(clampedIndex, ScreenMode.value);

            PlayerPrefs.SetInt(ResolutionModeKey, clampedIndex);
            PlayerPrefs.Save();
        }

        public void SetBGMVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);

            ApplyBGMVolume(clampedVolume);

            PlayerPrefs.SetFloat(BgmVolumeKey, clampedVolume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            float clampedVolume = Mathf.Clamp01(volume);

            ApplySFXVolume(clampedVolume);

            PlayerPrefs.SetFloat(SfxVolumeKey, clampedVolume);
            PlayerPrefs.Save();
        }

        private void ApplyScreenMode(int index)
        {
            Screen.fullScreenMode = GetFullScreenMode(index);
        }

        private void ApplyResolution(int resolutionIndex, int screenModeIndex)
        {
            int clampedIndex = Mathf.Clamp(resolutionIndex, 0, ResolutionOptions.Length - 1);
            Vector2Int resolution = ResolutionOptions[clampedIndex];

            Screen.SetResolution(resolution.x, resolution.y, GetFullScreenMode(screenModeIndex));
        }

        private void ApplyBGMVolume(float volume)
        {
            float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
            BGM_AudioMixer.SetFloat("BGMVolume", dB);
        }

        private void ApplySFXVolume(float volume)
        {
            float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
            SFX_AudioMixer.SetFloat("SFXVolume", dB);
        }

        private static FullScreenMode GetFullScreenMode(int index)
        {
            return index switch
            {
                1 => FullScreenMode.Windowed,
                2 => FullScreenMode.ExclusiveFullScreen,
                _ => FullScreenMode.FullScreenWindow,
            };
        }

        private static float GetMixerVolumeAsSlider(AudioMixer mixer, string parameterName)
        {
            if (!mixer.GetFloat(parameterName, out float currentVolume))
            {
                return 1f;
            }

            return Mathf.Clamp01(Mathf.Pow(10f, currentVolume / 20f));
        }
    }
}
