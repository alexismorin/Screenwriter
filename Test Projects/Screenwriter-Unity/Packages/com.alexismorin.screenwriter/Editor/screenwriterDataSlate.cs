using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class screenwriterDataSlate : MonoBehaviour {

    [SerializeField]
    public GameObject characterPrefab; // default actor prefab
    [SerializeField]
    public GameObject locationPrefab; // default location prefab
    [SerializeField]
    public GameObject cameraPrefab; // default camera prefab

    [SerializeField]
    public List<string> shots = new List<string> ();
    [SerializeField]
    public List<string> locations = new List<string> ();
    [SerializeField]
    public List<string> characters = new List<string> ();
    [SerializeField]
    public List<string> lightingScenarios = new List<string> ();
    [SerializeField]
    public List<string> lines = new List<string> ();

    public void TryAddCharacter (string input) {
        if (characters.Contains (input) == false) {
            characters.Add (input);
            CreateSequenceComponent (input);
        }

    }

    public void TryAddLocation (string input) {
        if (locations.Contains (input) == false) {
            locations.Add (input);
            CreateSequenceComponent (input);
        }

    }

    public void TryAddShot (string input) {
        if (shots.Contains (input) == false) {
            shots.Add (input);
            CreateSequenceComponent (input);
        }

    }

    public void TryAddLightingScenario (string input) {

        char trimChar = ' ';

        if (lightingScenarios.Contains (input.Trim (trimChar)) == false) {
            lightingScenarios.Add (input.Trim (trimChar));
            CreateSequenceComponent (input.Trim (trimChar) + " LIGHTING SCENARIO");
        }

    }

    // create a host gameobject
    public void CreateSequenceComponent (string name) {
        var sequenceInstance = new GameObject ();
        sequenceInstance.name = name;
        sequenceInstance.transform.parent = this.transform;
    }

}