using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KModkit;
using System.Text.RegularExpressions;

public class Recolored_Switches : MonoBehaviour {
    public GameObject[] Switches;
    public GameObject[] LEDs;
    static int moduleIDCounter = 1;
    int moduleID;
    bool moduleSolved;
    int stage = 0;
    int lastFlipped;
    int correctSwitch;
    bool[] switchStates = new bool[5];
    string[] order = { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "last" };
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public Material[] SwitchesColors;
    public Material[] LEDsColors;
    System.Text.StringBuilder LEDsColorsString = new System.Text.StringBuilder("KKKKKKKKKK");
    System.Text.StringBuilder SwitchesColorsString = new System.Text.StringBuilder("KKKKK");
    int[] ledIndices = new int[10];
    int[] switchIndices = new int[5];
    string colorNames = "OPGRBCW";
    int[][] tableLow  = new int[][] { new int[] { 0, 0, 4, 2, 2, 2, 3 }, new int[] { 2, 2, 1, 2, 0, 3, 1 }, new int[] { 3, 2, 2, 0, 4, 2, 4 }, new int[] { 2, 0, 0, 3, 2, 2, 1 }, new int[] { 0, 2, 1, 3, 2, 0, 4 }, new int[] { 2, 0, 1, 0, 2, 4, 3 } };
    int[][] tableHigh = new int[][] { new int[] { 2, 1, 1, 3, 1, 4, 4 }, new int[] { 4, 2, 0, 0, 3, 4, 0 }, new int[] { 1, 3, 2, 4, 4, 1, 1 }, new int[] { 3, 3, 2, 0, 3, 3, 1 }, new int[] { 3, 2, 2, 0, 3, 3, 1 }, new int[] { 2, 4, 1, 0, 0, 3, 4 } };
    bool isAnimating;

    // Use this for initialization
    //Debug.LogFormat("[Recolored Switches #{0}]", moduleID);

    void Start()
    {
        moduleID = moduleIDCounter++;
        GenerateSwitches();
        FlipSwitchRandom();
        Debug.LogFormat("[Recolored Switches #{0}] The switches colors are: {1}", moduleID, SwitchesColorsString.ToString());
        Debug.Log(switchIndices.Join());
        lastFlipped = switchIndices[2];
        GenerateLEDs();
        SetLEDColor();
        for (int i = 0; i < 5; i++)
        {
            int j = i;
            Switches[j].GetComponent<KMSelectable>().OnInteract += delegate () { CheckSwitchFlip(j); GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Switches[j].transform); Switches[j].GetComponent<KMSelectable>().AddInteractionPunch(.25f); return false; };
        }
    }
    void FlipSwitchRandom()
    {
        for (int i = 0; i < 5; i++)
        {
            if(Random.Range(0, 2) == 1)
            {
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Switches[i].transform);
                StartCoroutine(FlipSwitch(i));
            }
        }
    }
    void CheckSwitchFlip(int pos)
    {
        if (moduleSolved)
        {
            StartCoroutine(FlipSwitch(pos));
            return;
        }
        if (correctSwitch == pos)
        {
            StartCoroutine(FlipSwitch(pos));
            if (stage == 9) 
			{ 
                moduleSolved = true;
				Module.HandlePass(); 
				Debug.LogFormat("[Recolored Switches #{0}] Congratulations! You solved the module!", moduleID);
				for(int i = 0; i < 10; i++)
				{
					LEDs[i].GetComponent<MeshRenderer>().material = LEDsColors[7];
				} 
			}
            else
            {
                stage++;
                lastFlipped = switchIndices[pos];
                SetLEDColor();
            }
        }
        else
        {
            Module.HandleStrike();
			Debug.LogFormat("[Recolored Switches #{0}] Strike! the selected switch which was the {1} switch is wrong", moduleID, order[pos]);
			SetLEDColor();
        }

    }
    IEnumerator FlipSwitch(int selected)
    {
        isAnimating = true;
        switchStates[selected] = !switchStates[selected];
        const float duration = .3f;
        var startTime = Time.fixedTime;
        if (Switches[selected].transform.localEulerAngles.x >= 50 && Switches[selected].transform.localEulerAngles.x <= 60)
        {
            do
            {
                Switches[selected].transform.localEulerAngles = new Vector3(easeOutSine(Time.fixedTime - startTime, duration, 55f, -55f), 0, 0);
                yield return null;
            }
            while (Time.fixedTime < startTime + duration);
            Switches[selected].transform.localEulerAngles = new Vector3(-55f, 0, 0);
        }
        else
        {
            do
            {
                Switches[selected].transform.localEulerAngles = new Vector3(easeOutSine(Time.fixedTime - startTime, duration, -55f, 55f), 0, 0);
                yield return null;
            }
            while (Time.fixedTime < startTime + duration);
            Switches[selected].transform.localEulerAngles = new Vector3(55f, 0, 0);
        }
        isAnimating = false;
    }
    private float easeOutSine(float time, float duration, float from, float to)
    {
        return (to - from) * Mathf.Sin(time / duration * (Mathf.PI / 2)) + from;
    }
    void SetLEDColor()
    {
        Debug.LogFormat("[Recolored Switches #{0}] The {2} LED Color is: {1}", moduleID, LEDsColorsString[stage], order[stage]);
        LEDs[stage].GetComponent<MeshRenderer>().material = LEDsColors[ledIndices[stage]];
        GenerateStage();
    }
	void GenerateLEDs()
    {
        for(int i = 0; i < 10; i++)
        {
            ledIndices[i] = Random.Range(0, 7);
            LEDsColorsString[i] = colorNames[ledIndices[i]];
        }
    }
	void GenerateSwitches()
    {
        for(int i = 0; i < 5; i++)
        {
            int r = Random.Range(0,6);
            Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[r];
            switchIndices[i] = r;
            SwitchesColorsString[i] = colorNames[r];
        }
    }

    void GenerateStage()
    {
        int count = switchStates.Count(x => x == true);
        int[][] usingTable = (count < 3) ? tableLow : tableHigh;
        Debug.Log(usingTable.Join());
        correctSwitch = usingTable[lastFlipped  ][ledIndices[stage]];

        Debug.LogFormat("[Recolored Switches #{0}] There are {1} switches in the up position. The {2} LED is {3} and the previous switch is {4}.", moduleID, count, order[stage], LEDsColorsString[stage], colorNames[lastFlipped]);
        Debug.LogFormat("[Recolored Switches #{0}] The correct switch to flip is switch {1}.", moduleID, correctSwitch + 1);

    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} 1 2 3 4 5 [Toggles the specified switches where 1 is leftmost and 5 is rightmost]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command.Trim().ToUpperInvariant();
        if (Regex.IsMatch(command, @"^\s*((press)|(flip)\s+)?[1-5]\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            while (isAnimating) yield return null;
            Switches[int.Parse(command.Split(' ').Last()) - 1].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        while (!moduleSolved)
        { 
            while (isAnimating) yield return true;
            Switches[correctSwitch].GetComponent<KMSelectable>().OnInteract();
        }
    }

}
