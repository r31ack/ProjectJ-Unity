using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    private Transform target = null;        //추적 대상
    public float moveDamping    = 15.0f;    //이동 속도 계수
    public float rotateDamping  = 10.0f;    //회전 속도 계수
    public float distance       = 5.0f;     //추적 대상과의 거리
    public float height         = 3.0f;     //추적 대상과의 높이
    public float targetOffset   = 2.0f;     //추적 좌표의 오프셋

    public bool rayCastFlag = false;

    //CameraRig의 Transform 컴포넌트
    private Transform tr;
    public bool cameraFlag = true;
    public bool rushKnockBack = false;
    private Dictionary<GameObject,float> m_lnkLstRayCastMap = new Dictionary<GameObject,float>();
    private float KnockBackTimer = 0.0f;


    void Start()
    {
        //CameraRig의 Transform 컴포넌트의 추출
        tr = GetComponent<Transform>();
        target = GameObject.Find("CameraTarget").GetComponent<Transform>();
    }

    //주인공 캐릭터의 이동 로직이 완료된 후 처리하기 위해 LateUpdate에서 구현
    void LateUpdate()
    {
        if (cameraFlag == true)
        {
            //카메라의 높이와 거리를 계산
            Vector3 camPos = target.position - (target.forward * distance)
                            + (target.up * height);

            //이동할 때의 속도 계수를 적용
            tr.position = Vector3.Slerp(tr.position, camPos,
                                        Time.deltaTime * moveDamping);

            //회전할 때의 속도 계수를 적용
            tr.rotation = Quaternion.Slerp(tr.rotation, target.rotation, Time.deltaTime * rotateDamping);

            //카메라를 추적 대상으로 Z축을 회전시킴
            tr.LookAt(target.position + (target.up * targetOffset));
        }

        if (KnockBackTimer > 0.0f)
        {
            KnockBackTimer -= Time.deltaTime;
            if (KnockBackTimer <= 0.0f)
                rushKnockBack = false;
        }
    }

    //추적할 좌표를 시작적으로 표현
    void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.green;

            //추적 및 시야를 맞춤 위치를 표시
            Gizmos.DrawWireSphere(target.position + (target.up * targetOffset), 0.1f);

            //메인 카메라와 추적 지점 간의 선을 표시
            Gizmos.DrawLine(target.position + (target.up * targetOffset), transform.position);
        }
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.tag == "Map")
        {
            MeshRenderer renderer = coll.GetComponent<MeshRenderer>();
            Color mapColor = renderer.material.color;
            mapColor.a = 0.2f;
            renderer.material.color = mapColor;
            Debug.Log("맵과부딪힘");
        }
        if (coll.gameObject.tag == "enemy")
        {
            if(rushKnockBack == true)
            {
                coll.transform.rotation = transform.rotation * new Quaternion(0, 1, 0, 0);
                coll.transform.Translate(coll.transform.forward * -100 * Time.deltaTime, Space.World);
                coll.GetComponent<EnemyInfomation>().attacted(3);
                KnockBackTimer = 0.1f;
            }
        }
    }

    private void OnTriggerExit(Collider coll)                // 맵충돌 투명화 복구
    {
        if (coll.gameObject.tag == "Map")
        {
            MeshRenderer renderer = coll.GetComponent<MeshRenderer>();
                Color mapColor = renderer.material.color;
                mapColor.a = 1.0f;
                renderer.material.color = mapColor;
        }
    }
}
