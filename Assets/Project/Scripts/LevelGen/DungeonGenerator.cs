using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// Генератор данжа из комнат (версия 3).
/// Мир = сетка комнат. Путь от старта к выходу + боковые комнаты.
/// Типы комнат: обычная, сокровищница, комната ловушек, зал с уступом.
/// Проходы между этажами: деревянные платформы, лестницы или смесь.
/// Все тайлы и декор необязательны: чего не назначено, то просто не появится.
public class RoomLevelGenerator : MonoBehaviour
{
    public enum RoomType { Normal, Treasure, Trap, Hall, Start, Exit }
    public enum ShaftStyle { Platforms, Ladders, Mixed }

    // =====================================================================
    [Header("1. TILEMAPS (объекты из Hierarchy)")]
    [Tooltip("Стены. Нужен коллайдер.")]
    public Tilemap ground;
    [Tooltip("Деревянные платформы (односторонние). Необязательно.")]
    public Tilemap platforms;
    [Tooltip("Фон комнат. Необязательно.")]
    public Tilemap background;
    [Tooltip("Декор спереди фона, без коллайдера. Необязательно.")]
    public Tilemap decor;
    [Tooltip("Шипы и пилы. Нужен Tilemap Collider 2D (Is Trigger) и скрипт Hazard.")]
    public Tilemap hazards;
    [Tooltip("Лестницы. Нужен Tilemap Collider 2D (Is Trigger) и слой Ladder.")]
    public Tilemap ladders;

    // =====================================================================
    [Header("2. СТЕНЫ (9 частей)")]
    [Tooltip("Dungeon Tile Set_0")] public TileBase wallTL;
    [Tooltip("Dungeon Tile Set_1")] public TileBase wallT;
    [Tooltip("Dungeon Tile Set_2")] public TileBase wallTR;
    [Tooltip("Dungeon Tile Set_11")] public TileBase wallL;
    [Tooltip("Dungeon Tile Set_12")] public TileBase wallC;
    [Tooltip("Dungeon Tile Set_13")] public TileBase wallR;
    [Tooltip("Dungeon Tile Set_22")] public TileBase wallBL;
    [Tooltip("Dungeon Tile Set_23")] public TileBase wallB;
    [Tooltip("Dungeon Tile Set_24")] public TileBase wallBR;
    [Tooltip("Кирпичи для толщи стены: 14, 15, 16, 17, 26, 27, 28")]
    public TileBase[] wallFill;

    // =====================================================================
    [Header("3. ДЕРЕВЯННЫЕ ПЛАТФОРМЫ")]
    [Tooltip("Dungeon Tile Set_69")] public TileBase platformLeft;
    [Tooltip("Dungeon Tile Set_70 (обязателен)")] public TileBase platformMid;
    [Tooltip("Dungeon Tile Set_71")] public TileBase platformRight;

    // =====================================================================
    [Header("4. ЛЕСТНИЦЫ")]
    [Tooltip("Dungeon Tile Set_91. У тайла поставьте Collider Type = Grid")]
    public TileBase ladderTile;
    [Tooltip("Dungeon Tile Set_72 (верхнее звено, необязательно)")]
    public TileBase ladderTopTile;
    public ShaftStyle shaftStyle = ShaftStyle.Mixed;
    [Range(0f, 1f)] public float ladderShaftChance = 0.5f;

    // =====================================================================
    [Header("5. ЛОВУШКИ")]
    [Tooltip("Шипы: 117, 118, 119, 120, 121, 122")]
    public TileBase[] spikeTiles;
    [Tooltip("Dungeon Tile Set_130")] public TileBase sawFloorL;
    [Tooltip("Dungeon Tile Set_131")] public TileBase sawFloorM;
    [Tooltip("Dungeon Tile Set_132")] public TileBase sawFloorR;
    [Tooltip("Dungeon Tile Set_127")] public TileBase sawCeilL;
    [Tooltip("Dungeon Tile Set_128")] public TileBase sawCeilM;
    [Tooltip("Dungeon Tile Set_129")] public TileBase sawCeilR;
    [Range(0f, 1f)] public float sawCeilChance = 0.5f;

    // =====================================================================
    [Header("6. СУНДУКИ")]
    [Tooltip("Dungeon Tile Set_78")] public TileBase chestClosed;
    [Tooltip("Dungeon Tile Set_79")] public TileBase chestOpen;
    [Range(0f, 1f)] public float chestOpenChance = 0.2f;
    [Tooltip("Интерактивные сундуки (pivot = Bottom). Если заданы, используются вместо тайлов.")]
    public GameObject[] chestPrefabs;
    [Range(0f, 1f)] public float chestChanceNormal = 0.12f;
    public Vector2Int chestsInTreasureRoom = new Vector2Int(1, 2);

