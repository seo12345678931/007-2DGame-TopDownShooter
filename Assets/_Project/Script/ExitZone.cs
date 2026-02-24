using TMPro;
using UnityEngine;

namespace _2DTopDown
{
    public class ExitZone : MonoBehaviour
    {
        public TextMeshProUGUI KeyAlrame;
        public SpriteRenderer SprRend;
        public bool ExitReady = false;

        private void Start()
        {
            KeyAlrame.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                KeyAlrame.gameObject.SetActive(true);
                ExitReady = true;
                SprRend.color = Color.black;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                KeyAlrame.gameObject.SetActive(false);
                ExitReady = false;
                SprRend.color = Color.white;
            }
        }

        private void Update()
        {
            if(ExitReady && Input.GetKeyDown(KeyCode.E))
            {
                GameManager_Project.instance.GameClear.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
}
