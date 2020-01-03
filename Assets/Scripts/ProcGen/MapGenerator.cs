using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using CielaSpike;

public class MapGenerator : MonoBehaviour
{
    [Header("Platform specific")] public int MaximumThreads;

    //TODO: separate into groundtype and tilecontent
    public enum TileType {Ground, Forest, Loot, Building,
        Road
    }
    //TODO: turn into a struct
    public class Tile
    {
        public int X;
        public int Y;
        public TileType Type;
        public Area Area;
        public bool Examined = false;
        public bool Hidable = false;
        public float ForestChance = 1f;
        public bool NextToRoad;

        public Tile(int x, int y)
        {
            this.X = x;
            this.Y = y;
            Type = TileType.Forest;
        }
    }

    [Header("Generation variables")]
    public int RandomSeed;

    public enum WorldSize { Small, Medium, Big, Giant }
    [Serializable]
    //TODO: included area size and buffer
    public struct MapSize
    {
        public WorldSize WorldSize;
        public int SizeX;
        public int SizeY;
        public int PointOfInterests;
        public int HumanSettlements;
        public int VillagesToGenerate;
    }

    [SerializeField] private List<MapSize> MapSizes = null;
    
    public int SizeX, SizeZ;
    public Area AreaTilePrefab;
    public int AreaSize;
    public int AreaBufferSize;
    public AnimationCurve WeightedBufferChance;
    //ONLY for runtime area gen
    //public int AreasToCreate;
    private int totalAreaSize;
    private int amountOfLoot;
    public List<Area> Areas;
    
    //public Area[,] AreaMap { get; private set; }

        
    private Tile[,] map;
    private List<Tile> movableTiles;
    private List<Tile> immovableTiles = new List<Tile>();
    

    [Header("References")]
    public NavMeshBuilder MeshBuilder; 
    public GameObject[] Npcs;
    public GameObject[] HidableObjects;
    public GameObject[] Forest;
    public GameObject ForestHolder, AreaHolder, TileHolder;
    public GameObject TileArtObject;
    public GameObject RoadTileArtObject;
    public RoadEdgeTile NextToRoadTile;
    public GameObject[] LootObjects;
    public PointOfInterest[] PointOfInterestPrefabs;
    public PointOfInterest[] HumanSettlementPrefab;

    [Header("Point of Interests")]
    public GameObject VillagePrefab;
    public int GoblinsForSalePrVillage = 3;

    private List<Goblin.Class> VillageGoblinClasses = new List<Goblin.Class>()
    {
        Goblin.Class.Scout,Goblin.Class.Meatshield,Goblin.Class.Slave,Goblin.Class.Scout,Goblin.Class.Ambusher
    };
    [Range(0f,1f)]
    public float EquipmentInLootChance = 0f;

    [Header("Character Generation")]
    [Range(0, 20)]
    public int GoblinsToGenerate;
    private int NpcsToGenerate;
    public GameObject DefaultCharacter;
    public PlayerTeam GoblinTeam;
    private int GroupDistance = 8;
    private float startTime;
    private Area center;
    private Area goblinStartArea;
    private Area[] nextToStartArea;
    private bool areaGenDone;
    private bool pathGenDone;
    private bool poiGenDone;
    private bool forestGenDone;
    private bool groundGenDone;

    //to track coroutines
    private int areasExpanding;
    private int roadsBeingBuilt;

    private Action<int, string> progressCallback;
    private int progress;
    private int charFact;
    private int areaFact;
    private int poiFact;
    private int progressPct;
    private int totalProgress;
    private int VillagesToGenerate;
    private int PointOfInterests;
    private int HumanSettlements;

