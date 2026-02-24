using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _2DTopDown
{
    public class GameClearPopup : MonoBehaviour
    {
        public TextMeshProUGUI ClearTimeTxt;
        public TextMeshProUGUI KillCountTxt;
        public TextMeshProUGUI ScoreTxt;

        private void Start()
        {
            // 1. 게임 매니저에서 사용했던 것과 동일하게 전체 흐른 시간(초)을 가져옵니다.
            float totalSeconds = Time.timeSinceLevelLoad;

            // 2. 분과 초를 계산합니다.
            int minutes = Mathf.FloorToInt(totalSeconds / 60); // 전체 초를 60으로 나눈 몫
            int seconds = Mathf.FloorToInt(totalSeconds % 60); // 60으로 나눈 나머지 초

            ClearTimeTxt.text = $"{minutes}분 {seconds}초";
            KillCountTxt.text = $"{GameManager_Project.instance.KillCount}명";
            ScoreTxt.text = $"{GameManager_Project.instance.currentScore}점";
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("MainGame");
                Player.instance.Anim.transform.parent = null;
                this.enabled = false;
                Player.instance.rb.isKinematic = true;
                Vector3 pos = Player.instance.Anim.transform.position;
                pos.y = 0.2f;
                Player.instance.Anim.transform.position = pos;
            }
        }
    }
}
