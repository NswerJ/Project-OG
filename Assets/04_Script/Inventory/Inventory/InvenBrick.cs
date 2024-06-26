using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public enum ItemType
{
    Weapon,
    Generator,
    Connector
}

public class InvenBrick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    public GameObject origin;
    [field: SerializeField] public InventoryObjectData InvenObject { get; private set; }
    public Vector2 InvenPoint { get; set; }

    public Vector3 prevPos;

    protected WeaponInventory inventory;
    protected InventoryActive inventoryActive;

    public ItemType Type = ItemType.Weapon;

    protected bool isDrag;
    public bool IsDrag => isDrag;

    protected RectTransform rectTransform;
    public RectTransform RectTransform => rectTransform;
    protected Image image;
    public Image Image => image;

    protected InventorySize invensize;


    protected virtual void Awake()
    {
        image = GetComponent<Image>();
        InvenObject = Instantiate(InvenObject);
        InvenObject.Init(transform);
        inventory = FindObjectOfType<WeaponInventory>();
        rectTransform = GetComponent<RectTransform>();
        inventoryActive = FindObjectOfType<InventoryActive>();
        invensize = FindObjectOfType<InventorySize>();
    }

    public virtual void Settings()
    {

    }

    public void Setting()
    {
        Settings();
    }

    protected void Update()
    {

        if (isDrag)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rectTransform.position = new Vector3(rectTransform.position.x, rectTransform.position.y, 0);
            if (!inventoryActive.IsOn)
            {
                SetPos();
            }
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (inventoryActive.IsOn && isDrag)
        {
            PlaySceneEffectSound.Instance.PlayDragAndDrop();
            SetPos();
        }
    }

    protected void SetPos()
    {
        isDrag = false;
        ItemExplain.Instance.isDrag = false;

        Vector3 tempPos = rectTransform.localPosition;
        tempPos.x += (rectTransform.rect.width / 100 % 2 == 0) ? 50 : 0;
        tempPos.y += (rectTransform.rect.height / 100 % 2 == 0) ? 50 : 0;

        if (GameManager.Instance.Inventory.StartWidth % 2 == 0)
            tempPos.x -= 50;
        if (GameManager.Instance.Inventory.StartHeight % 2 == 0)
            tempPos.y -= 50;

        //드래그앤 드랍 여기 건들여야 함
        Vector3Int p = Vector3Int.RoundToInt(tempPos / 100);
        p.x -= (int)(rectTransform.rect.width / 200);
        p.y -= (int)(rectTransform.rect.height / 200);
        p.z = 0;

        Vector2Int p2 = Vector2Int.RoundToInt(tempPos / 100);
        p2.x -= (int)(rectTransform.rect.width / 200);
        p2.y -= (int)(rectTransform.rect.height / 200);

        var point = inventory.FindInvenPoint(p2);

        if (point == null)
        {
            GameObject obj = Instantiate(origin, GameManager.Instance.player.position, Quaternion.identity);
            Destroy(gameObject);
            return;
             
        }


        if (inventory.CheckFills(InvenObject.bricks, point.Value))
        {
            inventory.AddItem(InvenObject, point.Value);
            InvenPoint = point.Value;

            rectTransform.localPosition = p * 100;

            rectTransform.localPosition += new Vector3((rectTransform.rect.width - 100) / 2, (rectTransform.rect.height - 100) / 2);

            if (GameManager.Instance.Inventory.StartWidth % 2 == 0)
                rectTransform.localPosition += new Vector3(50, 0);
            if (GameManager.Instance.Inventory.StartHeight % 2 == 0)
                rectTransform.localPosition += new Vector3(0, 50);

            Setting();
            ShowExplain();
        }
        else
        {
            Vector3 vPrevPos = prevPos;
            if (GameManager.Instance.Inventory.StartWidth % 2 == 0)
                vPrevPos -= new Vector3(50, 0);
            if (GameManager.Instance.Inventory.StartHeight % 2 == 0)
                vPrevPos -= new Vector3(0, 50);


            var prev = inventory.FindInvenPoint(Vector2Int.RoundToInt((vPrevPos - new Vector3Int
                ((int)rectTransform.rect.width - 100,
                ((int)rectTransform.rect.height) - 100) / 2) / 100));

            inventory.AddItem(InvenObject, prev.Value);
            InvenPoint = prev.Value;

            transform.localPosition = prevPos;

            Setting();
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        StopCoroutine("CheckMouse");
        ItemExplain.Instance.HoverEnd();


        prevPos = transform.localPosition;

        isDrag = true;
        StartCoroutine("IDoShake");
        ItemExplain.Instance.isDrag = true;
        inventory.RemoveItem(InvenObject, InvenObject.originPos);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine("CheckMouse");

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopCoroutine("CheckMouse");
        ItemExplain.Instance.HoverEnd();
    }

    IEnumerator CheckMouse()
    {
        transform.SetAsLastSibling();
        float x = (int)rectTransform.rect.width / 100;// * invensize.ratio;
        float y = (int)rectTransform.rect.height / 100;// * invensize.ratio;
        float len = GameManager.Instance.Inventory.tileRength;// * invensize.ratio;
        if (len == 0)
            Debug.LogError("len 0 : 무한루프");
        while (true)
        {
            bool isOpen = false;
            Vector2Int invenPos = new Vector2Int(-1, -1);
            Vector2 pos = rectTransform.position;// * invensize.ratio;
            pos -= new Vector2(x * len / 2, y * len / 2);
            Vector2 curPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            while (curPos.x > pos.x)
            {
                pos.x += len;
                invenPos.x++;
            }
            while (curPos.y > pos.y)
            {
                pos.y += len;
                invenPos.y++;
            }
            foreach (var v in InvenObject.bricks)
            {
                if (v.point == invenPos)
                    isOpen = true;
            }
            if (!ItemExplain.Instance.isDrag && isOpen)
            {
                ShowExplain();
            }
            else
            {
                ItemExplain.Instance.HoverEnd();
            }
            yield return null;
        }

    }

    IEnumerator IDoShake()
    {
        float rotation = 4f;
        float rotationTime = 0.08f;
        WaitForSeconds wfs = new WaitForSeconds(rotationTime);

        transform.DOScale(new Vector2(0.85f, 0.85f), 0.1f);
        while (isDrag)
        {
            transform.DORotate(new Vector3(0, 0, rotation), rotationTime);
            yield return wfs;
            transform.DORotate(new Vector3(0, 0, -rotation), rotationTime);
            yield return wfs;
        }
        transform.DORotate(new Vector3(0, 0, 0f), 0.01f);
        transform.DOScale(new Vector3(1, 1, 1), 0.01f);
        yield return null;
    }

    public virtual void ShowExplain()
    {
        if (Type == ItemType.Generator)
            ItemExplain.Instance.HoverGenerator(image.sprite, WeaponExplainManager.triggerExplain[InvenObject.generatorID].ToString(), WeaponExplainManager.weaponExplain[InvenObject.generatorID]);
    }

    public void OnDestroy()
    {
        foreach(var v in InvenObject.includes)
            Destroy(v);
    }
}