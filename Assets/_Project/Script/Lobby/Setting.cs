using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace _2DTopDown
{
    public class Setting : MonoBehaviour
    {
        public TMP_Dropdown ScreenMode;

        [Header("BGM 제어")]
        public AudioMixer BGM_AudioMixer;
        public Slider BGM_Slider;

        [Header("효과음 제어")]
        public AudioMixer SFX_AudioMixer;
        public Slider SFX_Slider;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            float currentVolume;
            if (BGM_AudioMixer.GetFloat("BGMVolume", out currentVolume))
            {
                // dB를 다시 0~1 사이의 값으로 역산해서 슬라이더에 넣어줌
                BGM_Slider.value = Mathf.Pow(10, currentVolume / 20);
            }
            // 슬라이더 이벤트 연결
            BGM_Slider.onValueChanged.AddListener(SetBGMVolume);

            float currentVolume_SFX;
            if (SFX_AudioMixer.GetFloat("SFXVolume", out currentVolume_SFX))
            {
                // dB를 다시 0~1 사이의 값으로 역산해서 슬라이더에 넣어줌
                SFX_Slider.value = Mathf.Pow(10, currentVolume_SFX / 20);
            }
            // 슬라이더 이벤트 연결
            SFX_Slider.onValueChanged.AddListener(SetSFXVolume);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnScreenModeChanged(int index)
        {
            switch (index)
            {
                case 0: // 전체 화면 (전체 화면 창 모드)
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    Debug.Log("화면 모드: 전체 화면");
                    break;
                case 1: // 창 모드
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    Debug.Log("화면 모드: 창 모드");
                    break;
                case 2:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    Debug.Log("화면 모드: 테두리 없는 창 모드");
                    break;
            }
        }

        // 볼륨 조절 로직
        public void SetBGMVolume(float volume)
        {
            // 슬라이더 값(0.0001 ~ 1)을 로그 함수를 이용해 데시벨(-80dB ~ 0dB)로 변환
            // 0일 때 로그를 취하면 에러가 나므로 최소값을 아주 작게 설정합니다.
            float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;

            BGM_AudioMixer.SetFloat("BGMVolume", dB);
        }
        public void SetSFXVolume(float volume)
        {
            // 슬라이더 값(0.0001 ~ 1)을 로그 함수를 이용해 데시벨(-80dB ~ 0dB)로 변환
            // 0일 때 로그를 취하면 에러가 나므로 최소값을 아주 작게 설정합니다.
            float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;

            SFX_AudioMixer.SetFloat("SFXVolume", dB);
        }
    }
}
