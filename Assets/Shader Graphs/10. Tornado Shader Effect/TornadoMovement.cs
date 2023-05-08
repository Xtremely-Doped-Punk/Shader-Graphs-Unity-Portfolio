using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

namespace GAP
{
    public class TornadoMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("without iTween, tornado movement is done using unity's update()")]
        public bool useITween = true;
        public iTween.EaseType mEaseType = iTween.EaseType.easeInOutSine;
        public float mRadius = 30f;
        public float mSpeed = 0.5f;

        Vector3 originalPosition;
        Vector3 targetPosition;
        float distThreshold;
        List<Collider> interactiveObjects = new();

        [Header("Physics Pull Settings")]
        public Transform pCenter;
        public float pCenterScale = 1;
        public float pCenterOffset;
        public AnimationCurve pCenterCurve;
        public float pRadius;
        public float pForceScale = 1f;
        public AnimationCurve pForceCurve;
        [Range(0, 1)] public float pTimeScale = 0.1f;  // slow down factor for time multiplier

        [Header("Spawn Interactables")]
        public bool canSpawn = false;
        public Transform parent;
        public Vector2 scaleRange = new Vector2(.25f, 2.5f);
        public Vector2 rbMassRange = new Vector2(.5f, 1.5f);
        public List<Mesh> meshes = new();
        public List<Material> materials = new();
        public float lifeTime = 5;
        public Vector2Int burstRange = new Vector2Int(5, 10);
        public float burstInterval = 1f;
        float burstTimer = 0;

        [Header("Debug Display")]
        public float pForceStep;
        public List<float> pColForces = new();

        float GetRand(Vector2 range) => Random.Range(range.x, range.y);

        float GetCurveTimeInterval(AnimationCurve curve) => curve.keys[curve.length - 1].time - curve.keys[0].time;

        void Start()
        {
            // Pull related ...
            if (pRadius <= 0) // causes division by 0 error
            {
                if (TryGetComponent<SphereCollider>(out var scol))
                    pRadius = scol.radius;
                else
                    pRadius = 1;
            }

            // movement related ...
            originalPosition = transform.position;
            targetPosition = transform.position;
            StartCoroutine(Movement());

            distThreshold = mSpeed * Time.deltaTime;

            if (pCenterOffset == 0)
                pCenterOffset = pCenter.position.y - pCenterScale;

            //Debug.Log(GetCurveTimeInterval(pCenterCurve)); Debug.Log(GetCurveTimeInterval(pForceCurve));
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.tag == "Tornado") // for testing purposes
            {
                //StartCoroutine(IncreasePull(col, true));
                interactiveObjects.Add(col);

                pColForces.Add(0);
            }
        }

        private void OnTriggerExit(Collider col)
        {
            if ( col.tag == "Tornado") // for testing purposes
            {
                //StartCoroutine(IncreasePull(col, false));
                StartCoroutine(RemoveInteractable(col));
            }
        }

        IEnumerator RemoveInteractable(Collider col)
        {
            yield return new WaitForEndOfFrame();

            interactiveObjects.Remove(col);

            pColForces.RemoveAt(pColForces.Count - 1);
        }

        IEnumerator Movement()
        {
            // choose location (along xz plane)
            targetPosition = new Vector3(originalPosition.x + Random.Range(-mRadius, mRadius),
                originalPosition.y,
                originalPosition.z + Random.Range(-mRadius, mRadius));

            var distance = (targetPosition - originalPosition).magnitude;
            var time = distance / mSpeed;

            // move to loaction
            if (useITween)
                iTween.MoveTo(gameObject, iTween.Hash("position", targetPosition, "easeType", mEaseType, "time", time));

            yield return new WaitForSeconds(time + 0.1f);
            StartCoroutine(Movement()); // start next movement
        }

        IEnumerator IncreasePull(Collider col, bool pull)
        {
            if (pull)
            {
                // pull force controlled by curve
                pForceStep = pForceCurve.Evaluate((Time.time * pTimeScale) % GetCurveTimeInterval(pForceCurve));

                // get direction from tornado to object
                Vector3 forceDir = pCenter.position - col.transform.position;

                // apply force to object towards 
                if (col.TryGetComponent<Rigidbody>(out var rb))
                    rb.AddForce(forceDir.normalized * pForceStep * Time.deltaTime);

                // pulling center keeps animating in y axis of the tornado
                pCenter.position = new Vector3(pCenter.position.x,
                    pCenterCurve.Evaluate((Time.time * pTimeScale) % GetCurveTimeInterval(pCenterCurve)),
                    pCenter.position.y);

                yield return Time.frameCount / Time.time; // avg refresh rate
                StartCoroutine(IncreasePull(col, pull));
            }
            else
            {
                // stop the resp couroutine with 'col' as its arg
            }
            yield return null;
        }

        private void Update()
        {
            if (!useITween)
            {
                if (Vector3.SqrMagnitude(targetPosition - transform.position) > distThreshold)
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, mSpeed * Time.deltaTime);
            }

            if (interactiveObjects.Count > 0)
            {
                // pull force controlled by curve
                pForceStep = pForceCurve.Evaluate((Time.time * pTimeScale) % GetCurveTimeInterval(pForceCurve));

                int idx = 0;
                foreach (Collider col in interactiveObjects)
                {
                    if (col == null)
                    {
                        StartCoroutine(RemoveInteractable(col));
                        continue;
                    }
                    // get direction from tornado to object
                    var forceDiff = pCenter.position - col.transform.position;
                    var forceDir = forceDiff.normalized;

                    // get force scale based on dist bet obj and tornado in inverse quadratic form
                    var forceScale = (1f - (forceDiff.sqrMagnitude / (pRadius * pRadius))) * pForceScale;
                    var totalForce = pForceStep * forceScale;
                    pColForces[idx++ % pColForces.Count] = totalForce;


                    // apply force to object towards 
                    if (col.TryGetComponent<Rigidbody>(out var rb))
                        rb.AddForce(forceDir * totalForce * Time.deltaTime);
                }

                // pulling center keeps animating in y axis of the tornado
                pCenter.position = new Vector3(pCenter.position.x,
                    pCenterOffset + pCenterScale * pCenterCurve.Evaluate((Time.time * pTimeScale) % GetCurveTimeInterval(pCenterCurve)),
                    pCenter.position.z);
            }

            if (canSpawn)
            {
                if (burstTimer<=0)
                {
                    burstTimer = burstInterval;
                    StartCoroutine(BurstSpawnInteractables());
                }
                else
                    burstTimer -= Time.deltaTime;
            }
        }

        IEnumerator BurstSpawnInteractables()
        {
            var no_of_obj = Random.Range(burstRange.x, burstRange.y);
            for (int i = 0; i < no_of_obj; i++)
            {
                var obj = new GameObject("test-obj");
                obj.tag = "Tornado";
                obj.transform.parent = parent;

                Vector3 scale = new Vector3(GetRand(scaleRange), GetRand(scaleRange), GetRand(scaleRange));
                obj.transform.localScale = scale;
                obj.transform.position = transform.position +
                    new Vector3(Random.Range(-pRadius, pRadius), scale.y / 2, Random.Range(-pRadius, pRadius));

                var rb = obj.AddComponent<Rigidbody>();
                rb.mass = GetRand(rbMassRange);

                var mesh = meshes[Random.Range(0, meshes.Count - 1)];
                obj.AddComponent<MeshFilter>().mesh = mesh;
                var mc = obj.AddComponent<MeshCollider>();
                mc.sharedMesh = mesh; mc.convex = true;
                obj.AddComponent<MeshRenderer>().material = materials[Random.Range(0, materials.Count - 1)];

                Destroy(obj, lifeTime);

                yield return new WaitForEndOfFrame(); // just not to crowd the cpu
            }
        }
    }
}