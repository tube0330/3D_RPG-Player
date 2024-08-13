using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaDamage : MonoBehaviour
{
    [Header("Attack")]
    Animator ani;
    CapsuleCollider col;
    Rigidbody rb;
    public string wizardAttackTag = "WIZARD_ATTACK";
    int hitCnt = 0;
    public bool isDie = false;

    readonly int hashUnderAttack = Animator.StringToHash("Hit");
    readonly int hashDie = Animator.StringToHash("Die");

    void Start()
    {
        ani = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(wizardAttackTag))
        {
            ani.SetTrigger(hashUnderAttack);
            Debug.Log("마리아가 맞음");
            hitCnt++;

            if (hitCnt >= 5)
                MariaDie();
        }
    }

    void MariaDie()
    {
        isDie = true;
        ani.SetTrigger(hashDie);
        rb.isKinematic = true;
        col.enabled = false;
    }
}
