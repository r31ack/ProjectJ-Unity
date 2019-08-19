using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropGold : MonoBehaviour
{
    private Camera m_uiCamera;              // UI 카메라
    private int m_iGoldAmount;              // 골드 수량
    UILabel m_goldAmountLabel;              // 골드 수량을 나타내는 Label 컴포넌트
    private Transform m_trsPlayer;          // 플레이어 좌표
    private float m_fDistancePlayer;        // 플레이어와의 거리
    private bool m_bShowTextFlag;           // 텍스트를 보여주고 있는지
    private bool m_bVisibleCamera;          // 카메라 안에 포함 되어 있는지 여부

    // Start is called before the first frame update
    void Start()
    {
        m_uiCamera = GameObject.Find("NGUICamera").GetComponent<Camera>();
        m_iGoldAmount = Random.Range(10, 50);       // 골드량 10~50 임시
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
                m_goldAmountLabel = ObjectPoolManager.Instance.PopFromPool("DropGoldLabel").GetComponent<UILabel>();   // 오브젝트 풀로부터 Label 컴포넌트를 받아옴
                m_goldAmountLabel.text = m_iGoldAmount.ToString() + "골드";                                            // 텍스트 내용 삽입
                m_goldAmountLabel.gameObject.SetActive(true);                                                          // 오브젝트 활성화
                m_bShowTextFlag = true;                                                       // 텍스트를 보여주는 상태
            }
        }
        else                                                                                  // 거리차가 20이상이거나 카메라 밖을 벗어나면
        {
            if (m_bShowTextFlag == true)                                                                // 텍스트가 노출되어 있는 경우
            {
                ObjectPoolManager.Instance.PushToPool("DropGoldLabel", m_goldAmountLabel.gameObject);   // 오브젝트 풀로 Label을 반납해
                m_bShowTextFlag = false;                                                                // 텍스트를 없앤다.
            }
        }

        if (m_bShowTextFlag == true)                                                      // 텍스트를 보여주는 상태값이 true이면
            itemNameVisible();                                                            // 보여준다
    }

    void itemNameVisible()
    {
        Vector3 position = Camera.main.WorldToViewportPoint(transform.position);          // 아이템의 위치를 메인 카메라의 viewPort좌표로 변환
        m_goldAmountLabel.transform.position = m_uiCamera.ViewportToWorldPoint(position); // 해당 viewPort좌표를 UI카메라의 world좌표로 변환
        position = m_goldAmountLabel.transform.localPosition;                             // 포지션 위치 재조립
        position.z = 0.0f;                                                                // Z축을 없애고 
        m_goldAmountLabel.transform.localPosition = position;                             // 재조립
        m_goldAmountLabel.transform.localScale = Vector3.one;                             // 스케일이 이상해지므로 1로 변경
    }

    public int getGoldAmount()  // 골드를 먹어버리는 경우에도 
    {
        ObjectPoolManager.Instance.PushToPool("DropGoldLabel", m_goldAmountLabel.gameObject);   // 오브젝트 풀로 Label을 반납해
        m_bShowTextFlag = false;                                                                // 텍스트를 없앤다.
        return m_iGoldAmount;             
    }

    void OnBecameVisible()                 // 카메라에서 보이는 경우 이벤트 함수                                      
    {
        m_bVisibleCamera = true;
    }

    void OnBecameInvisible()               // 카메라에서 보이지 않는 경우 이벤트 함수
    {
        m_bVisibleCamera = false;
    }

    // Unity Documentation : MonoBehaviour.OnBecameVisible() Description
    // OnBecameVisible은 렌더러가 카메라에서 보이는 경우에 호출됩니다.
    // 이 메시지는 렌더러에 첨부된 모든 스크립트에 전달됩니다. 
    // OnBecameInvisible과 OnBecameInvisible은 해당 오브젝트가 보이는 경우에만 필요한 계산을 피하는 데 매우 유용합니다.
    // OnBecameVisible은 함수 안에 간단히 yield 구문을 사용해서 co-routine으로 실행할 수 있습니다. 
    // 에디터상에서 실행하는 경우에, 씬 뷰 카메라 또한 이 함수의 호출을 야기합니다.
}
