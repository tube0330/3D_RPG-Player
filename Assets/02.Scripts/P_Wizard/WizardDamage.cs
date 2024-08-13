using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardDamage : MonoBehaviour
{
    [Header("Attack")]
    Animator ani;
    CapsuleCollider col;
    Rigidbody rb;
    public string swordAttackTag = "SWORD_ATTACK";
    int hitCnt = 0;
    public bool isDie = false;

    readonly int hashUnderAttack = Animator.StringToHash("UnderAttack");
    readonly int hashDie = Animator.StringToHash("Die");

    void Start()
    {
        ani = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag(swordAttackTag))
        {
            ani.SetTrigger(hashUnderAttack);
            Debug.Log($"위저드가 {hitCnt}만큼 맞음");
            hitCnt++;

            if (hitCnt >= 10)
                WizardDie();
        }
    }

    void WizardDie()
    {
        isDie = true;
        ani.SetTrigger(hashDie);
        col.enabled = false;
        rb.isKinematic = true;
    }
}
