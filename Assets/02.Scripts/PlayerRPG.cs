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
        modelTr = GetComponentsInChildren<Transform>()[1];  //현재 게임 오브젝트와 그 자식 오브젝트들 중에서 Transform 컴포넌트를 가진 오브젝트들을 모두 찾아, 그 중 두 번째 오브젝트의 Transform 컴포넌트를 modelTr 변수에 할당
        ani = transform.GetChild(0).GetComponent<Animator>();
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
                AttackTimeState();
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
        //마우스 이동 값 누적 (상하: x축, 좌우: y축)

        if (mouseMove.x < -40f)
            mouseMove.x = -40f;

        else if (mouseMove.x > 40f)
            mouseMove.x = 40f;

        camPivot.eulerAngles = mouseMove;   //카메라 피봇 회전 계산 (누적된 마우스 이동 값 적용)

        RaycastHit hit;
        Vector3 dir = (camTr.position - camPivot.position).normalized;

        if (Physics.Raycast(camPivot.position, dir, out hit, camDist, ~(1 << playerLayer)/*PlayerLayer제외*/)) //장애물 존재
            camTr.localPosition = Vector3.back * hit.distance;  //hit.distance만큼 back

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

    //움직임 계산
    void CalcInputMove()
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun ? runSpeed : walkSpeed);  //이동 방향과 속도를 계산
        ani.SetFloat(hashSpeedX, Input.GetAxis("Horizontal"));
        ani.SetFloat(hashSpeedY, Input.GetAxis("Vertical"));
        moveVelocity = transform.TransformDirection(moveVelocity);  //moveVelocity를 절대좌표로 움직이도록

        if (0.01f < moveVelocity.sqrMagnitude)
        {
            Quaternion camRot = camPivot.rotation;
            camRot.x = camRot.z = 0f;
            transform.rotation = camRot;    //움직이고 있을 때 카메라 회전 제한

            if (IsRun)
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity);
                characterRot.x = characterRot.z = 0f;
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, characterRot, Time.deltaTime * 10f);
            }

            else
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
        camDist -= Input.GetAxis("Mouse ScrollWheel");
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
    void AttackTimeState()
    {
        nextTime += Time.deltaTime;

        if (1f <= nextTime)
            nextTime += Time.deltaTime;
        state = PlayerState.IDLE;
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

        else if(Input.GetButtonDown("Fire2"))
        {
            state = PlayerState.ATTACK;
            ani.SetTrigger(hashShieldAttack);
            ani.SetFloat(hashSpeedX, 0f);
            ani.SetFloat(hashSpeedY, 0f);
            nextTime = 0f;
        }
    }
}
