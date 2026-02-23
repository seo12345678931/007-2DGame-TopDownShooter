using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _2DTopDown;
using System.Collections;

namespace _2DTopDown
{
    public class GameManager_Project : MonoBehaviour
    {
        [Header("게임 UI")]
        public TextMeshProUGUI TimerTxt;
        public TextMeshProUGUI ScoreTxt;
        public TextMeshProUGUI WeaponNameTxt;
        public TextMeshProUGUI WeaponAmmoTxt;
        public TextMeshProUGUI PistolReload_ArlarmTxt;
        public GameObject GameOver;

        [Header("게임 UI/ 체력")]
        public Image HealthBar;
        public TextMeshProUGUI HealthNum;

        [Header("게임 UI / 무기")]
        public Image WeaponIcon;
        public Image WeaponAmmoGuage;
        public Sprite[] WeaponIcons;
        public Sprite WeaponNullIcon;   // 아이템 무기에 담긴 탄약이 모두 소진되면 빈 아이콘으로 돌아오기

        [Tooltip("3번 무기 슬롯에 표시할 아이콘 제어")]
        public Image ItemWeaponIcon;

        [Header("게임 UI / 플레이어 피격")]
        public GameObject PlayerHitEffect;
        public GameObject PlayerDangerEffect;

        [Header("무기 선택 토글")]
        public Toggle[] WeaponSelectToggles;

        [Header("체력 0에 도달 시 게임오버 설정")]
        public bool gameOver = false;

        private int currentScore = 0;

        [Header("체력량에 따른 색상조정")]
        // 플레이어 체력 색상원복
        public Color healthColor = new Color32(164, 146, 84, 255);

        // 플레이어 체력이 50% 일 때
        public Color healthWarningColor = new Color32(234, 157, 26, 255);

        // 플레이어 체력이 20% 일 때
        public Color healthDangerColor = new Color32(255, 59, 59, 255);

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
            PistolReload_ArlarmTxt.enabled = false ;
            PlayerHitEffect.SetActive(false);
            PlayerDangerEffect.SetActive(false);
            GameOver.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            TimerTxt.text = TimeSpan.FromSeconds(Time.timeSinceLevelLoad).ToString("mm\\:ss");

            if (gameOver && Input.GetKeyDown(KeyCode.R))
            {
                Application.LoadLevel(Application.loadedLevel);
            }
        }

        public void SelectWeapon(Player.WeaponTypes weaponType)
        {
            switch (weaponType)
            {
                case Player.WeaponTypes.Knife:
                    WeaponNameTxt.text = "Combat Knife";
                    WeaponAmmoTxt.text = "";
                    WeaponIcon.sprite = WeaponIcons[0];
                    SetToggleState(0);
                    break;
                case Player.WeaponTypes.Pistol:
                    WeaponNameTxt.text = "1911 Pistol";
                    WeaponIcon.sprite = WeaponIcons[1];
                    SetToggleState(1);
                    break;
                case Player.WeaponTypes.ItemWeapon:
                    if (Player.instance.currentItemWeaponType == Item.ItemTypes.Rifle)
                    {
                        WeaponNameTxt.text = "ASK209 AR";
                        WeaponIcon.sprite = WeaponIcons[2];
                        ItemWeaponIcon.sprite = WeaponIcons[2];
                    }
                    else if (Player.instance.currentItemWeaponType == Item.ItemTypes.Shotgun)
                    {
                        WeaponNameTxt.text = "Mosberg SG";
                        WeaponIcon.sprite = WeaponIcons[3];
                        ItemWeaponIcon.sprite = WeaponIcons[3];
                    }
                    else
                    {
                        WeaponNameTxt.text = "None";
                        WeaponIcon.sprite = WeaponNullIcon;
                        ItemWeaponIcon.sprite = WeaponNullIcon;
                    }
                        SetToggleState(2);
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
