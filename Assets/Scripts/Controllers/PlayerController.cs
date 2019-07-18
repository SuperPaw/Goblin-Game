using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    [Header("References")]
    public PlayerTeam Team;
    public Camera Cam;
    public SoundController Sound;
    public Renderer FogOfWar;

    [Header("Controls")]
    public bool DragToPan;
    public bool ZoomEnabled;
    public int MouseMoveKey = 1;
    public enum MappableActions { Hide, Attack, Flee, Menu,FixCamOnLeader, Move, Camp,InvincibleMode, AddXp, ZoomIn, ZoomOut, Pause,Kill,RaiseDead } //TODO: move should contain direction maybe
    public LayerMask HitMask;

    [Serializable]
    public struct ActionOnStates
    {
        public MappableActions Action;
        public List<Character.CharacterState> EnabledOnLeaderStates;
    }

    [Serializable]
    public struct KeyMapping
    {
        public KeyCode Key;
        public MappableActions Action;
    }
    public KeyMapping[] KeyMappings;

    //TODO: move all shout stuff to seperate controller 
    [Serializable]
    public struct OrderType
    {
        public MappableActions Order;
        //TODO: create goblin speech struct for linking all goblin shouts with sounds
        public string Speech;
        public SoundBank.GoblinSound GoblinSound;
    }
    [Serializable]
    public struct Shout
    {
        public string Speech;
        public AudioClip GoblinSound;
    }


    [Serializable]
    public struct LocationReaction
    {
        public PointOfInterest.Poi LocationType;
        public Shout[] GoblinShouts;
    }

    [Serializable]
    public struct EnemyReaction
    {
        public Character.Race Race;
        public Shout[] GoblinShouts;
    }

    [Serializable]
    public struct StateChangeReaction
    {
        //Searching / Attacking / resting / Provoking
        public Character.CharacterState State;
        public Shout[] GoblinShouts;
    }

    [Serializable]
    public struct DynamicReaction
    {
        //Searching / Attacking / resting / Provoking
        public DynamicState State;
        public Shout[] GoblinShouts;
    }

    public enum DynamicState { Idle, ChiefBattleCheer, FoundStuff, Mocking, ChallengingChief}

    [Header("Order Controls")]
    public OrderType[] Orders;
    public OrderType MoveOrder;

    public List<ActionOnStates> EnabledActionOnStates;

    //TODO: use this
    public float OrderCooldown = 3f;

    public LocationReaction[] LocationReactions;
    public EnemyReaction[] EnemyReactions;
    public StateChangeReaction[] StateChangeReactions;
    public DynamicReaction[] DynamicReactions;
    
    private bool _mouseHeld;
    private Vector3 _mouseDragPos;

    [Header("Camera Controls")]
    public int GoblinViewSize;
    public int AreaViewSize;
    public int MapViewSize;
    public float ZoomMinBound= 2;
    public float ZoomMaxBound = 50;
    public float TouchZoomFactor;
    public float PcZoomFactor;
    public float PanSpeed = 1;
    public float ZoomSpeed = 1;

    public Goblin FollowGoblin;

    private Vector3 desiredCamPos;
    private float desiredOrtographicSize;
    
    private Vector2[] lastZoomPositions;
    private bool wasZoomingLastFrame;
    private int panFingerId;
    
    private float touchTime;

    public class ZoomLevelChangeEvent : UnityEvent<ZoomLevel> { }
    public static ZoomLevelChangeEvent OnZoomLevelChange = new ZoomLevelChangeEvent();

    public enum ZoomLevel {GoblinView, AreaView, MapView}
    private ZoomLevel currentZoomLevel;
    private bool showingMoveView;
    private Vector3 moveDelta;
    private float moveDamp = 0.85f;
    private readonly int FogPoints = 8;

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
        OnZoomLevelChange.AddListener(ChangeZoomLevel);
        ChangeZoomLevel(ZoomLevel.AreaView);
    }

    void Update()
    {
        if (!GameManager.Instance.GameStarted)
        {
            return;
        }

        //maybe at larger interval
        if(FollowGoblin)
            MoveToFollowGoblin();

        //TODO: divide these into methods
        if (Input.touchSupported)
        {
            HandleTouch();
        }
        else
        {
            HandleMouseKeys();
        }

        if (!_mouseHeld)
        {
            moveDelta *= moveDamp;
            desiredCamPos += moveDelta;
            Cam.transform.position = Vector3.Lerp(Cam.transform.position, desiredCamPos, PanSpeed * Time.unscaledDeltaTime);
        }

        Cam.orthographicSize = Mathf.Lerp(Cam.orthographicSize, desiredOrtographicSize, ZoomSpeed*Time.unscaledDeltaTime);

        if (Instance && Instance.Team)
            Instance.UpdateFogOfWar();
        
    }
    

    void HandleMouseKeys()
    {
        Zoom(Input.mouseScrollDelta.y, false);

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            PanCamera(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0))
        {
            _mouseHeld = false;
        }
        

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

                    //lastPanPosition = touch.position;
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

    private void ZoomOut()
    {
        if (currentZoomLevel > ZoomLevel.GoblinView) return;

        FollowGoblin = null;
        OnZoomLevelChange.Invoke( currentZoomLevel+1);
    }
    private void ZoomIn()
    {
        OnZoomLevelChange.Invoke(currentZoomLevel - 1);
    }

    private void ChangeZoomLevel(ZoomLevel newLevel)
    {
        if(newLevel == currentZoomLevel)
            return;
        if (newLevel > ZoomLevel.GoblinView)
            FollowGoblin = null;

        switch (newLevel)
        {
            case ZoomLevel.GoblinView:
                if (!FollowGoblin)
                    SetFollowGoblin(Team.Leader);
                break;
            case ZoomLevel.AreaView:
                MoveCamera(Team.Leader.InArea.transform.position, AreaViewSize);
                break;
            case ZoomLevel.MapView:
                MoveCamera(Team.Leader.InArea.transform.position, MapViewSize);
                break;
            default:
                return;
        }
        currentZoomLevel = newLevel;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
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
            if (hit.collider && hit.collider.GetComponent<Lootable>())
            {
                var loot = hit.collider.GetComponent<Lootable>();

                if (loot.InArea && loot.InArea.Visible() && !loot.InArea.AnyEnemies())
                    Team.Leader.Search(loot);
            }
            else if (hit.collider && hit.collider.GetComponent<Character>()) //TODO:check visibility else just move there
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
                            //ClickedArea(c.InArea);
                        }
                    }
                    else if (c as Goblin)
                    {
                        OnZoomLevelChange.Invoke(ZoomLevel.GoblinView);
                        (c as Goblin).CharacterUI.ShowCharacter();
                    }
                    else
                    {
                        //ClickedArea(c.InArea);
                    }
                }
            }
            else if (hit.collider && hit.collider.GetComponent<PointOfInterest>())
            {
                var v = hit.collider.GetComponent<PointOfInterest>();

                if (v.InArea.Visible())
                    v.SetupMenuOptions();
                //else
                  //  ClickedArea(v.InArea);
            }
            else if (hit.collider && hit.collider.GetComponent<Area>())
            {
                var a = hit.collider.GetComponent<Area>();
                //Debug.Log("Clicked Area : " + (a.name));

                //ClickedArea(a);
            }
            else
            {
                Debug.LogWarning("Not a valid hit: " + hit.point);
            }

        }
    }

    private void PanCamera(Vector2 newPanPosition)
    {
        if(!DragToPan)
            return;

        //To compensate for the 45 degree angle of the cam. TODO: Should have been calculated 
        newPanPosition.y *= 2f;

        var inWorld = Cam.ScreenToWorldPoint(new Vector3(newPanPosition.x, newPanPosition.y, Cam.nearClipPlane));

        //Sound.PlayMapCLick();
        if (!_mouseHeld)
        {
            _mouseDragPos =inWorld;
            _mouseHeld = true;
        }
        
        moveDelta = (_mouseDragPos - inWorld);
        moveDelta.y = 0;

        //TODO: the z axis does not move correctly due to the rotation of the camera

        desiredCamPos += moveDelta;
        //Outcomment to enable camera wiggle!
        Cam.transform.position = desiredCamPos;

        FollowGoblin = null;
        
    }
    

    //TODO: only from button click
    public void ClickedArea(Area a)
    {
        if (!ActionIsLegal(MappableActions.Move))
        {
            Debug.LogWarning("Moving should not be allowed now.");
            ZoomIn();
            return;
        }

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
        } //TODO: replace with ready for order
        else if(!Team.Leader.Fleeing() || Team.Leader.Attacking())
        {
            Team.LeaderShout(MoveOrder);
            
            Team.Move(a);

            OnZoomLevelChange.Invoke(ZoomLevel.AreaView);
            SetFollowGoblin(Team.Leader);
        }
    }

    public static void UpdateFog()
    {
        if(Instance && Instance.Team && GameManager.Instance.GameStarted)
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

        RemoveFogAtPos(Team.Leader.transform.position, id++);
        //RemoveFogAtPos(Team.Leader.InArea.transform.position, id++);
        Team.Leader.InArea.RemoveFogOfWar(true);
        //if (showingMoveView)
        //{
        //    foreach (var n in Team.Leader.InArea.Neighbours)
        //    {
        //        RemoveFogAtPos(n.transform.position, id++);
        //        n.RemoveFogOfWar(false);
        //    }
        //}

        //removing unused fog points TODO: make it actually remove instead of just hiding
        for (; id <= FogPoints; id++)
        {
            RemoveFogAtPos(new Vector3(), id,true);
        }

    }

    private void RemoveFogAtPos(Vector3 pos, int id,bool unused = false)
    {
        if (id < 1 || id > FogPoints)
        {
            Debug.Log("Unknown shader id: " + id);
            return;
        }

        if (unused)
        {
            FogOfWar.material.SetVector("_Player" + id.ToString() + "_Pos",new Vector4(-100,-100,-100,-100));
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
        //TODO: check for order legality
        if (!ActionIsLegal(action))
        {
            Debug.Log("Trying to call illegal aciton: "+ action);
            return;
        }

        if (Orders.Any(o => o.Order == action))
            Team.LeaderShout(Orders.First(o => o.Order == action));

        Team.OnOrder.Invoke();

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
                SetFollowGoblin(Team.Leader);
                break;
            case MappableActions.Camp:
                Team.Camp();
                break;
            case MappableActions.InvincibleMode:
                GameManager.Instance.InvincibleMode = !GameManager.Instance.InvincibleMode;
                break;
            case MappableActions.AddXp:
                FollowGoblin?.CharacterUI.AddXp();
                break;
            case MappableActions.Move:
                if (showingMoveView)
                    ZoomIn();
                else
                    MoveView();
                break;
            case MappableActions.ZoomIn:
                ZoomIn();
                break;
            case MappableActions.ZoomOut:
                ZoomOut();
                break;
            case MappableActions.Pause:
                if(GameManager.Instance.GamePaused)
                    GameManager.UnPause();
                else
                    GameManager.Pause();
                break;
            case MappableActions.Kill:
                FollowGoblin?.Kill();
                break;
            case MappableActions.RaiseDead:
                StartCoroutine(Team.RaiseDead());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public bool ReadyForInput()
    {
        if (!Team.Leader)
            return false;

        return Team.Leader.Idling();
    }

    public static bool ActionIsLegal(MappableActions action)
    {
        if (!Instance.EnabledActionOnStates.Any(e => e.Action == action))
        {
            Debug.Log(action + " not yet mapped to legal leader states");
            return true;
        }

        switch (action)
        {
            case MappableActions.RaiseDead:
                return Instance.EnabledActionOnStates.First(e => e.Action == action).EnabledOnLeaderStates
                           .Contains(Instance.Team.Leader.State) && Instance.Team.Leader.ClassType == Goblin.Class.Necromancer && Instance.Team.Leader.InArea.AnyGoblins(true);
            case MappableActions.Hide:
                return Instance.EnabledActionOnStates.First(e => e.Action == action).EnabledOnLeaderStates
                           .Contains(Instance.Team.Leader.State) && Instance.Team.Leader.InArea.RoadsTo.Any()
                       & !Instance.Team.Leader.InArea.AnyEnemies();
            case MappableActions.Camp:
                return Instance.EnabledActionOnStates.First(e => e.Action == action).EnabledOnLeaderStates.Contains(Instance.Team.Leader.State) 
                       & !Instance.Team.Leader.InArea.ContainsRoads
                       & !Instance.Team.Leader.InArea.PointOfInterest
                       & !Instance.Team.Leader.InArea.AnyEnemies();
            default:
                return Instance.EnabledActionOnStates.First(e => e.Action == action).EnabledOnLeaderStates
                    .Contains(Instance.Team.Leader.State);
        }
    }

    //TODO: move top camera controller
    public void Zoom(float deltaMagnitudeDiff, bool touch)
    {
        if(!ZoomEnabled) return;

        if(Math.Abs(deltaMagnitudeDiff) < 0.001) return;

        if (Cam.orthographicSize >= ZoomMaxBound - 0.2f && deltaMagnitudeDiff * (touch ? TouchZoomFactor : PcZoomFactor) < -0.4f)
        {
            if(ActionIsLegal(MappableActions.Move))
                MoveView();
            return;
        }

        if (currentZoomLevel > ZoomLevel.AreaView) currentZoomLevel = ZoomLevel.AreaView;

        desiredOrtographicSize -= deltaMagnitudeDiff * (touch ?  TouchZoomFactor: PcZoomFactor);
        // set min and max value of Clamp function upon your requirement

        desiredOrtographicSize = Mathf.Clamp(desiredOrtographicSize, ZoomMinBound, ZoomMaxBound);
    }

    public static void Follow(Goblin g)
    {
        Instance.SetFollowGoblin(g);
    }

    public void SetFollowGoblin(Goblin g)
    {
        if(FollowGoblin != g) FollowGoblin?.CharacterUI.Close();

        FollowGoblin = g;

        currentZoomLevel = ZoomLevel.GoblinView;
        desiredOrtographicSize = GoblinViewSize;
    }

    public static void MoveCameraToPos(Vector3 pos,int ortoSize)
    {
        Instance.MoveCamera(pos,ortoSize);
    }


    private void MoveToFollowGoblin()
    {
        var offset = 51f;

        var xz = FollowGoblin.transform.position;
        desiredCamPos = new Vector3(xz.x, Cam.transform.position.y, xz.z - offset);
    }

    private void MoveCamera(Vector3 xz, int orthographicSize)
    {
        //currentLocation = loc;
        var offset = 49;
        
        desiredCamPos = new Vector3(xz.x, Cam.transform.position.y, xz.z - offset);
        desiredOrtographicSize = orthographicSize;
    }

    private void MoveView()
    {
        var l = Team.Leader.InArea.Neighbours.ToList();

        var scouts = Team.Members.Count(m => m.ClassType == Goblin.Class.Scout);

        foreach (var movable in l)
        {
            if(movable.Visited)
                movable.EnableAreaUI(true);
            else
                movable.EnableAreaUI(scouts-- >0);
        }

        showingMoveView = true;

        OnZoomLevelChange.Invoke(ZoomLevel.MapView);

        StartCoroutine(DisableAreaUIOnAction(l));
    }

    private IEnumerator DisableAreaUIOnAction(List<Area> toDisable)
    {
        yield return new WaitUntil(() => currentZoomLevel != ZoomLevel.MapView || !ActionIsLegal(MappableActions.Move));

        if(currentZoomLevel == ZoomLevel.MapView)
            ZoomIn();
        
        showingMoveView = false;

        toDisable.ForEach(d => d.DisableAreaUI());
    }

    private T GetRandom<T>(T[] arr) => arr[Random.Range(0, arr.Length)];

    public static Shout GetLocationReaction(PointOfInterest.Poi Type)
    {
        return Instance.GetRandom(Instance.LocationReactions.First(l=> l.LocationType == Type).GoblinShouts);
    }
    public static Shout GetEnemyReaction(Character.Race Type)
    {
        return Instance.GetRandom(Instance.EnemyReactions.First(l => l.Race == Type).GoblinShouts);
    }
    public static Shout GetStateChangeReaction(Character.CharacterState Type)
    {
        return Instance.GetRandom(Instance.StateChangeReactions.First(l => l.State == Type).GoblinShouts);
    }
    public static bool IsStateChangeShout(Character.CharacterState Type)
    {
        return Instance.StateChangeReactions.Any(l => l.State == Type);
    }

    public static Shout GetDynamicReactions(DynamicState trigger)
    {
        return Instance.GetRandom(Instance.DynamicReactions.First(l => l.State == trigger).GoblinShouts);
    }
}
