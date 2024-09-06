using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 휠콜라이더에 따라서 타이어도 같이 움직이기 위해서 
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
