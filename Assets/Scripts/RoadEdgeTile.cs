using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadEdgeTile : MonoBehaviour
{
    //TODO: make general naming to handle all edges
    public SpriteRenderer SpriteRenderer;
    public Sprite[] SsSprites;
    public Sprite SwSprite, SwnSprite, SnSprite, SwneSprite, NullRoad;
    public bool SRoad, WRoad, NRoad, ERoad;

    public void SetupSprite()
    {
        int rs = 0;
        if (SRoad) rs++;
        if (WRoad) rs++;
        if (NRoad) rs++;
        if (ERoad) rs++;

        switch (rs)
        {
            case 0:
                SpriteRenderer.sprite = NullRoad;
                break;
            case 1:
                SpriteRenderer.sprite = SsSprites[Random.Range(0, SsSprites.Length)];
                if (WRoad)
                {
                    SpriteRenderer.transform.eulerAngles += new Vector3(0,90,0);
                }
                else if(NRoad)
                {
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 180, 0);
                }
                else if (ERoad)
                {
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 270, 0);
                }
                break;
            case 2:
                if (SRoad && WRoad)
                {
                    SpriteRenderer.sprite = SwSprite;
                }
                else if (NRoad && WRoad)
                {
                    SpriteRenderer.sprite = SwSprite;
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 90, 0);

                }
                else if (NRoad && ERoad)
                {
                    SpriteRenderer.sprite = SwSprite;
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 180, 0);
                }
                else if (ERoad && SRoad)
                {
                    SpriteRenderer.sprite = SwSprite;
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 270, 0);
                }
                else if (NRoad && SRoad)
                {
                    SpriteRenderer.sprite = SnSprite;
                }
                else if (WRoad && ERoad)
                {
                    SpriteRenderer.sprite = SnSprite;
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 90, 0);
                }
                else
                {
                    Debug.Log("unhahned casesss");
                }
                break;
            case 3:
                SpriteRenderer.sprite = SwnSprite;
                if (!SRoad)
                {
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 90, 0);
                }
                else if (!WRoad)
                {
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 180, 0);
                }
                else if (!NRoad)
                {
                    SpriteRenderer.transform.eulerAngles += new Vector3(0, 270, 0);
                }
                break;
            case 4:
                SpriteRenderer.sprite = SwneSprite;
                break;
            default:
                Debug.Log("unhahned deafutlcasesss");
                break;

        }
    }
}
