using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2DTopDown
{
    public class Lobby : MonoBehaviour
    {
        [Header("처음에 게임진입 시 표시할 메인타이틀")]
        public GameObject MainTitle;

        [Header("메인메뉴")]
        public GameObject SelectMenu;

        [Header("스테이지 선택")]
        public GameObject SelectStage;

        [Header("환경설정")]
        public GameObject Setting;

        [Header("게임엔딩 출력")]
        public GameObject Ending;

        [Header("연출 애니메이션 제어")]
        public Animator Title_Anim;

        public AudioSource UISFX;
        public AudioClip StartButtonm;

        private CanvasGroup selectMenuCanvasGroup;
        private bool isLoaded = false; // 중복 실행 방지용 플래그

        public void Start()
        {
            Time.timeScale = 1f;

            MainTitle.SetActive(true);
            SelectMenu.SetActive(false);
            SelectStage.SetActive(false);
            Setting.SetActive(false);

            if (GameManager_Project.isAllClear)
            {
                StartCoroutine(IsEnding());

                // 중요: 한 번 보여줬으니 다시 로비에 올 때는 안 뜨도록 초기화
                GameManager_Project.isAllClear = false;
            }
            else
            {
                Ending.SetActive(false);
            }

            //  SelectMenu에 CanvasGroup이 없으면 추가하고 초기 알파값을 0으로 설정
            selectMenuCanvasGroup = SelectMenu.GetComponent<CanvasGroup>();
            if (selectMenuCanvasGroup == null)
                selectMenuCanvasGroup = SelectMenu.AddComponent<CanvasGroup>();

            selectMenuCanvasGroup.alpha = 0;
        }
        public IEnumerator IsEnding()
        {
            Ending.SetActive(true);
            yield return new WaitForSeconds(3);
            Ending.SetActive(false);
        }

        public void Update()
        {
            if (!isLoaded && Input.anyKeyDown)
            {
                LoadMainMenu();
                StartCoroutine(LoadMenuWithSound());
            }
        }

        // 스테이지 선택에서 다시 메뉴로 돌아갈 때
        public void ReLoadMenu()
        {
            MainTitle.SetActive(false);
            SelectMenu.SetActive(true);
            SelectStage.SetActive(false);
            Setting.SetActive(false);
            Ending.SetActive(false);
        }

        public void LoadMainMenu()
        {
            Title_Anim.SetBool("isMenu", true);
            MainTitle.SetActive(false);

            // 2. 코루틴 실행 ((endAlpha)초 동안 페이드), duration: 페이드하는데 걸리는 시간
            StartCoroutine(FadeSelectMenu(0f, 2f, 3.0f));
        }
        IEnumerator LoadMenuWithSound()
        {
            isLoaded = true;

            if (UISFX != null)
            {
                UISFX.PlayOneShot(StartButtonm);
                // 효과음 길이만큼 대기 (사운드가 뚝 끊기는 것 방지)
                yield return new WaitForSeconds(UISFX.clip.length);
            }
        }

        private IEnumerator FadeSelectMenu(float startAlpha, float endAlpha, float duration)
        {
            SelectMenu.SetActive(true);
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                // 시간에 따라 알파값을 보간(Lerp)합니다.
                selectMenuCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
                yield return null; // 다음 프레임까지 대기
            }

            selectMenuCanvasGroup.alpha = endAlpha; // 마지막 값 보정
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
						Application.Quit();
#endif
        }

        public void IntoSelectStage()
        {
            SelectStage.SetActive(true);
            MainTitle.SetActive(false);
            SelectMenu.SetActive(false);
            Setting.SetActive(false);
        }

        public void IntoMainGame(int stageIndex)
        {
            // MainGame씬 로드 전 정적 번호에 할당하여 저장(인덱스 번호로 저장)
            GameManager_Project.SelectedStageIndex = stageIndex;

            SceneManager.LoadScene("MainGame");
        }

        public void IntoSetting()
        {
            SelectStage.SetActive(false);
            MainTitle.SetActive(false);
            SelectMenu.SetActive(false);
            Setting.SetActive(true);
        }
    }
}
