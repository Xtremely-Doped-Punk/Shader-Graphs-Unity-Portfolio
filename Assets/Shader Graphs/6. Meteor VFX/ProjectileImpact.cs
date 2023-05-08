using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GAP
{
    public class ProjectileImpact : MonoBehaviour
    {
        public const float ParticleLoopDelay = 4.25f;
        public float speed;
        public GameObject impactPrefab;
        public ParticleSystem[] trails;
        bool impacted = false;

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (trails.Length == 0)
            {
                trails = GetComponentsInChildren<ParticleSystem>();
            }
        }

        void FixedUpdate()
        {
            if (speed!=0 && rb!=null)
            {
                rb.position += transform.forward * (speed * Time.deltaTime);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (impacted) return;

            impacted = true;
            speed = 0;

            ContactPoint contact = collision.contacts[0]; // taking the first contact point of collision
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;


            if (impactPrefab!=null)
            {
                var impactVFX = Instantiate(impactPrefab, pos, rot) as GameObject;
                Destroy(impactVFX, ParticleLoopDelay+0.5f);
            }
            if (trails.Length > 0)
            {
                foreach (var ps in trails)
                {
                    ps.transform.parent = null;
                    if (ps != null)
                    {
                        ps.Stop();
                        Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
                        // inorder to prevent instantious deletion of smoke trail of meteor once it crashes on any ground
                    }
                }
            }

            Destroy(gameObject, .5f);
        }
    }
}