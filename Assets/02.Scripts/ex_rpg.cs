using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ex_rpg : MonoBehaviour
{
    public enum playerState
    {
        IDLE = 0, ATTACK, UNDER_ATTACK, DEAD
    }
    public playerState state = playerState.IDLE;

    public float walkSpeed = 5f;
    public float runSpeed = 10f;

    Transform camTr;
    Transform camPivotTr;
    float camDist = 0f;
    Vector3 mouseMove = Vector3.zero;
    int playerLayer;

    Transform modelTr;
    Animator ani;
    CharacterController controller;
    Vector3 moveVelocity = Vector3.zero;

    readonly int hashSpeedX = Animator.StringToHash("speedX");
    readonly int hashSpeedY = Animator.StringToHash("speedY");
    readonly int hashRun = Animator.StringToHash("isRun");
    readonly int hashGround = Animator.StringToHash("isGround");
    readonly int hashSwordAttack = Animator.StringToHash("SwordAttack");
    readonly int hashShieldAttack = Animator.StringToHash("ShieldAttack");

    bool isGround = false;

    bool isRun;
    public bool IsRun
    {
        get { return isRun; }
        set
        {
            isRun = value;
            ani.SetBool("hashRun", value);
        }
    }

    void Start()
    {
        camTr = Camera.main.transform;
        camPivotTr = camTr.parent;
        playerLayer = LayerMask.NameToLayer("Player");
        ani = transform.GetChild(0).GetComponent<Animator>();
        modelTr = transform.GetChild(0).GetComponent<Transform>();
        controller = GetComponent<CharacterController>();

        camDist = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        FreezeXZ();

        switch (state)
        {
            case playerState.IDLE:
                PlayerIdleAndMove();
                break;

            case playerState.ATTACK:
                AttackTimeState();
                break;

            case playerState.UNDER_ATTACK:
                break;

            case playerState.DEAD:
                break;
        }

        CameraDistCtrl();
    }

    void LateUpdate()
    {
        float cam_H = 1.3f;
        camPivotTr.position = transform.position + Vector3.up * cam_H;

        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0f);


        if (mouseMove.x < -40f)
            mouseMove.x = -40f;

        else if (mouseMove.x > 40f)
            mouseMove.x = 40f;

        camPivotTr.eulerAngles = mouseMove;

        RaycastHit hit;

        Vector3 dir = (camTr.position - camPivotTr.position).normalized;        //방향나옴

        if (Physics.Raycast(camPivotTr.position, dir, out hit, camDist, ~(1 << playerLayer)))
            camTr.localPosition = Vector3.back * hit.distance;

        else
            camTr.localPosition = Vector3.back * camDist;
    }

    void PlayerIdleAndMove()
    {
        RunCheck();

        if (controller.isGrounded)
        {
            if (!isGround) isGround = true;
            ani.SetBool(hashGround, true);

            CalcInputMove(); //움직임 계산

            RaycastHit groundHit;

            if (GroundCheck(out groundHit))
                moveVelocity.y = IsRun ? -runSpeed : -walkSpeed;

            else
                moveVelocity.y = -1f;

            PlayerAttack();
        }

        else
        {
            ani.SetBool(hashGround, false);
            moveVelocity += Physics.gravity * Time.deltaTime;
        }

        controller.Move(moveVelocity * Time.deltaTime);
    }

    void CalcInputMove()
    {
        moveVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized * (IsRun ? runSpeed : walkSpeed);
        ani.SetFloat(hashSpeedX, Input.GetAxis("Horizontal"));
        ani.SetFloat(hashSpeedY, Input.GetAxis("Vertical"));
        moveVelocity = transform.TransformDirection(moveVelocity);

        if (0.01f < moveVelocity.sqrMagnitude)
        {
            Quaternion camRot = camPivotTr.rotation;
            camRot.x = camRot.z = 0f;
            transform.rotation = camRot;

            if (IsRun)
            {
                Quaternion characterRot = Quaternion.LookRotation(moveVelocity);
                characterRot.x = camRot.z = 0f;
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
        return Physics.Raycast(transform.position, Vector3.down, out hit, 0.25f);
    }

    float nextTime = 0f;
    void AttackTimeState()
    {
        nextTime += Time.deltaTime;

        if (1f <= nextTime)
            nextTime += Time.deltaTime;
        state = playerState.IDLE;
    }

    void PlayerAttack()
    {
        if (Input.GetButtonDown("Fire1"))    //edit - progectsetting - inputManager - axis : Fire1
        {
            state = playerState.ATTACK;
            ani.SetTrigger(hashSwordAttack);
            ani.SetFloat(hashSpeedX, 0f);   //Attack할 때 안움직이려고
            ani.SetFloat(hashSpeedY, 0f);   //Attack할 때 안움직이려고
            nextTime = 0f;
        }

        else if (Input.GetButtonDown("Fire2"))
        {
            state = playerState.ATTACK;
            ani.SetTrigger(hashShieldAttack);
            ani.SetFloat(hashSpeedX, 0f);
            ani.SetFloat(hashSpeedY, 0f);
            nextTime = 0f;
        }
    }
}