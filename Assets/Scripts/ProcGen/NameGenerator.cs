using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class NameGenerator : MonoBehaviour
{
    public static NameGenerator Instance;

    private static string[] endings = new[] {"k", "p", "ka", "k", "l", "k", "ki"}; //g
    private static string[] specialEndings = new[] { "zo", "zak","ox","ax"}; //g
    private static string[] vowels = new[] {"i", "u", "o", "o", "a"};

    private static string[] startings = new[] {"r", "k", "t", "s", "y", "b", "n", "g", "b"}
        ; //v, m "" no start only for singles and doubles

    private static string[] startingsWithNone = startings.Concat(new[] {""}).ToArray();

    //private string[] surNames = new[] {"", " the Great", " Horse-killer", " the Beautiful", " Big-Bottom", " the Loud", " BIg-mind", " Ankle-shankER"," the many-TeEthed", " the IMmortal", " thE BIG", " Nose-CutTER", " thE Smart", " thE Hung", " the TalL", " supER-StabBer", " the RemEMBEreR", " GoOD-sTank", " the Smelly", " Big-Heart", " Large-FeEt", " smAlL-hands", " pretTy-EARS"};

    private static string[] surNameCompliments = new[]
    {
        "pretty", "strong", "great", "green", "loud", "smart", "small", "sneaky", "beautiful", "many-teethed", "fierce",
        "scarred", "half", "stabby",
        "immortal", "dirty", "hungry", "muddy", "heavy", "nasty", "greasy", "greedy", "sharp", "big", "good", "smelly",
        "silent", "bad", "dark", "bright"
    };

    private static string[] surNameEnemies = new[]
    {
        "man", "horse", "orc", "troll", "spider", "goat", "elf", "bear", "bug", "king", "knight", "dog", "wolf", "cow",
        "pig", "sheep", "snake", "book", "word"
    };

    private static string[] surNameBodyParts = new[]
    {
        "mind", "ear", "ass", "bottom", "heart", "foot", "mouth", "crotch", "soul", "marrow", "bowel", "head", "shadow",
        "tongue", "ankle", "bone"
    };

    private static string[] food = new[]
    {
        "goo", "meat", "intestines", "heart", "liver", "eyes", "tongue", "food"
    };

    private static string[] surNameAttacks = new[]
    {
        "thrasher", "burner", "breaker", "shanker", "killer", "slayer", "stabber", "slitter", "shooter", "chopper",
        "hacker", "kicker", "whacker", "gnawer", "slapper", "gnasher", "basher", "biter", "cutter", "eater",
    };


    /// TREASURE STUFF  --- should be inedible
    private enum treasureTypes { RACEPART, ANIMALPART,BIRDPART,THING,BURNEDBOOK,EMPTYCONTAINER,LOOKSLIKE,STATUE,SMALLSKELETON}
    private static string[] treasureAdjectives = new[] { "strange","big", "small", "heavy", "greasy", "golden", "slimy", "dirty", "muddy", "stained", "broken", "pretty", "beautiful"};
    private static string[] races = new[] { "human","orc","troll","elf","goblin","fishman","birdman","lizardman" };
    private static string[] amimals = new[] { "monkey", "bear", "badger", "cat", "horse", "boar", "dog", "spider", "rat", "wolf"};
    private static string[] smallAnimal = new[] { "rat", "spider", "cat", "dog", "toad", "monkey", "bird", "mouse" };
    private static string[] birds = new[] { "owl", "birdman", "chicken", "raven", "parrot", "crow"};
    private static string[] bodyParts = new[] { "foot", "hand", "tooth", "skull", "leg", "arm","skin","bone" };
    private static string[] animalParts = new[] { "skull", "claw","tooth", "fur",  "bone","horn" };
    private static string[] birdParts = new[] { "skull", "claw","feather", "beak", "bone","wing"};
    private static string[] colors = new[] { "red","blue","black","grey","brown","golden","green","yellow" };
    private static string[] magicAttributes = new[] { "shiny","glowing","magic","engraved","radiant","shiny" };
    private static string[] fancyStuff = new[] {"ring", "coin", "crystal","crown","armband"};
    private static string[] stuff = new[] { "rock", "stone", "stick","branch","leaf" };
    private static string[] containers = new[] { "bottle","bag","chest"};
    private static string[] burnedStuff = new[] { "book", "piece of paper", "scroll" };
    private static string[] statues = new[] { "statue", "figure", "effigy","puppet" };


    //chance for double
    [Range(1, 10)]
    public int TwotimesSyllabalChance;
    //chance for double
    [Range(1, 10)]
    public int RhymingSyllabalChance;
    //chance for single sylabyl
    [Range(1, 10)]
    public int SingleSyllabalChance;
    //chance for mix
    [Range(1, 10)]
    public int DoubleSyllabalChance;
    //chance for special start syllabyl
    [Range(1, 10)]
    public int ShortLongSyllabalChance;
    [Range(1, 10)]
    public int SpecialEndingChance;

    private int TotalChance;

    // Use this for initialization
    void Awake ()
    {
        if (!Instance)
            Instance = this;

        TotalChance = TwotimesSyllabalChance +RhymingSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance + ShortLongSyllabalChance + SpecialEndingChance;

        //for (int i = 0; i < 20; i++)
        //{
        //    Debug.Log(GetName() + GetSurName());
        //}

        //for (int i = 0; i < 20; i++)
        //{
        //    Debug.Log("a "+GetTreasureName());
        //}
    }

    public static string GetName()
    {
        if(!Instance)
            return "Squee";

        var name = Instance.NameGen();

        return name.First().ToString().ToUpper() + name.Substring(1); ;
    }



    public static string GetFoodName()
    {
        if (!Instance)
            return "sausage";
        
        return Rnd(food);
    }



    public static string GetSurName()
    {
        if (!Instance)
            return " the forGotTen";

        var val = Random.value;

        if (val < 0.25f)
        {
            //The great
            return " the " + Rnd(surNameCompliments);
        }
        else if (val < 0.5f)
        {
            //big-ass
            return " " + Rnd(surNameCompliments) + "-" + Rnd(surNameBodyParts);
        }
        else if (val < 0.75f)
        {
            //ass-whackker
            return " " + Rnd(surNameBodyParts) + "-" + Rnd(surNameAttacks);
        }
        else
        {
            //elf-stabber
            return " " + Rnd(surNameEnemies) + "-" + Rnd(surNameAttacks);
        }
    }


    public static string GetTribeIntro(string chiefName)
    {
        return chiefName + " came, we saw, we ran";
    }


    public static string GetTreasureName(string randomGoblinName = "old chief")
    {
        string treasure = "";
        Array values = Enum.GetValues(typeof(treasureTypes));
        var type = (treasureTypes)values.GetValue((int)(Random.value * (values.Length)));

        //attributes: MAGIC and COLOR
        switch (type)
        {
            case treasureTypes.RACEPART:
                treasure = Rnd(races) + " " + Rnd(bodyParts);
                treasure = AddAttribute(treasure, magicAttributes, 0.3f);
                break;
            case treasureTypes.ANIMALPART:
                treasure = Rnd(amimals) + " " + Rnd(animalParts);
                treasure = AddAttribute(treasure, magicAttributes, 0.3f);
                break;
            case treasureTypes.BIRDPART:
                treasure = Rnd(birds) + " " + Rnd(birdParts);
                treasure = AddAttribute(treasure, magicAttributes, 0.3f);
                break;
            case treasureTypes.THING:
                treasure = Random.value > 0.4f ? Rnd(fancyStuff) : Rnd(stuff);
                treasure = AddAttribute(treasure, colors, 0.3f);
                treasure = AddAttribute(treasure, magicAttributes, 0.5f);
                break;
            case treasureTypes.BURNEDBOOK:
                treasure = "burned "+ Rnd(burnedStuff);
                treasure = AddAttribute(treasure, colors, 0.25f);
                break;
            case treasureTypes.EMPTYCONTAINER:
                treasure = "empty " + Rnd(containers);
                treasure = AddAttribute(treasure, colors, 0.4f);
                break;
            case treasureTypes.LOOKSLIKE:
                treasure = Rnd(stuff) + " that look like " + randomGoblinName;
                treasure = AddAttribute(treasure, magicAttributes, 0.15f);
                break;
            case treasureTypes.STATUE:
                var val = Random.value;
                //animals, birds, smallanimals, races, bodyparts
                if (val < 0.2)
                {
                    treasure = Rnd(amimals);
                }
                else if (val < 0.4)
                {
                    treasure = Rnd(birds);
                }
                else if (val < 0.6)
                {
                    treasure = Rnd(smallAnimal);
                }
                else 
                {
                    treasure = Rnd(races);
                }

                treasure = treasure + " " + Rnd(statues);
                treasure = AddAttribute(treasure, colors, 0.15f);
                treasure = AddAttribute(treasure, magicAttributes, 0.3f);
                break;
            case treasureTypes.SMALLSKELETON:
                treasure = Rnd(smallAnimal) + " skeleton";
                treasure = AddAttribute(treasure, magicAttributes, 0.2f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        treasure = AddAttribute(treasure, treasureAdjectives,0.55f);

        return treasure;
    }

    #region Private Methods

    private string NameGen()
    {
        var valRange = Random.Range(0, TotalChance);

        if (valRange < TwotimesSyllabalChance)
        {
            var syllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);

            return syllabal + syllabal;
        }
        else if (valRange < TwotimesSyllabalChance + RhymingSyllabalChance)
        {
            var ending = Rnd(vowels) + Rnd(endings);

            var syllabal = Rnd(startings) + ending;
            var sndSyllabal = Rnd(startings) + ending;

            return syllabal + sndSyllabal;
        }
        else if (valRange < TwotimesSyllabalChance + RhymingSyllabalChance + SingleSyllabalChance)
        {
            var syllabal = Rnd(startingsWithNone) + Rnd(vowels) + Rnd(endings);

            return syllabal ;
        }
        else if (valRange < TwotimesSyllabalChance + RhymingSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance)
        {
            var start = Rnd(startings);
            var end = Rnd(endings);

            var syllabal = start+ Rnd(vowels) + end;
            var sndSyllabal = start+ Rnd(vowels) + end;

            return syllabal+sndSyllabal;
        }
        else if (valRange < TwotimesSyllabalChance + RhymingSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance + SpecialEndingChance)
        {
            return Rnd(startings) + Rnd(vowels) + Rnd(endings) + Rnd(specialEndings);
        }
        else 
        {

            var syllabal = Rnd(startingsWithNone) + Rnd(vowels) ;
            var sndSyllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);

            return syllabal + sndSyllabal;
        }


    }

    private static string Rnd( string[] arr)
    {
        return arr[Random.Range(0, arr.Length)];
    }

    private static string AddAttribute(string thing,string[] attributes, float chance = 1f)
    {
        if (Random.value > chance)
            return thing;

        return Rnd(attributes) + " " + thing;
    }

    #endregion
}

//TukTuk
//YipYip
//NokNok
//RikShak
//RikRik
//ShakShak
//BopBop
//TokTok
//Rok
//Rak
//Ruk

//Na
//Yi


//chance for double
//chance for single sylabyl
//chance for mix
//chance for start syllabyl