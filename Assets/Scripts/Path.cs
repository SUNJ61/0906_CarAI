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
            if (pathTransforms[i] != transform) //�ڱ� �ڽ��� ���� ���� Ʈ�������� ��´�.
            {
                Nodes.Add(pathTransforms[i]);
            }
        }
        for(int i = 0; i < Nodes.Count; i++)
        {     //������ 
            Vector3 currentNode = Nodes[i].position;
            //���� ���
            Vector3 previousNode = Vector3.zero; 
            if(i>0)
            {
                previousNode = Nodes[i-1].position;
            }
            else if(i==0 && Nodes.Count >1) //i�� 0�� ���� ���ī��Ʈ�� 1�̻��̸� 
            {
                previousNode= Nodes[Nodes.Count-1].position;
            }
                           //��ǥ��  ���� ���� ��忡�� ������� 
            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawSphere(currentNode, 1.0f); //�����忡 ������ �ִ´�. 1��ŭ
        }


    }
}
