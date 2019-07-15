using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterView : MonoBehaviour
{
    public StatEntry XpTextEntry;
    public StatEntry HealthTextEntry;
    public StatEntry StatEntry;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI ClassLevelText;
    public TextMeshProUGUI EquipmentInfo;
    //public TextMeshProUGUI ClassSelectText;
    //public GameObject ClassSelectionHolder;
    [SerializeField]
    private Image ClassIcon = null;
    private Goblin character;
    private readonly List<GameObject> generatedObjects = new List<GameObject>(10);
    [SerializeField]
    private Button levelUpButton = null;
    [NotNull]
    public AiryUIAnimationManager ViewHolder;

    public GameObject EquipmentHolder;

    new void Awake()
    {
        levelUpButton.onClick.AddListener(OpenLevelUp);
        
    }

    private void OpenLevelUp()
    {
        LevelUpView.OpenLevelUp(character);
    }

    public  void SetCharacter(Goblin g)
    {
        character = g;
    }

    public void ShowCharacter()
    {
        //Debug.Log("Showing character: "+ c);

        showCharacter(character);
    }

    public void AddXp()
    {
        character.Xp += 10;
        showCharacter(character);
    }

    public  void Kill()
    {
        character.Kill();
    }
    
    protected void Open()
    {
        SoundController.PlayMenuPopup();

        ViewHolder.SetActive(true);
        //GameManager.Pause();
    }

    private void showCharacter(Goblin c)
    {
        Open();

        LevelUpView.CloseLevelUp();

        character = c;

        PlayerController.Follow(c);

        foreach (var generatedObject in generatedObjects)
        {
            Destroy(generatedObject);
        }
        generatedObjects.Clear();
        
        Name.text = c.name;
        var lvl = Goblin.GetLevel((int) c.Xp);
        ClassLevelText.text = "LVL " + lvl;// + " " + ClassName(c.ClassType);
        ClassLevelText.GetComponent<OnValueHover>().Class = c.ClassType;
        //ClassLevelText.gameObject.SetActive(!c.WaitingOnClassSelection || !c.Alive());
        //ClassSelectionHolder.SetActive(c.WaitingOnClassSelection && c.Alive());

        levelUpButton.interactable = c.WaitingOnLevelup();

        if (c.WaitingOnLevelup())//LevelUps > 0 || c.WaitingOnClassSelection)
        {
            XpTextEntry.Value.text = "LVL up";
            XpTextEntry.FillImage.fillAmount = 1;
            levelUpButton.interactable = true;
        }
        else
        {
            XpTextEntry.Value.text = $" {c.Xp:F0} / {Goblin.LevelCaps[lvl]}";
            XpTextEntry.FillImage.fillAmount = c.Xp / Goblin.LevelCaps[lvl];
        }
        HealthTextEntry.Value.text = $" {c.Health} / {c.HEA.GetStatMax()}";
        HealthTextEntry.FillImage.fillAmount = c.Health / (float) c.HEA.GetStatMax();
        //HealthTextEntry.ValueHover.Stat = c.HEA;
        ClassIcon.sprite = c.Alive() ? GameManager.GetClassImage(c.ClassType): GameManager.GetIconImage(GameManager.Icon.Dead);
        ClassIcon.GetComponent<OnValueHover>().Class = c.ClassType;
        
        //TODO: update current stats instead replacing
        foreach (var stat in c.Stats.Values)
        {
            var entry = Instantiate(StatEntry, StatEntry.transform.parent);

            var val = stat.GetStatMax();

            entry.Name.sprite = GameManager.GetAttributeImage(stat.Type);
            entry.Value.text = val.ToString();
            //var style = val > 3 ? FontStyles.Bold : FontStyles.Normal;
            var color = val > 3 ? entry.HighStatColor : val < 3 ? entry.LowStatColor : entry.Value.color;
            entry.Value.color = color;
            //entry.Name.fontStyle = entry.Value.fontStyle = style;
            entry.gameObject.SetActive(true);
            entry.ValueHover.Stat = stat;
            generatedObjects.Add(entry.gameObject);
            
        }

        if(EquipmentHolder)
            EquipmentHolder.SetActive(c.Equipped.Values.Any(v => v));

        //TODO: update current stats instead replacing
        foreach (var equipment in c.Equipped.Values.Where(v => v))
        {
            var entry = Instantiate(EquipmentInfo, EquipmentInfo.transform.parent);
            entry.text = equipment.EquipLocation + ": " + equipment.name;
            entry.gameObject.SetActive(true);
            var hover =entry.GetComponent<OnValueHover>();
            if (hover)
            {
                hover.ShowEquipment = true;
                hover.Equipment = equipment;
            }

            generatedObjects.Add(entry.gameObject);
        }

        StartCoroutine(CloseOnPan());
    }

    private IEnumerator CloseOnPan()
    {
        yield return new WaitUntil(() => !PlayerController.Instance.FollowGoblin);

        Close();
    }

    public void Close()
    {
        StartCoroutine(MarkAsClosedAfterAnimation());
    }

    private IEnumerator MarkAsClosedAfterAnimation()
    {
        ViewHolder.SetActive(false);

        yield return new WaitUntil(() => ViewHolder.AllHidden());

        //Debug.Log("Closing view: " +Type);
        
        //PlayerController.FollowGoblin = null;
    }

    private string ClassName(Goblin.Class cl)
    {
        switch (cl)
        {
            case Goblin.Class.Meatshield:
                return "Goblin Meatshield";
            case Goblin.Class.Shooter:
                return "Goblin Shooter";
            case Goblin.Class.Ambusher:
                return "Goblin Ambusher";
            case Goblin.Class.Scout:
                return "Goblin Scout";
            case Goblin.Class.Slave:
                return "Goblin Slave";
            case Goblin.Class.Necromancer:
                return "Goblin Necromancer";
            case Goblin.Class.Beastmaster:
                return "Goblin Beastmaster";
            case Goblin.Class.Hunter:
                return "Goblin Hunter";
            case Goblin.Class.Cook:
                return "Goblin Cook";
            case Goblin.Class.Shaman:
                return "Goblin Shaman";
            case Goblin.Class.Diplomat:
                return "Goblin Diplomat";
            case Goblin.Class.NoClass:
            case Goblin.Class.Goblin:
            case Goblin.Class.END:
            case Goblin.Class.ALL:
            default:
                return "Goblin";
        }
    }
    

    
}
