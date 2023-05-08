using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAP
{
    public class SpawnMeteor : MonoBehaviour
    {
        public GameObject VFX;
        public Transform startPoint;
        public Transform endPoint;
        [Tooltip("Set to less than zero if dont want to be reseted.")] public float resetDelay = 0;

        GameObject instanceVFX = null;
        bool flagInvoked = true;

        void Start()
        {
            flagInvoked = false;
            var startPos = startPoint.position;
            var endPos = endPoint.position;

            instanceVFX = Instantiate(VFX, startPos, Quaternion.identity) as GameObject;
            RotateTo(instanceVFX, endPos);
        }

        private void Update()
        {
            if (!flagInvoked && instanceVFX == null && resetDelay >= 0)
            {
                flagInvoked = true;
                Invoke(nameof(Start), resetDelay + ProjectileImpact.ParticleLoopDelay);
            }
        }

        private void RotateTo(GameObject obj, Vector3 targetPos)
        {
            var dir = targetPos - obj.transform.position;
            var targetRotation = Quaternion.LookRotation(dir);
            obj.transform.localRotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, 1);
        }
    }
}