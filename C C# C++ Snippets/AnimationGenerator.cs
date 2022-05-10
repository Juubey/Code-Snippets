/*
 * AnimationGenerator.cs
 * Author(s): Albert Njubi
 * Date Created: 8/30/16
 */

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using AnimationGeneratorInfo;
using SimpleJSON;

/// <summary>
/// This class generates AnimationPlayerInfo from the animations JSON file,
/// </summary>
public class AnimationGenerator : Singleton<AnimationGenerator>
{

    #region Constants

    private const string idJSONKey = JSONKeys.id; 
    private const string fpsJSONKey = JSONKeys.fps;
    private const string loopJSONKey = JSONKeys.loopCount;
    private const string nameJSONKey = JSONKeys.name;
    private const string scaleJSONKey = JSONKeys.scaleOffset;
    private const string fpsIDJSONKey = JSONKeys.fpsID;
    private const string audioJSONKey = JSONKeys.audioID;
    private const string spriteJSONKey = JSONKeys.spriteID;
    private const string subFolderJSONKey = JSONKeys.subFolder;
    private const string animationsJSONKey = JSONKeys.animations;
    private const string audioEventsJSONKey = JSONKeys.audioEvents;
    private const string subFolderIDJSONKey = JSONKeys.subFolderID;
    private const string animationIDJSONKey = JSONKeys.animationID;
    private const string audioEventIDJSONKey = JSONKeys.audioEventID;
    private const string animationKindJSONKey = JSONKeys.animationKind;

    #endregion


    #region Private Variables

    private Dictionary<int, float> fpsBook;                          //A book of fps values
    private Dictionary<int, string> subFolderBook;                   //A book of subfolder directories containing animation Sprites
    private Dictionary<int, List<string>> fileNameBook;              //A book of string lists of Sprite file names in each animation subfolder.
    private Dictionary<int, AnimationAudioEventInfo> audioEventBook; //A book of information related to audio that plays during animations
    private Dictionary<int, AnimationPlayerInfo> animationInfoBook;  //A book of info used to build AnimationPlayers
    private Dictionary<AnimationType, int> animationIDBook;          //A book of animationID's to for AnimationKind access
    private TextAsset animationTxt;

    #endregion


    #region Public Functions

    /// <summary>
    /// Adds a fully functional AnimationPlayer component to a GameObject on the Unity canvas
    /// Returns a reference to the newly created AnimationPlayer
    /// </summary>
    public AnimationPlayer BuildAnimationPlayer(AnimationType kind)
    {
        if (!animationIDBook.ContainsKey(kind))
        {
            Debug.LogError("Error building BuildAnimationPlayer, AnimationKind." + kind.ToString() + " is not associated with an animation.");
            return null;
        }
        return BuildAnimationPlayer(animationIDBook[kind]);
    }


    /// <summary>
    /// Adds a fully functional AnimationPlayer component to a GameObject on the Unity canvas
    /// Returns a reference to the newly created AnimationPlayer
    /// </summary>
    public AnimationPlayer BuildAnimationPlayer(int animationID)
    {
        if(animationInfoBook == null)
        {
            return null;
        }
        if (!animationInfoBook.ContainsKey(animationID))
        {
            Debug.LogError("Error building BuildAnimationPlayer, AnimationID " + animationID.ToString() + " is not associated with an animation.");
            return null;
        }
        AnimationPlayerInfo info = animationInfoBook[animationID];
        return BuildAnimationPlayer(info);
    }


    /// <summary>
    /// Adds a fully functional AnimationPlayer component to a GameObject on the Unity canvas
    /// Returns a reference to the newly created AnimationPlayer
    /// </summary>
    private AnimationPlayer BuildAnimationPlayer(AnimationPlayerInfo info)
    {
        GameObject child = new GameObject();
        child.AddComponent<Image>();
        TransformHelper.Scale(child.transform, info.ScaleOffset);
        Image image = child.GetComponent<Image>();

        child.AddComponent<AnimationPlayer>();
        AnimationPlayer player = child.GetComponent<AnimationPlayer>();

        image.preserveAspect = true;
        image.raycastTarget = true;
        player.Initialize(image, info);
        return player;
    }

    #endregion


    #region Private Functions


    // Use this for initialization
    private void Start()
    {
        fpsBook = new Dictionary<int, float>();
        subFolderBook = new Dictionary<int, string>();
        audioEventBook = new Dictionary<int, AnimationAudioEventInfo>();
        animationInfoBook = new Dictionary<int, AnimationPlayerInfo>();
        fileNameBook = new Dictionary<int, List<string>>();
        animationIDBook = new Dictionary<AnimationType, int>();



        animationTxt = Tuning.Instance.animations;
        JSONNode N = JSON.Parse(animationTxt.ToString());
        InitSubFolders(N[subFolderJSONKey].AsArray);
        InitAudioEvents(N[audioEventsJSONKey].AsArray);
        InitFPS(N[fpsJSONKey].AsArray);
        InitAnimationKinds(N[animationKindJSONKey].AsArray);
        InitAnimationPlayerInfo(N[animationsJSONKey].AsArray);
        return;
    }


