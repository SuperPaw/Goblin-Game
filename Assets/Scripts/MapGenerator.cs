using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    public enum TileType {Ground, Forest, Loot}
    //TODO: turn into a struct
    public class Tile
    {
        public int X;
        public int Y;
        public TileType Type;
        public int Cluster;
        public Area Area;

        public Tile(int x, int y)
        {
            this.X = x;
            this.Y = y;
            Cluster = -1;
            Type = TileType.Ground;
        }
    }

    public int RandomSeed;
    public int SizeX, SizeZ;
    public Area AreaTilePrefab;
    public int AreaSize;
    public List<Area> Areas;

    public Area[,] AreaMap { get; private set; }

    public int ClustersOfNoWalking;
    public int MinClusterSize;
    public bool DistanceBetweenClusters;
    [Range(1.01f,5)]
    //for more fidelity around tiles
    public float Cohesion;

    public int BorderClusterSize;
    [Range(0f, 1)]
    public float LootableChance;
    private Tile[,] map;
    private readonly Dictionary<int, List<Tile>> clusters = new Dictionary<int, List<Tile>>();
    private List<Tile> movableTiles;
    private List<Tile> immovableTiles = new List<Tile>();

    [Range(0f, 1)]
    public float PctOfImmovableAreas;
    [Header("References")]
    public GameObject[] Npcs;
    public GameObject[] HidableObjects;
    public GameObject[] LootObjects;

    [Header("Character Generation")]
    public GameObject NpcHolder;
    public int NpcsToGenerate;
    [Range(0, 20)]
    public int GoblinsToGenerate;
    public GameObject DefaultCharacter;
    public TeamController GoblinTeam;
    private int noOfAreasX;
    private int noOfAreasZ;


    // Use this for initialization
    public IEnumerator GenerateMap(Action<int> progressCallback, Action endCallback)
    {
        var progress = 0;
        var charFact = 25;
        //could use factors on the two last to make them more important than each tiles
        var totalProgress = (int)(SizeX*SizeZ) + NpcsToGenerate*charFact + GoblinsToGenerate*charFact;
        var progressPct = 0;

        progressCallback(progressPct);

        // check that input numbers are actually allowed
        if (BorderClusterSize + ClustersOfNoWalking * MinClusterSize > SizeX * SizeZ)
        {
            Debug.LogError("Cluster sizes are too big for the map");
            yield break;
        }
        

        // using a tile array to generate the map before initializing it
        //GENERATING 
        movableTiles = new List<Tile>();
        map = new Tile[SizeX,SizeZ];
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeZ; j++)
            {
                map[i,j] = new Tile(i,j);
                movableTiles.Add(map[i,j]);
            }
        }
        
        var imovableSize = 0;
        var totalSize = map.Length;

        // --------------- CREATING THE BORDER CLUSTER --------------
        if (BorderClusterSize > 0)
        {
            var borderIdx = 0;

            clusters.Add(borderIdx, new List<Tile>());

            for (int i = 0; i < SizeX; i++)
            {
                Tile t = map[i, 0];
                Tile y = map[i, SizeZ - 1];

                movableTiles.Remove(t);
                movableTiles.Remove(y);
                AssignToCluster(t, borderIdx);
                AssignToCluster(y, borderIdx);
                
                imovableSize +=2;
            }
            for (int i = 0; i < SizeZ; i++)
            {
                Tile t = map[0,i];
                Tile y = map[SizeX-1,i];

                movableTiles.Remove(t);
                movableTiles.Remove(y);
                AssignToCluster(t, borderIdx);
                AssignToCluster(y, borderIdx);

                imovableSize += 2;
            }

            var count = clusters[borderIdx].Count;

            for (int i = 0; i < BorderClusterSize; i++)
            {
                Tile next = clusters[borderIdx][Random.Range(0, count)];
                while (next.Type != TileType.Ground)
                {
                    next = GetRandomNeighbour(next);
                }
                movableTiles.Remove(next);
                AssignToCluster(next, borderIdx);
                imovableSize++;
            }
        }

        //initialize clusters expanding untill clusters meets min size
        for (int i = clusters.Count; i < ClustersOfNoWalking; i++)
        {
            clusters.Add(i, new List<Tile>());

            Tile t = GetRandomGroundTile();
            movableTiles.Remove(t);
            AssignToCluster(t, i);
            imovableSize++;

            while (clusters[i].Count < MinClusterSize)
            {
                Tile next = GetRandomNeighbour(t);
                while (next.Type != TileType.Ground)
                {
                    next = GetRandomNeighbour(next);
                }
                movableTiles.Remove(next);
                AssignToCluster(next,i);
                imovableSize++;
            }
        }

        //iteratively expand clusters untill pct of movable is met
        while ((float)imovableSize++ / totalSize < PctOfImmovableAreas)
        {
            //select random cluster
            var i = Random.Range(0, ClustersOfNoWalking);
            //Switch to first for more round forests
            Tile next = GetRandomNeighbour(clusters[i][(int)(clusters[i].Count/Cohesion)]);
            //TODO: this can return the edge of another cluster
            while (next.Type != TileType.Ground)
            {
                i = next.Cluster;
                next = GetRandomNeighbour(next);
            }

            movableTiles.Remove(next);
            immovableTiles.Add(next);
            AssignToCluster(next, i);
            
        }

        //Turn adjacent cluster tiles into roads
        if (DistanceBetweenClusters)
        {
            var adjacentToOtherClusterTiles =
                immovableTiles.Where(t => GetNeightbours(t,true).Any(n => n.Type == TileType.Forest && n.Cluster != t.Cluster));

            foreach (var adjacentToOtherClusterTile in adjacentToOtherClusterTiles)
            {
                adjacentToOtherClusterTile.Type = TileType.Ground;
                movableTiles.Add(adjacentToOtherClusterTile);
                RemoveFromCluster(adjacentToOtherClusterTile);
                //immovableTiles.Remove(adjacentToOtherClusterTile);
            }
        }
        
        //Instantiating Area Tiles
        noOfAreasX = SizeX / AreaSize;
        noOfAreasZ = SizeZ / AreaSize;

        Areas = new List<Area>(noOfAreasZ*noOfAreasX);
        AreaMap = new Area[noOfAreasX, noOfAreasZ];

        for (int x = 0; x < noOfAreasX; x++)
        {
            for (int z = 0; z < noOfAreasZ; z++)
            {
                //TODO: use holders for these
                var next = Instantiate(AreaTilePrefab, transform);

                next.name = "Area (" + x + "," + z + ")";
                next.X = x;
                next.Z = z;

                var posAdjust = AreaSize / 2;

                next.transform.position = new Vector3(x*AreaSize+posAdjust,0.001f,z*AreaSize+posAdjust);
                next.Size = AreaSize;
                next.transform.localScale = new Vector3(AreaSize,0.1f,AreaSize);
                Areas.Add(next);
                AreaMap[x, z] = next;

                //var walkabletilesInArea= new List<Tile>();

                //for (int i = 0; i < AreaSize; i++)
                //{
                //    for (int j = 0; j < AreaSize; i++)
                //    {
                //        if (SizeX <= x * AreaSize + i || SizeZ <= z * AreaSize + j)
                //        {

                //            Debug.Log("not valid tile " + (x * AreaSize + i) + "," + (z * AreaSize + j));
                //            //continue;
                //        }
                //        else
                //        {
                //            var tile = map[x * AreaSize + i, z * AreaSize + j];
                //            tile.Area = next;
                //            if (tile.Type == TileType.Ground)
                //                walkabletilesInArea.Add(tile);
                //            //map[x * AreaSize+i, z * AreaSize+j].Area = next;
                //        }
                //    }
                //}

                //ADDING LOOT
                if (Random.value < LootableChance)
                {
                    //TODO: should only be in area 
                    Tile tile = GetRandomGroundTile();//;walkabletilesInArea[Random.Range(0, walkabletilesInArea.Count)])];//GetRandomGroundTile(next);

                    tile.Type = TileType.Loot;
                    movableTiles.Remove(tile);

                    var loot = Instantiate(LootObjects[Random.Range(0, LootObjects.Length)]);//, next.transform);

                    loot.name = "Lootable";

                    loot.transform.parent = next.transform;

                    next.Lootables.Add(loot.GetComponent<Lootable>());

                    loot.transform.position = new Vector3(tile.X, 1, tile.Y);
                }
            }
        }

        //set up neighbors
        foreach (var area in Areas)
        {
            foreach (var neightbour in GetNeightbours(area))
            {
                area.ConnectsTo.Add(neightbour);
            }
        }



        //INSTANTIATING MAP
        //y=1 for tree height
        for (int i = 0; i < clusters.Count; i++)
	    {
            for (var i1 = 0; i1 < clusters[i].Count; i1++)
            {
                var tile = clusters[i][i1];

                var next = Instantiate(HidableObjects[Random.Range(0, HidableObjects.Length)], transform);

                next.name = "Forest " + tile.X + "," + tile.Y;

                //TODO:check 
                next.transform.position = new Vector3(tile.X, 1, tile.Y);

                int loc = (++progress * 100) / totalProgress;
                if (loc != progressPct)
                {
                    progressPct = loc;
                    yield return null;
                    progressCallback(progressPct);
                }
            }
        }

        //HACK CHECK FOR AREA ACCESSIBILITY
        //select a middle point 
        //ground points at random a bunch of times and check that they are reachable
        //if not make a forest road
        


        for (int i = 0; i < NpcsToGenerate; i++)
        {
            //TODO: create a number of spawn positions and check their connectivity.
            CreateCharacter(Npcs[Random.Range(0, Npcs.Length)], NpcHolder.transform);

            progress += charFact;

	        int loc = (progress * 100) / totalProgress;
	        if (loc != progressPct)
	        {
	            progressPct = loc;
	            yield return null;
	            progressCallback(progressPct);
	        }
        }

        List<Goblin> members = new List<Goblin>();

        var a = GetRandomArea();
        //find middle ish tile
        //while (area.X < SizeX/4 || area.X > SizeX *0.75 ||
        //       area.Y < SizeZ / 4 || area.Y > SizeZ * 0.75 )
        //{
        //     area = GetRandomGroundTile();
        //}

        var pos = a.transform.position;

        Debug.Log("Initializing Goblin team in "+a);
        
        //Find suitable start position
        GoblinTeam.transform.position = new Vector3(pos.x,0,pos.z);

        var GroupDistance = 4;
        //TODO: check that we are not initializinig in a too small area. could be done with connectivity check
        //TODO: Use create character
        for (int i = 0; i < GoblinsToGenerate; i++)
        {

            var next = Instantiate(DefaultCharacter, GoblinTeam.transform);

            next.name = NameGenerator.GetName();

            pos = pos + Random.insideUnitSphere * GroupDistance;

            //Debug.Log("Creating gbolin at "+ pos.X +","+pos.Y);

            next.transform.position = new Vector3(pos.x, 0, pos.z);

            var g = next.GetComponent<Goblin>();
            //g.InArea = a;
            members.Add(g);


            progress += charFact;

            int loc = (progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct);
            }

        }
        GoblinTeam.Initialize(members);

        //TODO: give reference and move to overalle generation script
        FindObjectOfType<PlayerController>().Initialize();

        endCallback();
    }

    private void CreateCharacter(GameObject go, Transform parent)
    {
        var tile = GetRandomArea();
        var pos = tile.GetRandomPosInArea();

        var next = Instantiate(go, pos,Quaternion.identity);
        next.transform.parent = parent;
        //next.GetComponent<Character>().InArea = tile;

        //next.name +=  "(" + tile.X + "," + tile.Y + ")";

        //next.transform.position = 
    }

    private void AssignToCluster(Tile t, int cluster) //Type type)
    {
        if (t.Cluster > -1)
            clusters[t.Cluster].Remove(t);

        t.Cluster = cluster;
        t.Type = TileType.Forest;

        clusters[cluster].Add(t);
    }

    private void RemoveFromCluster(Tile t) //Type type)
    {
        if (t.Cluster > -1)
            clusters[t.Cluster].Remove(t);

        t.Cluster = -1;

        t.Type = TileType.Ground;
    }


    private Tile GetRandomNeighbour(Tile tile)
    {
        var neighbours = GetNeightbours(tile);
        
        return neighbours[(Random.Range(0, neighbours.Count))];
    }


    private List<Area> GetNeightbours(Area are, bool includeDiagonal = false)
    {
        var neighbours = new List<Area>(8);

        var notTopX = are.X < noOfAreasX - 1;
        var notBottomX = are.X > 0;
        var notTopY = are.Z < noOfAreasZ - 1;
        var notBottomY = are.Z > 0;

        if (notTopX) neighbours.Add(AreaMap[are.X + 1, are.Z]);
        if (notBottomX) neighbours.Add(AreaMap[are.X - 1, are.Z]);
        if (notTopY) neighbours.Add(AreaMap[are.X, are.Z + 1]);
        if (notBottomY) neighbours.Add(AreaMap[are.X, are.Z - 1]);

        if (includeDiagonal)
        {
            if (notTopX && notTopY) neighbours.Add(AreaMap[are.X + 1, are.Z + 1]);
            if (notBottomX && notTopY) neighbours.Add(AreaMap[are.X - 1, are.Z + 1]);
            if (notTopX && notBottomY) neighbours.Add(AreaMap[are.X + 1, are.Z - 1]);
            if (notBottomX && notBottomY) neighbours.Add(AreaMap[are.X - 1, are.Z - 1]);
        }

        return neighbours;
    }

    private List<Tile> GetNeightbours(Tile tile,bool includeDiagonal = false)
    {
        var neighbours = new List<Tile>(4);

        var notTopX = tile.X < SizeX - 1;
        var notBottomX = tile.X > 0;
        var notTopY = tile.Y < SizeZ - 1;
        var notBottomY = tile.Y > 0;

        if (notTopX) neighbours.Add(map[tile.X + 1, tile.Y]);
        if (notBottomX) neighbours.Add(map[tile.X - 1, tile.Y]);
        if (notTopY) neighbours.Add(map[tile.X, tile.Y + 1]);
        if (notBottomY) neighbours.Add(map[tile.X, tile.Y - 1]);

        if (includeDiagonal)
        {
            if (notTopX && notTopY) neighbours.Add(map[tile.X + 1, tile.Y + 1]);
            if (notBottomX && notTopY) neighbours.Add(map[tile.X - 1, tile.Y + 1]);
            if (notTopX &&notBottomY ) neighbours.Add(map[tile.X+1, tile.Y - 1]);
            if (notBottomX && notBottomY) neighbours.Add(map[tile.X-1, tile.Y - 1]);
        }

        return neighbours;
    }


    //TODO: not working with area for some reason
    private Tile GetRandomGroundTile(Area inArea = null)
    {
        if(!inArea)
            return movableTiles[Random.Range(0, movableTiles.Count)];

        var areaTiles = movableTiles.Where(t => t.Area == inArea).ToList();

        return areaTiles[Random.Range(0, areaTiles.Count)];
    }
    private Area GetRandomArea()
    {
        return Areas[Random.Range(0, Areas.Count)];
    }

}
