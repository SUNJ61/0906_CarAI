using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car_player : MonoBehaviour
{
    [Header("휠콜라이더")]
    [SerializeField] private WheelCollider FLC;
    [SerializeField] private WheelCollider FRC;
    [SerializeField] private WheelCollider RLC;
    [SerializeField] private WheelCollider RRC;
    [Header("바퀴 모델링")]
    [SerializeField] private Transform FLM;
    [SerializeField] private Transform FRM;
    [SerializeField] private Transform RLM;
    [SerializeField] private Transform RRM;
    [Header("무게 중심 잡기")]
    [SerializeField] private Rigidbody rbody;
    [SerializeField] private Vector3 CentOfMass = new Vector3 (0, -0.5f, 0);
    private float Steer;
    private float forward;
    private float backward;
    private float motor;
    private float brake = 0f;
    public float maxMotorTorque = 1000f;
    public float maxBrake = 3500f;
    private float maxSteer = 35f;
    public float currentSpeed = 0f;
    private bool isReverse;
    void Awake()
    {
        FLC = transform.GetChild(1).GetChild(0).GetComponent<WheelCollider>();
        FRC= transform.GetChild(1).GetChild(1).GetComponent<WheelCollider>();
        RRC = transform.GetChild(1).GetChild(2).GetComponent<WheelCollider>();
        RLC = transform.GetChild(1).GetChild(3).GetComponent<WheelCollider>();
        FLM = transform.GetChild(2).GetChild(0).GetComponent<Transform>();
        FRM = transform.GetChild(2).GetChild(1).GetComponent<Transform>();
        RRM = transform.GetChild(2).GetChild(2).GetComponent<Transform>();
        RLM = transform.GetChild(2).GetChild(3).GetComponent<Transform>();
        rbody = GetComponent<Rigidbody>();
        rbody.centerOfMass = CentOfMass;
    }
    void FixedUpdate()
    {
        currentSpeed = rbody.velocity.sqrMagnitude;
        Steer = Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f);
        forward = Mathf.Clamp(Input.GetAxis("Vertical"), 0f, 1f);
        backward = -1 * Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 0f);
        if(Input.GetKey(KeyCode.W))
        {
            StartCoroutine(ForwardCar());
        }
        if(Input.GetKey(KeyCode.S))
        {
            StartCoroutine (BackwardCar());
        }
        if(isReverse)
        {
            motor = -1 * backward;
            brake = forward;
        }
        else
        {
            motor = forward;
            brake = backward;
        }
        FLC.steerAngle = maxSteer * Steer;
        FRC.steerAngle = maxSteer * Steer;
        RLC.motorTorque = motor * maxMotorTorque;
        RRC.motorTorque = motor * maxMotorTorque;
        RLC.brakeTorque = brake * maxBrake;
        RRC.brakeTorque = brake * maxBrake;

        FLM.Rotate(FLC.rpm * Time.deltaTime, 0f, 0f);
        FRM.Rotate(FRC.rpm * Time.deltaTime, 0f, 0f);
        RLM.Rotate(RLC.rpm * Time.deltaTime, 0f, 0f);
        RRM.Rotate(RRC.rpm * Time.deltaTime, 0f, 0f);
    }
    IEnumerator ForwardCar()
    {
        yield return new WaitForSeconds(0.1f);
        currentSpeed = 0f;
        if(forward > 0f)
            isReverse = false;
        else if(backward >0f)
            isReverse = true;

    }
    IEnumerator BackwardCar()
    {
        yield return new WaitForSeconds(0.1f);
        currentSpeed = 0.1f;
        if (forward > 0f)
            isReverse = false;
        else if (backward > 0f)
            isReverse = true;

    }
}
