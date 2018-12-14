using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    public int MinDistanceBetweenClusters;
    [Range(1.01f,5)]
    //for more fidelity around tiles
    public float Cohesion;
    private Tile[,] map;
    private readonly Dictionary<int, List<Tile>> clusters = new Dictionary<int, List<Tile>>();
    private List<Tile> movableTiles;

    [Range(0f, 1)]
    public float PctOfImmovableAreas;
    [Header("References")]
    public GameObject MapTileGameObject;
    public GameObject ImmovableMapTile;
    public GameObject[] Npcs;
    public GameObject[] HidableObjects;
    public GameObject NpcHolder;
    public int NpcsToGenerate;

    
	// Use this for initialization
    public IEnumerator GenerateMap(Action<int> progressCallback, Action EndCallback)
    {
        var progress = 0;
        //could use factors on the two last to make them more important than each tiles
        var totalProgress = SizeX + (int)(SizeX*SizeZ*PctOfImmovableAreas) + NpcsToGenerate;
        var progressPct = 0;

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
                next = GetRandomNeighbour(next);
            }
            AssignToCluster(next, i);
            
        }

        //INSTANTIATING
        for (int x = startX; x < endX; x++)
	    {
	        for (int z = startZ; z < endZ; z++)
	        {
                //TODO: only create tiles where needed

	            var next =Instantiate(MapTileGameObject,transform);

                next.transform.position = new Vector3(x,0,z);
	            next.name = "Ground ("+x+","+z+")";

            }
            int loc = (++progress * 100) / totalProgress;
	        if (loc != progressPct)
	        {
                //TODO: move to method
	            progressPct = loc;
	            yield return null;
	            progressCallback(progressPct);
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


        for (int i = 0; i < NpcsToGenerate; i++)
	    {
	        var next = Instantiate(Npcs[Random.Range(0,Npcs.Length)], NpcHolder.transform);

	        var tile = GetRandomGroundTile();

            //TODO:check 
	        next.transform.position = new Vector3(tile.X, 0, tile.Y);

	        int loc = (++progress * 100) / totalProgress;
	        if (loc != progressPct)
	        {
	            progressPct = loc;
	            yield return null;
	            progressCallback(progressPct);
	        }

        }

        EndCallback();
    }

    private void AssignToCluster(Tile t, int cluster) //Type type)
    {
        if (t.Cluster > -1)
            clusters[t.Cluster].Remove(t);

        t.Cluster = cluster;
        t.Type = TileType.Forest;

        clusters[cluster].Add(t);
    }

    private Tile GetRandomNeighbour(Tile tile)
    {

        var neighbours = new List<Tile>(4);

        if( tile.X < SizeX-1) neighbours.Add(map[tile.X+1,tile.Y]);
        if (tile.X > 0 ) neighbours.Add(map[tile.X - 1,tile.Y]);
        if ( tile.Y < SizeX - 1) neighbours.Add(map[tile.X,tile.Y+1]);
        if (tile.Y > 0) neighbours.Add(map[tile.X ,tile.Y-1]);

        return neighbours[(int)(Random.value * neighbours.Count)];

    }

    private Tile GetRandomGroundTile()
    {
        //TODO: keep hold of ground tiles in list an return a random from that is cheaper
        Tile t = map[(int)(Random.value * SizeX), (int)(Random.value * SizeZ)]; ;

        while ( t.Type != TileType.Ground)
            t = map[(int) (Random.value * SizeX), (int) (Random.value * SizeZ)];
        
        return t;
    }
	
}
