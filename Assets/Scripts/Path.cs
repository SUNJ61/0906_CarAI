using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    [SerializeField] Color lineColor;
    [SerializeField] List<Transform> Nodes;

    private void OnDrawGizmos() 
    {      
        Gizmos.color = lineColor;
        Transform[] pathTransforms = GetComponentsInChildren<Transform>();
        Nodes = new List<Transform>();
        for(int i =0; i<pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != transform) //자기 자신을 빼고 하위 트랜스폼을 담는다.
            {
                Nodes.Add(pathTransforms[i]);
            }
        }
        for(int i = 0; i < Nodes.Count; i++)
        {     //현재노드 
            Vector3 currentNode = Nodes[i].position;
            //이전 노드
            Vector3 previousNode = Vector3.zero; 
            if(i>0)
            {
                previousNode = Nodes[i-1].position;
            }
            else if(i==0 && Nodes.Count >1) //i가 0과 같고 노드카운트가 1이상이면 
            {
                previousNode= Nodes[Nodes.Count-1].position;
            }
                           //좌표에  선은 이전 노드에서 현재노드로 
            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawSphere(currentNode, 1.0f); //현재노드에 색상을 넣는다. 1만큼
        }


    }
}
