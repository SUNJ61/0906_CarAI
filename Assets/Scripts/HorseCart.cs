using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseCart : MonoBehaviour
{
    [SerializeField] List<Transform> PathList;
    [SerializeField] Transform tr;
    int currentNode = 0;
    private float timePrev = 0f;
    void Start()
    {
        tr = transform;
        var path = GameObject.Find("PathTransforms").gameObject;
        if(path != null)
            path.GetComponentsInChildren<Transform>(PathList);
        PathList.RemoveAt(0);
        timePrev = Time.time;
    }

    void Update()
    {
        if(Time.time - timePrev >3f)
        {
            MoveWayPoint();
        }

        CheckWayPoint();
    }
    void MoveWayPoint()
    {
        Quaternion rot = Quaternion.LookRotation(PathList[currentNode].position -tr.position);
        tr.rotation = Quaternion.Slerp(tr.rotation,rot,Time.deltaTime *15f);
        tr.Translate(Vector3.forward * Time.deltaTime *5f);
    }
    void CheckWayPoint()
    {
        if (Vector3.Distance(tr.position, PathList[currentNode].position)<=3.0f)
        {
            if (currentNode == PathList.Count - 1)
                currentNode = 0;
            else
                currentNode++;

        }

    }
}
