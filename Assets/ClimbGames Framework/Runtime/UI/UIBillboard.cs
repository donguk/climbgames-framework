using System;
using UnityEngine;

namespace ClimbGames.UI
{
    public class UIBillboard : UIBase
    {
        [SerializeField] private Transform target;
        [SerializeField] private Camera targetCamera;

        void Start()
        {
            if (target == null)
                target = transform;

            if (targetCamera == null)
                targetCamera = UIManager.Instance.WorldUICamera;
        }

        void LateUpdate()
        {
            if (target == null || targetCamera == null)
                return;

            target.LookAt(target.position + targetCamera.transform.rotation * Vector3.forward, targetCamera.transform.rotation * Vector3.up);
        }
    }
}