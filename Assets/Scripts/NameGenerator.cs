using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    public static NameGenerator Instance;

    private static string[] endings = new []{"k","p","ka","k","l","k","ki"};//g
    private static string[] vowels = new []{"i","u","o","o","a"};
    private static string[] startings = new []{"r","k","t","s","y","b","n","g","b"};//v, m "" no start only for singles and doubles
    private static string[] startingsWithNone = startings.Concat(new []{""}).ToArray();

    //private string[] surNames = new[] {"", " the Great", " Horse-killer", " the Beautiful", " Big-Bottom", " the Loud", " BIg-mind", " Ankle-shankER"," the many-TeEthed", " the IMmortal", " thE BIG", " Nose-CutTER", " thE Smart", " thE Hung", " the TalL", " supER-StabBer", " the RemEMBEreR", " GoOD-sTank", " the Smelly", " Big-Heart", " Large-FeEt", " smAlL-hands", " pretTy-EARS"};

    private static string[] surNameCompliments = new[]
    {
        "pretty", "strong", "great", "green", "loud", "smart", "small","sneaky", "beautiful", "many-teethed","fierce","scarred",
        "immortal", "dirty", "hungry", "muddy","heavy", "nasty", "greasy", "greedy", "sharp", "big", "good","smelly","silent","bad","dark","bright"
    };
    private static string[] surNameEnemies = new[] { "man","horse","orc","troll","spider","goat","elf","bear","bug","king","knight","dog","wolf","cow","pig","sheep","snake","book","word"};
    private static string[] surNameBodyParts = new[] {"mind","ear","ass","bottom","heart","foot","mouth","crotch","soul","marrow","bowel","head","shadow","tongue", "ankle","bone" };
    private static string[] surNameAttacks = new[] {"thrasher", "burner","breaker","shanker","killer", "slayer","stabber", "slitter","shooter","chopper","hacker","kicker","whacker","gnawer","slapper","basher","biter", "cutter", "eater", };
    


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

    private int TotalChance;

    // Use this for initialization
    void Start ()
    {
        if (!Instance)
            Instance = this;

        TotalChance = TwotimesSyllabalChance +RhymingSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance + ShortLongSyllabalChance;

        //for (int i = 0; i < 20; i++)
        //{
        //    Debug.Log(GetName()+GetSurName());
        //}
    }

    public static string GetName()
    {
        if(!Instance)
            return "Squee";

        var name = Instance.NameGen();

        return name.First().ToString().ToUpper() + name.Substring(1); ;
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