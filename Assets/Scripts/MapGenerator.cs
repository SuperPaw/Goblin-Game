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
    public enum TileType {Ground, Forest}
    public class Tile
    {
        public int X;
        public int Y;
        public TileType Type;
        public int Cluster;

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
    public int ClustersOfNoWalking;
    public int MinClusterSize;
    public bool DistanceBetweenClusters;
    [Range(1.01f,5)]
    //for more fidelity around tiles
    public float Cohesion;
    private Tile[,] map;
    private readonly Dictionary<int, List<Tile>> clusters = new Dictionary<int, List<Tile>>();
    private List<Tile> movableTiles;
    private List<Tile> immovableTiles = new List<Tile>();

    [Range(0f, 1)]
    public float PctOfImmovableAreas;
    [Header("References")]
    public GameObject MapTileGameObject;
    public GameObject ImmovableMapTile;
    public GameObject[] Npcs;
    public GameObject[] HidableObjects;

    [Header("Character Generation")]
    public GameObject NpcHolder;
    public int NpcsToGenerate;
    [Range(0, 20)]
    public int GoblinsToGenerate;
    public GameObject DefaultCharacter;
    public TeamController GoblinTeam;

    // Use this for initialization
    public IEnumerator GenerateMap(Action<int> progressCallback, Action EndCallback)
    {
        var progress = 0;
        var charFact = 25;
        //could use factors on the two last to make them more important than each tiles
        var totalProgress = (int)(SizeX*SizeZ) + NpcsToGenerate*charFact + GoblinsToGenerate*charFact;
        var progressPct = 0;

        //TODO: Include navmesh gen in here


        progressCallback(progressPct);

	    var endX = SizeX;
	    var startX = 0 ;

	    var endZ = SizeZ ;
        var startZ = 0;

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

        //initialize clusters expanding untill clusters meets min size
        //TODO check that input numbers are actually allowed
        for (int i = 0; i < ClustersOfNoWalking; i++)
        {
            clusters.Add(i, new List<Tile>());

            Tile t = GetRandomGroundTile();
            movableTiles.Remove(t);
            AssignToCluster(t, i);
            imovableSize++;

            while (clusters[i].Count < MinClusterSize)
            {
                Tile next = GetRandomNeighbour(t);
                //TODO: this can return the edge of another cluster
                while (next.Type != TileType.Ground)
                {
                    next = GetRandomNeighbour(next);
                }
                movableTiles.Remove(t);
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

        List<Character> Members = new List<Character>();
        List<Tile> postions = new List<Tile>(GoblinsToGenerate);
        int TilesToMoveFromFirst = 4;

        var pos = GetRandomGroundTile();
        //find middle ish tile
        while (pos.X < SizeX/4 || pos.X > SizeX *0.75 ||
               pos.Y < SizeZ / 4 || pos.Y > SizeZ * 0.75 )
        {
             pos = GetRandomGroundTile();
        }

        Debug.Log("Initializing Goblin team in pos ("+ pos.X + ","+pos.Y+")");

        postions.Add(pos);
        //Find suitable start position
        GoblinTeam.transform.position = new Vector3(pos.X,0,pos.Y);

        //TODO: check that we are not initializinig in a too small area. could be done with connectivity check
        //TODO: Use create character
        for (int i = 0; i < GoblinsToGenerate; i++)
        {

            var next = Instantiate(DefaultCharacter, GoblinTeam.transform);

            int x = 0;
            while (postions.Contains(pos) && x++ < TilesToMoveFromFirst)
            {
                var neighbour = GetRandomNeighbour(pos);

                if (neighbour.Type == TileType.Ground) pos = neighbour;
            }

            next.transform.position = new Vector3(pos.X, 0, pos.Y);

            Members.Add(next.GetComponent<Character>());


            progress += charFact;

            int loc = (progress * 100) / totalProgress;
            if (loc != progressPct)
            {
                progressPct = loc;
                yield return null;
                progressCallback(progressPct);
            }

        }
        GoblinTeam.Initialize(Members);

        //TODO: give reference and move to overalle generation script
        FindObjectOfType<PlayerController>().Initialize();

        EndCallback();
    }

    private void CreateCharacter(GameObject go, Transform parent)
    {
        var next = Instantiate(go, parent);


        var tile = GetRandomGroundTile();
        //next.name +=  "(" + tile.X + "," + tile.Y + ")";

        //TODO:check 
        next.transform.position = new Vector3(tile.X, 0, tile.Y);
        try
        {
            next.AddComponent<NavMeshAgent>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

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

    private Tile GetRandomGroundTile()
    {
        return movableTiles[Random.Range(0, movableTiles.Count)];
    }
	
}
