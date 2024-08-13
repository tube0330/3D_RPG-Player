using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaDamage : MonoBehaviour
{
    [Header("Attack")]
    MariaState mariaState;
    Animator ani;
    CapsuleCollider col;
    Rigidbody rb;
    string wizardAttackTag = "WIZARD_ATTACK";
    [SerializeField] int hitCnt = 0;

    readonly int hashUnderAttack = Animator.StringToHash("UnderAttack");
    readonly int hashDie = Animator.StringToHash("Die");

    void Start()
    {
        ani = GetComponent<Animator>();
        col = GetComponentsInChildren<CapsuleCollider>()[0];
        rb = GetComponent<Rigidbody>();
        mariaState = GetComponent<MariaState>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(wizardAttackTag))
        {
            ani.SetTrigger(hashUnderAttack);
            Debug.Log($"마리아가 {hitCnt}만큼 맞음");
            hitCnt++;

            if (hitCnt >= 5)
                MariaDie();
        }
    }

    void MariaDie()
    {
        Debug.Log("마리아 죽음");
        mariaState.state = MariaState.State.DIE;
        ani.SetTrigger(hashDie);
        rb.isKinematic = true;
        col.enabled = false;
    }
}
