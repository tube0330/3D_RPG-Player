using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField] Transform camTr;
    [SerializeField] Transform targetTr;    //카메라가 따라다닐 대상
    [SerializeField] float h;
    [SerializeField] float dis;
    [SerializeField] float targetOffset = 1.0f;
    [SerializeField] float moveDamping = 5.0f;  //급격하게 움직일 때 떨림 방지 위해 선언
    [SerializeField] float rotDamping = 10f;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        
    }
}
