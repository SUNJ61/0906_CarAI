using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Utility;


public class PlayerCar : MonoBehaviourPun,IPunObservable
{
    [Header("WheelCollider")]
    public WheelCollider frontLeft_Col;
    public WheelCollider frontRight_Col;
    public WheelCollider backLeft_Col;
    public WheelCollider backRight_Col;
    [Header("Wheel Models")]
    public Transform frontLeft_M;
    public Transform frontRight_M;
    public Transform backLeft_M;
    public Transform backRight_M;
    [Header("Mass Blance")] //무게 밸런스 잡기 
    public Vector3 CentOfMass = new Vector3 (0f, -0.5f, 0f);
    //자동차나 마차등 바퀴가 있는 모델의 무게중심은 y축은 항상 -0.5로 잡아야 함
    public Rigidbody rb;
    [Header("앞바퀴 최대 회전각 ")]
    public float maxSteer = 35f;
    [Header("최대 마찰력")]
    public float maxTorque = 2500f; //토르크 라고 불린다.
    [Header("최대 브레이크 강도 ")]
    public float maxBrake = 3500f;
    [Header("현재 스피드")]
    public float currentSpeed = 0f;
    private float Steer = 0f; //A,D 키값을 받을 변수 방향잡는 용도
    private float forward = 0f; // W키만 받아서 전진 하는  변수 
    private float back = 0f; //S키만 받아서 후진 하는 변수
    bool isrevers = false; //전진인지 후진인지 판단 하는 변수 
    private float motor =0f; // 음수면 후진 양수이면 전진 
    private float brake = 0f; //브레이크 
    private GetInOut inOut;
    private Transform tr;
    private Vector3 curPos = Vector3.zero;
    private Quaternion curRot = Quaternion.identity;
    void Awake()
    {
       
        rb = GetComponent<Rigidbody>();
        photonView.Synchronization = ViewSynchronization.ReliableDeltaCompressed;
        //photonView.ObservedComponents[0] = this;
      
        if (photonView.IsMine)
        {
            rb.centerOfMass = CentOfMass;
            Camera.main.GetComponent<SmoothFollow>().target = transform;
        }
     
        tr = GetComponent<Transform>();
        curPos = tr.position;
        curRot = tr.rotation;
       
        //inOut = transform.GetChild(5).GetComponent<GetInOut>();
    }
    void  FixedUpdate() // 고정프레임 정확한 물리량이나 정확한 시간에 따라 움직이는 로직을 구현 하려면 
    {
        if (photonView.IsMine)
        {
            CarMove();
            if (Input.GetKey(KeyCode.B))
                CarBreak();
            else
                CarMove();
        }
      
        
            
            
    }

    private void CarMove()
    {
        frontLeft_Col.brakeTorque = 0f;
        frontRight_Col.brakeTorque = 0f;

        currentSpeed = rb.velocity.sqrMagnitude;
        //휠콜라이더가 마찰력에 의에 리지디 바디의 전체 속도(이륜)를 전달
        Steer = Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f);
        //W키는 전진만 한다.
        forward = Mathf.Clamp(Input.GetAxis("Vertical"), 0f, 1f);
        back = -1 * Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 0f);
        //S키는 후진만 한다. 
        if (Input.GetKey(KeyCode.W))
        {
            StartCoroutine(ForWardCar());
        }
        if (Input.GetKey(KeyCode.S))
        {
            StartCoroutine(BackWardCar());
        }
        if (isrevers) //후진중이라면
        {
            motor = -1 * back;
            brake = forward;
        }
        else
        {
            motor = forward;
            brake = back;
        }
        //뒤바퀴  토르크 회전력 
        backLeft_Col.motorTorque = motor * maxTorque;
        backRight_Col.motorTorque = motor * maxTorque;
        // 뒤바퀴 브레이크 
        backLeft_Col.brakeTorque = brake * maxBrake;
        backRight_Col.brakeTorque = brake * maxBrake;

        //앞바퀴 y축 회전각도 
        frontLeft_Col.steerAngle = maxSteer * Steer;
        frontRight_Col.steerAngle = maxSteer * Steer;

        // A,D 눌렀을 때 y축 회전 
        frontLeft_M.localEulerAngles =
            new Vector3(frontLeft_M.localEulerAngles.x,
            Steer * maxSteer, frontLeft_M.localEulerAngles.z);
        frontRight_M.localEulerAngles =
           new Vector3(frontRight_M.localEulerAngles.x,
           Steer * maxSteer, frontRight_M.localEulerAngles.z);
        //모델링 회전 휠콜라이더의 회전 토르크 값을 받아서 같이 회전
        frontLeft_M.Rotate(frontLeft_Col.rpm * Time.deltaTime,
            0f, 0f);
        frontRight_M.Rotate(frontRight_Col.rpm * Time.deltaTime,
            0f, 0f);
        backLeft_M.Rotate(backLeft_Col.rpm * Time.deltaTime,
            0f, 0f);
        backRight_M.Rotate(backRight_Col.rpm * Time.deltaTime,
            0f, 0f);
    }

    IEnumerator ForWardCar()
    {
        yield return new WaitForSeconds(0.1f);
        currentSpeed = 0f;
        if(back > 0f) isrevers = true;
        if (forward > 0f) isrevers = false;
    }
    IEnumerator BackWardCar()
    {
        yield return new WaitForSeconds(0.1f);
        currentSpeed = 0.1f;
        if (back > 0f) isrevers = true;
        if (forward > 0f) isrevers = false;
    }
    public void CarBreak()
    {
        //currentSpeed = 0f;
        frontRight_Col.motorTorque = 0f;
        frontRight_Col.motorTorque = 0f;
        frontLeft_Col.brakeTorque = 500000f;
        frontRight_Col.brakeTorque = 500000f;
        //rb.velocity = Vector3.zero;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
       if(stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
        }
       else
        {
           // curPos = (Vector3)stream.ReceiveNext();
           // curRot = (Quaternion)stream.ReceiveNext();
           tr.position =(Vector3)stream.ReceiveNext();
           tr.rotation =(Quaternion)stream.ReceiveNext();
        }
    }
}
