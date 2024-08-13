using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaState : MonoBehaviour
{
    public enum State
    {
        PATROL = 0, TRACE, ATTACK, DIE,
    }
    public State state = State.PATROL;

    MariaMove mariaMove;

    public Rigidbody rb;
    [SerializeField] Transform playerTr;
    [SerializeField] Transform tr;
    [SerializeField] Animator ani;

    float attackDist = 3.5f;
    float traceDist = 10f;
    public bool isDie = false;


    void Awake()
    {
        mariaMove = GetComponent<MariaMove>();
        ani = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        tr = transform;
        playerTr = GameObject.FindWithTag("Player").GetComponent<Transform>();

        if (playerTr != null)
            playerTr = playerTr.GetComponent<Transform>();
    }

    void OnEnable()
    {
        StartCoroutine(CheckState());
        StartCoroutine(Action());
    }

    IEnumerator CheckState()
    {
        while (!isDie)
        {
            if (state == State.DIE) yield break;

            float dist = (playerTr.position - tr.position).magnitude;

            if (dist <= attackDist)
                state = State.ATTACK;

            else if (dist <= traceDist)
                state = State.TRACE;

            else state = State.PATROL;

            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator Action()
    {
        while (!isDie)
        {
            yield return new WaitForSeconds(0.3f);

            switch (state)
            {
                case State.PATROL:
                    mariaMove.patroll = true;
                    break;

                case State.TRACE:
                    mariaMove.patroll = true;
                    ani.SetBool("run", true);
                    ani.SetBool("attack", false);
                    break;

                case State.ATTACK:
                    mariaMove.patroll = false;
                    ani.SetBool("attack", true);
                    ani.SetBool("run", false);
                    tr.LookAt(playerTr);
                    break;

                case State.DIE:
                    isDie = true;
                    mariaMove.patroll = false;
                    ani.SetTrigger("Die");
                    break;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
