using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardCtrl : MonoBehaviour
{
    [Header("Components")]
    /* [SerializeField] CharacterController controller;
    [SerializeField] Rigidbody rb;
    [SerializeField] CapsuleCollider col;*/
    [SerializeField] Transform tr;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator ani;

    [Header("Click")]
    public double Timer = 0d;               //시간 재는 타이머
    public float doubleClickSecond = 0.25f; //0.25초 안에 누르면 double click됨
    public bool isOneClick = false;
    int groundRayer;
    Ray ray;
    RaycastHit hit;
    Vector3 targetPos = Vector3.zero;

    [Header("Animation")]
    readonly int hashMoveSpeed = Animator.StringToHash("moveSpeed");
    readonly int hashAttack = Animator.StringToHash("Attack");

    [Header("Camera")]
    [SerializeField] Transform camPivot;
    [SerializeField] Transform camTr;
    [SerializeField] float camDist = 5f;
    [SerializeField] Vector3 mouseMove = Vector3.zero;  //마우스 이동 좌표
    [SerializeField] int playerLayer;

    public WizardDamage wizardDamage;

    void Start()
    {
        /* controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();*/
        tr = transform;
        agent = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();

        groundRayer = LayerMask.NameToLayer("GROUND");

        camTr = Camera.main.transform;
        camPivot = camTr.parent;
        playerLayer = LayerMask.NameToLayer("PLAYER");

        wizardDamage = GetComponent<WizardDamage>();
    }

    void Update()
    {
        if (wizardDamage.isDie) return;

        ClickCheck();
        PlayerMove();

        //Attack Animation
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCoroutine(AttackAnimation());
    }

    void PlayerMove()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << groundRayer))
            {
                if (isOneClick)
                {
                    agent.speed = 1.5f;
                    ani.SetFloat(hashMoveSpeed, agent.speed);
                }

                else    //더블클릭일 때
                {
                    agent.speed = 3f;
                    ani.SetFloat(hashMoveSpeed, agent.speed);
                }

                targetPos = hit.point;              //hit.point 지점으로 이동하기 위해 targetPos 초기화
                agent.SetDestination(targetPos);    //이동위치 설정
                agent.isStopped = false;            //이동 시작
            }
        }
    }

    IEnumerator AttackAnimation()
    {
        agent.speed = 0f;
        agent.isStopped = true;
        ani.SetFloat(hashMoveSpeed, agent.speed);
        ani.SetTrigger(hashAttack);
        yield return new WaitForSeconds(3f);
    }

    //카메라 이동
    void LateUpdate()
    {
        float cam_H = 1.3f;

        camPivot.position = tr.position + Vector3.up * cam_H;
        mouseMove += new Vector3(-Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"), 0f);

        if (mouseMove.x < -40f)
            mouseMove.x = -40f;

        else if (mouseMove.x > 40f)
            mouseMove.x = 40f;

        camPivot.eulerAngles = mouseMove;

        RaycastHit hit;
        Vector3 dir = (camTr.position - camPivot.position).normalized;
        if (Physics.Raycast(camPivot.position, dir, out hit, camDist, ~(1 << playerLayer)))
            camTr.localPosition = Vector3.back * hit.distance;

        else camTr.localPosition = Vector3.back * camDist;
    }

    void ClickCheck()
    {
        if (isOneClick && (Time.time - Timer) > doubleClickSecond/*더블 클릭 판단하는 허용 시간 초과*/)
        {
            Debug.Log("OneClick");
            isOneClick = false; // 한번 클릭했으니까 false로 돌려줌
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!isOneClick)
            {
                Timer = Time.time;
                isOneClick = true;
            }

            else if (isOneClick && (Time.time - Timer) <= doubleClickSecond)
            {
                Debug.Log("DoubleClick");
                isOneClick = false; // 두번 클릭했으니까 false로 돌려줌
            }
        }

        else
        {
            #region 목적지 도착시 Idle 하는 첫 번째 방법
            /* if (agent.remainingDistance <= 0.25f)
            {
                agent.speed = 0f;
                ani.SetFloat(hashMoveSpeed, agent.speed);
            } */
            #endregion

            #region 목적지 도착시 Idle 하는 두 번째 방법
            if (Vector3.Distance(tr.position, targetPos) <= 0.25f)
            {
                agent.speed = 0f;
                ani.SetFloat(hashMoveSpeed, agent.speed);
            }
            #endregion
        }
    }
}