    // =====================================================================
    [Header("7. ДЕКОР (тайлы)")]
    [Tooltip("Стоят на полу: 86, 107, 108, 109, 84, 85, 111, 112, 113")]
    public TileBase[] floorDecor;
    [Tooltip("Заборы: 92, 93, 94, 95")]
    public TileBase[] fenceTiles;
    [Tooltip("Висят с потолка: 98, 99, 100")]
    public TileBase[] chainTiles;
    [Tooltip("Dungeon Tile Set_101")] public TileBase garlandL;
    [Tooltip("Dungeon Tile Set_102")] public TileBase garlandM;
    [Tooltip("Dungeon Tile Set_103")] public TileBase garlandR;
    [Tooltip("Dungeon Tile Set_105")] public TileBase cageTop;
    [Tooltip("Dungeon Tile Set_115")] public TileBase cageBottom;
    [Tooltip("Dungeon Tile Set_123")] public TileBase cobwebLeft;
    [Tooltip("Dungeon Tile Set_126")] public TileBase cobwebRight;
    [Tooltip("Dungeon Tile Set_29")] public TileBase windowTL;
    [Tooltip("Dungeon Tile Set_30")] public TileBase windowTR;
    [Tooltip("Dungeon Tile Set_35")] public TileBase windowBL;
    [Tooltip("Dungeon Tile Set_36")] public TileBase windowBR;
    [Tooltip("Dungeon Tile Set_7")] public TileBase eyesTL;
    [Tooltip("Dungeon Tile Set_8")] public TileBase eyesTR;
    [Tooltip("Dungeon Tile Set_18")] public TileBase eyesBL;
    [Tooltip("Dungeon Tile Set_19")] public TileBase eyesBR;
    [Tooltip("Dungeon Tile Set_88")] public TileBase doorTop;
    [Tooltip("Dungeon Tile Set_97")] public TileBase doorBottom;
    public Vector2Int decorPerRoom = new Vector2Int(2, 5);
    public Vector2Int chainsPerRoom = new Vector2Int(0, 3);
    [Range(0f, 1f)] public float fenceChance = 0.25f;
    [Range(0f, 1f)] public float garlandChance = 0.25f;
    [Range(0f, 1f)] public float cageChance = 0.12f;
    [Range(0f, 1f)] public float cobwebChance = 0.4f;
    [Range(0f, 1f)] public float windowChance = 0.5f;
    [Range(0f, 1f)] public float eyesChance = 0.12f;

    // =====================================================================
    [Header("8. ФОН")]
    [Tooltip("Тёмные кирпичи: 20, 21, 31, 32, 33, 34, 46, 47, 55, 56, 57, 65, 66, 67")]
    public TileBase[] backgroundTiles;

    // =====================================================================
    [Header("9. СЕТКА КОМНАТ")]
    public int gridCols = 5;
    public int gridRows = 4;
    public int roomW = 24;
    public int roomH = 16;
    public int wall = 2;

    [Header("10. ПРОХОДЫ")]
    public int doorHeight = 4;
    public int shaftWidth = 4;
    [Tooltip("Перепад высоты между платформами в шахте. Подберите под свой прыжок.")]
    public int climbStep = 3;
    [Range(0f, 1f)] public float sideRoomChance = 0.5f;

    [Header("11. ТИПЫ КОМНАТ (веса)")]
    public float weightNormal = 5f;
    public float weightTrap = 2f;
    public float weightHall = 2f;
    public float weightTreasure = 1.5f;
    [Range(0f, 1f)] public float sideRoomTreasureChance = 0.5f;

    [Header("12. НАПОЛНЕНИЕ КОМНАТ")]
    public Vector2Int platformsPerRoom = new Vector2Int(1, 3);
    public Vector2Int platformLength = new Vector2Int(3, 5);
    public Vector2Int bumpsPerRoom = new Vector2Int(0, 3);
    public Vector2Int ceilingChunksPerRoom = new Vector2Int(0, 2);

    [Header("13. ПРЕФАБЫ (необязательно)")]
    [Tooltip("Готовые комнаты: префабы с Tilemap внутри")]
    public GameObject[] roomPrefabs;
    [Range(0f, 1f)] public float prefabChance = 0.3f;
    [Tooltip("Стоят на полу, pivot = Bottom")] public GameObject[] floorProps;
    [Tooltip("Висят с потолка, pivot = Top")] public GameObject[] ceilingProps;
    [Tooltip("На стене 2x2, pivot = Center")] public GameObject[] wallProps;
    public Vector2Int propsPerRoom = new Vector2Int(0, 3);
    [Tooltip("Враги (pivot = Bottom)")] public GameObject[] enemyPrefabs;
    public Vector2Int enemiesPerRoom = new Vector2Int(0, 2);

    [Header("14. ИГРОК И ВЫХОД")]
    public Transform player;
    [Tooltip("pivot = Bottom, ставится на пол комнаты выхода")]
    public Transform exitMarker;

    [Header("15. SEED")]
    public bool randomSeed = true;
    public int seed;
    public bool generateOnStart = true;

    // =====================================================================
    const byte AIR = 0, WALL = 1, PLAT = 2, CUSTOM = 3;

    class RoomInfo
    {
        public int c, r, ox, oy, floorY, ceilY, shaftDown, shaftUp;
        public RoomType type;
        public bool prefab;
    }

    System.Random rng;
    int W, H;
    byte[,] kind;
    TileBase[,] tile;
    bool[,] inside, used;
    TileBase[,] decorT, hazardT, ladderT;
    bool[,] open, linkRight, linkDown, onPath;
    int[,] shaftX;
    RoomInfo[,] rooms;
    List<RoomInfo> roomList;
    List<Vector3Int> chestCells;
    Vector2Int startCell, exitCell;
    Transform spawned;

    // ---------- Доступность возможностей ----------
    bool LaddersOn { get { return ladders != null && ladderTile != null; } }
    bool SpikesOn { get { return hazards != null && Has(spikeTiles); } }
    bool SawFloorOn { get { return hazards != null && sawFloorL != null && sawFloorM != null && sawFloorR != null; } }
    bool SawCeilOn { get { return hazards != null && sawCeilL != null && sawCeilM != null && sawCeilR != null; } }
    bool TrapsOn { get { return SpikesOn || SawFloorOn || SawCeilOn; } }
    bool ChestsOn { get { return Has(chestPrefabs) || (decor != null && chestClosed != null); } }

    static bool Has<T>(T[] a) { return a != null && a.Length > 0; }
    T Pick<T>(T[] a) { return a[rng.Next(a.Length)]; }

