using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardHit : MonoBehaviour
{
    [SerializeField] CapsuleCollider col;
    [SerializeField] MeshRenderer mesh;
    void Start()
    {
        col = GameObject.FindWithTag("WIZARD_ATTACK").GetComponent<CapsuleCollider>();
        mesh = GameObject.FindWithTag("WIZARD_ATTACK").GetComponent<MeshRenderer>();
    }

    public void WizardEnable()
    {
        col.enabled = true;
        mesh.enabled = true;
    }

    public void WizardDisable()
    {
        col.enabled = false;
        mesh.enabled = false;
    }
}
