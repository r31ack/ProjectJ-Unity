using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag and Drop Item")]
public class UIDragDropItemPlus : UIDragDropItem
{
    private Dictionary<string, DefaultItemInfo> m_dicItemInfo = new Dictionary<string, DefaultItemInfo>(); // 아이템 고유 정보를 가진 딕셔너리

    protected override void OnDragDropRelease(GameObject surface)
    {
        if (!cloneOnDrag)
        {
            // Clear the reference to the scroll view since it might be in another scroll view now
            var drags = GetComponentsInChildren<UIDragScrollView>();
            foreach (var d in drags) d.scrollView = null;

            // Re-enable the collider
            if (mButton != null) mButton.isEnabled = true;
            else if (mCollider != null) mCollider.enabled = true;
            else if (mCollider2D != null) mCollider2D.enabled = true;

            // Is there a droppable container?
            UIDragDropContainer container = surface ? NGUITools.FindInParents<UIDragDropContainer>(surface) : null;

            if (container != null && container.transform.childCount == 0)   // 컨테이너의 자식갯수가 0일때만 움직이도록 추가적인 제약
            {
                if (container.name == "ArmorSlot")  // 컨테이너가 방어구 슬롯이면
                {
                    ITEM_TYPE type = GameObject.Find("ItemWindow").GetComponent<ItemManager>().getItemType(mTrans.GetComponent<UIButton>().normalSprite); // 아이템 타입 정보를 받아와서
                    if(type == ITEM_TYPE.ARMOR)     // 타입이 같으면
                        successDrop(container);     // 드롭 성공
                    else                            // 타입이 다르면 
                        mTrans.parent = mParent;    // 드롭 실패
                }       
                else if (container.name == "WeaponSlot")
                {
                    ITEM_TYPE type = GameObject.Find("ItemWindow").GetComponent<ItemManager>().getItemType(mTrans.GetComponent<UIButton>().normalSprite); 
                    if (type == ITEM_TYPE.WEAPON)
                        successDrop(container);
                    else
                        mTrans.parent = mParent;        // 드롭 실패
                }
                else if (container.name == "PotionSlot")
                {
                    ITEM_TYPE type = GameObject.Find("ItemWindow").GetComponent<ItemManager>().getItemType(mTrans.GetComponent<UIButton>().normalSprite); 
                    if (type == ITEM_TYPE.POTION)
                        successDrop(container);
                    else
                        mTrans.parent = mParent;        // 드롭 실패
                }
                else
                    successDrop(container);
            }
            else
            {
                // No valid container under the mouse -- revert the item's parent
                mTrans.parent = mParent;
            }

            // Update the grid and table references
            mParent = mTrans.parent;
            mGrid = NGUITools.FindInParents<UIGrid>(mParent);
            mTable = NGUITools.FindInParents<UITable>(mParent);

            // Re-enable the drag scroll view script
            if (mDragScrollView != null) Invoke("EnableDragScrollView", 0.001f);

            // Notify the widgets that the parent has changed
            NGUITools.MarkParentAsChanged(gameObject);

            if (mTable != null) mTable.repositionNow = true;
            if (mGrid != null) mGrid.repositionNow = true;
        }

        // We're now done
        OnDragDropEnd(surface);

        if (cloneOnDrag) DestroySelf();
    }

    void successDrop(UIDragDropContainer container)  // 컨테이너가 존재하는 경우 매개변수로 컨테이너를 받아와 Drop을 성공시킨다.
    {
        mTrans.parent = (container.reparentTarget != null) ? container.reparentTarget : container.transform;  // 컨테이너의 타겟이 있으면 타겟으로, 없으면 컨테이너 트랜스폼으로 드롭 아이템의 부모를 설정
        Vector3 pos = mTrans.localPosition;
        pos.z = 0f;
        mTrans.localPosition = pos;


        if (container.tag == "shopItemSlot")    // 콘테이너가 상점 아이템슬롯이면
        {
            int saleGoldCount = GameObject.Find("ItemWindow").GetComponent<ItemManager>().getBuyGold(mTrans.GetComponent<UIButton>().normalSprite);   // 아이템 구매 가격을 받아와 플레이어 골드 상승
            CharacterInfoManager.instance.m_characterInfo.m_iGold += saleGoldCount;                                                             // 캐릭터 정보에 골드량을 증가시킨다.
            GameObject.Find("ProfileUI").GetComponent<ProfileUIManager>().changeGold();                                                               // UI에 골드량을 바꾼다.
            Destroy(mTrans.gameObject);                                                                                                               // 아이템 삭제
        }
    }
}