    void Start()
    {
        if (generateOnStart) Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (!Validate()) return;

        if (randomSeed) seed = Random.Range(int.MinValue, int.MaxValue);
        rng = new System.Random(seed);

        W = gridCols * roomW;
        H = gridRows * roomH;
        kind = new byte[W, H];
        tile = new TileBase[W, H];
        inside = new bool[W, H];
        used = new bool[W, H];
        decorT = new TileBase[W, H];
        hazardT = new TileBase[W, H];
        ladderT = new TileBase[W, H];
        chestCells = new List<Vector3Int>();

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                kind[x, y] = WALL;

        BuildLayout();
        BuildRoomInfos();

        foreach (var rm in roomList) BuildRoom(rm);
        for (int r = 0; r < gridRows; r++)
            for (int c = 0; c < gridCols; c++)
                BuildLinks(c, r);

        foreach (var rm in roomList)
            if (!rm.prefab) PlaceDecor(rm);

        Apply();

        ResetSpawned();
        SpawnObjects();
        PlaceMarkers();

        int treasure = 0, trap = 0, hall = 0;
        foreach (var rm in roomList)
        {
            if (rm.type == RoomType.Treasure) treasure++;
            else if (rm.type == RoomType.Trap) trap++;
            else if (rm.type == RoomType.Hall) hall++;
        }
        Debug.Log("Уровень сгенерирован. Seed: " + seed + ". Комнат: " + roomList.Count +
                  " (сокровищниц: " + treasure + ", ловушек: " + trap + ", залов: " + hall + ")");
    }

    // ---------- Проверки ----------

    bool Validate()
    {
        if (ground == null) { Debug.LogError("RoomLevelGenerator: не назначен Ground (Tilemap)."); return false; }
        if (wallC == null && !Has(wallFill)) { Debug.LogError("RoomLevelGenerator: назначьте хотя бы Wall C или Wall Fill."); return false; }
        if (platformMid == null) { Debug.LogError("RoomLevelGenerator: не назначен Platform Mid."); return false; }
        if (roomW < 2 * wall + shaftWidth + 10) { Debug.LogError("RoomLevelGenerator: roomW слишком мал для таких стен/шахты."); return false; }
        if (roomH < 2 * wall + 8) { Debug.LogError("RoomLevelGenerator: roomH слишком мал (минимум 2*wall + 8)."); return false; }
        if (gridCols < 1 || gridRows < 1) { Debug.LogError("RoomLevelGenerator: размер сетки должен быть >= 1."); return false; }

        string off = "";
        if (decor == null) off += " декор-тайлы (нет Decor Tilemap);";
        if (!TrapsOn) off += " ловушки (нет Hazards Tilemap или тайлов);";
        if (!LaddersOn) off += " лестницы (нет Ladders Tilemap или Ladder Tile), шахты будут на платформах;";
        if (!ChestsOn) off += " сундуки;";
        if (off != "") Debug.LogWarning("RoomLevelGenerator: отключено:" + off);
        return true;
    }

    // ---------- Планировка комнат ----------

    void BuildLayout()
    {
        open = new bool[gridCols, gridRows];
        onPath = new bool[gridCols, gridRows];
        linkRight = new bool[gridCols, gridRows];
        linkDown = new bool[gridCols, gridRows];
        shaftX = new int[gridCols, gridRows];

        int col = rng.Next(gridCols);
        int row = 0;
        open[col, row] = true;
        onPath[col, row] = true;
        startCell = new Vector2Int(col, row);

        while (true)
        {
            int dir = rng.Next(5); // 0,1 = влево; 2,3 = вправо; 4 = вниз
            if (dir == 4)
            {
                if (row == gridRows - 1) break;
                Link(col, row, col, row + 1);
                row++;
            }
            else
            {
                int nc = col + (dir < 2 ? -1 : 1);
                bool blocked = nc < 0 || nc >= gridCols || open[nc, row];
                if (blocked)
                {
                    if (row == gridRows - 1) continue;
                    Link(col, row, col, row + 1);
                    row++;
                }
                else
                {
                    Link(col, row, nc, row);
                    col = nc;
                }
            }
            open[col, row] = true;
            onPath[col, row] = true;
        }
        exitCell = new Vector2Int(col, row);

        var neighbours = new List<Vector2Int>();
        for (int r = 0; r < gridRows; r++)
            for (int c = 0; c < gridCols; c++)
            {
                if (open[c, r] || rng.NextDouble() >= sideRoomChance) continue;

                neighbours.Clear();
                if (c > 0 && open[c - 1, r]) neighbours.Add(new Vector2Int(c - 1, r));
                if (c < gridCols - 1 && open[c + 1, r]) neighbours.Add(new Vector2Int(c + 1, r));
                if (r > 0 && open[c, r - 1]) neighbours.Add(new Vector2Int(c, r - 1));
                if (r < gridRows - 1 && open[c, r + 1]) neighbours.Add(new Vector2Int(c, r + 1));
                if (neighbours.Count == 0) continue;

                var n = neighbours[rng.Next(neighbours.Count)];
                Link(c, r, n.x, n.y);
                open[c, r] = true;
            }
    }

    void Link(int c1, int r1, int c2, int r2)
    {
        if (r1 != r2)
        {
            int r = Mathf.Min(r1, r2);
            if (!linkDown[c1, r])
            {
                linkDown[c1, r] = true;
                shaftX[c1, r] = rng.Next(wall + 5, roomW - wall - shaftWidth - 4);
            }
        }
        else
        {
            linkRight[Mathf.Min(c1, c2), r1] = true;
        }
    }

    void BuildRoomInfos()
    {
        rooms = new RoomInfo[gridCols, gridRows];
        roomList = new List<RoomInfo>();
        for (int r = 0; r < gridRows; r++)
            for (int c = 0; c < gridCols; c++)
            {
                if (!open[c, r]) continue;
                var rm = new RoomInfo();
                rm.c = c;
                rm.r = r;
                rm.ox = c * roomW;
                rm.oy = (gridRows - 1 - r) * roomH;
                rm.floorY = rm.oy + wall;
                rm.ceilY = rm.oy + roomH - wall - 1;
                rm.shaftDown = linkDown[c, r] ? rm.ox + shaftX[c, r] : -1000;
                rm.shaftUp = (r > 0 && linkDown[c, r - 1]) ? rm.ox + shaftX[c, r - 1] : -1000;

                if (c == startCell.x && r == startCell.y) rm.type = RoomType.Start;
                else if (c == exitCell.x && r == exitCell.y) rm.type = RoomType.Exit;
                else rm.type = RollType(!onPath[c, r]);

                rooms[c, r] = rm;
                roomList.Add(rm);
            }
    }

