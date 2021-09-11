using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 高权限可通过低权限
/// </summary>
public enum GridPermit
{
    None = 0,
    Transparent = 1, //透明
    Walk = 2,   //行走权限
    NotWalk = 3, //不可行走
}

public enum MapEdit
{
    Edit = 1,
    FindPath = 2,
}

public class EditorMgr : MonoBehaviour
{
    public UnityEngine.UI.Image contentTexture;
    private RectTransform contentRect;
    public UnityEngine.UI.Text fileName;

    public UnityEngine.UI.InputField inputWidth;
    public UnityEngine.UI.InputField imputHeight;

    public GameObject tipsPanle;
    public UnityEngine.UI.Text tipsTxt;

    public UnityEngine.UI.Button BtnOpen;
    public UnityEngine.UI.Button BtnSave;
    public UnityEngine.UI.Button BtnReset;

    public UnityEngine.UI.Toggle editModeToggle;
    public UnityEngine.UI.Toggle findModeToggle;
    public UnityEngine.UI.Toggle findEightDirToggle;

    public GameObject FindPathPanle;

    private static EditorMgr instacne = null;

    public GridPermit GridValue { get; set; }

    public MapEdit MapMode { get; set; }

    private int ImgWidth { get; set; }
    private int ImgHeight { get; set; }
    private int CellWidth { get; set; }
    private int CellHeight { get; set; }

    private float sizeX = 0;
    private float sizeY = 0;
    private bool findEightDir = true;
    private string mapdataFile = "";
    private List<Grid> grids = new List<Grid>();

    private Queue<int> StartEnd;
    private AStar astar;

    public static EditorMgr Instance()
    {
        return EditorMgr.instacne;
    }

    private void Start()
    {
        EditorMgr.instacne = this;
        GridValue = GridPermit.Walk;
        contentRect = contentTexture.GetComponent<RectTransform>();
        MapMode = MapEdit.Edit;
        StartEnd = new Queue<int>();
        FindPathPanle.SetActive(false);
        astar = new AStar();
        findEightDir = true;
    }

