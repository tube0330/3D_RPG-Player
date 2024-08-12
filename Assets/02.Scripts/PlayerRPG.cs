using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRPG : MonoBehaviour
{
    public enum PlayerState
    {
        IDLE = 0, ATTACK, UNDER_ATTACK, DEAD
    }
    public PlayerState state = PlayerState.IDLE;

    [Tooltip("걷기속도")] public float walkSpeed = 5.0f;
    [Tooltip("달리기속도")] public float runSpeed = 10.0f;

    [Header("Camera 관련 변수")]
    [SerializeField] Transform camTr;
    [SerializeField] Transform camPivot;
    [SerializeField] float camDist = 0f;    //카메라와의 거리
    [SerializeField] Vector3 mouseMove = Vector3.zero;  //마우스 이동 좌표
    [SerializeField] int playerLayer;

    [Header("PlayerMove 관련 변수")]
    [SerializeField] Transform modelTr;
    [SerializeField] Animator ani;
    [SerializeField] CharacterController controller;
    [SerializeField] Vector3 moveVelocity = Vector3.zero;   //움직임 방향

    bool isGround = false;

    bool isRun;
    public bool IsRun
    {
        get { return isRun; }
        set
        {
            isRun = value;
            ani.SetBool("isRun", value);
        }
    }

    void Start()
    {
        camTr = Camera.main.transform;
        camPivot = camTr.parent;
        playerLayer = LayerMask.NameToLayer("PLAYER");  //6
        modelTr = GetComponentsInChildren<Transform>()[1];
        ani = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        camDist = 5f;
    }

    void Update()
    {
        FreezeXZ();

        switch (state)
        {
            case PlayerState.IDLE:
                PlayerIdleAndMove();
                break;

            case PlayerState.ATTACK:
                break;

            case PlayerState.UNDER_ATTACK:
                break;

            case PlayerState.DEAD:
                break;
        }

        CameraDistCtrl();
    }

    //카메라 움직임 조절
    void LateUpdate()
    {
        float cam_H = 1.3f;
        camPivot.position = transform.position + Vector3.up * cam_H;
        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0f);
        //마우스 위아래 움직임=> player x축 변화, 마우스 좌우로 움직임 => Player y축 변화

        if (mouseMove.x < -40f)
            mouseMove.x = -40f;

        else if (mouseMove.x > 40f)
            mouseMove.x = 40f;

        camPivot.eulerAngles = mouseMove;

        RaycastHit hit;
        Vector3 dir = (camTr.position - camPivot.position).normalized;

        if (Physics.Raycast(camPivot.position, dir, out hit, camDist, ~(1 << playerLayer)/*PlayerLayer제외*/)) //장애물 존재
            camTr.localPosition = Vector3.back * hit.distance;  //hit.distance만큼 back

        else
            camTr.localPosition = Vector3.back * camDist;
    }

    void PlayerIdleAndMove()
    {
        RunCheck();

        if (controller.isGrounded)
        {
            if (!isGround) isGround = true;
            ani.SetBool("isGround", true);
            CalInputMove(); //움직임 계산
        }
    }

    //움직임 계산
    void CalInputMove()
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;  //방향
    }

    void RunCheck()
    {
        if (!IsRun && Input.GetKey(KeyCode.LeftShift))
            IsRun = true;

        else if (IsRun && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
            IsRun = false;
    }

    //Mouse ScrollWheel로 카메라 거리 조절
    void CameraDistCtrl()
    {
        camDist -= Input.GetAxis("Mouse ScrollWheel");
    }

    //Characater Controller의 x축, z축 회전 제한
    void FreezeXZ()
    {
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }
}
