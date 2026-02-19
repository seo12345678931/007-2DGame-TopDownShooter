using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _2DTopDown;

namespace _2DTopDown
{
    public class GameManager_Project : MonoBehaviour
    {
        [Header("게임 UI")]
        public TextMeshProUGUI TimerTxt;
        public TextMeshProUGUI ScoreTxt;
        public TextMeshProUGUI WeaponNameTxt;
        public Image WeaponIcon;
        public Sprite[] WeaponIcons;

        [Header("무기 선택 토글")]
        public Toggle[] WeaponSelectToggles;

        [Header("체력 0에 도달 시 게임오버 설정")]
        public bool gameOver = false;

        private int currentScore = 0;

        // 싱글톤 인스턴스 설정
        public static GameManager_Project instance;
        void Awake()
        {
            // 싱글톤 초기화
            if (instance == null) instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            ScoreTxt.text = $"Score: {currentScore}";
        }

        // Update is called once per frame
        void Update()
        {
            TimerTxt.text = TimeSpan.FromSeconds(Time.timeSinceLevelLoad).ToString("mm\\:ss");
        }

        public void SelectWeapon(Player.WeaponTypes weaponType)
        {
            switch (weaponType)
            {
                case Player.WeaponTypes.Knife:
                    WeaponNameTxt.text = "Combat Knife";
                    WeaponIcon.sprite = WeaponIcons[0];
                    SetToggleState(0);
                    break;
                case Player.WeaponTypes.Pistol:
                    WeaponNameTxt.text = "1911 Pistol";
                    WeaponIcon.sprite = WeaponIcons[1];
                    SetToggleState(1);
                    break;
            }
        }

        // 반복되는 코드를 줄이기 위한 헬퍼 함수입니다.
        private void SetToggleState(int index)
        {
            for (int i = 0; i < WeaponSelectToggles.Length; i++)
            {
                // 인덱스가 맞으면 true, 아니면 false
                WeaponSelectToggles[i].isOn = (i == index);
            }
        }

        public void AddScore(int Point)
        {
            currentScore += Point;
            ScoreTxt.text = $"Score: {currentScore}";
        }
    }
}
