﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using BayatGames.SaveGameFree.Encoders;
using BayatGames.SaveGameFree.Serializers;
using BayatGames.SaveGameFree.Types;

namespace BayatGames.SaveGameFree
{
	[AddComponentMenu ( "Save Game Free/Auto Save" )]
	public class SaveController : MonoBehaviour
	{
		public enum SaveFormat
		{
			XML,
			JSON,
			Binary
		}


		[Header ( "Settings" )]
		[Space]


		[Tooltip ( "You must specify a value for this to be able to save it." )]
		public string highscoreIdentifier = "enter the highscore identifier";

		[Tooltip ( "You must specify a value for this to be able to save it." )]
		/// <summary>
		/// The rotation identifier.
		/// </summary>
		public string legacyIdentifier = "enter the rotation identifier";

		[Tooltip ( "You must specify a value for this to be able to save it." )]
		/// <summary>
		/// The scale identifier.
		/// </summary>
		public string scaleIdentifier = "enter the scale identifier";

		[Tooltip ( "Encode the data?" )]
		/// <summary>
		/// The encode.
		/// </summary>
		public bool encode = false;

		[Tooltip ( "If you leave it blank this will reset to it's default value." )]
		/// <summary>
		/// The encode password.
		/// </summary>
		public string encodePassword = "";

		[Tooltip ( "Which serialization format?" )]
		public SaveFormat format = SaveFormat.JSON;

		[Tooltip ( "If you leave it blank this will reset to it's default value." )]
		/// <summary>
		/// The serializer.
		/// </summary>
		public ISaveGameSerializer serializer;

		[Tooltip ( "If you leave it blank this will reset to it's default value." )]
		/// <summary>
		/// The encoder.
		/// </summary>
		public ISaveGameEncoder encoder;

		[Tooltip ( "If you leave it blank this will reset to it's default value." )]
		/// <summary>
		/// The encoding.
		/// </summary>
		public Encoding encoding;

		[Tooltip ( "Where to save? (PersistentDataPath highly recommended)." )]
		/// <summary>
		/// The save path.
		/// </summary>
		public SaveGamePath savePath = SaveGamePath.PersistentDataPath;

		[Tooltip ( "Reset the empty fields to their default value." )]
		/// <summary>
		/// The reset blanks.
		/// </summary>
		public bool resetBlanks = true;


		[Header ( "What to Save?" )]
		[Space]


		[Tooltip ( "Save Scores?" )]
		public bool saveHighscores = true;

		[Tooltip ( "Save Rotation?" )]
		/// <summary>
		/// The save rotation.
		/// </summary>
		public bool saveLegacy = true;
        
		[Header ( "Defaults" )]
		[Space]


		[Tooltip ( "Default Position Value" )]
		/// <summary>
		/// The default position.
		/// </summary>
		public Vector3 defaultPosition = Vector3.zero;

		[Tooltip ( "Default Rotation Value" )]
		/// <summary>
		/// The default rotation.
		/// </summary>
		public Vector3 defaultRotation = Quaternion.identity.eulerAngles;

		[Tooltip ( "Default Scale Value" )]
		/// <summary>
		/// The default scale.
		/// </summary>
		public Vector3 defaultScale = Vector3.one;


		[Header ( "Save Events" )]
		[Space]


		[Tooltip ( "Save on Awake()" )]
		/// <summary>
		/// The save on awake.
		/// </summary>
		public bool saveOnAwake;

		[Tooltip ( "Save on Start()" )]
		/// <summary>
		/// The save on start.
		/// </summary>
		public bool saveOnStart;

		[Tooltip ( "Save on OnEnable()" )]
		/// <summary>
		/// The save on enable.
		/// </summary>
		public bool saveOnEnable;

		[Tooltip ( "Save on OnDisable()" )]
		/// <summary>
		/// The save on disable.
		/// </summary>
		public bool saveOnDisable = true;

		[Tooltip ( "Save on OnApplicationQuit()" )]
		/// <summary>
		/// The save on application quit.
		/// </summary>
		public bool saveOnApplicationQuit = true;