    RoomType RollType(bool side)
    {
        float wTreasure = ChestsOn ? weightTreasure : 0f;
        float wTrap = TrapsOn ? weightTrap : 0f;

        if (side && ChestsOn && rng.NextDouble() < sideRoomTreasureChance) return RoomType.Treasure;

        float total = weightNormal + wTrap + weightHall + (side ? 0f : wTreasure);
        if (total <= 0f) return RoomType.Normal;
        float v = (float)rng.NextDouble() * total;
        v -= weightNormal; if (v < 0f) return RoomType.Normal;
        v -= wTrap; if (v < 0f) return RoomType.Trap;
        v -= weightHall; if (v < 0f) return RoomType.Hall;
        return side ? RoomType.Normal : RoomType.Treasure;
    }

    // ---------- Построение комнат ----------

    int MinX(RoomInfo rm) { return rm.ox + wall + 4; }
    int MaxX(RoomInfo rm) { return rm.ox + roomW - wall - 5; }
    bool Clash2(RoomInfo rm, int x0, int x1) { return Clash(x0, x1, rm.shaftDown) || Clash(x0, x1, rm.shaftUp); }

    void BuildRoom(RoomInfo rm)
    {
        for (int x = rm.ox + wall; x < rm.ox + roomW - wall; x++)
            for (int y = rm.oy + wall; y < rm.oy + roomH - wall; y++)
                Clear(x, y);

        bool special = rm.type == RoomType.Start || rm.type == RoomType.Exit;
        if (!special && Has(roomPrefabs) && rng.NextDouble() < prefabChance &&
            StampPrefab(rm.ox + wall, rm.oy + wall, roomW - 2 * wall, roomH - 2 * wall))
        {
            rm.prefab = true;
            return;
        }

        switch (rm.type)
        {
            case RoomType.Treasure: BuildTreasure(rm); break;
            case RoomType.Trap: BuildTrap(rm); break;
            case RoomType.Hall: BuildHall(rm); break;
            case RoomType.Normal: BuildNormal(rm, false); break;
            default: BuildNormal(rm, true); break;   // старт и выход спокойные
        }
    }

    void BuildNormal(RoomInfo rm, bool calm)
    {
        int bumps = calm ? rng.Next(0, 2) : rng.Next(bumpsPerRoom.x, bumpsPerRoom.y + 1);
        for (int i = 0; i < bumps; i++) AddBump(rm);
        if (calm) return;

        int count = rng.Next(platformsPerRoom.x, platformsPerRoom.y + 1);
        for (int i = 0; i < count; i++) AddRandomPlatform(rm);

        int chunks = rng.Next(ceilingChunksPerRoom.x, ceilingChunksPerRoom.y + 1);
        for (int i = 0; i < chunks; i++) AddCeilingChunk(rm);
    }

    void BuildHall(RoomInfo rm)
    {
        AddLedge(rm, rng.NextDouble() < 0.5);
        int p = rng.Next(1, 3);
        for (int i = 0; i < p; i++) AddRandomPlatform(rm);
        int chunks = rng.Next(0, 3);
        for (int i = 0; i < chunks; i++) AddCeilingChunk(rm);
        if (rng.NextDouble() < 0.5) AddBump(rm);
    }

    void BuildTreasure(RoomInfo rm)
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            int lo = MinX(rm) + 1, hi = MaxX(rm) - 1;
            if (hi < lo) break;
            int cx = rng.Next(lo, hi + 1);
            if (Clash2(rm, cx - 1, cx + 1)) continue;

            // Постамент 3 тайла шириной, на нём сундук
            for (int dx = -1; dx <= 1; dx++) Put(cx + dx, rm.floorY, WALL, null);
            PlaceChest(cx, rm.floorY + 1);

