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
    int last;
    string[] order = { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "last" };
    List<int> Submission;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public Material[] SwitchesColors;
    public Material[] LEDsColors;
    System.Text.StringBuilder LEDsColorsString = new System.Text.StringBuilder("KKKKKKKKKK");
    System.Text.StringBuilder SwitchesColorsString = new System.Text.StringBuilder("KKKKK");
    int[] ledIndices = new int[10];
    int[] switchIndices = new int[5];
    string colorNames = "OPGRBCW";

    // Use this for initialization
    //Debug.LogFormat("[Recolored Switches #{0}]", moduleID);

    void Start()
    {
        moduleID = moduleIDCounter++;
        GenerateSwitches();
        FlipSwitchRandom();
        Debug.LogFormat("[Recolored Switches #{0}] The switches colors are: {1}", moduleID, SwitchesColorsString.ToString());
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
        if (true)
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
            else{stage++; SetLEDColor();}
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
    }
    private float easeOutSine(float time, float duration, float from, float to)
    {
        return (to - from) * Mathf.Sin(time / duration * (Mathf.PI / 2)) + from;
    }
    void SetLEDColor()
    {
        Debug.LogFormat("[Recolored Switches #{0}] The {2} LED Color is: {1}", moduleID, LEDsColorsString[stage], order[stage]);
        LEDs[stage].GetComponent<MeshRenderer>().material = LEDsColors[ledIndices[stage]];
        Debug.LogFormat("[Recolored Switches #{0}] The safe {1}", moduleID, LoggingSafePos());
    }
    string LoggingSafePos()
    {
        return "notimplemented";
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
            SwitchesColorsString[i] = colorNames[r];
        }
    }
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} 1 2 3 4 5 [Toggles the specified switches where 1 is leftmost and 5 is rightmost]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        bool extraitem = false;
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*toggle\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*switch\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*flip\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            extraitem = true;
            if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the switches that need to be flipped!";
                yield break;
            }
        }
        string[] valids = { "1", "2", "3", "4", "5" };
        if (extraitem)
        {
            for(int i = 1; i < parameters.Length; i++)
            {
                if (!valids.Contains(parameters[i]))
                {
                    yield return "sendtochaterror The specified switch '"+parameters[i]+"' is invalid!";
                    yield break;
                }
            }
            yield return null;
            for (int i = 1; i < parameters.Length; i++)
            {
                int temp = 0;
                int.TryParse(parameters[i], out temp);
                temp -= 1;
                Switches[temp].GetComponent<KMSelectable>().OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
        else
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!valids.Contains(parameters[i]))
                {
                    yield return "sendtochaterror The specified switch '" + parameters[i] + "' is invalid!";
                    yield break;
                }
            }
            yield return null;
            for (int i = 0; i < parameters.Length; i++)
            {
                int temp = 0;
                int.TryParse(parameters[i], out temp);
                temp -= 1;
                Switches[temp].GetComponent<KMSelectable>().OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
    }
}
