using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
//1.무게 중심 2. pathTransforms 참조해서 이동 
//3. 타이어 휠콜라이더 배치 타이어  모델링 배치 
public class AICar : MonoBehaviour
{
    [Header("CentOfMass")]
    [SerializeField] Rigidbody rb;
    public Vector3 CentOfMass = new Vector3 (0f, -0.5f, 0f);
    [Header("Path")]
    [SerializeField] Transform path;
    [SerializeField] Transform[] pathTransforms;
    [SerializeField]List<Transform> pathList;
    [Header("WheelCollider")]
    [SerializeField] WheelCollider FL;
    [SerializeField] WheelCollider FR;
    [SerializeField] WheelCollider BL;
    [SerializeField] WheelCollider BR;
    [Header("Modeling")]
    [SerializeField] Transform FLTr;
    [SerializeField] Transform FRTr;
    [SerializeField] Transform BLTr;
    [SerializeField] Transform BRTr;
    //현재 스피드
    public float currentSpeed = 0; //현재 스피드
    private int currentNode = 0; //현재 노드 
    private float maxSpeed = 150f;// 150이상 못달리게 제한 
    public float maxMotorTorque = 4000f; //휠콜라이더가 회전하는 최대 힘 
    public float maxSteerAngle = 30f; // 앞바퀴 회전 각 
    [Header("Obstacle Avoid")]
    [SerializeField] private float sensorLenght = 30.0f; // 장애물을 탐지하는 센서 길이.
    [SerializeField] private float frontSideSensorPos = 1.2f; //전방 센서 사이드 포지션
    [SerializeField] private float frontSensorAngle = 30f; //회전 할 때 센서 각도
    private float targetSteerAngle = 0f;
    [SerializeField]private Vector3 frontSensorPos; //전방 센서
    [SerializeField]private Vector3 frontLeftSensorPos; //좌전방 센서, 내가 추가함
    [SerializeField]private Vector3 frontRightSensorPos; //우전방 센서, 내가 추가함
    bool avoiding = false; //장애물을 피할 것인지 판단하는 변수
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = CentOfMass;
        path = GameObject.Find("PathTransforms").transform;
        pathTransforms = path.GetComponentsInChildren<Transform>();
        for(int i =0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path)
                pathList.Add(pathTransforms[i]);
        }
        frontSensorPos = new Vector3(0f, 0.25f, 4.0f);
        frontLeftSensorPos = new Vector3(-1.5f, 0.25f, 4.0f);
        frontRightSensorPos = new Vector3(1.5f, 0.25f, 4.0f);
    }

    
    void FixedUpdate()
    {
        ApplySteer();
        Drive();
        CheckWayPointDistance();
        CarSensor();
        //MyCarSensor();
        LerpToSteerAngle();
    }
    void ApplySteer() //앞바퀴가 휠콜라이더가 path에 따라서 회전 하는 메서드
    {
        Vector3 relativeVector = transform.InverseTransformPoint(pathList[currentNode].position);
        //실제적인  방향 =   월드좌표를(게임상의 좌표)를 로컬좌표로 변환 한다. 
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
                           // (패스트랜스폼.x값 나누기 패스트랜폼 개체 크기)  *30

        //FL.steerAngle = newSteer;
        //FR.steerAngle = newSteer;
        //휠콜라이더 앵글 각이 결정된다.
        targetSteerAngle = newSteer; 
                         
    }
    void Drive()
    {
        //차가 코너링 할때 속도를 감속 하는 것도 포함 
        currentSpeed = 2 * Mathf.PI * FL.radius * FL.rpm * 60 / 1000;
        #region 위의 공식 설명
        // 2 * Mathf.PI: 원의 둘레를 구하는 공식의 일부입니다. 2π는 원의 둘레를 반지름과 연결하는 상수입니다.
        //FL.radius: 회전하는 물체의 반지름입니다.
        //FL.rpm: 회전하는 물체의 분당 회전 수(Revolutions Per Minute)입니다.
        //60: 분당 회전 수를 초당 회전 수로 변환하기 위한 값입니다. (1분 = 60초)
        //1000: 미터로 변환하기 위한 값입니다. (1킬로미터 = 1000미터)
        #endregion

        if(currentSpeed < maxSpeed) //현재 스피드가 최고 스피드보다 작다면 
        {
            BL.motorTorque = maxMotorTorque;
            BR.motorTorque = maxMotorTorque;
        }
        else //현재스피드 최고스피드보다 높다면 
        {
            BL.motorTorque = 0f;
            BR.motorTorque = 0f;
        }

    }
    void CarSensor()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position; //센서의 스타트 포지션을 현재 차위치로 한다.
        sensorStartPos += transform.forward * frontSensorPos.z; //센서의 포워드(전방) 값 (안맞으면 위에 벡터 고치기)
        sensorStartPos += transform.up * frontSensorPos.y; //센서의 높이 값
        float avoidMultilplier = 0f; //회피 곱
        avoiding = false;

        #region frontCenter Sensor
        if(Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //전방의 방향에 레이를 쏘아 장애물이 있는지 감지.
            avoiding = true; //피해라
            Debug.Log("장애물 발견");
            Debug.DrawLine(sensorStartPos,hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor
        sensorStartPos -= transform.right * frontSideSensorPos; //센서의 포워드(전방) 값 (안맞으면 위에 벡터 고치기)
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght,1<<8))
        { //왼쪽에서 전방 방향으로 레이를 쏘아 장애물이 있는지 감지.
            avoiding = true; //피해라
            avoidMultilplier += 1f;
            Debug.Log("장애물 발견");
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor Angle (전방 왼쪽 사이드쪽 센서 회전 할 때 Ray에 의해 각을 만들어 회전)
        else if(Physics.Raycast(sensorStartPos, Quaternion.AngleAxis
            (-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1<<8))
        { //왼쪽 전방 방향에 장애물이 감지되지 않을 경우 왼쪽에서 30도 방향으로 레이를 쏘아 장애물을 감지한다.
            Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier += 0.5f; //(왼쪽에 레이가 맞을 경우 오른쪽으로 이동해야함.)
        }
        #endregion
        #region frontRight Sensor
        sensorStartPos += transform.right * frontSideSensorPos * 2f; //센서의 포워드(전방) 값 (안맞으면 위에 벡터 고치기)
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //오른쪽에서 전방 방향으로 레이를 쏘아 장애물이 있는지 감지.
            avoiding = true; //피해라
            avoidMultilplier -= 1.0f;
            Debug.Log("장애물 발견");
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);
        }
        #endregion
        #region frontRight Sensor Angle (전방 오른쪽 사이드쪽 센서 회전 할 때 Ray에 의해 각을 만들어 회전)
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis
            (frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1 << 8))
        { //오른쪽 전방 방향에 장애물이 감지되지 않을 경우 왼쪽에서 30도 방향으로 레이를 쏘아 장애물을 감지한다.
            Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier -= 0.5f; //회피곱 수정 (오른쪽에 레이가 맞을 경우 왼쪽으로 이동해야함.)
        }
        #endregion
        if(avoidMultilplier ==  0) //회피 곱의 값이 0과 같다면? == 회피를 하지 않았다면
        {
            if(Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward,
                out hit, sensorLenght, 1 << 8))
            {
                avoiding = true;
                if(hit.normal.x < 0) //normal은 좌표값 비교시 쓰는 속성이다. 각도 x가 0보다 작을 때
                {
                    avoidMultilplier = -1f;
                }
                else //normal은 좌표값 비교시 쓰는 속성이다. 각도 x가 0보다 클 때
                {
                    avoidMultilplier = 1f;
                }
            }
        }
        if(avoiding) //피해야 한다면?
        {
            targetSteerAngle = maxSteerAngle * avoidMultilplier; //위에서 정한 방향으로 maxSteerAngle 만큼 회전각을 정한다.
        }
    }

    void MyCarSensor()
    {
        RaycastHit hit;
        float avoidMultilplier = 0f;
        avoiding = false;

        #region frontCenter Sensor
        if (Physics.Raycast(frontSensorPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //전방의 방향에 레이를 쏘아 장애물이 있는지 감지.
            avoiding = true;
            Debug.DrawLine(frontSensorPos, hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor
        if (Physics.Raycast(frontLeftSensorPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //왼쪽에서 전방 방향으로 레이를 쏘아 장애물이 있는지 감지.
            avoiding = true;
            avoidMultilplier += 1.0f;
            Debug.DrawLine(frontLeftSensorPos, hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor Angle (전방 왼쪽 사이드쪽 센서 회전 할 때 Ray에 의해 각을 만들어 회전)
        else if (Physics.Raycast(frontLeftSensorPos, Quaternion.AngleAxis
            (-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1 << 8))
        { //왼쪽 전방 방향에 장애물이 감지되지 않을 경우 왼쪽에서 30도 방향으로 레이를 쏘아 장애물을 감지한다.
            Debug.DrawLine(frontLeftSensorPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier += 0.5f;
        }
        #endregion
        #region frontRight Sensor
        if (Physics.Raycast(frontRightSensorPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //오른쪽에서 전방 방향으로 레이를 쏘아 장애물이 있는지 감지.
            avoiding = true; //피해라
            avoidMultilplier -= 1.0f;
            Debug.DrawLine(frontRightSensorPos, hit.point, Color.red);
        }
        #endregion
        #region frontRight Sensor Angle (전방 오른쪽 사이드쪽 센서 회전 할 때 Ray에 의해 각을 만들어 회전)
        else if (Physics.Raycast(frontRightSensorPos, Quaternion.AngleAxis
            (frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1 << 8))
        { //오른쪽 전방 방향에 장애물이 감지되지 않을 경우 왼쪽에서 30도 방향으로 레이를 쏘아 장애물을 감지한다.
            Debug.DrawLine(frontRightSensorPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier -= 0.5f; //회피곱 수정 (오른쪽에 레이가 맞을 경우 왼쪽으로 이동해야함.)
        }
        #endregion
        //if (avoidMultilplier == 0) //회피 곱의 값이 0과 같다면? == 회피를 하지 않았다면
        //{
        //    if (Physics.Raycast(frontSensorPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward,
        //        out hit, sensorLenght, 1 << 8))
        //    {
        //        avoiding = true;
        //        if (hit.normal.x < 0) //normal은 좌표값 비교시 쓰는 속성이다. 각도 x가 0보다 작을 때
        //        {
        //            avoidMultilplier = -1f;
        //        }
        //        else //normal은 좌표값 비교시 쓰는 속성이다. 각도 x가 0보다 클 때
        //        {
        //            avoidMultilplier = 1f;
        //        }
        //    }
        //}
        if (avoiding) //피해야 한다면?
        {
            targetSteerAngle = maxSteerAngle * avoidMultilplier; //위에서 정한 방향으로 maxSteerAngle 만큼 회전각을 정한다.
        }
    }

    void CheckWayPointDistance()
    {
          //현재거리가 도착지점에서 3.5보다 작거나 같다면 
        if (Vector3.Distance(transform.position, pathList[currentNode].position)<= 3.5f)
        {
            if(currentNode == pathList.Count - 1) // 마지막에 왔을 때 다시 0으로 초기화
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }

        }
    }

    void LerpToSteerAngle()
    {
        FL.steerAngle = Mathf.Lerp(FL.steerAngle, targetSteerAngle, Time.deltaTime * 50.0f);
        FR.steerAngle = Mathf.Lerp(FR.steerAngle, targetSteerAngle, Time.deltaTime * 50.0f);
    }
}