            // Шипы перед постаментом
            if (SpikesOn)
            {
                int side = rng.Next(2) == 0 ? -1 : 1;
                int gx = side < 0 ? cx - 4 : cx + 3;
                if (gx >= MinX(rm) && gx + 1 <= MaxX(rm) && !Clash2(rm, gx, gx + 1))
                    AddSpikePatch(rm.floorY, gx, 2);
            }
            break;
        }

        int extra = rng.Next(chestsInTreasureRoom.x, chestsInTreasureRoom.y + 1) - 1;
        for (int i = 0; i < extra; i++) AddLedge(rm, true);

        if (rng.NextDouble() < 0.5) AddRandomPlatform(rm);
    }

    void BuildTrap(RoomInfo rm)
    {
        int x = MinX(rm) + rng.Next(0, 3);
        int segs = 0;
        while (x <= MaxX(rm) && segs < 4)
        {
            if (!SpikesOn && !SawFloorOn) break;

            int type = rng.Next(3);   // 0 = яма с шипами, 1 = шипы на полу, 2 = пила
            int w = rng.Next(2, 4);
            if (type == 2 && !SawFloorOn) type = 1;
            if (type != 2 && !SpikesOn) type = 2;
            if (type == 0 && wall < 2) type = 1;
            if (type == 2) w = 3;

            if (x + w - 1 > MaxX(rm)) break;
            if (Clash2(rm, x, x + w - 1)) { x += w + 2; continue; }

            if (type == 0) AddSpikePit(rm.floorY, x, w);
            else if (type == 1) AddSpikePatch(rm.floorY, x, w);
            else AddFloorSaw(rm.floorY, x);

            // Обходная платформа над ловушкой
            if (rng.NextDouble() < 0.4 && AreaFree(x - 1, x + w, rm.floorY + 3, rm.floorY + 3))
                Platform(x - 1, rm.floorY + 3, w + 2);

            segs++;
            x += w + rng.Next(3, 6);
        }

        if (SawCeilOn && rng.NextDouble() < sawCeilChance) AddCeilingSaw(rm);
        if (rng.NextDouble() < 0.5) AddRandomPlatform(rm);
    }

    // ---------- Элементы комнат ----------

    void AddBump(RoomInfo rm)
    {
        int len = rng.Next(2, 5);
        int h = rng.Next(1, 3);
        int lo = MinX(rm), hi = MaxX(rm) - len + 1;
        if (hi < lo) return;
        int bx = rng.Next(lo, hi + 1);
        if (Clash2(rm, bx, bx + len - 1)) return;
        if (!AreaFree(bx, bx + len - 1, rm.floorY, rm.floorY + h - 1)) return;
        for (int dx = 0; dx < len; dx++)
            for (int dy = 0; dy < h; dy++)
                Put(bx + dx, rm.floorY + dy, WALL, null);
    }

    void AddRandomPlatform(RoomInfo rm)
    {
        int len = rng.Next(platformLength.x, platformLength.y + 1);
        int lo = rm.ox + wall + 2, hi = rm.ox + roomW - wall - 2 - len;
        int lowY = rm.floorY + 4, highY = rm.ceilY - 2;
        if (hi < lo || highY < lowY) return;
        int px = rng.Next(lo, hi + 1);
        int py = rng.Next(lowY, highY + 1);
        if (Clash2(rm, px, px + len - 1)) return;
        if (!AreaFree(px, px + len - 1, py, py)) return;
        Platform(px, py, len);
    }

    void AddCeilingChunk(RoomInfo rm)
    {
        int len = rng.Next(2, 5);
        int h = rng.Next(1, 3);
        int lo = rm.ox + wall + 3, hi = rm.ox + roomW - wall - 3 - len;
        if (hi < lo) return;
        int cx = rng.Next(lo, hi + 1);
        if (Clash2(rm, cx, cx + len - 1)) return;
        int y0 = rm.ceilY - h + 1;
        if (!AreaFree(cx, cx + len - 1, y0, rm.ceilY)) return;
        for (int dx = 0; dx < len; dx++)
            for (int y = y0; y <= rm.ceilY; y++)
                Put(cx + dx, y, WALL, null);
    }

    // Высокий уступ. Подъём по лестнице, а если лестниц нет, то по ступенчатой платформе.
    bool AddLedge(RoomInfo rm, bool withChest)
    {
        int len = rng.Next(5, 8);
        int ledgeY = rm.floorY + 5 + rng.Next(0, 2);
        if (ledgeY > rm.ceilY - 3) ledgeY = rm.floorY + 5;
        if (ledgeY > rm.ceilY - 3) return false;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            int lo = rm.ox + wall + 6, hi = rm.ox + roomW - wall - 6 - len;
            if (hi < lo) return false;
            int px = rng.Next(lo, hi + 1);
            int side = rng.Next(2) == 0 ? -1 : 1;
            int lx = side < 0 ? px - 1 : px + len;
            int a0 = Mathf.Min(px, lx), a1 = Mathf.Max(px + len - 1, lx);
            if (Clash2(rm, a0, a1)) continue;
            if (!AreaFree(px, px + len - 1, ledgeY, ledgeY)) continue;

            if (LaddersOn)
            {
                if (!AreaFree(lx, lx, rm.floorY, ledgeY + 1)) continue;
                Platform(px, ledgeY, len);
                for (int y = rm.floorY; y <= ledgeY + 1; y++) PutLadder(lx, y, y == ledgeY + 1);
            }
            else
            {
                int sx0 = side < 0 ? px - 4 : px + len + 1;
                int sy = ledgeY - 3;
                if (sx0 < rm.ox + wall + 2 || sx0 + 2 > rm.ox + roomW - wall - 3) continue;
                if (!AreaFree(sx0, sx0 + 2, sy, sy)) continue;
                Platform(sx0, sy, 3);
                Platform(px, ledgeY, len);
            }

            if (withChest) PlaceChest(px + len / 2, ledgeY + 1);
            return true;
        }
        return false;
    }

    void AddSpikePatch(int y, int x, int w)
    {
        for (int i = 0; i < w; i++)
        {
            hazardT[x + i, y] = Pick(spikeTiles);
            used[x + i, y] = true;
        }
    }

    void AddSpikePit(int y, int x, int w)
    {
        for (int i = 0; i < w; i++)
        {
            Clear(x + i, y - 1);
            hazardT[x + i, y - 1] = Pick(spikeTiles);
            used[x + i, y - 1] = true;
        }
    }

    void AddFloorSaw(int y, int x)
    {
        hazardT[x, y] = sawFloorL;
        hazardT[x + 1, y] = sawFloorM;
        hazardT[x + 2, y] = sawFloorR;
        used[x, y] = used[x + 1, y] = used[x + 2, y] = true;
    }

    void AddCeilingSaw(RoomInfo rm)
    {
        for (int attempt = 0; attempt < 4; attempt++)
        {
            int lo = MinX(rm), hi = MaxX(rm) - 2;
            if (hi < lo) return;
            int x = rng.Next(lo, hi + 1);
            if (Clash2(rm, x, x + 2)) continue;
            if (!AreaFree(x, x + 2, rm.ceilY, rm.ceilY)) continue;
            hazardT[x, rm.ceilY] = sawCeilL;
            hazardT[x + 1, rm.ceilY] = sawCeilM;
            hazardT[x + 2, rm.ceilY] = sawCeilR;
            used[x, rm.ceilY] = used[x + 1, rm.ceilY] = used[x + 2, rm.ceilY] = true;
            return;
        }
    }

    void PlaceChest(int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H || used[x, y]) return;
        if (K(x, y) != AIR || K(x, y - 1) == AIR) return;

        if (Has(chestPrefabs)) chestCells.Add(new Vector3Int(x, y, 0));
        else if (decor != null && chestClosed != null)
            decorT[x, y] = (chestOpen != null && rng.NextDouble() < chestOpenChance) ? chestOpen : chestClosed;
        else return;

        used[x, y] = true;
    }

    void PutLadder(int x, int y, bool top)
    {
        ladderT[x, y] = (top && ladderTopTile != null) ? ladderTopTile : ladderTile;
        used[x, y] = true;
    }

    // Тайл (0,0) префаба = левый нижний угол внутренней области комнаты
    bool StampPrefab(int ix, int iy, int iw, int ih)
    {
        var prefab = roomPrefabs[rng.Next(roomPrefabs.Length)];
        if (prefab == null) return false;
        var tm = prefab.GetComponentInChildren<Tilemap>();
        if (tm == null) return false;

        foreach (var pos in tm.cellBounds.allPositionsWithin)
        {
            var t = tm.GetTile(pos);
            if (t == null) continue;
            if (pos.x < 0 || pos.y < 0 || pos.x >= iw || pos.y >= ih) continue;
            Put(ix + pos.x, iy + pos.y, CUSTOM, t);
        }
        return true;
    }

    // ---------- Проходы между комнатами ----------

    void BuildLinks(int c, int r)
    {
        int ox = c * roomW;
        int oy = (gridRows - 1 - r) * roomH;

        if (linkRight[c, r])
        {
            for (int x = ox + roomW - wall; x < ox + roomW + wall; x++)
                for (int y = oy + wall; y < oy + wall + doorHeight; y++)
                    Clear(x, y);
        }

        if (!linkDown[c, r]) return;

        int dx = ox + shaftX[c, r];
        for (int x = dx; x < dx + shaftWidth; x++)
            for (int y = oy - wall; y < oy + wall; y++)
                Clear(x, y);

        int floorFree = oy - roomH + wall;   // пол нижней комнаты

        bool ladder = LaddersOn && (shaftStyle == ShaftStyle.Ladders ||
                      (shaftStyle == ShaftStyle.Mixed && rng.NextDouble() < ladderShaftChance));
        if (ladder)
        {
            int lx = dx + shaftWidth / 2;
            int topY = oy + wall;
            for (int y = floorFree; y <= topY; y++) PutLadder(lx, y, y == topY);
            return;
        }

        // Зигзаг из деревянных платформ прямо в шахте
        int topLimit = oy + wall - 2;
        int plen = Mathf.Max(2, shaftWidth / 2);
        int lastTy = floorFree - 1;
        int k = 1;
        while (true)
        {
            int ty = lastTy + climbStep;
            if (ty > topLimit) ty = topLimit;
            if (ty <= lastTy) break;

            int x0 = (k % 2 == 1) ? dx : dx + shaftWidth - plen;
            Platform(x0, ty, plen);

            lastTy = ty;
            k++;
            if (ty == topLimit) break;
        }
    }

    // ---------- Декор (тайлы) ----------

    void PlaceDecor(RoomInfo rm)
    {
        // Сундуки в обычных комнатах и залах
        if ((rm.type == RoomType.Normal || rm.type == RoomType.Hall) &&
            rng.NextDouble() < chestChanceNormal && MaxX(rm) >= MinX(rm))
        {
            int cx = rng.Next(MinX(rm), MaxX(rm) + 1);
            if (!Clash2(rm, cx, cx))
            {
                int cy = FindFloorY(cx, rm.floorY, rm.floorY + 3);
                if (cy >= 0) PlaceChest(cx, cy);
            }
        }

        if (decor == null) return;

        // Двери
        if (rm.type == RoomType.Start) PlaceDoor(rm.ox + wall + 1, rm.floorY);
        if (rm.type == RoomType.Exit) PlaceDoor(rm.ox + roomW - wall - 2, rm.floorY);

        int lo = rm.ox + wall + 3, hi = rm.ox + roomW - wall - 4;   // включительно

        // Напольный декор
        if (Has(floorDecor))
        {
            int n = rng.Next(decorPerRoom.x, decorPerRoom.y + 1) + (rm.type == RoomType.Treasure ? 2 : 0);
            for (int i = 0; i < n; i++)
            {
                int x = rng.Next(lo, hi + 1);
                int y = FindFloorY(x, rm.floorY, rm.floorY + 3);
                if (y < 0) continue;
                decorT[x, y] = Pick(floorDecor);
                used[x, y] = true;
            }
        }

        // Забор
        if (Has(fenceTiles) && rng.NextDouble() < fenceChance)
        {
            int len = rng.Next(2, 5);
            int x0 = rng.Next(lo, hi - len + 2);
            bool ok = true;
            for (int i = 0; i < len; i++)
                if (FindFloorY(x0 + i, rm.floorY, rm.floorY) < 0) { ok = false; break; }
            if (ok)
                for (int i = 0; i < len; i++)
                {
                    decorT[x0 + i, rm.floorY] = Pick(fenceTiles);
                    used[x0 + i, rm.floorY] = true;
                }
        }

        // Цепи с потолка
        if (Has(chainTiles))
        {
            int n = rng.Next(chainsPerRoom.x, chainsPerRoom.y + 1);
            for (int i = 0; i < n; i++)
            {
                int x = rng.Next(lo, hi + 1);
                if (CeilingFree(x, rm.ceilY)) { decorT[x, rm.ceilY] = Pick(chainTiles); used[x, rm.ceilY] = true; }
            }
        }

        // Гирлянда из цепи
        if (garlandL != null && garlandM != null && garlandR != null && rng.NextDouble() < garlandChance)
        {
            int x = rng.Next(lo, hi - 1);
            if (CeilingFree(x, rm.ceilY) && CeilingFree(x + 1, rm.ceilY) && CeilingFree(x + 2, rm.ceilY))
            {
                decorT[x, rm.ceilY] = garlandL; decorT[x + 1, rm.ceilY] = garlandM; decorT[x + 2, rm.ceilY] = garlandR;
                used[x, rm.ceilY] = used[x + 1, rm.ceilY] = used[x + 2, rm.ceilY] = true;
            }
        }

        // Клетка
        if (cageTop != null && cageBottom != null && rng.NextDouble() < cageChance)
        {
            int x = rng.Next(lo, hi + 1);
            if (CeilingFree(x, rm.ceilY) && K(x, rm.ceilY - 1) == AIR && !used[x, rm.ceilY - 1])
            {
                decorT[x, rm.ceilY] = cageTop;
                decorT[x, rm.ceilY - 1] = cageBottom;
                used[x, rm.ceilY] = used[x, rm.ceilY - 1] = true;
            }
        }

        // Паутина в верхних углах
        int lx = rm.ox + wall, rx = rm.ox + roomW - wall - 1;
        if (cobwebLeft != null && rng.NextDouble() < cobwebChance && CeilingFree(lx, rm.ceilY))
        { decorT[lx, rm.ceilY] = cobwebLeft; used[lx, rm.ceilY] = true; }
        if (cobwebRight != null && rng.NextDouble() < cobwebChance && CeilingFree(rx, rm.ceilY))
        { decorT[rx, rm.ceilY] = cobwebRight; used[rx, rm.ceilY] = true; }

        // Окно и глаза в темноте (блоки 2x2)
        if (windowTL != null && windowTR != null && windowBL != null && windowBR != null && rng.NextDouble() < windowChance)
            TryPlace2x2(rm, windowTL, windowTR, windowBL, windowBR);
        if (eyesTL != null && eyesTR != null && eyesBL != null && eyesBR != null && rng.NextDouble() < eyesChance)
            TryPlace2x2(rm, eyesTL, eyesTR, eyesBL, eyesBR);
    }

    bool CeilingFree(int x, int ceilY)
    {
        return K(x, ceilY) == AIR && K(x, ceilY + 1) == WALL && !used[x, ceilY];
    }

    void TryPlace2x2(RoomInfo rm, TileBase tl, TileBase tr, TileBase bl, TileBase br)
    {
        int lo = rm.ox + wall + 3, hi = rm.ox + roomW - wall - 5;
        int lowY = rm.floorY + 3, highY = rm.ceilY - 3;
        if (hi < lo || highY < lowY) return;
        for (int attempt = 0; attempt < 4; attempt++)
        {
            int x = rng.Next(lo, hi + 1);
            int y = rng.Next(lowY, highY + 1);
            if (!AreaFree(x, x + 1, y, y + 1)) continue;
            decorT[x, y + 1] = tl; decorT[x + 1, y + 1] = tr;
            decorT[x, y] = bl; decorT[x + 1, y] = br;
            used[x, y] = used[x + 1, y] = used[x, y + 1] = used[x + 1, y + 1] = true;
            return;
        }
    }

    void PlaceDoor(int x, int floorY)
    {
        if (doorTop == null || doorBottom == null) return;
        decorT[x, floorY] = doorBottom;
        decorT[x, floorY + 1] = doorTop;
        used[x, floorY] = used[x, floorY + 1] = true;
    }

    // Первая свободная клетка воздуха над твёрдым (или платформой) в колонке x
    int FindFloorY(int x, int y0, int y1)
    {
        if (x < 0 || x >= W) return -1;
        for (int y = y0; y <= y1; y++)
        {
            if (y < 0 || y >= H) break;
            if (K(x, y) == AIR && K(x, y - 1) != AIR && !used[x, y]) return y;
        }
        return -1;
    }

    // ---------- Вспомогательное ----------

    bool Clash(int a0, int a1, int shaft)
    {
        if (shaft < 0) return false;
        return a1 >= shaft - 4 && a0 <= shaft + shaftWidth + 3;
    }

    byte K(int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return WALL;
        return kind[x, y];
    }

    bool Open(int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return false;
        return kind[x, y] == AIR || kind[x, y] == PLAT;
    }

    bool AreaFree(int x0, int x1, int y0, int y1)
    {
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
            {
                if (x < 0 || y < 0 || x >= W || y >= H) return false;
                if (kind[x, y] != AIR || used[x, y]) return false;
            }
        return true;
    }

    Vector3 Center(int x, int y) { return ground.GetCellCenterWorld(new Vector3Int(x, y, 0)); }
    Vector3 HalfCell() { return Vector3.Scale(ground.cellSize, ground.transform.lossyScale) * 0.5f; }

    void Platform(int x, int y, int len)
    {
        for (int i = 0; i < len; i++)
        {
            TileBase t = platformMid;
            if (len > 1 && i == 0 && platformLeft != null) t = platformLeft;
            else if (len > 1 && i == len - 1 && platformRight != null) t = platformRight;
            Put(x + i, y, PLAT, t);
        }
    }

    void Clear(int x, int y)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return;
        kind[x, y] = AIR;
        tile[x, y] = null;
        inside[x, y] = true;
    }

    void Put(int x, int y, byte k, TileBase t)
    {
        if (x < 0 || y < 0 || x >= W || y >= H) return;
        kind[x, y] = k;
        tile[x, y] = t;
    }

    // ---------- Запись в Tilemap ----------

    void Apply()
    {
        var gPos = new List<Vector3Int>();
        var gTiles = new List<TileBase>();
        var pPos = new List<Vector3Int>();
        var pTiles = new List<TileBase>();
        bool sep = platforms != null;
        bool hasFill = Has(wallFill);
        TileBase[] slice = { wallTL, wallT, wallTR, wallL, wallC, wallR, wallBL, wallB, wallBR };

        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
            {
                byte k = kind[x, y];
                if (k == AIR) continue;
                var pos = new Vector3Int(x, y, 0);

                if (k == WALL)
                {
                    bool up = Open(x, y + 1), down = Open(x, y - 1), left = Open(x - 1, y), right = Open(x + 1, y);
                    int row = up ? 0 : (down ? 2 : 1);
                    int col = left ? 0 : (right ? 2 : 1);
                    TileBase t = slice[row * 3 + col];
                    if ((row == 1 && col == 1) || t == null)
                        t = hasFill ? wallFill[rng.Next(wallFill.Length)] : wallC;
                    gPos.Add(pos);
                    gTiles.Add(t);
                }
                else if (k == PLAT && sep)
                {
                    pPos.Add(pos);
                    pTiles.Add(tile[x, y]);
                }
                else
                {
                    gPos.Add(pos);
                    gTiles.Add(tile[x, y]);
                }
            }

        ground.ClearAllTiles();
        ground.SetTiles(gPos.ToArray(), gTiles.ToArray());

        if (sep)
        {
            platforms.ClearAllTiles();
            platforms.SetTiles(pPos.ToArray(), pTiles.ToArray());
        }

        if (background != null)
        {
            background.ClearAllTiles();
            if (Has(backgroundTiles))
            {
                var bp = new List<Vector3Int>();
                var bt = new List<TileBase>();
                for (int x = 0; x < W; x++)
                    for (int y = 0; y < H; y++)
                        if (inside[x, y])
                        {
                            bp.Add(new Vector3Int(x, y, 0));
                            bt.Add(backgroundTiles[rng.Next(backgroundTiles.Length)]);
                        }
                background.SetTiles(bp.ToArray(), bt.ToArray());
            }
        }

        if (decor != null) WriteLayer(decor, decorT);
        if (hazards != null) WriteLayer(hazards, hazardT);
        if (ladders != null) WriteLayer(ladders, ladderT);
    }

    void WriteLayer(Tilemap map, TileBase[,] src)
    {
        var pos = new List<Vector3Int>();
        var tiles = new List<TileBase>();
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++)
                if (src[x, y] != null)
                {
                    pos.Add(new Vector3Int(x, y, 0));
                    tiles.Add(src[x, y]);
                }
        map.ClearAllTiles();
        map.SetTiles(pos.ToArray(), tiles.ToArray());
    }

    // ---------- Объекты (префабы) ----------

    void ResetSpawned()
    {
        var old = transform.Find("GeneratedObjects");
        if (old != null)
        {
            if (Application.isPlaying) Destroy(old.gameObject);
            else DestroyImmediate(old.gameObject);
        }
        spawned = new GameObject("GeneratedObjects").transform;
        spawned.SetParent(transform, false);
    }

    void SpawnObjects()
    {
        Vector3 half = HalfCell();

        // Интерактивные сундуки
        if (Has(chestPrefabs))
            foreach (var cell in chestCells)
                Instantiate(Pick(chestPrefabs), Center(cell.x, cell.y) - Vector3.up * half.y, Quaternion.identity, spawned);

        foreach (var rm in roomList)
        {
            if (rm.prefab) continue;
            SpawnProps(rm, half);
            SpawnEnemies(rm, half);
        }
    }

    void SpawnProps(RoomInfo rm, Vector3 half)
    {
        int n = rng.Next(propsPerRoom.x, propsPerRoom.y + 1);
        for (int i = 0; i < n; i++)
        {
            int cat = rng.Next(4);
            cat = cat < 2 ? 0 : cat - 1;   // 0 = пол, 1 = потолок, 2 = стена
            var arr = cat == 0 ? floorProps : (cat == 1 ? ceilingProps : wallProps);
            if (!Has(arr)) continue;
            var prefab = Pick(arr);
            if (prefab == null) continue;

            int x = rng.Next(rm.ox + wall + 3, rm.ox + roomW - wall - 3);
            if (Clash2(rm, x, x)) continue;

            Vector3 pos;
            if (cat == 0)
            {
                int y = FindFloorY(x, rm.floorY, rm.floorY + 3);
                if (y < 0) continue;
                pos = Center(x, y) - Vector3.up * half.y;
            }
            else if (cat == 1)
            {
                int y = rm.ceilY;
                if (!(K(x, y) == AIR && K(x, y + 1) == WALL && K(x, y - 1) == AIR && K(x, y - 2) == AIR)) continue;
                pos = Center(x, y) + Vector3.up * half.y;
            }
            else
            {
                int y = rng.Next(rm.floorY + 4, rm.ceilY - 2);
                if (K(x - 1, y - 1) != AIR || K(x, y - 1) != AIR || K(x - 1, y) != AIR || K(x, y) != AIR) continue;
                pos = Center(x, y) + new Vector3(-half.x, -half.y, 0f);
            }
            Instantiate(prefab, pos, Quaternion.identity, spawned);
        }
    }

    void SpawnEnemies(RoomInfo rm, Vector3 half)
    {
        if (!Has(enemyPrefabs)) return;
        if (rm.type == RoomType.Start || rm.type == RoomType.Exit) return;

        int n = rng.Next(enemiesPerRoom.x, enemiesPerRoom.y + 1);
        for (int i = 0; i < n; i++)
        {
            if (MaxX(rm) < MinX(rm)) return;
            int x = rng.Next(MinX(rm), MaxX(rm) + 1);
            int y = FindFloorY(x, rm.floorY, rm.floorY + 3);
            if (y < 0) continue;
            var prefab = Pick(enemyPrefabs);
            if (prefab == null) continue;
            Instantiate(prefab, Center(x, y) - Vector3.up * half.y, Quaternion.identity, spawned);
        }
    }

    void PlaceMarkers()
    {
        Vector3 half = HalfCell();
        if (player != null)
        {
            var cell = new Vector3Int(startCell.x * roomW + wall + 1,
                                      (gridRows - 1 - startCell.y) * roomH + wall, 0);
            Vector3 pos = ground.GetCellCenterWorld(cell) + Vector3.up * half.y;
            player.position = pos;
            player.SendMessage("SetSpawn", pos, SendMessageOptions.DontRequireReceiver);
        }
        if (exitMarker != null)
        {
            var cell = new Vector3Int(exitCell.x * roomW + roomW - wall - 2,
                                      (gridRows - 1 - exitCell.y) * roomH + wall, 0);
            exitMarker.position = ground.GetCellCenterWorld(cell) - Vector3.up * half.y;
        }
    }
}