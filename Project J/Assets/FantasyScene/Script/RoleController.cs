using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class RoleController : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 off = new Vector3(1f, 1f, 1f);
    NavMeshAgent agent;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 10f;
        agent.baseOffset = 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position = transform.position + off;
        Vector3 pos;
        if(Input.GetKey(KeyCode.UpArrow))
        {
            pos = transform.position + Vector3.forward * 0.1f;

            agent.SetDestination(pos);
        }
        else if(Input.GetKey(KeyCode.DownArrow))
        {
            agent.SetDestination(transform.position - Vector3.forward * 0.1f);
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            agent.SetDestination(transform.position + Vector3.right * 0.1f);
        }
        else if(Input.GetKey(KeyCode.LeftArrow))
        {
            agent.SetDestination(transform.position - Vector3.right * 0.1f);
        }
    }

    private void LateUpdate()
    {
        Camera.main.transform.LookAt(transform.position);
    }
}
