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
    [SerializeField] float camDist = 0f;    //카메라와 플레이어 거리
    [SerializeField] Vector3 mouseMove = Vector3.zero;  //마우스 이동 좌표
    [SerializeField] int playerLayer;

    [Header("PlayerMove 관련 변수")]
    [SerializeField] Transform modelTr;
    [SerializeField] Animator ani;
    [SerializeField] CharacterController controller;
    [SerializeField] Vector3 moveVelocity = Vector3.zero;   //이동 속도

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

    readonly int hashSpeedX = Animator.StringToHash("speedX");
    readonly int hashSpeedY = Animator.StringToHash("speedY");
    readonly int hashAttack = Animator.StringToHash("SwordAttack");
    readonly int hashShieldAttack = Animator.StringToHash("ShieldAttack");

    void Start()
    {
        camTr = Camera.main.transform;
        camPivot = camTr.parent;
        playerLayer = LayerMask.NameToLayer("PLAYER");  //6
        modelTr = GetComponentsInChildren<Transform>()[1];  //현재 오브젝트와 자식 오브젝트들 중 두 번째 Transform을 modelTr에 저장
        ani = transform.GetChild(0).GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        camDist = 5f;
    }

    void Update()
    {
        FreezeXZ(); //플레이어의 X축과 Z축 회전 고정 메서드

        switch (state)
        {
            case PlayerState.IDLE:
                PlayerIdleAndMove();
                break;

            case PlayerState.ATTACK:
                IdleAfterAttack();
                break;

            case PlayerState.UNDER_ATTACK:
                break;

            case PlayerState.DEAD:
                break;
        }

        CameraDistCtrl();   //카메라의 거리 조절 메서드 호출
    }

    //카메라 움직임 조절
    void LateUpdate()
    {
        float cam_H = 1.3f;

        camPivot.position = transform.position + Vector3.up * cam_H;    //플레이어 위치 + up * cam_H 값으로 고정
        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0f);
        //마우스 위아래 움직임=> player x축 변화, 마우스 좌우로 움직임 => Player y축 변화
        //마우스 이동 값 누적 (상하: x축, 좌우: y축)

        if (mouseMove.x < -40f)
            mouseMove.x = -40f;

        else if (mouseMove.x > 40f)
            mouseMove.x = 40f;

        camPivot.eulerAngles = mouseMove;   //카메라 피봇 회전 계산 (누적된 마우스 이동 값 적용)

        RaycastHit hit;
        Vector3 dir = (camTr.position - camPivot.position/*플레이어 위에 있음*/).normalized;

        if (Physics.Raycast(camPivot.position, dir, out hit, camDist, ~(1 << playerLayer)/*PlayerLayer제외*/)) //장애물 존재
            camTr.localPosition = Vector3.back * hit.distance;  //카메라의 로컬 Z좌표를 hit.distance로 설정

        else
            camTr.localPosition = Vector3.back * camDist;
    }

    private void OnDrawGizmos()
    {
        if (!camTr || !camPivot) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(camPivot.position, (camTr.position - camPivot.position).normalized * camDist);   // 카메라 Pivot에서 카메라까지의 Ray

        Gizmos.color = Color.green;
        Gizmos.DrawLine(camTr.position, camPivot.position);     // 카메라에서 카메라 Pivot까지의 벡터
    }

    void PlayerIdleAndMove()
    {
        RunCheck();

        if (controller.isGrounded)
        {
            if (isGround == false) isGround = true;
            ani.SetBool("isGround", true);

            CalcInputMove(); //움직임 계산

            RaycastHit groundHit;

            if (GroundCheck(out groundHit))
                moveVelocity.y = IsRun ? -runSpeed : -walkSpeed;    //Ground에 닿았는데 울퉁불퉁하다면 런스피드나 워크스피드만큼 다시 착지하게 (?)

            else
                moveVelocity.y = -1f;

            PlayerAttack();
        }

        else
        {
            ani.SetBool("isGround", false);

            moveVelocity += Physics.gravity * Time.deltaTime;
        }

        controller.Move(moveVelocity * Time.deltaTime);
    }

    //움직임 계산 (?)
    void CalcInputMove()
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun ? runSpeed : walkSpeed);  //이동 방향과 속도를 계산
        ani.SetFloat(hashSpeedX, Input.GetAxis("Horizontal"));
        ani.SetFloat(hashSpeedY, Input.GetAxis("Vertical"));
        moveVelocity = transform.TransformDirection(moveVelocity);  //moveVelocity를 절대좌표로 움직이도록 월드 좌표계로 변환

        if (0.01f < moveVelocity.sqrMagnitude)  //캐릭터 이동중
        {
            Quaternion camRot = camPivot.rotation;
            camRot.x = camRot.z = 0f;
            transform.rotation = camRot;    //캐릭터의 회전을 카메라의 회전과 동일하게 설정

            if (IsRun)  //캐릭터가 이동하는 방향에 따라 카메라가 따라가는 효과
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity);
                characterRot.x = characterRot.z = 0f;
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, characterRot, Time.deltaTime * 10f);
            }

            else    //캐릭터 모델의 회전을 카메라의 회전에 맞춤
            {
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, camRot, Time.deltaTime * 10f);
            }
        }
    }

    void RunCheck()
    {
        if (IsRun == false && Input.GetKey(KeyCode.LeftShift))
            IsRun = true;

        else if (IsRun && Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
            IsRun = false;
    }

    //Mouse ScrollWheel로 카메라 거리 조절
    void CameraDistCtrl()
    {
        camDist = camDist - Input.GetAxis("Mouse ScrollWheel");
        /*
         * 마우스 휠을 위로 굴림
         * : camDist 값에서 양수를 빼 camDist 값이 감소. 카메라가 캐릭터에게 가까워짐
         * camDist = 5 - 3      //2
         * 마우스 휠을 아래로 굴림
         * : camDist 값에서 음수를 빼(=더하기) camDist 값이 증가. 카메라가 캐릭터에게 멀어짐
         * camDist = 5 - (-3)   //8
         */
    }

    //Characater Controller의 x축, z축 회전 제한
    void FreezeXZ()
    {
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }

    bool GroundCheck(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f);   //레이캐스트가 Ground 방향으로 쏘아 충돌감지 후 정확하게 Ground에 착지 되었는지 충돌 검사
    }

    float nextTime = 0f;
    void IdleAfterAttack()
    {
        nextTime += Time.deltaTime; //공격이 시작된 후 경과된 시간을 계속해서 증가

        if (1f <= nextTime) //공격이 시작된 후 1초가 지났는지 판단 (쿨타임)
            state = PlayerState.IDLE;   //1초 쿨타임 지났으면 IDLE 상태로 전환
    }

    void PlayerAttack()
    {
        if (Input.GetButtonDown("Fire1"))    //edit - progectsetting - inputManager - axis : Fire1
        {
            state = PlayerState.ATTACK;
            ani.SetTrigger(hashAttack);
            ani.SetFloat(hashSpeedX, 0f);   //Attack할 때 안움직이려고
            ani.SetFloat(hashSpeedY, 0f);   //Attack할 때 안움직이려고
            nextTime = 0f;
        }

        else if (Input.GetButtonDown("Fire2"))
        {
            state = PlayerState.ATTACK;
            ani.SetTrigger(hashShieldAttack);
            ani.SetFloat(hashSpeedX, 0f);
            ani.SetFloat(hashSpeedY, 0f);
            nextTime = 0f;
        }
    }
}
