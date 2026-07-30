using _2DTopDown;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _2DTopDown
{
    public class GameManager_Project : MonoBehaviour
    {
        // 스테이지별 맵에 담을 변수
        public static int SelectedStageIndex = 0;
        private const string FirstTutorialSeenKey = "FirstTutorialSeen_Map1";

        [Header("생성할 맵에 담을 변수")]
        public GameObject[] Maps;

        [Header("게임 UI")]
        public TextMeshProUGUI TimerTxt;
        public TextMeshProUGUI ScoreTxt;
        public TextMeshProUGUI WeaponNameTxt;
        public TextMeshProUGUI WeaponAmmoTxt;
        public TextMeshProUGUI PistolReload_ArlarmTxt;
        public TMP_Text loadingSecText;

        [Header("팝업(첫 튜토리얼)")]
        public GameObject GameFirstTutorialPanel;
        public Button CountinueButton_FirstTutorial;
        public TMP_Text CountinueButtonOpenSecText;
        
        [Header("팝업(게임오버, 게임 클리어, 일시정지)")]
        public GameObject GameOver;
        public GameObject GameClear;
        public GameObject GamePause;

        [Header("게임 UI/ 체력")]
        public Image HealthBar;
        public TextMeshProUGUI HealthNum;

        [Header("게임 UI / 무기")]
        public Image WeaponIcon;
        public Image WeaponAmmoGuage;
        public Sprite[] WeaponIcons;
        public Sprite WeaponNullIcon;   // 아이템 무기에 담긴 탄약이 모두 소진되면 빈 아이콘으로 돌아오기
        public Image Scope;

        [Tooltip("3번 무기 슬롯에 표시할 아이콘 제어")]
        public Image ItemWeaponIcon;

        [Header("게임 UI / 플레이어 피격")]
        public GameObject PlayerHitEffect;
        public GameObject PlayerDangerEffect;

        [Header("무기 선택 시 나타나는 이미지 연출")]
        public Toggle[] WeaponSelectToggles;
        //public GameObject WeaponSelectedImage;

        [Header("게임오버 및 게임 클리어 설정")]
        public bool gameOver = false;
        public bool gameClear = false;

        [HideInInspector] // 게임점수. 스크립트만 제어할 것이기에 인스펙터를 숨김
        public int currentScore = 0;

        [Header("체력량에 따른 색상조정")]
        // 플레이어 체력 색상원복
        public Color healthColor = new Color32(164, 146, 84, 255);

        // 플레이어 체력이 50% 일 때
        public Color healthWarningColor = new Color32(234, 157, 26, 255);

        // 플레이어 체력이 20% 일 때
        public Color healthDangerColor = new Color32(255, 59, 59, 255);

        [HideInInspector]
        // 적 처치 수 (스크립트로만 제어할 예정이기에 인스펙터를 숨김)
        public int KillCount;

        // 싱글톤 인스턴스 설정
        public static GameManager_Project instance;
        private bool isFirstTutorialOpen;
        private bool isLoadingScene;
        void Awake()
        {
            // 싱글톤 초기화
            if (instance == null) instance = this;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (Maps != null && SelectedStageIndex < Maps.Length)
            {
                Instantiate(Maps[SelectedStageIndex], Vector3.zero, Quaternion.identity);
            }

            Time.timeScale = 1;
            ScoreTxt.text = $"Score: {currentScore}";
            PistolReload_ArlarmTxt.enabled = false ;
            PlayerHitEffect.SetActive(false);
            PlayerDangerEffect.SetActive(false);
            GameOver.SetActive(false);
            GameClear.SetActive(false);
            Scope.gameObject.SetActive(false);
            GamePause.SetActive(false);
            if (loadingSecText != null)
                loadingSecText.gameObject.SetActive(false);

            SetupFirstTutorial();
        }

        void Update()
        {
            TimerTxt.text = TimeSpan.FromSeconds(Time.timeSinceLevelLoad).ToString("mm\\:ss");

            if (isLoadingScene)
                return;

            if (gameOver == true)
            {
                if(Input.GetKeyDown(KeyCode.R))
                {
                    StartCoroutine(LoadSceneWithLoadingText("MainGame"));
                }
                else if(Input.GetKeyDown(KeyCode.Escape))
                {
                    SelectedStageIndex = 0;
                    StartCoroutine(LoadSceneWithLoadingText("Lobby"));
                }
            }

            if (gameClear == true)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    GameCountinue();
                    return;
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GameEnd();
                    return;
                }
            }
        
            if(!gameOver && !gameClear && !isFirstTutorialOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Time.timeScale = 0;
                GamePause.SetActive(true);
            }
        }

        // Map1 최초 시작 시 조작 튜토리얼 팝업 제어
        private void SetupFirstTutorial()
        {
            if (GameFirstTutorialPanel != null)
                GameFirstTutorialPanel.SetActive(false);

            if (CountinueButton_FirstTutorial != null)
            {
                CountinueButton_FirstTutorial.interactable = true;
                CountinueButton_FirstTutorial.onClick.RemoveListener(CloseFirstTutorial);
                CountinueButton_FirstTutorial.onClick.AddListener(CloseFirstTutorial);
            }

            if (SelectedStageIndex == 0 && PlayerPrefs.GetInt(FirstTutorialSeenKey, 0) == 0)
                ShowFirstTutorial();
        }

        private void ShowFirstTutorial()
        {
            if (GameFirstTutorialPanel == null)
                return;

            isFirstTutorialOpen = true;
            Time.timeScale = 0f;
            GameFirstTutorialPanel.SetActive(true);

            if (CountinueButton_FirstTutorial != null)
            {
                CountinueButton_FirstTutorial.interactable = false;
                StartCoroutine(EnableFirstTutorialButton());
            }
        }

        private IEnumerator EnableFirstTutorialButton()
        {
            const int buttonOpenDelaySec = 3;

            for (int sec = buttonOpenDelaySec; sec > 0; sec--)
            {
                if (CountinueButtonOpenSecText != null)
                    CountinueButtonOpenSecText.text = $"{sec}초 후 버튼 활성화";

                yield return new WaitForSecondsRealtime(1f);
            }

            if (CountinueButton_FirstTutorial != null)
                CountinueButton_FirstTutorial.interactable = true;

            if (CountinueButtonOpenSecText != null)
                CountinueButtonOpenSecText.text = "돌아가기";
        }

        public void CloseFirstTutorial()
        {
            PlayerPrefs.SetInt(FirstTutorialSeenKey, 1);
            PlayerPrefs.Save();

            isFirstTutorialOpen = false;

            if (GameFirstTutorialPanel != null)
                GameFirstTutorialPanel.SetActive(false);

            Time.timeScale = 1f;
        }

        public static bool isAllClear = false;
        public void GameCountinue()
        {
            if (isLoadingScene)
                return;

            isAllClear = false;
            SelectedStageIndex++;

            if (SelectedStageIndex >= Maps.Length)
            {
                // 모든 스테이지 클리어 시 로비로
                isAllClear = true;
                StartCoroutine(LoadSceneWithLoadingText("Lobby"));
                return;
            }

            StartCoroutine(LoadSceneWithLoadingText("MainGame"));
        }
        public void GameEnd()
        {
            if (isLoadingScene)
                return;

            StartCoroutine(LoadSceneWithLoadingText("Lobby"));
        }

        private IEnumerator LoadSceneWithLoadingText(string sceneName)
        {
            isLoadingScene = true;
            Time.timeScale = 1f;
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

            if (loadingSecText != null)
            {
                loadingSecText.gameObject.SetActive(true);

                string[] loadingTexts =
                {
                    "배치중..",
                    "배치중...",
                    "배치중...."
                };

                int loadingTextIndex = 0;
                float nextTextChangeTime = 0f;
                while (!loadOperation.isDone)
                {
                    if (Time.unscaledTime >= nextTextChangeTime)
                    {
                        loadingSecText.text = loadingTexts[loadingTextIndex];
                        loadingTextIndex = (loadingTextIndex + 1) % loadingTexts.Length;
                        nextTextChangeTime = Time.unscaledTime + 1f;
                    }

                    yield return null;
                }
            }
            else
            {
                while (!loadOperation.isDone)
                    yield return null;
            }
        }

        public void TheGamePause()
        {
            Time.timeScale = 1;
            GamePause.SetActive(false);
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
                    Scope.gameObject.SetActive(false);
                    break;
                case Player.WeaponTypes.Pistol:
                    WeaponNameTxt.text = "1911 Pistol";
                    WeaponIcon.sprite = WeaponIcons[1];
                    SetToggleState(1);
                    // 원래는 조준사격 상태에서 탄약이 소진 될 때 Player 스크립트로 감춰지게 제어할 예정이었으나
                    // 이상하게도 false가 안 먹히는 문제가 발생해 어쩔 수 없이 게임매니저에서 제어하기로 결정
                    Scope.gameObject.SetActive(false);
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
                    else if(Player.instance.currentItemWeaponType == Item.ItemTypes.SMGSD)
                    {
                        WeaponNameTxt.text = "MP5 SD";
                        WeaponIcon.sprite = WeaponIcons[4];
                        ItemWeaponIcon.sprite = WeaponIcons[4];
                    }
                    else if(Player.instance.currentItemWeaponType == Item.ItemTypes.DMR)
                    {
                        WeaponNameTxt.text = "M1A DMR";
                        WeaponIcon.sprite = WeaponIcons[5];
                        ItemWeaponIcon.sprite = WeaponIcons[5];
                    }
                    else if (Player.instance.currentItemWeaponType == Item.ItemTypes.Null_Weapon)
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

        // * 백업용 *
        // R키로 통합해서 마지막 스테이지까지 클리어하면 메인화면으로 가고 엔딩으로 가는 연출을
        // 할 예정이었으나 키가 겹치는 문제점인지 로비로 가지 못해 결국 버튼식으로 철저하게 분리하는
        // 방향으로 가기로 결정.
        //if(gameClear == true)
        //{
        //    if (Input.GetKeyDown(KeyCode.R))
        //    {
        //        // [디버깅 로그] 현재 상태를 콘솔창에서 확인하기 위함
        //        Debug.Log($"[Clear] 현재 인덱스: {SelectedStageIndex}, 총 맵 수: {Maps.Length}");

        //        // 1. 현재 맵이 마지막 맵(인덱스 2)인지 확인
        //        // Maps.Length가 3이면, Maps.Length - 1은 2입니다.
        //        if (SelectedStageIndex >= Maps.Length - 1)
        //        {
        //            Debug.Log("마지막 스테이지입니다. 로비로 이동합니다.");

        //            // [중요] 다음 게임을 위해 인덱스를 0으로 초기화하고 로비로 이동
        //            SceneManager.LoadScene("Lobby");
        //            SelectedStageIndex = 0;
        //        }
        //        else
        //        {
        //            // 2. 아직 다음 맵이 남아있다면 인덱스 증가 후 재시작
        //            SelectedStageIndex++;
        //            SceneManager.LoadScene("MainGame");
        //        }
        //    }
        //    else if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        SelectedStageIndex = 0;
        //        SceneManager.LoadScene("Lobby");
        //    }
        //}
    }
}
