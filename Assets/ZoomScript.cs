using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class ZoomScript : MonoBehaviour
{

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;

    private string[] modulenames = { "101 Dalmatians", "3D Tunnels", "Accumulation", "Adventure Game", "Alphabet", "Bases", "Binary Puzzle", "Blackjack", "Boggle", "British Slang", "Calendar", "Chord Qualities", "Color Decoding", "Color Morse", "Combination Lock", "Countdown", "Cruel Countdown", "Determinants", "Digital Root", "Dragon Energy", "Encrypted Morse", "English Test", "Extended Password", "Filibuster", "Follow the Leader", "Functions", "Graffiti Numbers", "Grid Matching", "Guitar Chords", "Hieroglyphics", "Hot Potato", "Hunting", "IKEA", "Instructions", "Jack Attack", "Know Your Way", "Kudosudoku", "Lasers", "LED Grid", "Lightspeed", "Logic Gates", "Maintenance", "Mashematics", "Mineseeker", "Module Homework" };
    private int current;
    private string answer;
    public TextMesh moddisp;

    public Transform timer;
    private Coroutine timeco;
    private float timef;

    public Material[] pictures;
    public Renderer zoompic;
    private Coroutine zoomrunning;
    private bool zoomisrun;

    private bool started;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
            pressed.OnInteractEnded += delegate () { ReleaseButton(pressed); };
        }
    }

    void Start()
    {
        current = 0;
        zoomisrun = false;
        started = false;
        moddisp.text = "";
        randomizeMod();
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true)
        {
            if (pressed == buttons[2])
            {
                if (started == false)
                {
                    pressed.AddInteractionPunch(0.5f);
                    audio.PlaySoundAtTransform("tick", pressed.transform);
                    started = true;
                    StartCoroutine(startZoom());
                }
                else if (zoomisrun == false && modulenames.Contains(moddisp.text))
                {
                    pressed.AddInteractionPunch(0.5f);
                    audio.PlaySoundAtTransform("tick", pressed.transform);
                    if (moddisp.text.Equals(answer))
                    {
                        Debug.LogFormat("[Zoom #{0}] Correct answer submitted! Module resetting without penalty...", moduleId);
                        Start();
                    }
                    else
                    {
                        Debug.LogFormat("[Zoom #{0}] Incorrect answer submitted ({1})! Module resetting WITH penalty...", moduleId, moddisp.text);
                        GetComponent<KMBombModule>().HandleStrike();
                        Start();
                    }
                }
                else if (zoomisrun == true && modulenames.Contains(moddisp.text))
                {
                    pressed.AddInteractionPunch(0.5f);
                    audio.PlaySoundAtTransform("tick", pressed.transform);
                    StopCoroutine(zoomrunning);
                    if (moddisp.text.Equals(answer))
                    {
                        Debug.LogFormat("[Zoom #{0}] Correct answer submitted! Module Disarmed!", moduleId);
                        zoompic.material.SetTextureScale("_MainTex", new Vector2(1f, 1f));
                        zoompic.material.SetTextureOffset("_MainTex", new Vector2(0f, 0f));
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                    }
                    else
                    {
                        Debug.LogFormat("[Zoom #{0}] Incorrect answer submitted ({1})! Module Resetting WITH penalty...", moduleId, moddisp.text);
                        GetComponent<KMBombModule>().HandleStrike();
                        Start();
                    }
                }
            }
            else if (pressed == buttons[0] && started == true && modulenames.Contains(moddisp.text))
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlaySoundAtTransform("tick", pressed.transform);
                if (current == 0)
                {
                    moddisp.text = modulenames[modulenames.Length - 1];
                    current = modulenames.Length - 1;
                }
                else
                {
                    moddisp.text = modulenames[current - 1];
                    current--;
                }
                timeco = StartCoroutine(time(pressed));
            }
            else if (pressed == buttons[1] && started == true && modulenames.Contains(moddisp.text))
            {
                pressed.AddInteractionPunch(0.25f);
                audio.PlaySoundAtTransform("tick", pressed.transform);
                if (current == modulenames.Length - 1)
                {
                    moddisp.text = modulenames[0];
                    current = 0;
                }
                else
                {
                    moddisp.text = modulenames[current + 1];
                    current++;
                }
                timeco = StartCoroutine(time(pressed));
            }
        }
    }

    void ReleaseButton(KMSelectable released)
    {
        if ((buttons[0] == released || buttons[1] == released) && timeco != null)
        {
            timef = 0f;
            StopCoroutine(timeco);
        }
    }

    private void randomizeMod()
    {
        int rand = UnityEngine.Random.Range(0, modulenames.Length);
        answer = modulenames[rand];
        zoompic.material = pictures[rand];
        zoompic.material.SetTextureScale("_MainTex", new Vector2(0.1f, 0.1f));
        zoompic.material.SetTextureOffset("_MainTex", new Vector2(0.5f, 0.5f));
        timer.localPosition = new Vector3(-0.0651f, 0.01f, -0.0485f);
        timer.localScale = new Vector3(0, 0.0001f, 0.0015f);
        Debug.LogFormat("[Zoom #{0}] The displayed picture is the module '{1}'", moduleId, answer);
    }

    private IEnumerator time(KMSelectable btn)
    {
        while (true)
        {
            yield return null;
            timef += Time.deltaTime;
            if (timef > 0.5f)
            {
                if (buttons[0] == btn)
                {
                    if (current == 0)
                    {
                        moddisp.text = modulenames[modulenames.Length - 1];
                        current = modulenames.Length - 1;
                    }
                    else
                    {
                        moddisp.text = modulenames[current - 1];
                        current--;
                    }
                    yield return new WaitForSeconds(0.03f);
                }
                else if (buttons[1] == btn)
                {
                    if (current == modulenames.Length - 1)
                    {
                        moddisp.text = modulenames[0];
                        current = 0;
                    }
                    else
                    {
                        moddisp.text = modulenames[current + 1];
                        current++;
                    }
                    yield return new WaitForSeconds(0.03f);
                }
            }
        }
    }

    private IEnumerator startZoom()
    {
        char[] things = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        int counter = 0;
        while (counter < 25)
        {
            counter++;
            int rando1 = UnityEngine.Random.Range(0, things.Length);
            int rando2 = UnityEngine.Random.Range(0, things.Length);
            int rando3 = UnityEngine.Random.Range(0, things.Length);
            int rando4 = UnityEngine.Random.Range(0, things.Length);
            int rando5 = UnityEngine.Random.Range(0, things.Length);
            moddisp.text = things[rando1]+""+things[rando2]+""+things[rando3]+""+things[rando4]+""+things[rando5];
            yield return new WaitForSeconds(0.02f);
        }
        StopCoroutine("startZoom");
        int rand = UnityEngine.Random.Range(0, modulenames.Length);
        current = rand;
        moddisp.text = modulenames[rand];
        zoomrunning = StartCoroutine(zoomOut());
    }

    private IEnumerator zoomOut()
    {
        zoomisrun = true;
        Debug.LogFormat("[Zoom #{0}] The timer has started! Submit the correct answer before the timer runs out to solve the module!", moduleId);
        Vector2 tempscale = zoompic.material.GetTextureScale("_MainTex");
        Vector2 tempoff = zoompic.material.GetTextureOffset("_MainTex");
        while (tempscale.x < 1.0f && tempscale.y < 1.0f)
        {
            timer.localPosition += new Vector3(0.000061f, 0f, 0f);
            timer.localScale += new Vector3(0.000122f, 0f, 0f);
            tempscale = zoompic.material.GetTextureScale("_MainTex");
            tempoff = zoompic.material.GetTextureOffset("_MainTex");
            tempscale.x = tempscale.x + 0.00085f;
            tempscale.y = tempscale.y + 0.00085f;
            if (tempoff.x > 0f)
            {
                tempoff.x = tempoff.x - 0.00047f;
                tempoff.y = tempoff.y - 0.00047f;
            }
            else
            {
                tempoff.x = 0;
                tempoff.y = 0;
            }
            zoompic.material.SetTextureScale("_MainTex", tempscale);
            zoompic.material.SetTextureOffset("_MainTex", tempoff);
            yield return new WaitForSeconds(0.01f);
        }
        zoomisrun = false;
        Debug.LogFormat("[Zoom #{0}] The timer has ran out! Submitting correct answers from now until another reset WILL NOT solve the module and instead WILL reset it without penalty!", moduleId);
        StopCoroutine(zoomrunning);
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} start [Starts the module] | !{0} submit <module name> [Submits the specified module]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*start\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && started == false)
        {
            yield return null;
            buttons[2].OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && started == true)
        {
            if (parameters.Length > 1)
            {
                yield return null;
                string mod = command.Substring(7);
                List<string> modnames = modulenames.ToList();
                for (int i = 0; i < modnames.Count; i++)
                {
                    modnames[i] = modnames[i].ToLower();
                }
                mod = mod.ToLower();
                if (modnames.Contains(mod))
                {
                    if (!moddisp.text.EqualsIgnoreCase(mod))
                    {
                        int diff = modnames.IndexOf(mod) - current;
                        Debug.Log(diff);
                        if (Math.Abs(diff) > modnames.Count / 2)
                        {
                            diff = Math.Abs(diff) - modnames.Count;

                            if (modnames.IndexOf(mod) < current)
                                diff = -diff;
                        }
                        if (diff > 0)
                        {
                            buttons[1].OnInteract();
                            while (!moddisp.text.EqualsIgnoreCase(mod)) yield return null;
                            buttons[1].OnInteractEnded();
                        }
                        else
                        {
                            buttons[0].OnInteract();
                            while (!moddisp.text.EqualsIgnoreCase(mod)) yield return null;
                            buttons[0].OnInteractEnded();
                        }
                    }
                    buttons[2].OnInteract();
                }
                else
                {
                    yield return "sendtochaterror!f I couldn't find '" + command.Substring(7) + "' in my module directory!";
                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!started)
        {
            buttons[2].OnInteract();
        }
        while (!modulenames.Contains(moddisp.text)) { yield return null; }
        yield return ProcessTwitchCommand("submit " + answer);
    }
}