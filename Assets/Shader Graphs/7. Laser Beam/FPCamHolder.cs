using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPCamHolder : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] GameObject camSwitchUI;
    //[SerializeField] float camSwitchDelay = 1f;

    [Range(0, 1), SerializeField] float exitKeyHold = 0.5f;

    Transform mainCam;
    CameraMover camComp;

    [HideInInspector] public bool isEnabled = false;
    Vector3 resetPos;
    Vector3 resetRot;

    float exitHold;
    Quaternion initRot;

    void Start()
    {
        mainCam = Camera.main.transform;
        camComp = mainCam.GetComponent<CameraMover>();
        initRot = transform.rotation;
    }

    void LateUpdate()
    {
        if (!isEnabled) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            exitHold = 0;
        if (Input.GetKey(KeyCode.Escape))
        {
            if (exitHold > exitKeyHold)
            {
                ExitFPS();
                return;
            }
            exitHold += Time.deltaTime;
        }

        mainCam.position = transform.position;
    }

    public void EnterFPS() // Button fn
    {
        isEnabled = true;

        resetPos = mainCam.position - transform.position;
        resetRot = mainCam.eulerAngles - transform.eulerAngles;
        mainCam.rotation = transform.rotation;

        camComp.AllowMovement = false;
        camComp.SetHolders(player, transform);

        if (camSwitchUI != null)
            camSwitchUI.SetActive(false);
    }

    public void ExitFPS()
    {
        isEnabled = false;

        mainCam.position = transform.position + resetPos;
        mainCam.eulerAngles = transform.eulerAngles + resetRot;
        transform.rotation = initRot;

        camComp.AllowMovement = true;
        camComp.RemoveHolders();

        if (camSwitchUI!=null)
            camSwitchUI.SetActive(true);
    }
}
