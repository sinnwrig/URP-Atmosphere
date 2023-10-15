using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraTransition
    {
        public string name;
        public GameObject controller;
        public AnimationCurve easingCurve;
    }

    public Transform managedCamera;
    public List<CameraTransition> cameras;
    CameraTransition currentTranstion;


    public void Awake()
    {
        foreach (CameraTransition trs in cameras)
        {
            trs.controller.SetActive(false);
        }

        SetControllerImmediate(0);
    }



    public void SetController(int index)
    {
        if (index < 0 || index >= cameras.Count)
        {
            return;
        }

        currentTranstion = cameras[index];
        SetController(currentTranstion, cameras[index]);
    }


    public void SetController(CameraTransition from, CameraTransition to)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeController(from, to));
    }


    public void SetControllerImmediate(int index)
    {
        if (index < 0 || index >= cameras.Count)
        {
            return;
        }
    
        currentTranstion = cameras[index];
        SetControllerImmediate(currentTranstion, cameras[index]);
    }


    public void SetControllerImmediate(CameraTransition from, CameraTransition to)
    {
        StopAllCoroutines();

        if (from != null) from.controller.SetActive(false);

        managedCamera.transform.SetParent(to.controller.transform, true);
        managedCamera.transform.rotation = Quaternion.identity;
        managedCamera.transform.position = Vector3.zero;

        to.controller.SetActive(true);
    }


    void Update()
    {
        if (Input.anyKeyDown)
        {
            string input = Input.inputString;

            if (input != null && input.Length > 0)
            {
                if (int.TryParse(input[0].ToString(), out int result))
                {
                    SetController(result - 1);
                }
            }
        }
    }


    public IEnumerator ChangeController(CameraTransition from, CameraTransition to)
    {

        Debug.Log("Changing controller");
        managedCamera.transform.SetParent(null, true);

        if (from != null) from.controller.SetActive(false);
        to.controller.SetActive(false);

        Vector3 startPos = from != null && from.controller != null ? from.controller.transform.position : managedCamera.transform.position;
        Vector3 targetPos = to.controller.transform.position;

        Quaternion startRot = from != null && from.controller != null ? from.controller.transform.rotation : managedCamera.transform.rotation;
        Quaternion targetRot = to.controller.transform.rotation;

        float time = 0.0f;

        var lastKeyframe = to.easingCurve[to.easingCurve.length - 1];
        float duration = lastKeyframe.time;

        while (time < duration)
        {
            time += Time.deltaTime * to.easingCurve.Evaluate(time);

            managedCamera.transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            managedCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, time / duration);

            yield return null;
        }

        managedCamera.transform.SetParent(to.controller.transform, true);
        to.controller.SetActive(true);
    }
}
