using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public PlayerTeam Team;
    public Camera Cam;
    public SoundController Sound;

    [Header("Controls")]
    public int MouseMoveKey = 1;
    public enum MappableActions { Hide, Attack, Flee, Menu,FixCamOnLeader, Move, Camp,InvincibleMode, AddXp } //TODO: move should contain direction maybe
    public LayerMask HitMask;

    [Serializable]
    public struct KeyMapping
    {
        public KeyCode Key;
        public MappableActions Action;
    }
    public KeyMapping[] KeyMappings;


    [Serializable]
    public struct OrderType
    {
        public MappableActions Order;
        //TODO: create goblin speech struct for linking all goblin shouts with sounds
        public string Speech;
        public SoundBank.GoblinSound GoblinSound;
    }

    public OrderType[] Orders;

    public OrderType MoveOrder;

    public float OrderCooldown = 3f;

    private bool _mouseHeld;
    private Vector3 _mouseDragPos;

    public float ZoomMinBound= 2;
    public float ZoomMaxBound = 50;
    public float ZoomSpeed;
    public float PcZoomSpeed;
    public static Goblin FollowGoblin;

    public Renderer FogOfWar;

    [Header("Follow Animation")] public float MoveTime = 1;
    public AnimationCurve MoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Vector2[] lastZoomPositions;
    private bool wasZoomingLastFrame;
    private Vector2 lastPanPosition;
    private int panFingerId;

    public float FollowZoomSize = 8;
    public float PanSpeed;
    private float touchTime;
    private Coroutine camMoveRoutine;

    void Awake()
    {
        if (!Instance)
            Instance = this;
    }

    public void Initialize()
    {
        Team = GetComponent<PlayerTeam>();

        if(!Team) Debug.LogWarning("Unable to find team for player controls!");

        if (!Cam) Cam = Camera.main;

        if (!Sound) Sound = FindObjectOfType<SoundController>();

        //UpdateFogOfWar();
        MoveToGoblin(Team.Leader);
    }

    void FixedUpdate()
    {
        //maybe at larger interval
        if(FollowGoblin && camMoveRoutine == null)
            MoveToGoblin(FollowGoblin);
    }


    void Update()
    {
        //TODO: divide these into methods
        if (Input.touchSupported)
        {
            HandleTouch();
        }
        else
        {
            HandleMouseKeys();
        }
    }

    void HandleMouseKeys()
    {
        Zoom(Input.mouseScrollDelta.y, false);

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            PanCamera(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0)) _mouseHeld = false;
        

        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleClick(Input.mousePosition);
        }

        foreach (var mapping in KeyMappings)
        {
            if (Input.GetKeyDown(mapping.Key))
            {
                Action(mapping.Action);
            }
        }
    }

    //ref: https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
    void HandleTouch()
    {
        switch (Input.touchCount)
        {
            case 1: // Panning
                wasZoomingLastFrame = false;
                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchTime = Time.time;

                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                    _mouseHeld = false;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    PanCamera(touch.position);
                    touchTime -= 0.01f;
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended && touchTime +0.5f > Time.time &! IsPointerOverUIObject())
                {
                    HandleClick(Input.GetTouch(0).position);
                }
                break;

            case 2: // Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    Zoom(offset, true);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }

    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void HandleClick(Vector3 position)
    {
        Sound.PlayMapCLick();

        //TODO Move this to a routine and insert a pause here and a shout from the Chief

        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000, HitMask))
        {

            if (hit.collider && hit.collider.GetComponent<Character>()) //TODO:check visibility else just move there
            {

                var c = hit.collider.GetComponent<Character>();
                if (c)
                {
                    //Debug.Log("Clicked charaacter " + c.name);

                    if (c.tag == "Enemy" && c.Alive())
                    {
                        if (c.InArea.Visible())
                            Team.Attack(c);
                        else
                        {
                            ClickedArea(c.InArea);
                        }
                    }
                    else if (c as Goblin)
                    {
                        CharacterView.ShowCharacter(c as Goblin);
                    }
                    else
                        ClickedArea(c.InArea);
                }
            }
            //TODO use parent class for these
            else if (hit.collider && hit.collider.GetComponent<GoblinWarrens>())
            {
                var v = hit.collider.GetComponent<GoblinWarrens>();

                if (v.InArea.Visible())
                    VillageView.OpenVillageView(v, Team);
                else
                    ClickedArea(v.InArea);
            }
            else if (hit.collider && hit.collider.GetComponent<Monument>())
            {
                var monument = hit.collider.GetComponent<Monument>();

                if (monument.InArea.Visible())
                    BigStoneView.OpenStoneView(monument, Team);
                else
                    ClickedArea(monument.InArea);
            }
            else if (hit.collider && hit.collider.GetComponent<Area>())
            {
                var a = hit.collider.GetComponent<Area>();
                //Debug.Log("Clicked Area : " + (a.name));

                ClickedArea(a);
            }
            else
            {
                Debug.LogWarning("Not a valid hit: " + hit.point);
            }

        }
    }

    private void PanCamera(Vector2 newPanPosition)
    {
        //To compensate for the 45 degree angle of the cam. TODO: Should have been calculated 
        newPanPosition.y *= 2f;

        var inWorld = Cam.ScreenToWorldPoint(new Vector3(newPanPosition.x, newPanPosition.y, Cam.nearClipPlane));

        //Sound.PlayMapCLick();
        if (!_mouseHeld)
        {
            _mouseDragPos =inWorld;
            _mouseHeld = true;
        }
        
        var moveDelta = (_mouseDragPos - inWorld);
        moveDelta.y = 0;

        //TODO: the z axis does not move correctly due to the rotation of the camera

        Cam.transform.position += moveDelta;

        FollowGoblin = null;
        //Debug.Log("Draggingggg");

        //// Determine how much to move the camera
        //Vector3 offset = Cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        //Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

        //// Perform the movement
        //Cam.transform.Translate(move, Space.World);
        
        //// Cache the position
        //lastPanPosition = newPanPosition;
    }


    //Vector3 GetPosOnPlane(Vector2 posOnViewport)
    //{
    //    Vector3 pt = new Vector3(posOnViewport.x, posOnViewport.y, 0);
    //    Ray d = Cam.ViewportPointToRay(pt);
    //    float t = (planePos.z - d.origin.z) / d.direction.z;
    //    float x = d.direction.x * t + d.origin.x;
    //    float y = d.direction.y * t + d.origin.y;
    //    return new Vector3(x, y, planePos.z);
    //}


    private void ClickedArea(Area a)
    {
        var target = a.transform.position;//hit.point;
        target.y = 0;

        if (a == Team.Leader.InArea && Team.Leader.Travelling())
        {
            Team.Move(a);
        }
        else if (!Team.Leader.InArea.Neighbours.Contains(a))
        {
            Debug.LogWarning("Not possible to move to " + a + " from " + Team.Leader.InArea);
            return;
        }
        else
        {
            Team.LeaderShout(MoveOrder);
            
            Team.Move(a);
        }
    }

    public static void UpdateFog()
    {
        if(Instance && Instance.Team)
            Instance.UpdateFogOfWar();
    }

    private void UpdateFogOfWar()
    {
        if (!Team.Leader.InArea)
        {
            //Debug.LogWarning("Chief not present in any area");
            return;
        }

        var id = 1;

        RemoveFogAtPos(Team.Leader.InArea.transform.position, id++);
        Team.Leader.InArea.RemoveFogOfWar(true);
        foreach (var n in Team.Leader.InArea.Neighbours)
        {
            RemoveFogAtPos(n.transform.position, id++);
            n.RemoveFogOfWar(false);
        }
        
    }

    private void RemoveFogAtPos(Vector3 pos, int id)
    {
        if (id < 1 || id > 8)
        {
            Debug.Log("Unknown shader id: " + id);
            return;
        }

        Vector3 screenPos = Cam.WorldToScreenPoint(pos);
        Ray rayToPlayerPos = Cam.ScreenPointToRay(screenPos);
        int layermask = (int)(1 << 8);
        RaycastHit hit;
        if (Physics.Raycast(rayToPlayerPos, out hit, 1000, layermask))
        {
            FogOfWar.material.SetVector("_Player" + id.ToString() + "_Pos", hit.point);
        }
    }

    public void Action(MappableActions action)
    {
        if (Orders.Any(o => o.Order == action))
            Team.LeaderShout(Orders.First(o => o.Order == action));

        switch (action)
        {
            case MappableActions.Hide:
                //Shout from leader
                Team.Hide();
                break;
            case MappableActions.Attack:
                Team.Attack();
                break;
            case MappableActions.Flee:
                Team.Flee();
                break;
            case MappableActions.Menu:
                break;
            case MappableActions.FixCamOnLeader:
                FollowGoblin = Team.Leader;
                break;
            case MappableActions.Camp:
                Team.Camp();
                break;
            case MappableActions.InvincibleMode:
                GameManager.Instance.InvincibleMode = !GameManager.Instance.InvincibleMode;
                break;
            case MappableActions.AddXp:
                CharacterView.AddXp();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //TODO: this should be less hacky
    public void Action(string action)
    {
        Action((MappableActions)Enum.Parse(typeof(MappableActions),action,true) );
    }

    //TODO: seperate team stuff and interface into two classes
    internal static void BuyFood(int amount, int price)
    {
        Instance.Team.Food += amount;
        Instance.Team.Treasure -= price;

        //TODO: play caching
    }

    internal static void SacTreasure(int amount)
    {
        Instance.Team.Treasure -= amount;

        //TODO: effect
    }

    internal static void SacFood(int v)
    {
        Instance.Team.Food -= v;

        //TODO: effect
    }

    internal static void StealTreasure(Monument stone)
    {
        Instance.Team.Treasure += stone.Treasure;
        stone.Treasure = 0;

        SoundController.PlayStinger(SoundBank.Stinger.Sneaking);

        BigStoneView.CloseStone();

        //if (Random.value < 0.6f)
            stone.SpawnDead(Instance.Team);
    }


    internal static void SellFood(int amount, int price)
    {
        Instance.Team.Food -= amount;
        Instance.Team.Treasure += price;

        //TODO: play caching
    }

    internal static void SellGoblin(Goblin goblin, int price, GoblinWarrens newVillage)
    {
        Instance.Team.Members.Remove(goblin);
        Instance.Team.Treasure += price;

        GoblinUIList.UpdateGoblinList();

        goblin.Team = null;

        goblin.transform.parent = newVillage.transform;

        goblin.tag = "NPC";

        newVillage.Members.Add(goblin);
    }
    internal static void SacGoblin(Goblin goblin, Monument sacrificeStone)
    {
        Instance.Team.Members.Remove(goblin);
        
        SoundController.PlayStinger(SoundBank.Stinger.Sacrifice);

        goblin.Speak(SoundBank.GoblinSound.Death);

        goblin.Team = null;

        goblin.transform.parent = sacrificeStone.transform;

        goblin.tag = "Enemy";

        //goblin.CharacterRace = Character.Race.Undead;

        goblin.Health = 0;

        BigStoneView.CloseStone();
        
    }

    internal static void BuyGoblin(Goblin goblin, int price, GoblinWarrens oldVillage)
    {
        Instance.Team.Treasure -= price;

        goblin.Team = Instance.Team;
        
        //TODO: use method for these
        Instance.Team.Members.Add(goblin);
        goblin.transform.parent = Instance.Team.transform;
        goblin.tag = "Player";

        GoblinUIList.UpdateGoblinList();

        oldVillage.Members.Remove(goblin);
    }

    //TODO: move top camera controller
    public void Zoom(float deltaMagnitudeDiff, bool touch)
    {
        if(Math.Abs(deltaMagnitudeDiff) < 0.001) return;

        Cam.orthographicSize -= deltaMagnitudeDiff * (touch ?  ZoomSpeed: PcZoomSpeed);
        // set min and max value of Clamp function upon your requirement
        Cam.orthographicSize = Mathf.Clamp(Cam.orthographicSize, ZoomMinBound, ZoomMaxBound);
    }


    private void MoveToGoblin(Goblin g)
    {
        camMoveRoutine = StartCoroutine(MoveCamera(g.transform));
    }

    private IEnumerator MoveCamera(Transform loc)
    {
        //currentLocation = loc;
        var offset = 47;

        var start = Cam.transform.position;
        var startSize = Cam.orthographicSize;
        var endSize = FollowZoomSize;
        for (var t = 0f; t < MoveTime; t += Time.deltaTime)
        {
            yield return null;
            
            var xz = loc.position;
            var end = new Vector3(xz.x, Cam.transform.position.y, xz.z - offset);
            Cam.transform.position = Vector3.LerpUnclamped(start, end, MoveCurve.Evaluate(t / MoveTime));

            Cam.orthographicSize = Mathf.LerpUnclamped(startSize, endSize, MoveCurve.Evaluate(t / MoveTime));
        }

        while (FollowGoblin)
        {
            var xz = FollowGoblin.transform.position;
            Cam.transform.position = new Vector3(xz.x,Cam.transform.position.y,xz.z-offset);

            yield return null;
        }

        camMoveRoutine = null;
    }
    
}
