using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using UnityEngine;
//1.���� �߽� 2. pathTransforms �����ؼ� �̵� 
//3. Ÿ�̾� ���ݶ��̴� ��ġ Ÿ�̾�  �𵨸� ��ġ 
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
    //���� ���ǵ�
    public float currentSpeed = 0; //���� ���ǵ�
    private int currentNode = 0; //���� ��� 
    private float maxSpeed = 150f;// 150�̻� ���޸��� ���� 
    public float maxMotorTorque = 4000f; //���ݶ��̴��� ȸ���ϴ� �ִ� �� 
    public float maxSteerAngle = 30f; // �չ��� ȸ�� �� 
    [Header("Obstacle Avoid")]
    [SerializeField] private float sensorLenght = 30.0f; // ��ֹ��� Ž���ϴ� ���� ����.
    [SerializeField] private float frontSideSensorPos = 1.2f; //���� ���� ���̵� ������
    [SerializeField] private float frontSensorAngle = 30f; //ȸ�� �� �� ���� ����
    private float targetSteerAngle = 0f;
    [SerializeField]private Vector3 frontSensorPos; //���� ����
    [SerializeField]private Vector3 frontLeftSensorPos; //������ ����, ���� �߰���
    [SerializeField]private Vector3 frontRightSensorPos; //������ ����, ���� �߰���
    bool avoiding = false; //��ֹ��� ���� ������ �Ǵ��ϴ� ����
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
    void ApplySteer() //�չ����� ���ݶ��̴��� path�� ���� ȸ�� �ϴ� �޼���
    {
        Vector3 relativeVector = transform.InverseTransformPoint(pathList[currentNode].position);
        //��������  ���� =   ������ǥ��(���ӻ��� ��ǥ)�� ������ǥ�� ��ȯ �Ѵ�. 
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
                           // (�н�Ʈ������.x�� ������ �н�Ʈ���� ��ü ũ��)  *30

        //FL.steerAngle = newSteer;
        //FR.steerAngle = newSteer;
        //���ݶ��̴� �ޱ� ���� �����ȴ�.
        targetSteerAngle = newSteer; 
                         
    }
    void Drive()
    {
        //���� �ڳʸ� �Ҷ� �ӵ��� ���� �ϴ� �͵� ���� 
        currentSpeed = 2 * Mathf.PI * FL.radius * FL.rpm * 60 / 1000;
        #region ���� ���� ����
        // 2 * Mathf.PI: ���� �ѷ��� ���ϴ� ������ �Ϻ��Դϴ�. 2��� ���� �ѷ��� �������� �����ϴ� ����Դϴ�.
        //FL.radius: ȸ���ϴ� ��ü�� �������Դϴ�.
        //FL.rpm: ȸ���ϴ� ��ü�� �д� ȸ�� ��(Revolutions Per Minute)�Դϴ�.
        //60: �д� ȸ�� ���� �ʴ� ȸ�� ���� ��ȯ�ϱ� ���� ���Դϴ�. (1�� = 60��)
        //1000: ���ͷ� ��ȯ�ϱ� ���� ���Դϴ�. (1ų�ι��� = 1000����)
        #endregion

        if(currentSpeed < maxSpeed) //���� ���ǵ尡 �ְ� ���ǵ庸�� �۴ٸ� 
        {
            BL.motorTorque = maxMotorTorque;
            BR.motorTorque = maxMotorTorque;
        }
        else //���罺�ǵ� �ְ��ǵ庸�� ���ٸ� 
        {
            BL.motorTorque = 0f;
            BR.motorTorque = 0f;
        }

    }
    void CarSensor()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position; //������ ��ŸƮ �������� ���� ����ġ�� �Ѵ�.
        sensorStartPos += transform.forward * frontSensorPos.z; //������ ������(����) �� (�ȸ����� ���� ���� ��ġ��)
        sensorStartPos += transform.up * frontSensorPos.y; //������ ���� ��
        float avoidMultilplier = 0f; //ȸ�� ��
        avoiding = false;

        #region frontCenter Sensor
        if(Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //������ ���⿡ ���̸� ��� ��ֹ��� �ִ��� ����.
            avoiding = true; //���ض�
            Debug.Log("��ֹ� �߰�");
            Debug.DrawLine(sensorStartPos,hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor
        sensorStartPos -= transform.right * frontSideSensorPos; //������ ������(����) �� (�ȸ����� ���� ���� ��ġ��)
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght,1<<8))
        { //���ʿ��� ���� �������� ���̸� ��� ��ֹ��� �ִ��� ����.
            avoiding = true; //���ض�
            avoidMultilplier += 1f;
            Debug.Log("��ֹ� �߰�");
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor Angle (���� ���� ���̵��� ���� ȸ�� �� �� Ray�� ���� ���� ����� ȸ��)
        else if(Physics.Raycast(sensorStartPos, Quaternion.AngleAxis
            (-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1<<8))
        { //���� ���� ���⿡ ��ֹ��� �������� ���� ��� ���ʿ��� 30�� �������� ���̸� ��� ��ֹ��� �����Ѵ�.
            Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier += 0.5f; //(���ʿ� ���̰� ���� ��� ���������� �̵��ؾ���.)
        }
        #endregion
        #region frontRight Sensor
        sensorStartPos += transform.right * frontSideSensorPos * 2f; //������ ������(����) �� (�ȸ����� ���� ���� ��ġ��)
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //�����ʿ��� ���� �������� ���̸� ��� ��ֹ��� �ִ��� ����.
            avoiding = true; //���ض�
            avoidMultilplier -= 1.0f;
            Debug.Log("��ֹ� �߰�");
            Debug.DrawLine(sensorStartPos, hit.point, Color.red);
        }
        #endregion
        #region frontRight Sensor Angle (���� ������ ���̵��� ���� ȸ�� �� �� Ray�� ���� ���� ����� ȸ��)
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis
            (frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1 << 8))
        { //������ ���� ���⿡ ��ֹ��� �������� ���� ��� ���ʿ��� 30�� �������� ���̸� ��� ��ֹ��� �����Ѵ�.
            Debug.DrawLine(sensorStartPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier -= 0.5f; //ȸ�ǰ� ���� (�����ʿ� ���̰� ���� ��� �������� �̵��ؾ���.)
        }
        #endregion
        if(avoidMultilplier ==  0) //ȸ�� ���� ���� 0�� ���ٸ�? == ȸ�Ǹ� ���� �ʾҴٸ�
        {
            if(Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward,
                out hit, sensorLenght, 1 << 8))
            {
                avoiding = true;
                if(hit.normal.x < 0) //normal�� ��ǥ�� �񱳽� ���� �Ӽ��̴�. ���� x�� 0���� ���� ��
                {
                    avoidMultilplier = -1f;
                }
                else //normal�� ��ǥ�� �񱳽� ���� �Ӽ��̴�. ���� x�� 0���� Ŭ ��
                {
                    avoidMultilplier = 1f;
                }
            }
        }
        if(avoiding) //���ؾ� �Ѵٸ�?
        {
            targetSteerAngle = maxSteerAngle * avoidMultilplier; //������ ���� �������� maxSteerAngle ��ŭ ȸ������ ���Ѵ�.
        }
    }

    void MyCarSensor()
    {
        RaycastHit hit;
        float avoidMultilplier = 0f;
        avoiding = false;

        #region frontCenter Sensor
        if (Physics.Raycast(frontSensorPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //������ ���⿡ ���̸� ��� ��ֹ��� �ִ��� ����.
            avoiding = true;
            Debug.DrawLine(frontSensorPos, hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor
        if (Physics.Raycast(frontLeftSensorPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //���ʿ��� ���� �������� ���̸� ��� ��ֹ��� �ִ��� ����.
            avoiding = true;
            avoidMultilplier += 1.0f;
            Debug.DrawLine(frontLeftSensorPos, hit.point, Color.red);
        }
        #endregion
        #region frontLeft Sensor Angle (���� ���� ���̵��� ���� ȸ�� �� �� Ray�� ���� ���� ����� ȸ��)
        else if (Physics.Raycast(frontLeftSensorPos, Quaternion.AngleAxis
            (-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1 << 8))
        { //���� ���� ���⿡ ��ֹ��� �������� ���� ��� ���ʿ��� 30�� �������� ���̸� ��� ��ֹ��� �����Ѵ�.
            Debug.DrawLine(frontLeftSensorPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier += 0.5f;
        }
        #endregion
        #region frontRight Sensor
        if (Physics.Raycast(frontRightSensorPos, transform.forward, out hit, sensorLenght, 1 << 8))
        { //�����ʿ��� ���� �������� ���̸� ��� ��ֹ��� �ִ��� ����.
            avoiding = true; //���ض�
            avoidMultilplier -= 1.0f;
            Debug.DrawLine(frontRightSensorPos, hit.point, Color.red);
        }
        #endregion
        #region frontRight Sensor Angle (���� ������ ���̵��� ���� ȸ�� �� �� Ray�� ���� ���� ����� ȸ��)
        else if (Physics.Raycast(frontRightSensorPos, Quaternion.AngleAxis
            (frontSensorAngle, transform.up) * transform.forward, out hit, sensorLenght, 1 << 8))
        { //������ ���� ���⿡ ��ֹ��� �������� ���� ��� ���ʿ��� 30�� �������� ���̸� ��� ��ֹ��� �����Ѵ�.
            Debug.DrawLine(frontRightSensorPos, hit.point, Color.blue);
            avoiding = true;
            avoidMultilplier -= 0.5f; //ȸ�ǰ� ���� (�����ʿ� ���̰� ���� ��� �������� �̵��ؾ���.)
        }
        #endregion
        //if (avoidMultilplier == 0) //ȸ�� ���� ���� 0�� ���ٸ�? == ȸ�Ǹ� ���� �ʾҴٸ�
        //{
        //    if (Physics.Raycast(frontSensorPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward,
        //        out hit, sensorLenght, 1 << 8))
        //    {
        //        avoiding = true;
        //        if (hit.normal.x < 0) //normal�� ��ǥ�� �񱳽� ���� �Ӽ��̴�. ���� x�� 0���� ���� ��
        //        {
        //            avoidMultilplier = -1f;
        //        }
        //        else //normal�� ��ǥ�� �񱳽� ���� �Ӽ��̴�. ���� x�� 0���� Ŭ ��
        //        {
        //            avoidMultilplier = 1f;
        //        }
        //    }
        //}
        if (avoiding) //���ؾ� �Ѵٸ�?
        {
            targetSteerAngle = maxSteerAngle * avoidMultilplier; //������ ���� �������� maxSteerAngle ��ŭ ȸ������ ���Ѵ�.
        }
    }

    void CheckWayPointDistance()
    {
          //����Ÿ��� ������������ 3.5���� �۰ų� ���ٸ� 
        if (Vector3.Distance(transform.position, pathList[currentNode].position)<= 3.5f)
        {
            if(currentNode == pathList.Count - 1) // �������� ���� �� �ٽ� 0���� �ʱ�ȭ
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
