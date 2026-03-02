using TMPro;
using UnityEngine;

namespace _2DTopDown
{
    public class Item : MonoBehaviour
    {
        // 아이템 타입
        public enum ItemTypes
        { 
            Null_Weapon,
            MedKit,
            Rifle,
            Shotgun,
            SMGSD,
            DMR
        }
        public ItemTypes itemTypes = ItemTypes.Null_Weapon;

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
            // 해당 무기의 탄약이 가득 차있거나 이미 획득했다면 함수 빠져나오기
            if (Player.instance.currentItemWeaponType == itemTypes && 
        Player.instance.AmmoCount >= Player.instance.AmmoCount_Max)
            {
                return;
            }

            switch (itemTypes)
            {
                case ItemTypes.Null_Weapon:
                    Player.instance.EquipItem(ItemTypes.Null_Weapon);
                    break;
                case ItemTypes.MedKit:
                    // 플레이어 체력이 최대체력(100) 이상일 시 함수 빠져나오기
                    if (Player.instance.currentHP >= Player.instance.maxHP)
                    {
                        return;
                    }
                    Player.instance.PlayerHeal(50);
                    Player.instance.HPItem_SFX.Play();
                    Destroy(gameObject);
                    break;
                case ItemTypes.Rifle:
                    Player.instance.EquipItem(ItemTypes.Rifle);
                    Player.instance.WeaponItemEquipSFX.Play();
                    Destroy(gameObject);
                    break;
                case ItemTypes.Shotgun:
                    Player.instance.EquipItem(ItemTypes.Shotgun);
                    Player.instance.WeaponItemEquipSFX.Play();
                    Destroy(gameObject);
                    break;
                case ItemTypes.SMGSD:
                    Player.instance.EquipItem(ItemTypes.SMGSD);
                    Player.instance.WeaponItemEquipSFX.Play();
                    Destroy(gameObject);
                    break;
                case ItemTypes.DMR:
                    Player.instance.EquipItem(ItemTypes.DMR);
                    Player.instance.WeaponItemEquipSFX.Play();
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
