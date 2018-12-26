using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    public static NameGenerator Instance;

    private string[] endings = new []{"k","p","ka","k","l","k","ki"};
    private string[] vowels = new []{"i","u","o","o","a"};
    private string[] startings = new []{"","r","k","t","sh","y","b","n"};
    private string[] surNames = new[] {"", " the Great", " Horse-killer", " the Beautiful", " Big-Bottom", " the Loud", " BIg-mind", " Ankle-shankER"," the many-TeEthed", " the IMmortal", " thE BIG", " Nose-CutTER", " thE Smart", " thE Hung", " the TalL", " supER-StabBer", " the RemEMBEreR", " GoOD-sTank", " the Smelly", " Big-Heart", " Large-FeEt", " smAlL-hands", " pretTy-EARS"};

    //public string[] prenames; are just without vowels


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

        for (int i = 0; i < 10; i++)
        {
            Debug.Log(GetName());
        }
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
        
        return Rnd(Instance.surNames); ;
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
            var syllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);

            return syllabal ;
        }
        else if (valRange < TwotimesSyllabalChance + RhymingSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance)
        {

            var syllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);
            var sndSyllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);

            return syllabal+sndSyllabal;
        }
        else 
        {

            var syllabal = Rnd(startings) + Rnd(vowels) ;
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