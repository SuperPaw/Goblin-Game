using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NameGenerator : MonoBehaviour
{
    public static NameGenerator Instance;

    public string[] endings = new []{"k","p","ka","k","l","k","ki"};
    public string[] vowels = new []{"i","u","o","o","a"};
    public string[] startings = new []{"","r","k","t","sh","y","b","n"};

    //public string[] prenames; are just without vowels

    //chance for double
    [Range(1, 10)]
    public int TwotimesSyllabalChance;
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

        TotalChance = TwotimesSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance + ShortLongSyllabalChance;

        for (int i = 0; i < 20; i++)
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

    private string NameGen()
    {
        var valRange = Random.Range(0, TotalChance);

        if (valRange < TwotimesSyllabalChance)
        {
            var syllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);

            return syllabal + syllabal;
        }
        else if (valRange < TwotimesSyllabalChance + SingleSyllabalChance)
        {
            var syllabal = Rnd(startings) + Rnd(vowels) + Rnd(endings);

            return syllabal ;
        }
        else if (valRange < TwotimesSyllabalChance + SingleSyllabalChance + DoubleSyllabalChance)
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