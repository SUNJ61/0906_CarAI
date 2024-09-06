using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAI : MonoBehaviour
{
    public Rigidbody rb;
    public Transform frontLeftM;
    public Transform frontRightM;
    public Transform backLeftM;
    public Transform backRightM;
    public WheelCollider frontLeftC;
    public WheelCollider frontRightC;
    public WheelCollider backLeftC;
    public WheelCollider backRightC;
    public float currentSpeed = 0f;
    public float maxTourque = 700f;
    public float maxBrake = 3500f;
    public float maxSteer = 30f;
    public Transform tr;
    public List<Transform>NodeList = new List<Transform>();
    private int currentNode = 0;
    private float maxSpeed = 100f;
  
    void Start()
    {
        tr = GetComponent<Transform>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3 (0, -0.5f, 0);
        var path = GameObject.Find("PathTransforms");
        if (path != null)
            path.GetComponentsInChildren<Transform>(NodeList);
        NodeList.RemoveAt(0);
    }
    void Update()
    {
        ApplyCar();
        Drive();
        CheckWayPoint();
    }
    void ApplyCar()
    {
        Vector3 relativeVector = tr.InverseTransformPoint(NodeList[currentNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteer;
        frontLeftC.steerAngle = newSteer;
        frontRightC.steerAngle = newSteer;
    }
    void Drive()
    {
        currentSpeed = 2f * Mathf.PI * frontLeftC.radius * frontLeftC.rpm * 60 / 1000;

        if(currentSpeed < maxSpeed)
        {
            backRightC.motorTorque =  maxTourque;
            backLeftC.motorTorque =  maxTourque;
        }
        else
        {
            backLeftC.motorTorque = 0f;
            backRightC.motorTorque = 0f;
        }

    }
    void CheckWayPoint()
    {
        if (Vector3.Distance(tr.position, NodeList[currentNode].position)<=5.5f)
        {
            if (currentNode == NodeList.Count - 1)
            {
                currentNode = 0;
            }
            else
                currentNode++;

        }

    }
}
