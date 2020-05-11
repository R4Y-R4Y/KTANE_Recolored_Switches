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
    int current = 0;
    int last;
    string[] order = { "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth", "last" };
    List<int> Submission;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    public Material[] SwitchesColors;
    public Material[] LEDsColors;
    System.Text.StringBuilder LEDsColorsString = new System.Text.StringBuilder("KKKKKKKKKK");
    System.Text.StringBuilder SwitchesColorsString = new System.Text.StringBuilder("KKKKK");
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
        int random = 0;
        for (int i = 0; i < 5; i++)
        {
            random = Random.Range(0, 2);
            if(random == 1)
            {
                GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Switches[i].transform);
                StartCoroutine(FlipSwitch(i));
            }
        }
    }
    void CheckSwitchFlip(int pos)
    {
        if (Submission.Contains(pos))
        {
            StartCoroutine(FlipSwitch(pos));
            if (current == 9) 
			{ 
				Module.HandlePass(); 
				Debug.LogFormat("[Recolored Switches #{0}] Congratulations! You solved the module!", moduleID); 
				for(int i=0;i<10;i++)
				{
					LEDs[i].GetComponent<MeshRenderer>().material = LEDsColors[7];
				} 
			}
            else{current++; SetLEDColor();}
            

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
        Debug.LogFormat("[Recolored Switches #{0}] The {2} LED Color is: {1}", moduleID, LEDsColorsString[current], order[current]);
        switch (LEDsColorsString[current])
        {
            case 'R':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[0];
                Submission = RuleRed();
                break;
            case 'G':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[1];
                Submission = RuleGreen();
                break;
            case 'B':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[2];
                Submission = RuleBlue();
                break;
            case 'T':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[3];
                Submission = RuleTurquoise();
                break;
            case 'O':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[4];
                Submission = RuleOrange();
                break;
            case 'P':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[5];
                Submission = RulePurple();
                break;
            case 'W':
                LEDs[current].GetComponent<MeshRenderer>().material = LEDsColors[6];
                Submission = RuleWhite();
                break;
        }
        Debug.LogFormat("[Recolored Switches #{0}] The safe {1}", moduleID, LoggingSafePos());
    }
    string LoggingSafePos()
    {
        string log = "";
        if (Submission.Count() == 1) {log = "switch is the "; }
        else { log = "switches are the "; }
            for (int i = 0; i < Submission.Count(); i++)
        {
			if(i == Submission.Count() -1) {log = log + order[Submission[i]];}
            else {log = log + order[Submission[i]] + " ," ;}
        }
        if (Submission.Count() == 1) { log = log + " switch."; }
        else log = log + " switches.";
        return log;
    }
    List<int> RuleWhite()
    {
        //DONE
        List<int> answers = new List<int>();
        if (current > 4)
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            if (current != 9) { answers.Add((current + 1) % 5); }
            else if (current == 9){ answers.Add(4); };
            return answers;
        }
        if(Bomb.GetModuleNames().Any(x => x.ToLowerInvariant().Contains("switch") && x != "Recolored Switches"))
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            answers.Add((Bomb.GetModuleNames().Count(x => x.ToLowerInvariant().Contains("switch") && x != "Recolored Switches")-1 +5) % 5);
            return answers;
        }
        if(SwitchesColorsString.ToString().Contains("T") || SwitchesColorsString.ToString().Contains("O"))
        {
            Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
            answers.Add(2);
            return answers;
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(3);
        return answers;
    }
    List<int> RuleTurquoise()
    {
        //DONE
        List<int> answers = new List<int>();
        if (current % 2 == 1)
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            answers.Add(current % 5);
            return answers;
        }
        if (ColorAdjacentToCurrent())
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            answers.Add(0);
            return answers;
        }
        if (Bomb.GetBatteryCount() == 0)
        {
            Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
            if (current >= 5) { answers.Add(0); answers.Add(1); answers.Add(2); answers.Add(3); answers.Add(4); return answers; }
            else
            {
                for (int i = 0; i <= current; i++) { answers.Add(i); } return answers; 
            }
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(2);
        return answers;
    }
    List<int> RuleOrange()
    {
        //DONE
        List<int> answers = new List<int>();
        if (SwitchesColorsString.ToString().Contains("B"))
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            answers.Add(2);
            return answers;
        }
        if (Bomb.GetSerialNumberLetters().Any("AEIOU".Contains))
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            answers.Add(0);
            return answers;
        }
        if(current!=0 && FindColor(LEDsColorsString,'O'))
        {
            Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
            answers.Add(InitialColor(LEDsColorsString,'O') % 5);
            return answers;
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(4);
        return answers;
    }
    List<int> RulePurple()
    {
        //DONE
        int sum = 0;
        List<int> answers = new List<int>();
        if (current + 1 == 1)
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            answers.Add(4);
            return answers;
        }
        if(LEDsColorsString[current] == SwitchesColorsString[current % 5])
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            answers.Add(current % 5);
            return answers;
        }
        if(LEDsColorsString.ToString().Contains('G') || LEDsColorsString.ToString().Contains('R'))
        {
            
            for (int i = 0; i < current + 1; i++)
            {
                if(LEDsColorsString[i]=='G' || LEDsColorsString[i] == 'R')
                {
                    sum = sum++;
                }
            }
            if (sum > (current + 1 - sum))
            {
                Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
                for (int i = 0; i < 5; i++)
                {
                    if (SwitchesColorsString[i] == 'G' || SwitchesColorsString[i] == 'R')
                    {
                        answers.Add(i);
                    }
                }
                if (answers.Count == 0)
                {
                    answers.Add(2);
                }
                return answers;
            }
            
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(1);
        return answers;
    }
    List<int> RuleBlue()
    {
        //DONE
        List<int> answers = new List<int>();
        if (Bomb.IsIndicatorOff(Indicator.FRQ) || Bomb.IsIndicatorOff(Indicator.IND) || Bomb.IsIndicatorOff(Indicator.SND))
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            answers.Add((Bomb.GetIndicators().Count() - 1) % 5);
            return answers;
        }
        if (current % 5 == 4)
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            answers.Add(0);
            return answers;
        }
        if (current / 5 == 0)
        {
            Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
            answers.Add(4);
            return answers;
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(2);
        return answers;
    }
    List<int> RuleGreen()
    {
        //DONE
        List<int> answers = new List<int>();
        if(Bomb.IsIndicatorOn(Indicator.CAR) || Bomb.IsIndicatorOn(Indicator.BOB) || Bomb.IsIndicatorOn(Indicator.NSA))
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            if (SwitchesColorsString.ToString().Contains("P"))
            {
                for(int i = 0; i < 5; i++)
                {
                    if (SwitchesColorsString[i] == 'P')
                    {
                        answers.Add(i);
                    }
                }
            }
            else
            {
                answers.Add(1);
            }
            return answers;
        }
        if (Bomb.IsTwoFactorPresent() || Bomb.IsDateOfManufacturePresent())
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            answers.Add(0);
            return answers;
        }
        if (!(SwitchesColorsString.ToString().Contains("R") || SwitchesColorsString.ToString().Contains("G") || SwitchesColorsString.ToString().Contains("B")))
        {
            Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
            answers.Add(0); answers.Add(1); answers.Add(2); answers.Add(3); answers.Add(4);
            return answers;
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(3);
        return answers;
    }
    List<int> RuleRed()
    {
        //DONE
        List<int> answers = new List<int>();
        if (Bomb.GetSerialNumberNumbers().Sum() < 10)
        {
            Debug.LogFormat("[Recolored Switches #{0}] First rule applied!", moduleID);
            answers.Add((Bomb.GetSerialNumberNumbers().Last() - 1 + 5)%5);
            return answers;
        }
        if (LEDsColorsString[current] == 'R' && !FindColor(LEDsColorsString, 'R'))
        {
            Debug.LogFormat("[Recolored Switches #{0}] Second rule applied!", moduleID);
            if (SwitchesColorsString.ToString().Contains("R"))
            {
                for (int i = 0; i < 5; i++)
                {
                    if (SwitchesColorsString[i] == 'R')
                    {
                        answers.Add(i);
                    }
                }
                return answers;
            }
            else if (SwitchesColorsString.ToString().Contains("B"))
            {
                for (int i = 0; i < 5; i++)
                {
                    if (SwitchesColorsString[i] == 'B')
                    {
                        answers.Add(i);
                    }
                }
                return answers;
            }
            else { answers.Add(3); return answers; }
        }
        if(Bomb.GetStrikes() != 0)
        {
            Debug.LogFormat("[Recolored Switches #{0}] Third rule applied!", moduleID);
            answers.Add((Bomb.GetStrikes()-1) % 5);
			return answers;
        }
        Debug.LogFormat("[Recolored Switches #{0}] None of the rules are applied!", moduleID);
        answers.Add(4);
        return answers;
    }
    bool ColorAdjacentToCurrent()
    {
        if(current < 5)
        {
            if (current == 0)
            {
                return false;
            }
            else
            {
                if(LEDsColorsString[current-1]=='B'|| LEDsColorsString[current - 1] == 'W')
                {
                    return true;
                }
                else return false;
            }
        }
        else
        {
            if (current == 5)
            {
                if (LEDsColorsString[0] == 'B' || LEDsColorsString[0] == 'W')
                {
                    return true;
                }
                else return false;
            }
            
            else
            {
                if (LEDsColorsString[current - 1] == 'B' || LEDsColorsString[current - 1] == 'W' || LEDsColorsString[current - 5] == 'B' || LEDsColorsString[current - 5] == 'W')
                {
                    return true;
                }
                else return false;
            }
        }
        
    }
	int InitialColor(System.Text.StringBuilder stuff,char color)
	{
		int i = 0;
			
		while(stuff[i] != color)
		{
			i++;
		}
		return i;
	}
    bool FindColor(System.Text.StringBuilder stuff,char color)
    {
        for(int i = 0; i < current; i++)
        {
            if(stuff[i] == color)
            {
                return true;
            }
        }
        return false;
    }
	void GenerateLEDs()
    {
        for(int i = 0; i < 10; i++)
        {
            int random = Random.Range(0,7);
            switch (random)
            {
                case 0:
                    LEDsColorsString[i] = 'R';
                    break;
                case 1:
                    LEDsColorsString[i] = 'G';
                    break;
                case 2:
                    LEDsColorsString[i] = 'B';
                    break;
                case 3:
                    LEDsColorsString[i] = 'P';
                    break;
                case 4:
                    LEDsColorsString[i] = 'O';
                    break;
                case 5:
                    LEDsColorsString[i] = 'T';
                    break;
                case 6:
                    LEDsColorsString[i] = 'W';
                    break;
            }
        }
    }
	void GenerateSwitches()
    {
        for(int i = 0; i < 5; i++)
        {
            int random = Random.Range(0,6);
            switch (random)
            {
                case 0:
                    Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[0];
                    SwitchesColorsString[i] = 'R';
                    break;
                case 1:
                    Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[1];
                    SwitchesColorsString[i] = 'G';
                    break;
                case 2:
                    Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[2];
                    SwitchesColorsString[i] = 'B';
                    break;
                case 3:
                    Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[3];
                    SwitchesColorsString[i] = 'T';
                    break;
                case 4:
                    Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[4];
                    SwitchesColorsString[i] = 'O';
                    break;
                case 5:
                    Switches[i].GetComponent<MeshRenderer>().material = SwitchesColors[5];
                    SwitchesColorsString[i] = 'P';
                    break;
            }
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
