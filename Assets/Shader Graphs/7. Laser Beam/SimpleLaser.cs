using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

namespace GAP
{
    public class SimpleLaser : MonoBehaviour
    {
        public FPCamHolder FPCam;

        public GameObject laser;
        public LineRenderer laserLR;
        public Transform firePoint;
        public float maxDist = 10;

        [Tooltip ("Press 'Right Mouse Button' to switch between different gradients")] 
        public List<Gradient> laserGradients = new();
        int activeGrad = -1;
        bool isControllable; bool CheckEnable => !(FPCam == null || !FPCam.isEnabled);
        Vector3 defaultImpact;
        float stretchRatio;
        Material laserMat;
        Vector2 defaultTiling;

        void Start()
        {
            isControllable = CheckEnable;
            if (isControllable)
                DisableLaser();

            defaultImpact = laserLR.GetPosition(1);

            laserMat = laserLR.material;
            defaultTiling = laserMat.GetVector("_Texture_Tiling");
            stretchRatio = defaultTiling.x / defaultImpact.z;

            ChangeLaser();
        }

        void Update()
        {
            if (!CheckEnable)
            {
                if (CheckEnable != isControllable)
                {
                    isControllable = CheckEnable;
                    EnableLaser();
                }
                return;
            }
            if (CheckEnable != isControllable)
            {
                isControllable = CheckEnable;
                DisableLaser();
            }

            // shoot
            if (Input.GetMouseButtonDown(0))
            {
                EnableLaser();
            }
            if (Input.GetMouseButton(0))
            {
                UpdateLaser();
            }
            if (Input.GetMouseButtonUp(0))
            {
                DisableLaser();
            }

            // switch colors
            if (Input.GetMouseButtonDown(1)) // right mouse btn
            {
                ChangeLaser();
            }
        }

        private void ChangeLaser()
        {
            if (laserGradients.Count == 0) return;

            activeGrad++; activeGrad %= laserGradients.Count;
            laserLR.colorGradient = laserGradients[activeGrad];
        }

        private void UpdateLaser()
        {
            if (firePoint != null)
            {
                laser.transform.position = firePoint.position;
            }

            var impactDist = maxDist;

            if (Physics.Raycast(FPCam.transform.position,
                FPCam.transform.forward, out RaycastHit hit, maxDist))
            {
                //Debug.Log(hit.point - firePoint.position);
                //laserLR.SetPosition(1, hit.point);

                impactDist = hit.distance;
            }
            
            laserLR.SetPosition(1, new Vector3(defaultImpact.x, defaultImpact.y, impactDist));
            laserMat.SetVector("_Texture_Tiling", new Vector2(impactDist / stretchRatio, defaultTiling.y));
        }

        private void EnableLaser()
        {
            laser.SetActive(true);
            laserMat.SetVector("_Texture_Tiling", defaultTiling);
            laserLR.SetPosition(1, defaultImpact);
        }

        private void DisableLaser()
        {
            laser.SetActive(false);
        }
    }
}