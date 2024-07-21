using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedGraph : MonoBehaviour
{
    public int nodeCount = 10;  
    public float attractionForce = 0.01f;  
    public float repulsionForce = 0.05f;  
    public float collisionRepulsionForce = 10.0f; 
    public float connectionProbability = 0.2f;  
    public GameObject nodePrefab;  

    private List<GameObject> nodes;  
    private Dictionary<int, List<int>> connections;  
    private GameObject selectedNode;  
    private Vector3 offset;  

    void Start()
    {
        nodes = new List<GameObject>();
        connections = new Dictionary<int, List<int>>();

        for (int i = 0; i < nodeCount; i++)
        {
            GameObject node = Instantiate(nodePrefab, Random.insideUnitSphere * 5f, Quaternion.identity);
            nodes.Add(node);
            connections[i] = new List<int>();
        }

        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = i + 1; j < nodeCount; j++)
            {
                if (Random.value < connectionProbability)
                {
                    connections[i].Add(j);
                    connections[j].Add(i);
                }
            }
        }
    }

    void Update()
    {
        HandleMouseInput();

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == selectedNode) continue;  

            Vector3 force = Vector3.zero;

            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;

                Vector3 direction = nodes[j].transform.position - nodes[i].transform.position;
                float distance = direction.magnitude;
                direction.Normalize();

                if (connections[i].Contains(j))
                {
                    if (distance < 1.0f)
                    {
                        force -= direction * collisionRepulsionForce * (1.0f - distance);
                    }
                    else
                    {
                        force += direction * attractionForce * distance;
                    }
                }
                else
                {
                    force -= direction * repulsionForce / distance;
                }
            }

            nodes[i].transform.position += force * Time.deltaTime;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (nodes.Contains(hit.transform.gameObject))
                {
                    selectedNode = hit.transform.gameObject;
                    offset = selectedNode.transform.position - GetMouseWorldPosition();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedNode = null;
        }

        if (selectedNode != null)
        {
            Vector3 newPosition = GetMouseWorldPosition() + offset;
            MoveNodeAndConnected(nodes.IndexOf(selectedNode), newPosition);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.WorldToScreenPoint(nodes[0].transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    void MoveNodeAndConnected(int nodeIndex, Vector3 newPosition)
    {
        Vector3 displacement = newPosition - nodes[nodeIndex].transform.position;
        nodes[nodeIndex].transform.position = newPosition;

        foreach (int connectedIndex in connections[nodeIndex])
        {
            nodes[connectedIndex].transform.position += displacement / 2.0f;
        }
    }

    void OnDrawGizmos()
    {
        if (connections == null || nodes == null) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < connections.Count; i++)
        {
            foreach (int j in connections[i])
            {
                if (i < j)
                {
                    Gizmos.DrawLine(nodes[i].transform.position, nodes[j].transform.position);
                }
            }
        }
    }
}
