using System;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public Transform slot;
    public Weapon weaponPrefab;
}

public class Inventory : MonoBehaviour
{
    public Transform handPosition;
    public PlayerVisionRay visionRay;

    public InventoryItem[] inventoryItem = new InventoryItem[1];
    public Sprite[] spriteItem = new Sprite[1];

    public int currentIndex;


    private void Start()
    {
        StartInitializeWeapon();
    }

    private void Update()
    {
        ChangeSlot();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeWeapon();
        }
    }

    public void ChangeSlot()
    {
        for (int i = 0; i < inventoryItem.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectedSlot(i);
            };
        }
    }

    public void SelectedSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventoryItem.Length) return;

        currentIndex = slotIndex;

        for (int i = 0; i < inventoryItem.Length; i++)
        {
            if (inventoryItem[currentIndex].slot)
            {
                inventoryItem[currentIndex].slot.gameObject.SetActive(true);
            }
            if (inventoryItem[i] != inventoryItem[currentIndex])
            {
                inventoryItem[i].slot.gameObject.SetActive(false);
            }
        }
    }


    public void StartInitializeWeapon()
    {
        try
        {
            currentIndex = 0;

            Weapon weap1 = Instantiate(inventoryItem[currentIndex].weaponPrefab, handPosition.position,
                Quaternion.identity);
            weap1.transform.SetParent(inventoryItem[currentIndex].slot);

            if (inventoryItem[currentIndex + 1] != null)
            {
                Weapon weap2 = Instantiate(inventoryItem[currentIndex + 1].weaponPrefab, handPosition.position,
                    Quaternion.identity);
                weap2.transform.SetParent(inventoryItem[currentIndex + 1].slot);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Оружия нет" + e.Message);
        }
    }

    public void TakeWeapon()
    {
        if (visionRay.hit.collider != null)
        {
            DropedItem droppedItem = visionRay.hit.collider.GetComponent<DropedItem>();
            if (droppedItem != null && droppedItem.TakedItempPefab != null)
            {
                if (inventoryItem[currentIndex].slot.childCount > 0)
                {
                    Destroy(inventoryItem[currentIndex].slot.GetChild(0).gameObject);
                }

                Weapon newWeapon = Instantiate(
                    droppedItem.TakedItempPefab,
                    handPosition.position,
                    Quaternion.identity,
                    inventoryItem[currentIndex].slot
                );

                newWeapon.transform.localPosition = Vector3.zero;
                newWeapon.transform.localRotation = Quaternion.identity;

                inventoryItem[currentIndex].weaponPrefab = droppedItem.TakedItempPefab;

                inventoryItem[currentIndex].slot.gameObject.SetActive(true);
            }
        }
    }
}
