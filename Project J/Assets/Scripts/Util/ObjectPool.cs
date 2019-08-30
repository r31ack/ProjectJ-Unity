using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]                                               // 인스펙터 뷰에 적용시키기위한 명령어
public class ObjectPool
{
    public string poolObjectName;                                   // 풀 오브젝트 이름
    public GameObject prefab;                                       // 프리팹 
    public int poolCount;                                           // 풀 오브젝트 최대 갯수
    public Transform parentTransform = null;                        // 오브젝트를 담아둘 부모 트랜스폼
    public Queue<GameObject> m_queuePool = new Queue<GameObject>();  // 풀 오브젝트 저장소
  
    public void init(Transform parent = null)
    {
        for (int i=0; i<poolCount; i++)
        {
            m_queuePool.Enqueue(createObject());   // 풀 갯수만큼 세팅            
        }
    }

    public void push(GameObject poolObject)  // 사용한 객체를 다시 오브젝트 풀에 반환
    {
        poolObject.transform.SetParent(parentTransform);        // 부모 세팅
        poolObject.SetActive(false);                            // 활성화 끄기
        m_queuePool.Enqueue(poolObject);                           // 오브젝트 풀에 삽입
    }

    public GameObject pop()              // 객체가 필요할 때 오브젝트 풀에 요청
    {
        if (m_queuePool.Count == 0)                               // 갯수가 0이면
            m_queuePool.Enqueue(createObject());                // 재할당
        
        GameObject poolObject = m_queuePool.Dequeue();         // 끝에있는 오브젝트 풀을 반환한다
        return poolObject;                                      
    }

    private GameObject createObject()           // prefab 변수에 지정된 게임 오브젝트를 생성
    {
        GameObject poolObject = Object.Instantiate(prefab) as GameObject;   // 프리팹으로부터 오브젝트 생성
        poolObject.name = poolObjectName;                                   // 이름 세팅
        poolObject.transform.SetParent(parentTransform);                    // 부모 세팅
        poolObject.SetActive(false);                                        // 활성화 상태 초기화
        return poolObject;                                                  // 오브젝트 반환
    }
}
