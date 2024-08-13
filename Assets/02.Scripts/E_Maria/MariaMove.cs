using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MariaMove : MonoBehaviour
{
    public List<Transform> PointList;
    Transform tr;
    public int nextIndex;
    float damping = 1.0f;
    public float attackDist = 10f;
    public float traceDist = 3.5f;
    public bool patroll = true;
    public float rotSpeed;
    float patrolSpeed = 5f;

    void Start()
    {
        tr = transform;

        var point = GameObject.Find("Points");
        if(point != null)
        {
            point.GetComponentsInChildren<Transform>(PointList);
            PointList.RemoveAt(0);
        }

        nextIndex = Random.Range(0, PointList.Count);
    }

    void Update()
    {
        if(!patroll) return;

        Vector3 nextPoint = PointList[nextIndex].position;
        float dist = Vector3.Distance(tr.position, nextPoint);  //거리
        Vector3 dir = nextPoint - tr.position;  //방향

        rotSpeed = damping * Time.deltaTime;
        Quaternion rot = Quaternion.LookRotation(dir);
        tr.rotation = Quaternion.Slerp(tr.rotation, rot, rotSpeed);
        
        tr.position = Vector3.MoveTowards(tr.position, nextPoint, patrolSpeed * Time.deltaTime);

        if(dist <= 0.5f) nextIndex = Random.Range(0, PointList.Count);
    }
}
