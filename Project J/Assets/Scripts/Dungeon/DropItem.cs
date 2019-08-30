﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    private Camera m_uiCamera;              // UI 카메라
    private DefaultItemInfo m_itemInfo;     // 아이템 정보
    private string m_strItemImageName;      // 아이템 이름
    UILabel m_ItemNameLabel;                // 아이템 이름을 나타내는 Label 컴포넌트
    private Transform m_trsPlayer;          // 플레이어 좌표
    private float m_fDistancePlayer;        // 플레이어와의 거리
    private bool m_bShowTextFlag;           // 텍스트를 보여주고 있는지
    private bool m_bVisibleCamera;          // 카메라 안에 포함 되어 있는지 여부

    // Start is called before the first frame update
    void Start()
    {
        m_uiCamera = GameObject.Find("NGUICamera").GetComponent<Camera>();
        m_strItemImageName = "W_Sword00" + Random.Range(1, 3);                               // 아이템 이름을 랜덤으로 설정해서 1~2범위
        m_itemInfo = DefaultDataManager.instance.m_dicDefaultItemInfo[m_strItemImageName];   // 딕셔너리에서 이름에 해당하는 아이템 정보를 받아와 저장함
        m_trsPlayer = GameObject.Find("Player").GetComponent<Transform>();                   // 플레이어의 좌표
    }

    // Update is called once per frame
    void Update()
    {
        m_fDistancePlayer = Vector3.Distance(m_trsPlayer.position, transform.position);       // 플에이어와의 거리차

        if (m_fDistancePlayer < 20 && m_bVisibleCamera == true)                               // 거리차가 20 이하이면서 카메라 안에 들어와 있으면
        {
            if (m_bShowTextFlag == false)                                                     // 텍스트가 보여지고 있지 않으면
            {
                m_ItemNameLabel = ObjectPoolManager.Instance.PopFromPool("DropItemLabel").GetComponent<UILabel>(); // 오브젝트 풀로부터 Label 컴포넌트를 받아옴
                m_ItemNameLabel.text = m_strItemImageName;                                                         // 텍스트 내용 삽입
                m_ItemNameLabel.gameObject.SetActive(true);                                                        // 오브젝트 활성화
                m_bShowTextFlag = true;                                                       // 텍스트를 보여주는 상태
            }
        }
        else                                                                                  // 거리차가 20이상이거나 카메라 밖을 벗어나면
        {
            if (m_bShowTextFlag == true)                                                              // 텍스트가 노출되어 있는 경우
            {
                ObjectPoolManager.Instance.PushToPool("DropItemLabel", m_ItemNameLabel.gameObject);   // 오브젝트 풀로부터 Label을 반납
                m_ItemNameLabel = null;
                m_bShowTextFlag = false;                                                              // 텍스트를 없앤다.
            }
        }

        if (m_bShowTextFlag == true)                                                      // 텍스트를 보여주는 상태값이 true이면
            itemNameVisible();                                                            // 보여준다
    }

    void itemNameVisible()
    {
        m_ItemNameLabel.text = m_itemInfo.m_strName;
        Vector3 position = Camera.main.WorldToViewportPoint(transform.position);        // 아이템의 위치를 메인 카메라의 viewPort좌표로 변환
        m_ItemNameLabel.transform.position = m_uiCamera.ViewportToWorldPoint(position); // 해당 viewPort좌표를 UI카메라의 world좌표로 변환
        position = m_ItemNameLabel.transform.localPosition;                             // 포지션 위치 재조립
        position.z = 0.0f;                                                              // Z축을 없애고 재조립
        m_ItemNameLabel.transform.localPosition = position;                             // 재조립
        m_ItemNameLabel.transform.localScale = Vector3.one;                             // 스케일이 이상해지므로 1로 변경
    }

    public string getItenName()    // 아이템 고유명 반환 
    {
        if (m_ItemNameLabel != null)
            ObjectPoolManager.Instance.PushToPool("DropItemLabel", m_ItemNameLabel.gameObject);   // 오브젝트 풀로부터 Label을 반납
        m_bShowTextFlag = false;                                                              // 텍스트를 없앤다.
        return m_strItemImageName;
    }

    void OnBecameVisible()                 // 카메라에서 보이는 경우 이벤트 함수                                      
    {
        m_bVisibleCamera = true;
    }

    void OnBecameInvisible()               // 카메라에서 보이지 않는 경우 이벤트 함수
    {
        m_bVisibleCamera = false;
    }

    private void OnEnable()                 // 활성화 되면
    {
        GetComponent<Collider>().isTrigger = false;

        if (IsInvoking("getTime") == true)
            CancelInvoke("getTime");
        if (IsInvoking("pushItem") == true)
            CancelInvoke("pushItem");

        Invoke("getTime", 2.0f);              // 2초후 먹을 수 있다.
        Invoke("pushItem", 10.0f);              // 10초후 소실된다.
    }

    void getTime()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void pushItem()
    {
        if (m_ItemNameLabel != null)
            ObjectPoolManager.Instance.PushToPool("DropItemLabel", m_ItemNameLabel.gameObject);   // 오브젝트 풀로부터 Label을 반납
        ObjectPoolManager.Instance.PushToPool("DropItem", gameObject);
    }

    // Unity Documentation : MonoBehaviour.OnBecameVisible() Description
    // OnBecameVisible은 렌더러가 카메라에서 보이는 경우에 호출됩니다.
    // 이 메시지는 렌더러에 첨부된 모든 스크립트에 전달됩니다. 
    // OnBecameInvisible과 OnBecameInvisible은 해당 오브젝트가 보이는 경우에만 필요한 계산을 피하는 데 매우 유용합니다.
    // OnBecameVisible은 함수 안에 간단히 yield 구문을 사용해서 co-routine으로 실행할 수 있습니다. 
    // 에디터상에서 실행하는 경우에, 씬 뷰 카메라 또한 이 함수의 호출을 야기합니다.
}