		[Tooltip ( "Save on OnApplicationPause()" )]
		/// <summary>
		/// The save on application pause.
		/// </summary>
		public bool saveOnApplicationPause;


		[Header ( "Load Events" )]
		[Space]


		[Tooltip ( "Load on Awake()" )]
		/// <summary>
		/// The load on awake.
		/// </summary>
		public bool loadOnAwake;

		[Tooltip ( "Load on Start()" )]
		/// <summary>
		/// The load on start.
		/// </summary>
		public bool loadOnStart = true;

		[Tooltip ( "Load on OnEnable()" )]
		/// <summary>
		/// The load on enable.
		/// </summary>
		public bool loadOnEnable = false;

		protected virtual void Awake ()
		{
			if ( resetBlanks )
			{
				if ( string.IsNullOrEmpty ( encodePassword ) )
				{
					encodePassword = SaveGame.EncodePassword;
				}
				if ( serializer == null )
				{
					serializer = SaveGame.Serializer;
				}
				if ( encoder == null )
				{
					encoder = SaveGame.Encoder;
				}
				if ( encoding == null )
				{
					encoding = SaveGame.DefaultEncoding;
				}
			}
			switch ( format )
			{
				case SaveFormat.Binary:
					serializer = new SaveGameBinarySerializer ();
					break;
				case SaveFormat.JSON:
					serializer = new SaveGameJsonSerializer ();
					break;
				case SaveFormat.XML:
					serializer = new SaveGameXmlSerializer ();
					break;
			}
			if ( loadOnAwake )
			{
				Load ();
			}
			if ( saveOnAwake )
			{
				Save ();
			}
		}

		protected virtual void Start ()
		{
			if ( loadOnStart )
			{
				Load ();
			}
			if ( saveOnStart )
			{
				Save ();
			}
		}

		protected virtual void OnEnable ()
		{
			if ( loadOnEnable )
			{
				Load ();
			}
			if ( saveOnEnable )
			{
				Save ();
			}
		}

		protected virtual void OnDisable ()
		{
			if ( saveOnDisable )
			{
				Save ();
			}
		}

		protected virtual void OnApplicationQuit ()
		{
			if ( saveOnApplicationQuit )
			{
				Save ();
			}
		}

		protected virtual void OnApplicationPause ()
		{
			if ( saveOnApplicationPause )
			{
				Save ();
			}
		}

	    public void SaveHighscores()
	    {
	        SaveGame.Save(
	            highscoreIdentifier, GreatestGoblins.GetScores(),
	            encode,
	            encodePassword,
	            serializer,
	            encoder,
	            encoding,
	            savePath
	        );
        }

	    public void SaveLegacy()
	    {
	        SaveGame.Save(
	            legacyIdentifier,
	            LegacySystem.GetAchievements(),
	            encode,
	            encodePassword,
	            serializer,
	            encoder,
	            encoding,
	            savePath);
        }

		/// <summary>
		/// Save this instance.
		/// </summary>
		public virtual void Save ()
		{
			if ( saveHighscores )
			{
                SaveHighscores();
			}
			if ( saveLegacy )
			{
                SaveLegacy();
			}
		}

		/// <summary>
		/// Load this instance.
		/// </summary>
		public virtual void Load ()
		{
			if ( saveHighscores )
			{
			    if(SaveGame.Exists(highscoreIdentifier))
                    GreatestGoblins.SetScores (SaveGame.Load<List<GreatestGoblins.Score>> ( 
					    highscoreIdentifier
                        ));
			}
			if ( saveLegacy )
			{
                if(SaveGame.Exists(legacyIdentifier))
                    LegacySystem.SetAchievements(SaveGame.Load<List<LegacySystem.Achievement>>(legacyIdentifier));
			}
		}

	    public static bool SaveTest()
	    {
	        var rndValue = Random.Range(100, 100000);
	        var identifier = "testvalue";

	        SaveGame.Save(
	            identifier, rndValue
	        );

	        var loadedValue = SaveGame.Load<int>(identifier);
            
            return rndValue == loadedValue;
	    }
	}

}