    // Use this for initialization
    public IEnumerator GenerateMap(Action<int,string> callback, Action endCallback)
    {
        Debug.Log("Setting max threads to number of cores: "+ SystemInfo.processorCount);

        MaximumThreads = SystemInfo.processorCount;

        progressCallback = callback;

        startTime = Time.time;

        progress = 0;
        charFact = 25;
        areaFact = 2500;
        poiFact = 1000;
        //could use factors on the two last to make them more important than each tiles
        progressPct = 0;

        callback(progressPct,"");
        
        yield return null;

        totalAreaSize = AreaSize + AreaBufferSize;

        var areasToCreate = (SizeX / totalAreaSize) * (SizeZ / totalAreaSize);
        
        if (areasToCreate < 1)
        {
            Debug.LogError("Map size too small for area gen");
            yield break;
        }

        Debug.Log("Creating "+ areasToCreate + " areas");

        NpcsToGenerate = areasToCreate;

        amountOfLoot = areasToCreate;//noOfAreasX * noOfAreasZ;
        
        totalProgress =  SizeX * SizeZ +
            areasToCreate * areaFact * 2
            + VillagesToGenerate * poiFact
            + PointOfInterests * poiFact
            + amountOfLoot
            + NpcsToGenerate * charFact 
            + GoblinsToGenerate * charFact;

        totalProgress = (int)(totalProgress * 0.83f);

        yield return null;

        //Setting up mesh builder
        MeshBuilder.transform.localScale = new Vector3(SizeX / 8f, 1, SizeZ / 8f);
        //MeshBuilder.m_Size = new Vector3(SizeX, 10, SizeZ);
        //Adjust for border size. Should be 
        MeshBuilder.transform.position = new Vector3(SizeX+20, 0, SizeZ+20 );

        MeshBuilder.gameObject.SetActive(true);


        //Forest gen

            // using a tile array to generate the map before initializing it
        //GENERATING 
        movableTiles = new List<Tile>();
        map = new Tile[SizeX,SizeZ];
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeZ; j++)
            {
                map[i,j] = new Tile(i,j);
                immovableTiles.Add(map[i,j]);
            }
        }
        
        //Instantiating Area Tiles

        Areas = new List<Area>();
        //AreaMap = new Area[noOfAreasX, noOfAreasZ];
        
        //TODO: could simply test for when variables are ready, when they are needed
        StartCoroutine(AreaGenRoutine());

        yield return new WaitUntil(() => areaGenDone);

        PathGenRoutine();

        //Probably need dependency

        StartCoroutine(POIGenRoutine());

        StartCoroutine(LootGenRoutine());

        yield return new WaitUntil(() => pathGenDone && roadsBeingBuilt <= 0);
        StartCoroutine(GroundGenRoutine());

        yield return new WaitUntil(() => poiGenDone && areasExpanding <= 0 );
        StartCoroutine(ForestGenRoutine());
        
        //HACK CHECK FOR AREA ACCESSIBILITY
        //select a middle point 
        //ground points at random a bunch of times and check that they are reachable
        //if not make a forest road

        for (int i = 0; i < NpcsToGenerate; i++)
        {
            //TODO: create a number of spawn positions and check their connectivity.
            CreateEnemyCharacter(Npcs[Random.Range(0, Npcs.Length)], NpcHolder.Instance.transform,goblinStartArea);
            
            progress += charFact;

	        int loc = (progress * 100) / totalProgress;
	        if (loc != progressPct)
	        {
	            progressPct = loc;
	            yield return null;
	            callback(progressPct, "Warming up enemies...");
	        }
        }
        
        //Debug.Log("Finished NPC gen : " + (Time.time - startTime) + " seconds");

        foreach (var ar in Areas)
        {
            ar.SetUpUI();
        }

        List<Goblin> members = new List<Goblin>();
        
        yield return new WaitUntil(() => goblinStartArea!= null);
        var pos = goblinStartArea.transform.position;

        Debug.Log("Initializing Goblin team in "+goblinStartArea);
        
        //Find suitable start position
        GoblinTeam.transform.position = new Vector3(pos.x,0,pos.z);

        goblinStartArea.Visited = true;

        var b = PlayerTeam.TribeBlessing;
        if (b == LegacySystem.Blessing.SoloGoblin)
            GoblinsToGenerate = 1;
        if (b == LegacySystem.Blessing.ExtraGoblin)
            GoblinsToGenerate++;
        if (b == LegacySystem.Blessing.ExtraSlaves)
            GoblinsToGenerate += 2;

        //TODO: check that we are not initializinig in a too small area. could be done with connectivity check
        //TODO: Use create character
        for (int i = 0; i < GoblinsToGenerate; i++)
        {
            pos = pos + Random.insideUnitSphere * GroupDistance;

            Debug.Log("Creating gbolin at "+ pos.x +","+pos.z);

            var next = Instantiate(DefaultCharacter, new Vector3(pos.x, 0, pos.z), DefaultCharacter.transform.rotation,GoblinTeam.transform);

            //next.transform.position = 

            var g = next.GetComponent<Goblin>();

            g.name = NameGenerator.GetName();
            
            members.Add(g);

            g.InArea = goblinStartArea;


            if (b == LegacySystem.Blessing.ExtraSlaves && i >= GoblinsToGenerate-2)
                g.SelectClass(Goblin.Class.Slave);

            progress += charFact;

            int loc = (progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                callback(progressPct, "Giving birth to beatiful goblins...");
            }
        }
        
        //Debug.Log("Finished Goblin gen : " + (Time.time - startTime) + " seconds");

        yield return new WaitUntil((() => forestGenDone && groundGenDone));

        CreateTreeBorder(20);

        yield return null;

        GoblinTeam.Initialize(members);
        
        GameManager.StartGame();

        GoblinUIList.UpdateGoblinList();

        PlayerController.UpdateFog();

        //Debug.Log("Finishing generation in : " + (Time.time -startTime) + " seconds");

        endCallback();

    }


    public void SetSize(WorldSize sz)
    {
        var mpsz = MapSizes.First(s => s.WorldSize == sz);

        SizeX = mpsz.SizeX;
        SizeZ = mpsz.SizeY;

        HumanSettlements = mpsz.HumanSettlements;
        PointOfInterests = mpsz.PointOfInterests;
        VillagesToGenerate = mpsz.VillagesToGenerate;
    }

    private IEnumerator AreaGenRoutine()
    {

        //Debug.Log("Started Area gen : " + (Time.time - startTime) + " seconds");
        
        var noOfAreasX = SizeX / totalAreaSize;
        var noOfAreasZ = SizeZ / totalAreaSize;
        
        var adj = totalAreaSize / 2f;

        for (int i = 0; i < noOfAreasX; i++)
        {
            for (int j = 0; j < noOfAreasZ; j++)
            {

                progress += areaFact;
                int loca = (progress * 100) / totalProgress;
                if (loca != progressPct)
                {
                    progressPct = loca;
                    yield return null;
                    progressCallback(progressPct, "Creating areas");
                }

                //Debug.Log("Weight: " + WeightedBufferChance.Evaluate(Random.Range(-1, 1)));

                var x = i * totalAreaSize + adj + WeightedBufferChance.Evaluate(Random.Range(-1f, 1f)) * AreaBufferSize / 2;
                var z = j * totalAreaSize + adj + WeightedBufferChance.Evaluate(Random.Range(-1f, 1f)) * AreaBufferSize / 2;

                yield return new WaitUntil((() => areasExpanding < MaximumThreads));
                CreateArea(new Vector3(x,0,z));
                
            }
        }
        
        center = Areas[(noOfAreasX * noOfAreasZ) / 2];
        

        while (Areas.Count <= HumanSettlements + PointOfInterests + VillagesToGenerate)
        {
            Debug.LogWarning("Too few areas for desired POIs");
            HumanSettlements--;
            PointOfInterests--;
            VillagesToGenerate--;
        }

        areaGenDone = true;
        //Debug.Log("Finished Area gen : " + (Time.time - startTime) + " seconds");
    }

    private void PathGenRoutine()
    {
        //Create roads TODO: make this less expensive
        foreach (var n in Areas)
        {
            var position = n.transform.position;

            var toConnect = Areas.Where(e => e != n && !e.HasMaximumConnections).OrderBy(ne => (ne.transform.position - position).sqrMagnitude).Take(2).ToList();

            //yield return null;

            //TODO: use threads. maybe just for each area
            foreach (var area in toConnect)
            {
                StartCoroutine(CreateRoad(n, area));
            }
        }

        pathGenDone = true;
        //Debug.Log("Finished path gen : " + (Time.time - startTime) + " seconds");
    }

    private IEnumerator POIGenRoutine()
    {

        PointOfInterest last = null;

        for (int i = 0; i < HumanSettlements; i++)
        {
            progress += poiFact;
            int loc = (progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct, "Building villages...");
            }

            var area = GetRandomArea();

            while (area.PointOfInterest || !area.MovablePositions.Any())
            {
                area = GetRandomArea();
                yield return null;
            }


            var x = i % HumanSettlementPrefab.Length;

            var next = Instantiate(HumanSettlementPrefab[x]); //TODO: generate village name

            //keeping y position
            next.transform.position = new Vector3(area.transform.position.x, next.transform.position.y, area.transform.position.z);

            next.transform.parent = area.transform;

            AddAreaPoi(area, next);

            area.name += next;
            next.InArea = area;

            ////Create characters
            //for (int j = 0; j < next.InitialEnemies; j++)
            //{
            //    GenerateCharacter(next.SpawnEnemies[Random.Range(0, next.SpawnEnemies.Length)], next.InArea, NpcHolder.Instance.transform, true);//Instantiate(next.SpawnEnemies[Random.Range(0,next.SpawnEnemies.Length)],);

            //}

            if (last != null)
            {
                //TODO: handle no path
                var path = Pathfinding.FindPath(last.InArea, next.InArea);

                //Debug.Log("Creating road from " + last.InArea + " to " + next.InArea +"; "+ path.Count);

                for (int j = 0; j + 1 < path.Count; j++)
                {
                    //Debug.Log("road: "+ path[j] + ", " + path[j+1]);
                    StartCoroutine(CreateRoad(path[j], path[j + 1], true));
                }
            }

            last = next;
        }

        //Setting up villages

        for (int i = 0; i < VillagesToGenerate; i++)
        {
            progress += poiFact;
            int loc = (progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct, "Building villages...");
            }


            var area = GetRandomArea();

            int maxTries = 12;
            int tries = 0;

            while ((area.PointOfInterest || area.ContainsRoads) && tries++ < maxTries)
                area = GetRandomArea();

            if (tries >= maxTries)
                Debug.Log("CHUBACUBHA Max tries reached.. . !!REPLACE THIS WITH OTHER METHOD, PAW!!");

            var next = Instantiate(VillagePrefab); //TODO: generate village name

            //keeping y position
            next.transform.position = new Vector3(area.transform.position.x, VillagePrefab.transform.position.y, area.transform.position.z);

            next.transform.parent = area.transform;

            //next.name = "Village " + area.X + "," + area.Z;

            var village = next.GetComponent<PointOfInterest>();

            AddAreaPoi(area, village);
            area.name += ": Warrens";
            village.InArea = area;
            
        }

        //Create goblin start area
        goblinStartArea = Areas.First(a => !a.PointOfInterest && !a.ContainsRoads && !a.Neighbours.Any(n => n.PointOfInterest|| n.ContainsRoads));

        if (!goblinStartArea)
        {
            Debug.LogWarning("not able to find perfect start location");
            goblinStartArea = Areas.First(a => !a.PointOfInterest && !a.ContainsRoads);
        }

        if (!goblinStartArea) goblinStartArea = Areas.First(a => !a.PointOfInterest);

        nextToStartArea = goblinStartArea.Neighbours.ToArray();

        foreach(var a in nextToStartArea)
        {
            a.RoomForDifficulty = 2;
        }

        for (int i = 0; i < PointOfInterests; i++)
        {
            progress += poiFact;
            int loc = (progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct, "Building villages...");
            }

            var area = GetRandomArea();

            int maxTries = 12;
            int tries = 0;

            while ((area.PointOfInterest || area == goblinStartArea || area.ContainsRoads) && tries++ < maxTries)
                area = GetRandomArea();

            if (tries >= maxTries)
                Debug.Log("CHUBACUBHA Max tries reached.. !! REPLACE THIS DOODOD!!");

            var x = i % PointOfInterestPrefabs.Length;

            var next = Instantiate(PointOfInterestPrefabs[x]); //TODO: generate village name

            //keeping y position
            next.transform.position = new Vector3(area.transform.position.x, next.transform.position.y, area.transform.position.z);

            next.transform.parent = area.transform;

            AddAreaPoi(area, next);
            area.name += next;
            next.InArea = area;
        }

        poiGenDone = true;
        //Debug.Log("Finished POI gen : " + (Time.time - startTime) + " seconds");

    }

    private IEnumerator LootGenRoutine()
    {

        //ADDING LOOT
        for (int i = 0; i < amountOfLoot; i++)
        {
            var parentArea = GetRandomArea();

            //TODO: should only be in area 
            Tile tile = GetRandomGroundTile(parentArea);
            if(tile == null) continue;

            tile.Type = TileType.Loot;
            movableTiles.Remove(tile);
            parentArea.MovablePositions.Remove(tile);

            var loot = Instantiate(LootObjects[Random.Range(0, LootObjects.Length)]);

            loot.name = "Lootable";

            loot.transform.position = new Vector3(tile.X, 1, tile.Y);

            loot.transform.parent = parentArea.transform;
            var l = loot.GetComponent<Lootable>();

            parentArea.Lootables.Add(l);
            parentArea.MovablePositions.Remove(tile);

            l.InArea = parentArea;

            if (EquipmentInLootChance > Random.value)
            {
                var randomEquipment = EquipmentGen.GetRandomEquipment();
                randomEquipment.transform.parent = l.transform;
                l.EquipmentLoot.Add(randomEquipment);

                l.ContainsLoot = false;
            }

            int loc = (++progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct, "Hiding goblin treasures...");
            }

        }


        //Debug.Log("Finished Loot gen : " + (Time.time - startTime) + " seconds");

    }

    private IEnumerator GroundGenRoutine()
    {
        //yield return null;
        //Debug.Log("Ground gen started");

        //creating ground tiles
        foreach (var tile in map)
        {
            //TODO: no grass under trees, different if in area
            var obj = tile.Type == TileType.Road ? RoadTileArtObject : TileArtObject;
            GameObject next;
            if (tile.Type != TileType.Road && tile.NextToRoad)
            {
                var roadEdge = Instantiate(NextToRoadTile, TileHolder.transform);

                if (map[tile.X + 1, tile.Y].Type == TileType.Road)
                {
                    roadEdge.ERoad = true;
                }
                if (map[tile.X - 1, tile.Y].Type == TileType.Road)
                {
                    roadEdge.WRoad = true;
                }
                if (map[tile.X, tile.Y + 1].Type == TileType.Road)
                {
                    roadEdge.NRoad = true;
                }
                if (map[tile.X, tile.Y - 1].Type == TileType.Road)
                {
                    roadEdge.SRoad = true;
                }
                roadEdge.SetupSprite();

                next = roadEdge.gameObject;
            }
            else
            {
                next = Instantiate(obj, TileHolder.transform);

            }

            next.name = "Tile " + tile.X + "," + tile.Y;

            //TODO:check 
            next.transform.position = new Vector3(tile.X, 0, tile.Y);

            int loc = (++progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct, "Craeting gorund...");
            }
        }

        groundGenDone = true;
        //Debug.Log("Finished ground tile gen : " + (Time.time - startTime) + " seconds");
    }

    private IEnumerator ForestGenRoutine()
    {
        //yield return null;
        //Debug.Log("Forest gen started");

        //int forestType = Random.Range(0, Forest.Length);

        //var forestChangeChance = 0.75f;

        //var order = immovableTiles.OrderBy((a) => a.X + a.Y);

        //INSTANTIATING MAP
        //y=1 for tree height
        foreach (var tile in immovableTiles)
        {

            //TODO: test this is not problematic?
            if (tile == null|| tile.Type != TileType.Forest)
                continue;

            //if (Random.value > forestChangeChance)
            //{
            //    forestType += Random.value > 0.5f ? -1 : 1;
            //    if (forestType >= Forest.Length) forestType = 0;
            //    if (forestType < 0) forestType = Forest.Length-1;
            //}
            

            if (GetNeightbours(tile,true).Any(n => n.Type != TileType.Forest))
            {
                //TODO: use create forest method
                CreateForest(new Vector3(tile.X, 0, tile.Y), true);

                //GameObject next = 
                //    Instantiate(HidableObjects[Random.Range(0, HidableObjects.Length)], ForestHolder.transform);
                //next.transform.position = new Vector3(tile.X, 1, tile.Y);


                //next.name = "Forest " + tile.X + "," + tile.Y;

            }
            else
            {
                //ignore chance for less thick forests
                if (Random.value < 0.2)
                    continue;

                CreateForest(new Vector3(tile.X, 0, tile.Y));
                tile.Hidable = false;
            }

            //TODO:check 

            int loc = (++progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct, "Creating sneaky hiding locations...");
            }
        }

        forestGenDone = true;
        //Debug.Log("Finished forest gen : " + (Time.time - startTime) + " seconds");

    }

    private void AddAreaPoi(Area area ,PointOfInterest next)
    {
        area.PointOfInterest = next;

        var ctr = GetAreaMidPoint(area);

        //81 tiles of unmovables to prevent forest and 
        int dgr = 5;
        for (int x = ctr.X- dgr; x <= ctr.X+ dgr; x++)
        {
            for (int y = ctr.Y- dgr; y <= ctr.Y+dgr; y++)
            {
                var t = map[x, y];

                immovableTiles.Add(t);
                movableTiles.Remove(t);
                t.Type = TileType.Building;
                area.MovablePositions.Remove(t);
            }
        }
    }


    private void CreateTreeBorder(int thickness)
    {
        for (int i = 1; i <= thickness; i++)
        {
            var corner1 = new Vector3(-i,0,-i);
            var corner2 = new Vector3(-i, 0, SizeZ-1+i);
            var corner3 = new Vector3(SizeX-1+i, 0, SizeZ + i);
            var corner4 = new Vector3(SizeX-1 + i, 0, -i);

            for (Vector3 pos = corner1; pos.z <= corner2.z; pos.z++)
            {
                var ground = Instantiate(TileArtObject, TileHolder.transform);
                ground.name = "Tile " + pos.x + "," + pos.z;
                ground.transform.position = new Vector3(pos.x, 0, pos.z);

                //Ignore chance
                if (Random.value < 0.2)
                    continue;
                CreateForest(pos);
                

            }
            for (Vector3 pos = corner1; pos.x <= corner4.x; pos.x++)
            {
                var ground = Instantiate(TileArtObject, TileHolder.transform);
                ground.name = "Tile " + pos.x + "," + pos.z;
                ground.transform.position = new Vector3(pos.x, 0, pos.z);

                if (Random.value < 0.2)
                    continue;

                CreateForest(pos);
            }
            for (Vector3 pos = corner4; pos.z <= corner3.z; pos.z++)
            {
                var ground = Instantiate(TileArtObject, TileHolder.transform);
                ground.name = "Tile " + pos.x + "," + pos.z;
                ground.transform.position = new Vector3(pos.x, 0, pos.z);

                if (Random.value < 0.2)
                    continue;
                CreateForest(pos);
            }
            for (Vector3 pos = corner2; pos.x <= corner3.x; pos.x++)
            {
                var ground = Instantiate(TileArtObject, TileHolder.transform);
                ground.name = "Tile " + pos.x + "," + pos.z;
                ground.transform.position = new Vector3(pos.x, 0, pos.z);

                if (Random.value < 0.2)
                    continue;
                CreateForest(pos);

            }
        }
    }

    private void CreateForest(Vector3 pos, bool hidable = false)
    {
        float adjustment = 0.2f;


        int forestType = (int)(((float)(pos.x * pos.z) / map.Length) * Forest.Length) + Random.Range(-5, 5);
        if (forestType >= Forest.Length) forestType = 0;
        if (forestType < 0) forestType = Forest.Length - 1;

        var forest = Instantiate(Forest[forestType], ForestHolder.transform);
        
        Transform trans;

        if(hidable)
        {
            var hideObject = Instantiate(HidableObjects[Random.Range(0, HidableObjects.Length)], ForestHolder.transform);

            hideObject.name = "Hidable " + pos.x.ToString("N0") + "," + pos.y.ToString("N0");
            forest.transform.parent = hideObject.transform;
            forest.transform.localPosition = Vector3.zero;

            trans = hideObject.transform;
            
            var parentArea = GetAreaAtPoint(pos);

            if (parentArea)
            {
                parentArea.Hidables.Add(hideObject.GetComponent<Hidable>());

                trans.parent = parentArea.transform;
            }
        }
        else
        {
            trans = forest.transform;
            forest.name = "Forest " + pos.x.ToString("N0") + "," + pos.y.ToString("N0");
        }

        trans.position = pos + new Vector3(Random.Range(-adjustment, adjustment), 0, Random.Range(-adjustment, adjustment));

        trans.localRotation = Quaternion.Euler(0, Random.Range(0, 90), 0);

    }

    private Area CreateArea(Vector3 position, List<Area> neighbour = null)
    {
        //TODO: use holders for these
        var area = Instantiate(AreaTilePrefab, AreaHolder.transform);

        //Debug.Log("Creating area at: " +position);
        
        area.transform.position = position;
        area.Size = AreaSize;
        area.GetComponent<BoxCollider>().size = new Vector3(AreaSize, 0.1f, AreaSize);
        //area.FogOfWarSprite.transform.localScale *= (float)totalAreaSize / (float)AreaSize;

        area.X = (int)position.x;
        area.Y = (int)position.z;

        area.name = "Area (" + area.X + "," + area.Y + ")";

        Areas.Add(area);

        //TODO check this
        var centerTile = GetAreaMidPoint(area);

        areasExpanding++;
        
        this.StartCoroutineAsync(GenerateAreaFromPoint(centerTile, area));

        return area;
    }

    private GameObject CreateEnemyCharacter(GameObject go, Transform parent, Area goblinStartArea)
    {

        var ch = go.GetComponent<Enemy>();

        var areas = Areas.Where(ar =>
            !ar.PointOfInterest && ar != goblinStartArea && !ar.ContainsRoads && ar.RoomForDifficulty >= ch.Difficulty &!
              ar.PresentCharacters.Any(c => c.CharacterRace != ch.CharacterRace) && ar.PresentCharacters.Count < ch.MaxGroupSize).ToList();


        if (areas.Any())
        {
            var a = areas[Random.Range(0, areas.Count)];
            a.RoomForDifficulty -= ch.Difficulty;
            return GenerateCharacter(go,a , parent);
        }
        else
        {
            Debug.Log($"Not able to find suitable area for: {ch}, max group: {ch.MaxGroupSize}");
            return null;
        }
    }


    public static GameObject GenerateCharacter(GameObject go, Area inArea, Transform parent, Vector3 pos)
    {

        var next = Instantiate(go, pos, Quaternion.identity);
        next.transform.parent = parent;
        next.name = go.name;

        var c = next.GetComponent<Character>();

        inArea.PresentCharacters.Add(c);

        c.InArea = inArea;

        return next;
    }

    public static GameObject GenerateCharacter(GameObject go, Area inArea, Transform parent, bool pointOfInterest = false)
    {
        var pos = inArea.GetRandomPosInArea();

        if (pointOfInterest)
        {
            pos = inArea.transform.position + Random.onUnitSphere * 5;

            pos.y = 0; // = new Vector3(vector3.x, 0, vector3.z);
        }
        return GenerateCharacter(go, inArea, parent, pos);
    }
    

    private Tile GetRandomNeighbour(Tile tile)
    {
        var neighbours = GetNeightbours(tile);
        
        return neighbours[(Random.Range(0, neighbours.Count))];
    }

    //TODO: could be recursive
    //TODO: improve running time 
    private IEnumerator GenerateAreaFromPoint(Tile startTile, Area a )
    {
        //Debug.Log("generating in thread");

        yield return null;

        //Using system random to be threadsafe
        var rnd = new System.Random();

        //Mark Tile as examined
        startTile.Examined = true;
        startTile.ForestChance = 0f;

        var growthChance = 0.02f;

        var increase = growthChance *2 ;

        startTile.Type = TileType.Ground;

        movableTiles.Add(startTile);
        immovableTiles.Remove(startTile);
        a.MovablePositions.Add(startTile);

        //last ring = areamiddle;
        List <Tile> lastRing = new List<Tile>() {startTile};

        int rings = 0;

        //while (!last.all forest chance == 100)
        while (rings < AreaSize/2 && lastRing.Any(t=>t.ForestChance < 1f))
        {
            rings++;

            List<Tile> nextRing = new List<Tile>(lastRing.Count*2);

            //Increase growthchance
            growthChance += increase;

            //for last
            //    .selectnighbours.where not examined
            foreach (var tile in lastRing)
            {
                var ns = GetNeightbours(tile, false, false);


                foreach (var n in ns)
                {
                    //neighbour.forestChance = last.forestchance + Rnd < (growth chance + last.forestchance)
                    //mark examined
                    n.Examined = true;
                    n.ForestChance = tile.ForestChance + rnd.NextDouble() < (growthChance + tile.ForestChance) ? growthChance : 0;

                    n.Type = rnd.NextDouble() <= n.ForestChance ? TileType.Forest : TileType.Ground;

                    n.Hidable = n.Type == TileType.Forest;

                    if (n.Type == TileType.Ground)
                    {
                        movableTiles.Add(n);
                        immovableTiles.Remove(n);
                        nextRing.Add(n);
                        a.MovablePositions.Add(n);
                    }

                }
            }

            lastRing = nextRing;

        }

        //Debug.Log("finshings area in "+ rings + " rings");

        areasExpanding--;
        //Debug.Log("Finishing area gen in thread");
    }


    private List<Tile> GetNeightbours(Tile tile,bool includeDiagonal = false, bool includeExamined = true)
    {
        var neighbours = new List<Tile>(4);

        var notTopX = tile.X < SizeX - 1;
        var notBottomX = tile.X > 0;
        var notTopY = tile.Y < SizeZ - 1;
        var notBottomY = tile.Y > 0;

        if (notTopX&& (includeExamined || !map[tile.X + 1, tile.Y].Examined))
            neighbours.Add(map[tile.X + 1, tile.Y]);
        if (notBottomX && (includeExamined || !map[tile.X - 1, tile.Y].Examined))
            neighbours.Add(map[tile.X - 1, tile.Y]);
        if (notTopY && (includeExamined || !map[tile.X, tile.Y + 1].Examined))
            neighbours.Add(map[tile.X, tile.Y + 1]);
        if (notBottomY && (includeExamined || !map[tile.X, tile.Y - 1].Examined))
            neighbours.Add(map[tile.X, tile.Y - 1]);

        if (includeDiagonal)
        {
            if (notTopX && notTopY && (includeExamined || !map[tile.X + 1, tile.Y + 1].Examined))
                neighbours.Add(map[tile.X + 1, tile.Y + 1]);
            if (notBottomX && notTopY && (includeExamined || !map[tile.X - 1, tile.Y + 1].Examined))
                neighbours.Add(map[tile.X - 1, tile.Y + 1]);
            if (notTopX &&notBottomY && (includeExamined || !map[tile.X + 1, tile.Y - 1].Examined))
                neighbours.Add(map[tile.X+1, tile.Y - 1]);
            if (notBottomX && notBottomY && (includeExamined || !map[tile.X - 1, tile.Y - 1].Examined))
                neighbours.Add(map[tile.X-1, tile.Y - 1]);
        }

        return neighbours;
    }


    //TODO: not working with area for some reason
    private Tile GetRandomGroundTile(Area inArea)
    {
        if (!inArea || !inArea.MovablePositions.Any())
        {
            Debug.LogWarning("Not any movable positions in area: "+ inArea);
            return null;
        }

        var areaTiles = inArea.MovablePositions;
            //movableTiles.Where(t => t.Area == inArea).ToList();

        return areaTiles[Random.Range(0, areaTiles.Count)];
    }

    private Area GetRandomArea()
    {
        return Areas[Random.Range(0, Areas.Count)];
    }

    //TODO: 
    private Area GetAreaAtPoint(Vector3 point)
    {
        if (!Areas.Any(a => a.PointIsInArea(point)))
            return null;

        return Areas.First(a => a.PointIsInArea(point));

        //return AreaMap[(int)(point.x / totalAreaSize), (int)(point.z / totalAreaSize)];
    }

    private Tile GetAreaMidPoint(Area area)
    {
        //TODO: check for out of bounds
        return map[(int)area.transform.position.x, (int)area.transform.position.z];
    }

    private IEnumerator CreateRoad(Area from, Area to, bool drawRoad = false)
    {
        if (from.RoadsTo.Contains(to))
            yield break;
        
        roadsBeingBuilt++;
        
        from.Neighbours.Add(to);
        to.Neighbours.Add(from);

        var current =GetAreaMidPoint(from);
        var end = GetAreaMidPoint(to);

        var wrongWayMod = drawRoad ? 0.075f : 0.35f;

        if (drawRoad)
        {
            from.RoadsTo.Add(to);
            to.RoadsTo.Add(from);        
        }

        while (current != end)
        {

            //moce vertically or horizontally
            var horizontally =  Random.value < 0.5f;
            //if (drawRoad && horizontally && current.X == end.X)
            //    horizontally = false;
            //else if (drawRoad && !horizontally && current.Y == end.Y)
            //    horizontally = true;

            var wrongDirection = Random.value < wrongWayMod;

            var mod = wrongDirection ? -1 : 1;

            if (current.X >= SizeX - 2 || current.X <= 1 || current.Y >= SizeZ - 2 || current.Y <= 1)
                mod = 1;

            if (GetNeighbourTile(current, end, mod, horizontally).Type == TileType.Road)
                horizontally = !horizontally;

            current = GetNeighbourTile(current, end, mod, horizontally);
                //current.X < end.X  ? map[current.X + mod, current.Y] : map[current.X -mod, current.Y];
            
            if (current.Type == TileType.Forest)
            {
                current.Type = TileType.Ground;
                movableTiles.Add(current);
                immovableTiles.Remove(current);
            }

            if (!drawRoad) continue;
            current.Type = TileType.Road;
            foreach (var neightbour in GetNeightbours(current))
            {
                neightbour.NextToRoad = true;
                if (neightbour.Type == TileType.Forest)
                {
                    neightbour.Type = TileType.Ground;
                    movableTiles.Add(neightbour);
                    immovableTiles.Remove(neightbour);
                }
            }
        }

        roadsBeingBuilt--;
    }

    private Tile GetNeighbourTile(Tile current,Tile end, int mod, bool horizontally)
    {
        if (horizontally)
        {
            return current.X < end.X  ? map[current.X + mod, current.Y] : map[current.X -mod, current.Y];
        }
        else
        {
            return current.Y < end.Y ? map[current.X, current.Y + mod] : map[current.X, current.Y - mod];
        }
    }

    
}
