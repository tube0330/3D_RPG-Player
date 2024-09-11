using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MariaMovement : MonoBehaviour
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
    [SerializeField] Vector3 move = Vector3.zero;   //이동 속도

    MariaInput mariaInput;

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
    /* readonly int hashAttack = Animator.StringToHash("SwordAttack");
    readonly int hashShieldAttack = Animator.StringToHash("ShieldAttack");
 */
    void Start()
    {
        camTr = Camera.main.transform;
        camPivot = camTr.parent;
        playerLayer = LayerMask.NameToLayer("PLAYER");
        modelTr = GetComponentsInChildren<Transform>()[1];
        ani = transform.GetChild(0).GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        camDist = 5f;

        mariaInput = GetComponent<MariaInput>();
    }

    void Update()
    {
        switch (state)
        {
            case PlayerState.IDLE:
                PlayerIdleAndMove();
                break;

            /* case PlayerState.ATTACK:
                IdleAfterAttack();
                break; */

            case PlayerState.UNDER_ATTACK:
                break;

            case PlayerState.DEAD:
                break;
        }
    }

    private void OnDrawGizmos()
    {
        if (!camTr || !camPivot) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(camPivot.position, (camTr.position - camPivot.position).normalized * camDist);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(camTr.position, camPivot.position);
    }

    void PlayerIdleAndMove()
    {
        //RunCheck();

        if (controller.isGrounded)
        {
            if (isGround == false) isGround = true;
            ani.SetBool("isGround", true);

            CalcInputMove(); //움직임 계산

            RaycastHit groundHit;

            if (GroundCheck(out groundHit))
                move.y = IsRun ? -runSpeed : -walkSpeed;
            else
                move.y = -1f;

            //PlayerAttack();
        }
        else
        {
            ani.SetBool("isGround", false);
            move += Physics.gravity * Time.deltaTime;
        }

        controller.Move(move * Time.deltaTime);
    }

    void CalcInputMove()
    {
        Vector2 inputMove = mariaInput.moveInput;
        move = new Vector3(inputMove.x, 0f, inputMove.y).normalized * (IsRun ? runSpeed : walkSpeed);
        ani.SetFloat(hashSpeedX, inputMove.x);
        ani.SetFloat(hashSpeedY, inputMove.y);
        move = transform.TransformDirection(move);

        if (0.01f < move.sqrMagnitude)  //캐릭터 이동중
        {
            Quaternion camRot = camPivot.rotation;
            camRot.x = camRot.z = 0f;
            transform.rotation = camRot;

            if (IsRun)
            {
                Quaternion rot = Quaternion.LookRotation(move);
                rot.x = rot.z = 0f;
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, rot, Time.deltaTime * 10f);
            }
            else
            {
                modelTr.rotation = Quaternion.Slerp(modelTr.rotation, camRot, Time.deltaTime * 10f);
            }
        }
    }

    /* void RunCheck()
    {
        if (IsRun == false && Keyboard.current.leftShiftKey.isPressed)
            IsRun = true;
        else if (IsRun && mariaInput.moveInput == Vector2.zero)
            IsRun = false;
    } */

    bool GroundCheck(out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f);
    }

    /* float nextTime = 0f;
    void IdleAfterAttack()
    {
        nextTime += Time.deltaTime;

        if (1f <= nextTime)
            state = PlayerState.IDLE;
    }

    void PlayerAttack()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)    // "Fire1"을 키보드의 "e"키로 변경
        {
            state = PlayerState.ATTACK;
            ani.SetTrigger(hashAttack);
            ani.SetFloat(hashSpeedX, 0f);
            ani.SetFloat(hashSpeedY, 0f);
            nextTime = 0f;
        }
        else if (Keyboard.current.qKey.wasPressedThisFrame)  // "Fire2"을 키보드의 "q"키로 변경
        {
            state = PlayerState.ATTACK;
            ani.SetTrigger(hashShieldAttack);
            ani.SetFloat(hashSpeedX, 0f);
            ani.SetFloat(hashSpeedY, 0f);
            nextTime = 0f;
        }
    } */
}
