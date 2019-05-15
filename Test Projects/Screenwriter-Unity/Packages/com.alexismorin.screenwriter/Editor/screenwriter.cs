using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class screenwriter : MonoBehaviour {

    static List<string> parsedScrenplay = new List<string> ();

    static string currentLocation;
    static string currentTimeOfDay;
    static string currentCharacter;

    [MenuItem ("Tools/Parse Screenplay")]
    private static void ParseScreenplay () {

        parsedScrenplay = new List<string> ();

        var selectedFilepath = EditorUtility.OpenFilePanel ("Select Screenplay", "", "txt");
        FileInfo fileInfo = new FileInfo (selectedFilepath);
        string filePath = fileInfo.FullName;
        string line = null;
        StreamReader reader = new StreamReader (filePath);

        using (reader) {
            do {
                line = reader.ReadLine ();
                parsedScrenplay.Add (line);
                print (line);
                // m_codeContents += line + "\n";
            }
            while (line != null);
            reader.Close ();
        }
    }

    private static void ParseLine () {

    }
}