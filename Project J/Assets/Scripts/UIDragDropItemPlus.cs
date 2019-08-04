using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag and Drop Item")]
public class UIDragDropItemPlus : UIDragDropItem
{
    private Dictionary<string, ItemInfo> m_dicItemInfo = new Dictionary<string, ItemInfo>(); // 아이템 고유 정보를 가진 딕셔너리

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

            if (container != null && container.transform.childCount == 0) // 컨테이너의 자식갯수가 0일때만 움직이도록 추가적인 제약
            {
                // Container found -- parent this object to the container
                mTrans.parent = (container.reparentTarget != null) ? container.reparentTarget : container.transform;

                Vector3 pos = mTrans.localPosition;
                pos.z = 0f;
                mTrans.localPosition = pos;

                if (container.tag == "shopItemSlot")    // 콘테이너가 상점 아이템슬롯이면
                {
                    mTrans.GetComponent<ItemManager>().getBuyGold(mTrans.GetComponent<UIButton>().normalSprite);   // 아이템 구매 가격을 받아와 플레이어 골드 상승
                    Destroy(mTrans.gameObject);         // 아이템 삭제
                }
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
}
