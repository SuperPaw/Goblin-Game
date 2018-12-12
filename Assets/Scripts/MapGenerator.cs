using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public int SizeX, SizeZ;
    public GameObject MapTileGameObject;
    public Tilemap TileMap;
    public GameObject[] NPCs;
    public GameObject[] HidableObjects;
    public GameObject NpcHolder;
    public int NpcsToGenerate;

	// Use this for initialization
	void Start ()
	{
	    var endX = SizeX / 2;
	    var startX = 0 - endX;

	    var endZ = SizeZ / 2;
	    var startZ = 0 - endZ;

        //TODO: use a int array to generate the map before initializing it
        //generating

        //INSTANTIATING
        for (int x = startX; x < endX; x++)
	    {
	        for (int z = startZ; z < endZ; z++)
	        {
	            var next =Instantiate(MapTileGameObject,transform);

                next.transform.position = new Vector3(x,0,z);
	            next.name = "Ground ("+x+","+z+")";
	        }
        }


        //y=1 for tree height

	    for (int i = 0; i < NpcsToGenerate; i++)
	    {
	        var next = Instantiate(NPCs[Random.Range(0,NPCs.Length)], NpcHolder.transform);

            //TODO:check 
	        next.transform.position = new Vector3(Random.Range(startX,endX), 0, Random.Range(startZ, endZ));
            
	    }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
