using TMPro;
using UnityEngine;

namespace _2DTopDown
{
    public class Item : MonoBehaviour
    {
        // 아이템 타입
        public enum ItemTypes
        { 
            MedKit,
            Rifle,
            Shotgun
        }
        public ItemTypes itemTypes = ItemTypes.MedKit;

        public TextMeshProUGUI pickUpTxt;
        public bool isPickUp = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            pickUpTxt.gameObject.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
            if(isPickUp && Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                pickUpTxt.gameObject.SetActive(true);
                isPickUp = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                pickUpTxt.gameObject.SetActive(false);
                isPickUp = false;
            }
        }

        public void PickUp()
        {
            switch (itemTypes)
            {
                case ItemTypes.MedKit:
                    Player.instance.PlayerHeal(50);
                    Destroy(gameObject);
                    break;
                case ItemTypes.Rifle: 
                    break;
                case ItemTypes.Shotgun:
                    break;
            }
        }
    }
}
