using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField] Transform camTr;
    [SerializeField] Transform targetTr;    //카메라가 따라다닐 대상
    [SerializeField] float h = 3f;
    [SerializeField] float dis = 5f;
    [SerializeField] float targetOffset = 1.0f;
    [SerializeField] float moveDamping = 5.0f;  //급격하게 움직일 때 떨림 방지 위해 선언
    [SerializeField] float rotDamping = 10f;

    void Start()
    {
        camTr = transform;
    }

    void LateUpdate()
    {
        var camPos = targetTr.position - (Vector3.forward * dis) + (Vector3.up * h);
        camTr.position = Vector3.Slerp(camTr.position, camPos, Time.deltaTime * moveDamping);
        camTr.rotation = Quaternion.Slerp(camTr.rotation, targetTr.rotation, Time.deltaTime * rotDamping);

        camTr.LookAt(targetTr.position + Vector3.up * targetOffset);
    }
}
