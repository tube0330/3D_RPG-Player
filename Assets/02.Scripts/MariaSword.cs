using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaSword : MonoBehaviour
{
    [SerializeField] BoxCollider col;
    [SerializeField] MeshRenderer mesh;

    void Start()
    {
        col = GetComponentsInChildren<BoxCollider>()[0];
        mesh = GetComponentsInChildren<MeshRenderer>()[1];
    }

    public void SwordEnable()
    {
        col.enabled = true;
        mesh.enabled = true;
    }

    public void SwordDisable()
    {
        col.enabled = false;
        mesh.enabled = false;
    }
}
