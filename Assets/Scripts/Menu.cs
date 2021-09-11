using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public UnityEngine.UI.Button btnClose;
    public UnityEngine.UI.Button btnSure;
    public UnityEngine.UI.InputField inputTxt;
    public UnityEngine.UI.Text pointTxt;
    public Grid grid;
    private int x;
    private int y;
    // Start is called before the first frame update

    public void Init(Grid grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

    }
    void Start()
    {
        pointTxt.text = string.Format("{0},{1}", x, y);
        if (grid.monsterCeil != 0)
        {
            inputTxt.text = grid.monsterCeil.ToString();
        }
        btnClose.onClick.AddListener(() =>
        {
            Destroy(gameObject);
        });

        btnSure.onClick.AddListener(() =>
        {
            Save();
            Destroy(gameObject);
        });
    }

    void Save()
    {
        if (inputTxt.text != "" && inputTxt.text != "0")
        {
            var ceil = int.Parse(inputTxt.text);
            grid.SetImgMonsterPos(ceil);
            EditorMgr.Instance().ShowTips(string.Format("…Ë÷√π÷µ„:{0}", inputTxt.text));
        }
        else
        {
            grid.SetImgMonsterPos(0);
        }
    }
}
