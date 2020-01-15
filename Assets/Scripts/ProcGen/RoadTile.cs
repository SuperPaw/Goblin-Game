using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadTile : MonoBehaviour
{
    public bool RoadLeft, RoadUp, RoadRight, RoadDown;

    public void RotateClockWise(int times = 1)
    {
        for(;times > 0; times--)
        {
            transform.Rotate(new Vector3(0, 90));

            var oldLeft = RoadLeft;
            var oldRight = RoadRight;
            var oldUp = RoadUp;
            var oldDown = RoadDown;

            RoadUp = oldLeft;
            RoadRight = oldUp;
            RoadDown = oldRight;
            RoadLeft = oldDown;
        }
    }

    /// <summary>
    /// Rotates the tiles clockwise untill it has roads leading in the directions set to true
    /// </summary>
    public void RotateToPosition(bool left, bool up,bool right, bool down)
    {
        //Debug.Log($"{name}: Rotating to: {left},{up},{right},{down}");
        int tries = 0;              

        while(left != RoadLeft || up != RoadUp || right != RoadRight || down != RoadDown)
        {
            RotateClockWise();
            if(tries++ >=4 ) //TODO: test if one less try is needed
            {
                Debug.LogWarning("Tried to fit rotate wrong road : " + name);
                return;
            }
        }
        //Debug.Log($"{name}: Rotated {tries} times. {left},{up},{right},{down} == {RoadLeft},{RoadUp},{RoadRight},{RoadDown}");


    }
}
