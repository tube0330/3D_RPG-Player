using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardPlayer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform tr;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] CharacterController controller;
    [SerializeField] Rigidbody rb;
    [SerializeField] CapsuleCollider col;
    [SerializeField] Animator ani;

    [Header("Click")]
    public double Timer = 0d;               //시간 재는 타이머
    public float doubleClickSecond = 0.25f; //0.25초 안에 누르면 double click됨
    public bool isOneClick = false;
    Ray ray;
    RaycastHit hit;

    int groundRayer;

    readonly int hashMoveSpeed = Animator.StringToHash("moveSpeed");

    void Start()
    {
        tr = transform;
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        ani = GetComponent<Animator>();

        groundRayer = LayerMask.NameToLayer("GROUND");
    }

    void Update()
    {
        ClickCheck();

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 30f, Color.green);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << groundRayer/* LayerMask.NameToLayer("GROUND")*/))
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
            }
        }
    }

    private void ClickCheck()
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
    }
}
