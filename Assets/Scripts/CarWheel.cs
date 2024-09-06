using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ���ݶ��̴��� ���� Ÿ�̾ ���� �����̱� ���ؼ� 
public class CarWheel : MonoBehaviour
{
    public WheelCollider TargetWheel;
    public Vector3 WheelPosition = Vector3.zero;
    public Quaternion WheelRotation = Quaternion.identity;
   
    void Start()
    {
        
    }
    void Update()
    {
        TargetWheel.GetWorldPose(out WheelPosition, out WheelRotation);
        transform.position = WheelPosition;
        transform.rotation = WheelRotation;
    }
}
