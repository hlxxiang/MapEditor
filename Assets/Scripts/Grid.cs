using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Grid : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public int x = 0;
    public int y = 0;
    public int index = 0;
    public GridPermit value = GridPermit.Walk;
    public int monsterCeil = 0;

    private UnityEngine.UI.Text txtPoint;
    private UnityEngine.UI.Button btn;
    private UnityEngine.UI.Image img;
    private UnityEngine.UI.Text ceilTxt;

    public void Init(GridPermit v)
    {
        value = v;
    }

    public void Serializable(float ve)
    {
        value = (GridPermit)ve;
    }

    void Start()
    {
        var obj = GameObject.FindGameObjectWithTag("GridPoint");
        if (obj != null)
        {
            txtPoint = obj.GetComponent<UnityEngine.UI.Text>();
        }
        this.btn = this.GetComponent<UnityEngine.UI.Button>();
        this.img = this.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
        ceilTxt = transform.Find("txt").GetComponent<UnityEngine.UI.Text>();
        SetAlpha(value);
        SetImgMonsterPos(monsterCeil);
    }


    public void SetAlpha(GridPermit v)
    {
        value = v;
        if (value == GridPermit.NotWalk)
        {
            img.color = new Color(1, 0, 0, 0.5f);
            monsterCeil = 0;
            ceilTxt.text = "";
        }
        else if (value == GridPermit.Transparent)
        {
            img.color = new Color(1, 1, 1, 0.5f);
        }
        else if (value == GridPermit.Walk)
        {
            img.color = new Color(0, 0, 0, 0.0f);
        }
    }

    public void SetFindPos(bool s)
    {
        if (s)
        {
            img.color = new Color(0, 0.8f, 0, 0.3f);
        }
        else
        {
            SetAlpha(value);
        }
    }

    public void SetFindPosIndex(int idx)
    {
        if (idx != 0)
        {
            ceilTxt.text = idx.ToString();
        }
        else
        {
            SetImgMonsterPos(monsterCeil);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0) && EditorMgr.Instance().MapMode == MapEdit.Edit)
        {
            SetAlpha(EditorMgr.Instance().GridValue);
        }
        if (txtPoint)
        {
            txtPoint.text = string.Format("坐标: x:{0}, y:{1}", x, y);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (EditorMgr.Instance().MapMode == MapEdit.Edit)
            {
                SetAlpha(EditorMgr.Instance().GridValue);
            }
            else
            {
                EditorMgr.Instance().PushPathPos(index);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (EditorMgr.Instance().MapMode == MapEdit.Edit)
            {
                if (value != GridPermit.NotWalk)
                {
                    GameObject prefab = Resources.Load("Prefabs/Menu") as GameObject;
                    var tf = GameObject.Find("Canvas/Root").transform;
                    GameObject obj = Instantiate(prefab, tf);
                    Menu menu = obj.GetComponent<Menu>();
                    menu.Init(this, x, y);
                }
            }
        }
        if (txtPoint)
        {
            txtPoint.text = string.Format("坐标: x:{0}, y:{1}", x, y);
        }
    }

    public void SetImgMonsterPos(int ceil)
    {
        if (value != GridPermit.NotWalk && EditorMgr.Instance().MapMode == MapEdit.Edit)
        {
            monsterCeil = ceil;
            if (ceil != 0)
            {
                ceilTxt.text = ceil.ToString();
            }
            else
            {
                ceilTxt.text = "";
            }
        }
        else
        {
            monsterCeil = 0;
            ceilTxt.text = "";
        }
    }
}