    void Update()
    {
        //Zoom out  
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            var scale = contentRect.localScale;
            if (scale.x > 0.1 && scale.y > 0.1)
            {
                contentRect.localScale = new Vector3(scale.x - 0.1f, scale.y - 0.1f, 1);
            }
        }
        //Zoom in  
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            var scale = contentRect.localScale;
            contentRect.localScale = new Vector3(scale.x + 0.1f, scale.y + 0.1f, 1);
        }
    }

    public float Alpha(GridPermit value)
    {
        if (value == GridPermit.NotWalk)
        {
            return 0.6f;
        }
        else if (value == GridPermit.Transparent)
        {
            return 0.2f;
        }
        return 0;
    }

    public void SetGridValueTransparent()
    {
        GridValue = GridPermit.Transparent;
    }

    public void SetGridValueWalk()
    {
        GridValue = GridPermit.Walk;
    }

    public void SetGridValueNotWalk()
    {
        GridValue = GridPermit.NotWalk;
    }

    public void ResetCeilSize()
    {
        GameObject prefab = Resources.Load("Prefabs/BtnGrid") as GameObject;
        CellWidth = int.Parse(inputWidth.text);
        CellHeight = int.Parse(imputHeight.text);

        sizeX = (float)Math.Ceiling((double)ImgWidth / CellWidth);
        sizeY = (float)Math.Ceiling((double)ImgHeight / CellHeight);
        grids.ForEach((Grid grid) =>
        {
            Destroy(grid.gameObject);
        });
        grids = new List<Grid>();
        int index = 0;
        for (int j = 0; j < sizeY; ++j)
        {
            for (int i = 0; i < sizeX; ++i)
            {
                GameObject obj = Instantiate(prefab, contentTexture.transform);
                Grid grid = obj.GetComponent<Grid>();
                grid.Init(GridValue);
                grid.index = index;
                grid.x = i;
                grid.y = j;
                var tranRect = grid.GetComponent<RectTransform>();
                tranRect.anchoredPosition = new Vector2(i * CellWidth, -j * CellHeight);
                tranRect.sizeDelta = new Vector2(CellWidth, CellHeight);
                index++;
                grids.Add(grid);
            }
        }
    }

    public void OpenFile()
    {
        var dir = PlayerPrefs.GetString("dir");
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "图像文件(*.jpg;*.jpeg;*.gif;*.png;*.bmp)|*.jpg;*.jpeg;*.gif;*.png;*.bmp";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        if (dir != null && dir != "")
        {
            openFileName.initialDir = dir;
        }
        else
        {
            openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        }
        openFileName.title = "窗口标题";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetOFN(openFileName))
        {
            mapdataFile = openFileName.file;
            var pos = mapdataFile.LastIndexOf(".");
            mapdataFile = mapdataFile.Remove(pos) + ".mapdata";
            WWW www = new WWW("file:///" + openFileName.file);
            fileName.text = openFileName.file;
            while (!www.isDone)
            {
            }
            pos = mapdataFile.LastIndexOf("\\");
            dir = mapdataFile.Remove(pos);
            PlayerPrefs.SetString("dir", dir);
            ImgWidth = www.texture.width;
            ImgHeight = www.texture.height;
            contentTexture.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0.5f, 0.5f));
            contentTexture.SetNativeSize();
            if (File.Exists(mapdataFile))
            {
                LoadFileData(false);
            }
            else
            {
                ResetCeilSize();
            }
        }
    }

    public void SaveFile()
    {
        BinaryWriter bw = new BinaryWriter(new FileStream(mapdataFile, FileMode.Create));
        bw.Write((short)ImgWidth);
        bw.Write((short)ImgHeight);
        bw.Write((short)CellWidth);
        bw.Write((short)CellHeight);
        bw.Write((short)sizeX);
        bw.Write((short)sizeY);
        int i = 0;
        grids.ForEach((Grid grid) =>
        {
            var value = 0x000F & (short)grid.value;
            bw.Write((int)value);
            if (grid.monsterCeil != 0)
            {
                i++;
            }
        });
        bw.Write((short)i);
        grids.ForEach((Grid grid) =>
        {
            if (grid.monsterCeil != 0)
            {
                bw.Write((short)grid.index);
                bw.Write((short)grid.monsterCeil);
            }
        });
        bw.Close();
        ShowTips("保存成功");
    }

    public void ShowTips(string tips)
    {
        tipsPanle.SetActive(false);
        tipsTxt.text = tips;
        tipsPanle.SetActive(true);
    }

    private void LoadFileData(bool find)
    {
        FileStream fs = new FileStream(mapdataFile, FileMode.Open);
        long len = (int)fs.Length;
        Buffer buffer = new Buffer(len);
        fs.Read(buffer.Data, 0, (int)len);
        fs.Close();
        fs.Dispose();
        var imgWidth = buffer.ReadInt16();
        var imgHeidht = buffer.ReadInt16();
        CellWidth = buffer.ReadInt16();
        CellHeight = buffer.ReadInt16();
        sizeX = buffer.ReadInt16();
        sizeY = buffer.ReadInt16();
        if (imgWidth == ImgWidth && imgHeidht == ImgHeight && CellWidth > 0 && CellHeight > 0 && sizeX > 0 && sizeY > 0)
        {
            inputWidth.text = CellWidth.ToString();
            imputHeight.text = CellHeight.ToString();
            GameObject prefab = Resources.Load("Prefabs/BtnGrid") as GameObject;
            grids.ForEach((Grid grid) =>
            {
                Destroy(grid.gameObject);
            });
            grids = new List<Grid>();
            int index = 0;
            for (int j = 0; j < sizeY; ++j)
            {
                for (int i = 0; i < sizeX; ++i)
                {
                    var value = buffer.ReadInt32();
                    GameObject obj = Instantiate(prefab, contentTexture.transform);
                    Grid grid = obj.GetComponent<Grid>();
                    grid.index = index;
                    grid.x = i;
                    grid.y = j;
                    var tranRect = grid.GetComponent<RectTransform>();
                    tranRect.anchoredPosition = new Vector2(i * CellWidth, -j * CellHeight);
                    tranRect.sizeDelta = new Vector2(CellWidth, CellHeight);
                    index++;
                    grids.Add(grid);
                    grid.Serializable(value & 0x000F);
                }
            }
            try
            {
                int m = buffer.ReadInt16();
                Debug.Log("怪物点数量" + m);
                for (int i = 0; i < m; ++i)
                {
                    int idx = buffer.ReadInt16();
                    int c = buffer.ReadInt16();
                    grids[idx].monsterCeil = c;
                }
            }
            catch (Exception e)
            {

            }
            if (find)
            {
                buffer.Position = 0;
                astar = new AStar();
                astar.Init(buffer);
            }
        }
        else
        {
            ResetCeilSize();
        }
    }

    public void EditMode()
    {
        if (editModeToggle.isOn)
        {
            MapMode = MapEdit.Edit;
            if (mapdataFile != "")
            {
                FindPathPanle.SetActive(false);
                BtnOpen.enabled = true;
                BtnSave.enabled = true;
                BtnReset.enabled = true;
                LoadFileData(false);
            }
        }
    }

    public void FildPathMode()
    {
        if (findModeToggle.isOn)
        {
            MapMode = MapEdit.FindPath;
            if (mapdataFile != "")
            {
                FindPathPanle.SetActive(true);
                BtnOpen.enabled = false;
                BtnSave.enabled = false;
                BtnReset.enabled = false;
                SaveFile();
                LoadFileData(true);
            }
        }
    }

    public void PushPathPos(int index)
    {
        StartEnd.Enqueue(index);
        grids[index].SetFindPos(true);
        while (StartEnd.Count > 2)
        {
            int idx = StartEnd.Dequeue();
            grids[idx].SetFindPosIndex(0);
            grids[idx].SetFindPos(false);
        }
    }

    public void SetFindDir()
    {
        findEightDir = findEightDirToggle.isOn;
    }

    public void FindPath()
    {
        if (StartEnd.Count >= 2)
        {
            var arr = StartEnd.ToArray();
            var start = grids[arr[0]];
            var end = grids[arr[1]];
            var points = new List<Point>();
            var result = astar.Find(start.x, start.y, end.x, end.y, (int)GridPermit.Walk, findEightDir, points);
            if (result != null)
            {
                for (int i = 0; i < result.Count; ++i)
                {
                    int idx = (int)(result[i].y * sizeX + result[i].x);
                    grids[idx].SetFindPos(true);
                }
            }
            if (points != null && points.Count > 0)
            {
                for (int i = 0; i < points.Count; ++i)
                {
                    int idx = (int)(points[i].y * sizeX + points[i].x);
                    grids[idx].SetFindPosIndex(i);
                }
            }
        }
    }

    public void CleanPath()
    {
        if (MapEdit.FindPath == MapMode)
        {
            grids.ForEach((Grid grid) =>
            {
                grid.SetFindPos(false);
                grid.SetFindPosIndex(0);
            });
            while (StartEnd.Count > 0)
            {
                int idx = StartEnd.Dequeue();
                grids[idx].SetFindPosIndex(0);
                grids[idx].SetFindPos(false);
            }
        }
    }
}