    /// <summary>
    /// Initializes the subFolderBook and the fileNameBook
    /// </summary>
    private void InitSubFolders(JSONArray A)
    {
        foreach (JSONNode N in A)
        {
            // Load the JSON
            int id = int.Parse(N[idJSONKey]);
            string path = N[nameJSONKey];

            // Fill the books
            subFolderBook.Add(id, path);

            Sprite[] frames = Resources.LoadAll<Sprite>(path);

            List<string> fileNames = new List<string>();
            foreach (Sprite frame in frames)
            {
                fileNames.Add(frame.name);
                Resources.UnloadAsset(frame); // Frees up memory
            }
            fileNameBook.Add(id, fileNames);
        }


    }


    /// <summary>
    /// Initializes the audioEventBook
    /// </summary>
    private void InitAudioEvents(JSONArray A)
    {
        foreach (JSONNode N in A)
        {
            int id = int.Parse(N[idJSONKey]);
            int audioID = int.Parse(N[audioJSONKey]);
            string name = N[nameJSONKey];
            audioEventBook.Add(id, new AnimationAudioEventInfo(id, audioID, name));
        }
    }


    /// <summary>
    /// Initializes the fpsBook
    /// </summary>
    private void InitFPS(JSONArray A)
    {
        foreach (JSONNode N in A)
        {
            // Load the JSON
            int id = int.Parse(N[idJSONKey]);
            float fps = float.Parse(N[fpsJSONKey]);
            fps = 1.0f / fps;
            fpsBook.Add(id, fps);
        }


    }


    /// <summary>
    /// Initializes the animationIDBook
    /// </summary>
    private void InitAnimationKinds(JSONArray A)
    {
        foreach (JSONNode N in A)
        {
            string name = N[nameJSONKey];
            int id = int.Parse(N[animationIDJSONKey]);
            AnimationType kind = EnumUtil.ParseEnum<AnimationType>(name);
            animationIDBook.Add(kind, id);
        }
    }


    /// <summary>
    /// Initializes the animationInfoBook.
    /// Call after all other Dictionaries have been populated with JSON data
    /// </summary>
    private void InitAnimationPlayerInfo(JSONArray A)
    {
        foreach (JSONNode N in A)
        {
            int id = int.Parse(N[idJSONKey]);
            
            int subFolderID = int.Parse(N[subFolderIDJSONKey]);
            string subFolder = subFolderBook[subFolderID];

            int audioEventID = int.Parse(N[audioEventIDJSONKey]);
            int audioID = audioEventBook[audioEventID].AudioID;
            string audioEventName = audioEventBook[audioEventID].EventName;

            int loopCount = int.Parse(N[loopJSONKey]);
            int fpsID = int.Parse(N[fpsIDJSONKey]);
            float fps = fpsBook[fpsID];
            float scaleOffset = float.Parse(N[scaleJSONKey]);

            int spriteID = int.Parse(N[spriteJSONKey]);
            
            // Now we need to get the file name from the folder and id, and then load it
            string fileName = GetFullAnimationFileName(subFolderID, spriteID);
            Sprite[] sprites = Resources.LoadAll<Sprite>(subFolder + fileName);
            List<Sprite> spriteList = new List<Sprite>();
            
            foreach (Sprite s in sprites)
            {
                spriteList.Add(s);
            }
            AnimationPlayerInfo info = new AnimationPlayerInfo(id, spriteList, 
                fps, loopCount, audioID, audioEventName, scaleOffset);

            animationInfoBook.Add(id, info);

        }
    }


    /// <summary>
    /// Returns the full file name given part of the file name for animations
    ///  This is important because it obtains version information.
    /// </summary>
    private string GetFullAnimationFileName(int subFolderID, int spriteID)
    {
        //For more information about how Regex works in this function, please see
        //http://www.zytrax.com/tech/web/regex.htm

        //Unlike the Regex function in AudioLoader::GetFullFileName, this looks for text before and after the match
        // in short:
        // ^TEXT, looks for "TEXT" in the beginning of a phrase, not after (i.e.   TEXTbook, TEXTing, but not TEeeeXT, laTEXT, or conTEXTual
        // $TEXT, looks for "TEXT" in the ending of a phrase, not before   (i.e.   laTEXT,            but not TEeeeXT, conTEXTual, TEXTbook, or TEXTing 
        // .TEXT, looks for "TEXT" in in the middle of a phrase            (i.e.   conTEXTual,        but not TEeeeXT, TEXTbook, TEXTing, or laTEXT



        List<string> fileNames = fileNameBook[subFolderID];
        Regex rgx = new Regex("._" + spriteID.ToString() + "_");
        Regex rgxFail = new Regex("$_" + spriteID.ToString()); //This means that the Sprite in the editor was not split into a texture atlas

        foreach (string fileName in fileNames)
        {
            if(rgxFail.IsMatch(fileName))
            {
                Debug.LogWarning("Warning, did you forget to split " + fileNameBook[subFolderID] + "/" + fileName + " in the SpriteEditor?");
            }
            if (rgx.IsMatch(fileName))
            {
                int j = fileName.Length - 1;

                // Note, we have a match, but the match has a "_#" at the end of it.
                //we need to remove that, so when we load it we get all of the texture for the atlas
                while (j >= 0 && fileName[j] != '_')
                {
                    j--;
                }

                string fullFileName = fileName.Substring(0, j);
                return fullFileName;
            }
        }

        string message = "Key not found, rgx: " + rgx.ToString();

        message += "\n";
        message += DebugHelper.GetLog(fileNames, "This key was not found in file names located in subfolder directory, " + subFolderBook[subFolderID]);
        throw new KeyNotFoundException(message);
    }

    #endregion

}
