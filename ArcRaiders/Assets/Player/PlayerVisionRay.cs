using System;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class PlayerVisionRay : MonoBehaviour, IToolTips
{
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private bool showDebugRay = true;
    public Inventory inventory;
    public RaycastHit hit;

    private void Update()
    {
        CastRay();
    }

    public void CastRay()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        
        
        if (Physics.Raycast(ray, out hit, rayDistance, LayerMask.GetMask("Item")))
        {
            
            if (showDebugRay)
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }
        }
        else
        {
            
            if (showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
            }
        }

        if (hit.collider != null)
        {
            Debug.Log(hit.transform.name);
        }
    }

    public void ShowTip()
    {
        throw new NotImplementedException();
    }
}