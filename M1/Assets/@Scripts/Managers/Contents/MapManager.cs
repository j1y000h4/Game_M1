using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class MapManager 
{
    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    // 물체가 있느냐 없느냐를 판별하기 위한
    Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>();

    private int MinX;
    private int MaxX;
    private int MinY;
    private int MaxY;

    public Vector3Int World2Cell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 Cell2World(Vector3Int cellPos) { return CellGrid.CellToWorld(cellPos); }

    // 벽이 있느냐 없느냐?
    ECellCollisionType[,] _collision;

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.resourceManager.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{MapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();

        ParseCollisionData(map, mapName);

        SpawnObjectsByData(map, mapName);

    }

    public void DestroyMap()
    {
        ClearObjects();

        if (Map != null)
            Managers.resourceManager.Destory(Map);
    }

    // 파일을 읽어서 다시 저장하기
    void ParseCollisionData(GameObject map, string mapName, string tilemap = "Tilemap_Collision")
    {
        GameObject collision = Util.FindChild(map, tilemap, true);
        if (collision != null)
            collision.SetActive(false);

        // Collision 관련 파일
        TextAsset txt = Managers.resourceManager.Load<TextAsset>($"{mapName}Collision");
        StringReader reader = new StringReader(txt.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new ECellCollisionType[xCount, yCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                switch (line[x])
                {
                    case Define.MAP_TOOL_WALL:
                        _collision[x, y] = ECellCollisionType.Wall;
                        break;
                    case Define.MAP_TOOL_NONE:
                        _collision[x, y] = ECellCollisionType.None;
                        break;
                    case Define.MAP_TOOL_SEMI_WALL:
                        _collision[x, y] = ECellCollisionType.SemiWall;
                        break;
                }
            }
        }
    }

    void SpawnObjectsByData(GameObject map, string mapName, string tilemap = "Tilemap_Object")
    {
        Tilemap tm = Util.FindChild<Tilemap>(map, tilemap, true);

        if (tm != null)
            tm.gameObject.SetActive(false);

        for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
        {
            for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                CustomTile tile = tm.GetTile(cellPos) as CustomTile;
                if (tile == null)
                    continue;

                if (tile.ObjectType == Define.EObjectType.Env)
                {
                    Vector3 worldPos = Cell2World(cellPos);
                    Env env = Managers.objectManager.Spawn<Env>(worldPos, tile.DataTemplateID);
                    //env.SetCellPos(cellPos, true);
                }
                else
                {
                    if (tile.CreatureType == Define.ECreatureType.Monster)
                    {
                        Vector3 worldPos = Cell2World(cellPos);
                        Monster monster = Managers.objectManager.Spawn<Monster>(worldPos, tile.DataTemplateID);
                        //monster.SetCellPos(cellPos, true);
                    }
                    else if (tile.CreatureType == Define.ECreatureType.Npc)
                    {

                    }
                }
            }
        }
    }

    public bool MoveTo(Creature obj, Vector3Int cellPos, bool forceMove = false)
    {
        // CanGo로 갈 수 있는가? 체크
        if (CanGo(cellPos) == false)
        {
            return false;
        }

        // 기존 좌표에 있던 오브젝트를 밀어준다.
        // 단, 처음 MoveTo를 했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음
        RemoveObject(obj);

        // 새 좌표에 오브젝트를 등록
        AddObject(obj,cellPos);

        // 셀 좌표 이동
        obj.SetCellPos(cellPos, forceMove);

        return true;
    }

    #region Helpers

    // cellPos 버전
    // GetObject = 그 위치에 있는 걸 가져오는 기능
    public BaseObject GetObject(Vector3Int cellPos)
    {
        // 없으면 null
        _cells.TryGetValue(cellPos, out BaseObject value);
        return value;
    }

    // worldPos 버전
    public BaseObject GetObject(Vector3 worldPos)
    {
        Vector3Int cellPos = World2Cell(worldPos);
        return GetObject(cellPos);
    }

    public bool RemoveObject(BaseObject obj)
    {
        BaseObject prev = GetObject(obj.CellPos);

        // 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음
        if (prev != obj)
            return false;

        _cells[obj.CellPos] = null;
        return true;
    }

    public bool AddObject(BaseObject obj, Vector3Int cellPos)
    {
        if (CanGo(cellPos) == false)
        {
            Debug.LogWarning($"AddObject Failed");
            return false;
        }

        BaseObject prev = GetObject(cellPos);
        if (prev != null)
        {
            Debug.LogWarning($"AddObject Failed");
            return false;
        }

        _cells[cellPos] = obj;
        return true;
    }

    // 그 위치로 갈 수 있느냐?
    public bool CanGo(Vector3 worldPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
    {
        return CanGo(World2Cell(worldPos), ignoreObjects, ignoreSemiWall);
    }

    public bool CanGo(Vector3Int cellPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
    {
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        if (ignoreObjects == false)
        {
            BaseObject obj = GetObject(cellPos);
            if (obj != null)
                return false;
        }

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        ECellCollisionType type = _collision[x, y];
        if (type == ECellCollisionType.None)
            return true;

        if (ignoreSemiWall && type == ECellCollisionType.SemiWall)
            return true;

        return false;
    }

    public void ClearObjects()
    {
        _cells.Clear();
    }
    #endregion
}
