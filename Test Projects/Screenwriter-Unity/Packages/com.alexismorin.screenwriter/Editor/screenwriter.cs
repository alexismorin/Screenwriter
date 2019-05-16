using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class screenwriter : MonoBehaviour {

    [SerializeField]
    static AnimationCurve shotDensityScale; // how long should a shot be based on where it is inside of the sequence

    static List<string> parsedScrenplay = new List<string> ();

    static Dictionary<string, float> timeOfDay = new Dictionary<string, float> ();

    static GameObject currentPlayableGameObject;
    static PlayableDirector currentPlayable;
    static screenwriterDataSlate currentDataSlate;

    static TimelineAsset currentTimeline;
    static string currentLocation;
    static string currentLightingScenario;
    static string currentCharacter;
    static string currentMood;
    static double currentTimelineScrub;
    static TrackAsset masterTrack;

    static string thirdLastLine;
    static string lastLine;
    static string line;
    static int lineCount = 0;

    static void Clear () {

        parsedScrenplay = new List<string> ();
        currentPlayableGameObject = null;
        currentDataSlate = null;
        currentPlayable = null;
        currentMood = null;
        currentTimeline = null;
        currentLocation = null;
        currentLightingScenario = null;
        currentCharacter = null;

        lineCount = 0;
        currentTimelineScrub = 0.0;
    }

    [MenuItem ("Tools/Parse Screenplay")]
    static void ParseScreenplay () {

        Clear ();

        // load file
        var selectedFilepath = EditorUtility.OpenFilePanel ("Select Screenplay", "", "txt");
        FileInfo fileInfo = new FileInfo (selectedFilepath);
        string timeText = System.DateTime.Now.ToString ("hh.mm.ss"); // get time so we can mark where these changes were made

        // create playabledirector in scene
        currentPlayableGameObject = new GameObject ();

        currentPlayableGameObject.name = fileInfo.Name.Replace (".txt", string.Empty) + " " + timeText + " Timeline";
        currentDataSlate = currentPlayableGameObject.AddComponent<screenwriterDataSlate> ();
        currentPlayable = currentPlayableGameObject.AddComponent<PlayableDirector> ();

        // create timeline asset
        TimelineAsset currentTimeline = ScriptableObject.CreateInstance ("TimelineAsset") as TimelineAsset;
        AssetDatabase.CreateAsset (currentTimeline, "Assets/Timelines/" + fileInfo.Name.Replace (".txt", string.Empty) + " " + timeText + ".playable");

        currentTimeline = (TimelineAsset) AssetDatabase.LoadAssetAtPath ("Assets/Timelines/" + fileInfo.Name.Replace (".txt", string.Empty) + " " + timeText + ".playable", typeof (TimelineAsset));

        currentPlayable.playableAsset = currentTimeline;

        masterTrack = currentTimeline.CreateTrack<GroupTrack> (null, "Master Group");
        currentTimeline.CreateTrack<ControlTrack> (masterTrack, "Shots");
        currentTimeline.CreateTrack<ControlTrack> (masterTrack, "Lighting");
        //      currentTimeline.CreateTrack<SignalTrack> (null, "Track");
        //      currentTimeline.CreateTrack<AnimationTrack> (null, "Track");

        string filePath = fileInfo.FullName;
        line = null;
        StreamReader reader = new StreamReader (filePath);

        using (reader) {
            do {
                thirdLastLine = lastLine;
                lastLine = line;
                line = reader.ReadLine ();
                if (System.String.IsNullOrEmpty (line) == false) {

                    // this is where we actually parse the screenplay

                    lineCount++;
                    //  print ("Parsing line #" + lineCount);

                    // capital letters checker
                    char[] possibleCharacter = line.ToCharArray ();
                    int capitalization = 0;
                    for (int i = 0; i < possibleCharacter.Length; i++) {
                        if (System.Char.IsUpper (possibleCharacter[i])) {
                            capitalization++;
                        }
                    }

                    // location, time of day
                    if (line.Contains ("EXT") || line.Contains ("INT")) {
                        char splitChar = '-';

                        string[] splitString = line.Split (splitChar);

                        currentDataSlate.TryAddLocation (splitString[0]);
                        currentLocation = splitString[0];

                        currentDataSlate.TryAddLightingScenario (splitString[1]);
                        currentLightingScenario = splitString[1];
                        continue;
                    }

                    // character check - all capitals and no :
                    if (capitalization > possibleCharacter.Length * 0.8f && line.Contains (":") == false) {
                        currentDataSlate.TryAddCharacter (line);
                        currentCharacter = line;
                        continue;
                    }

                    // extra actor instructions check - has ()
                    if (line.Contains ("(")) {
                        currentMood = line;
                        continue;
                    }

                    // transition data - contains : and is caps
                    if (line.Contains (":") && capitalization > possibleCharacter.Length * 0.5f) {
                        continue;
                    }

                    // at this point there arent many edge cases left - check if we have a actor guidelines or an actor name above us to see if this is dialogue
                    if (lastLine != null) {
                        if (lastLine.Contains ("(") || currentDataSlate.characters.Contains (lastLine)) {

                            //dialogue callback

                            // check if we already have a character group
                            bool alreadyCreatedCharacter = false;
                            IEnumerable<TrackAsset> duplicateCharacterCheck = currentTimeline.GetRootTracks ();
                            foreach (TrackAsset track in duplicateCharacterCheck) {
                                print (track.name);
                                if (track.name == currentCharacter) {
                                    alreadyCreatedCharacter = true;

                                }
                            }

                            if (alreadyCreatedCharacter == false) {

                                TrackAsset characterTrack = currentTimeline.CreateTrack<GroupTrack> (null, currentCharacter);
                                TrackAsset possibleSignalTrack = currentTimeline.CreateTrack<SignalTrack> (characterTrack, currentCharacter + " Signals");
                                TrackAsset animationTrack = currentTimeline.CreateTrack<AnimationTrack> (characterTrack, currentCharacter + " Animation Overrides");
                            }

                            IEnumerable<TrackAsset> allTracksForActors = currentTimeline.GetOutputTracks ();
                            foreach (TrackAsset track in allTracksForActors) {
                                if (track.name == currentCharacter + " Signals") {

                                    IMarker newMarker = track.CreateMarker<SignalEmitter> (currentTimelineScrub);

                                }
                                if (track.name == currentCharacter + " Animation Overrides") {

                                    TimelineClip newClip = track.CreateDefaultClip ();
                                    newClip.start = currentTimelineScrub;
                                    newClip.duration = Convert.ToDouble (line.Length) / 12.0;
                                    newClip.displayName = "Dialogue Animation";
                                }

                            }

                        }
                    }

                    // shot callback
                    IEnumerable<TrackAsset> allTracksForActions = currentTimeline.GetOutputTracks ();
                    foreach (TrackAsset track in allTracksForActions) {
                        if (track.name == "Shots") {

                            GameObject cameraInstance = PrefabUtility.InstantiatePrefab (currentDataSlate.cameraPrefab) as GameObject;
                            cameraInstance.name = "Shot Camera";
                            cameraInstance.transform.parent = currentPlayableGameObject.transform;

                            //   TimelineClip newClip = track.CreateDefaultClip ();
                            //   

                            TimelineClip tlClip = track.CreateClip<ControlPlayableAsset> ();
                            ControlPlayableAsset clip = tlClip.asset as ControlPlayableAsset;
                            clip.sourceGameObject.exposedName = UnityEditor.GUID.Generate ().ToString ();
                            currentPlayable.SetReferenceValue (clip.sourceGameObject.exposedName, cameraInstance);

                            tlClip.clipIn = currentTimelineScrub;
                            tlClip.duration = Convert.ToDouble (line.Length) / 12.0;
                            tlClip.displayName = line;
                        }
                        if (track.name == "Lighting") {

                            TimelineClip newClip = track.CreateDefaultClip ();
                            newClip.displayName = currentLightingScenario;
                            newClip.clipIn = currentTimelineScrub;
                            newClip.duration = Convert.ToDouble (line.Length) / 12.0;
                        }
                    }

                    currentTimelineScrub = currentTimeline.duration;

                }
            }
            while (line != null);
            reader.Close ();
        }
    }

}