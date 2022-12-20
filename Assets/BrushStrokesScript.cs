using UnityEngine;
using System;
using Random = UnityEngine.Random;
using KModkit;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BrushStrokesScript : MonoBehaviour
{
    public KMBombModule Module;
    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMColorblindMode Colorblind;
    public KMSelectable[] btnSelectables;
    public MeshRenderer[] btnRenderers;
    public Material[] btnColors;
    public SpriteRenderer[] horizontalStrokes, verticalStrokes, tlbrStrokes, trblStrokes;
    public TextMesh[] colorblindText;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved = false;

    private int[] colors = new int[9];
    private readonly static char[] literallyJustTheEntireAlphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private readonly static char[] andAlsoSomeNumbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
    private readonly static string[] colorNames = { "red", "orange", "yellow", "lime", "green", "cyan", "sky", "blue", "purple", "magenta", "brown", "white", "gray", "black", "pink" };
    private int keyNum = 0, strikeCountUponGeneration = 0;

    private bool[,] gaps = { { false, false, false, false, false, false },  // horizontal lines, in reading order
                             { false, false, false, false, false, false },  // vertical lines, in reading order
                             { false, false, false, false, false, false },  // tl-br diagonals, in reading order (there's only 4)
                             { false, false, false, false, false, false }   // tr-bl diagonals, in reading order (there's only 4)
                           };
    private readonly static bool[,,] symbols =
    {
        {
            { true, true, false, false, true, false },
            { false, true, true, true, true, true },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false }
        }, // 1st symbol
        {
            { true, true, false, false, false, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 2nd symbol
        {
            { true, true, false, false, true, false },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 3rd symbol
        {
            { true, true, true, false, true, false },
            { false, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 4th symbol
        {
            { true, true, true, true, true, true },
            { false, false, true, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 5th symbol
        {
            { true, true, false, false, true, true },
            { false, true, true, false, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 6th symbol
        {
            { true, true, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 7th symbol
        {
            { true, true, true, true, true, true },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 8th symbol
        {
            { true, true, true, true, true, false },
            { false, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 9th symbol
        {
            { true, true, true, true, false, false },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, true, false, false }
        }, // 10th symbol
        {
            { true, true, false, false, true, true },
            { false, false, false, false, false, false },
            { true, false, false, true, false, false },
            { false, true, true, false, false, false }
        }, // 11th symbol
        {
            { true, true, false, false, true, true },
            { false, false, false, false, false, false },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false }
        }, // 12th symbol
        {
            { true, true, false, false, true, true },
            { false, false, true, false, false, false },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false }
        }, // 13th symbol
        {
            { false, true, true, true, false, false },
            { true, false, false, true, false, true },
            { false, false, false, false, false, false },
            { true, false, false, false, false, false }
        }, // 14th symbol
        {
            { true, true, true, true, true, true },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 15th symbol
        {
            { true, true, true, true, true, true },
            { true, false, false, false, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 16th symbol
        {
            { true, true, true, true, true, false },
            { false, false, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 17th symbol
        {
            { false, false, false, false, true, true },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 18th symbol
        {
            { false, true, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 19th symbol
        {
            { true, true, false, false, true, true },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 20th symbol
        {
            { true, false, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 21st symbol
        {
            { true, true, true, true, true, true },
            { false, false, true, false, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 22nd symbol
        {
            { true, false, false, true, true, false },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 23rd symbol
        {
            { false, false, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 24th symbol
        {
            { true, true, true, true, false, false },
            { false, true, false, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 25th symbol
        {
            { true, false, false, false, false, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 26th symbol
        {
            { false, true, true, false, false, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 27th symbol
        {
            { true, true, false, true, false, true },
            { true, false, true, true, true, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 28th symbol
        {
            { false, false, false, false, false, false },
            { true, false, true, true, false, true },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false }
        }, // 29th symbol
        {
            { true, true, false, true, true, false },
            { true, false, false, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 30th symbol
        {
            { true, true, true, false, true, false },
            { false, true, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 31st symbol
        {
            { false, true, false, false, true, true },
            { true, true, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 32nd symbol
        {
            { false, true, false, false, true, true },
            { false, true, true, false, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 31st symbol
        {
            { false, false, false, false, false, false },
            { false, true, false, false, true, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }, // 33rd symbol
        {
            { false, false, false, false, false, false },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        }// 35th symbol
    };

    private int btnSelected = 99;
    private bool colorblindActive = false;

    private readonly static string[] colorblindLetters = { "R", "O", "Y", "L", "G", "C", "S", "B", "P", "M", "N", "W", "A", "K", "I" };

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;

        colorblindActive = Colorblind.ColorblindModeActive;
        GenerateModule();
        for (int i = 0; i < 6; i++)
            horizontalStrokes[i].enabled = false;

        for (int i = 0; i < 6; i++)
            verticalStrokes[i].enabled = false;

        for (int i = 0; i < 4; i++)
            tlbrStrokes[i].enabled = false;

        for (int i = 0; i < 4; i++)
            trblStrokes[i].enabled = false;

    }

    void Activate()
    {
        for (int i = 0; i < btnSelectables.Length; i++)
        {
            int j = i;
            btnSelectables[i].OnInteract += delegate ()
            {
                if (!solved)
                    btnPress(j);
                btnSelectables[j].AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                StopAllCoroutines();
                StartCoroutine(Count());
                return false;
            };

            btnSelectables[i].OnInteractEnded += delegate ()
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Module.transform);
            };
        }
    }
    void CalculateKeyNumber(int colorIdx, bool MidRecalc = false)
    {
        keyNum = 0;
        strikeCountUponGeneration = Bomb.GetStrikes();
        switch (colorIdx) // if center point is...
        {
            case 0: // Red
                if (!MidRecalc)
                    DebugMsg("...Corresponding to Two Bits.");
                keyNum = Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumberLetters().First()) + 1;
                DebugMsg("The first letter of the serial number is \"{0}\". Its alphabetic position is {1}.", Bomb.GetSerialNumberLetters().First(), keyNum);
                keyNum += Bomb.GetSerialNumberNumbers().Last() * Bomb.GetBatteryCount();
                DebugMsg("There are {0} batter{1} and the last digit of the serial number is {2}. Multiplying these 2 values together results in {3}.", Bomb.GetBatteryCount(), Bomb.GetBatteryCount() == 1 ? "y" : "ies", Bomb.GetSerialNumberNumbers().Last(), Bomb.GetSerialNumberNumbers().Last() * Bomb.GetBatteryCount());
                if (Bomb.IsPortPresent(Port.StereoRCA) && !Bomb.IsPortPresent(Port.RJ45))
                {
                    keyNum *= 2;
                    DebugMsg("A Stereo RCA port is present, and an RJ-45 port is not present.");
                }
                else
                    DebugMsg("A Stereo RCA port is{0} present. An RJ port is{1} present.", Bomb.IsPortPresent(Port.StereoRCA) ? "" : " not", Bomb.IsPortPresent(Port.RJ45) ? "" : "not");
                keyNum %= 100;
                //DebugMsg("After modulo 100, the key number is {0}.", keyNum);
                break;
            case 1: // Orange
                if (!MidRecalc)
                    DebugMsg("...Corresponding to Color Generator.");
                if (literallyJustTheEntireAlphabet.Contains(Bomb.GetSerialNumber()[0]))
                {
                    keyNum = (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[0]) + 1) % 16 * 16;
                    DebugMsg("The first character of the serial number is a letter. Its alphabetic position modulo 16 is {0}. Times 16 is {1}.", (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[0]) + 1) % 16, (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[0]) + 1) % 16 * 16);
                }
                else
                {
                    keyNum = Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[0]) * 16;
                    DebugMsg("The first character of the serial number is a digit. 16 * {0} = {1}.", Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[0]), Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[0]) * 16);
                }

                if (literallyJustTheEntireAlphabet.Contains(Bomb.GetSerialNumber()[1]))
                {
                    keyNum += (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[1]) + 1) % 16;
                    DebugMsg("The second character of the serial number is a letter. Its alphabetic position modulo 16 is {0}.", (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[1]) + 1) % 16);
                }
                else
                {
                    keyNum += Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[1]);
                    DebugMsg("The second character of the serial number is a digit. 1 * {0} = {0}", Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[1]));
                }
                break;
            case 2: // Yellow
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Modern Cipher.");
                    foreach (var number in Bomb.GetSerialNumberNumbers())
                    {
                        keyNum += number;
                    }
                    DebugMsg("The sum of digits in the serial number is {0}.", keyNum);
                    keyNum += Bomb.GetStrikes();
                    DebugMsg("{0} strike(s) should have been added at the time of {2} to make the key number up to this point {1}.", Bomb.GetStrikes(), keyNum, MidRecalc ? "submission" : "generation");
                    break;
                }

            case 3: // Lime
                if (!MidRecalc)
                    DebugMsg("...Corresponding to Burglar Alarm.");
                if (Bomb.GetBatteryCount() > 4)
                {
                    DebugMsg("There are more than 4 batteries.");
                    if (Bomb.GetOffIndicators().Count() == 0)
                    {
                        DebugMsg("There are no unlit indicators. The 1st digit in the key number is a 2.");
                        keyNum = 20;
                    }
                    else
                    {
                        DebugMsg("There is at least 1 unlit indicator. The 1st digit in the key number is a 6.");
                        keyNum = 60;
                    }
                }

                else
                {
                    DebugMsg("There are at most 4 batteries.");
                    if (Bomb.GetOnIndicators().Count() == 0)
                    {
                        DebugMsg("There are no lit indicators. The 1st digit in the key number is a 4.");
                        keyNum = 40;
                    }
                    else
                    {
                        DebugMsg("There is at least 1 lit indicator. The 1st digit in the key number is a 9.");
                        keyNum = 90;
                    }
                }

                if (Bomb.GetBatteryCount() == Bomb.GetIndicators().Count())
                {
                    DebugMsg("The number of batteries equals the number of indicators.");
                    //if (Bomb.GetSerialNumber().Contains('B') || Bomb.GetSerialNumber().Contains('U') || Bomb.GetSerialNumber().Contains('R') || Bomb.GetSerialNumber().Contains('G') || Bomb.GetSerialNumber().Contains('1') || Bomb.GetSerialNumber().Contains('4') || Bomb.GetSerialNumber().Contains('R'))
                    if (Bomb.GetSerialNumber().Any(a => "BURG14R".Contains(a)))
                    {
                        DebugMsg("There is at least 1 character in the serial number that is in \"BURG14R.\" The 2nd digit in the key number is a 1.");
                        keyNum += 1;
                    }
                    else
                        DebugMsg("There are no characters in the serial number that is in \"BURG14R.\" The 2nd digit in the key number is a 0.");
                }
                else
                {
                    DebugMsg("The number of batteries does not equal the number of indicators.");
                    if (!Bomb.GetSerialNumber().Any(a => "AL53M".Contains(a))) //else if (!(Bomb.GetSerialNumber().Contains('A') || Bomb.GetSerialNumber().Contains('L') || Bomb.GetSerialNumber().Contains('5') || Bomb.GetSerialNumber().Contains('3') || Bomb.GetSerialNumber().Contains('M')))
                    {
                        DebugMsg("There are no characters in the serial number that is in \"AL53M.\" The 2nd digit in the key number is an 8.");
                        keyNum += 8;
                    }
                    else
                        DebugMsg("There is at least 1 character in the serial number that is in \"AL53M.\" The 2nd digit in the key number is a 0.");
                }
                break;
            case 4: // Green
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Catchphrase.");
                    var x = 0;
                    foreach (char c in Bomb.GetSerialNumberLetters())
                        x += Array.IndexOf(literallyJustTheEntireAlphabet, c) + 1;
                    DebugMsg("The sum of the alphabetic positions of all the letters in the serial number is {0}.", x);
                    x += Bomb.GetOnIndicators().Count();
                    DebugMsg("There {1} {0} lit indicator{2}.", Bomb.GetOnIndicators().Count(), Bomb.GetOnIndicators().Count() == 1 ? "is" : "are", Bomb.GetOnIndicators().Count() == 1 ? "" : "s");
                    keyNum = x;
                    break;
                }

            case 5: // Cyan
                if (!MidRecalc)
                    DebugMsg("...Corresponding to Regular Hexpressions.");
                if (Bomb.GetSerialNumberNumbers().Last() % 2 == 0)
                {
                    DebugMsg("The last digit of the serial number is even, calculating for the row offset.");
                    keyNum = Bomb.GetSolvableModuleNames().Count() % 11 + 1;
                }
                else
                {
                    DebugMsg("The last digit of the serial number is odd, calculating for the column offset.");
                    keyNum = (Bomb.GetBatteryCount() + Bomb.GetIndicators().Count()) % 5 + 1;
                }
                break;
            case 6: // Sky
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Safety Safe.");
                    keyNum = Bomb.CountUniquePorts() * 7;
                    DebugMsg("There are this many distinct port types: {0} (7 * {0} = {1})", Bomb.CountUniquePorts(), Bomb.CountUniquePorts() * 7);
                    int indicatorAmount = 0;

                    foreach (var indicator in Bomb.GetOnIndicators())
                    {
                        if (Bomb.GetSerialNumberLetters().Contains(indicator[0]) || Bomb.GetSerialNumberLetters().Contains(indicator[1]) || Bomb.GetSerialNumberLetters().Contains(indicator[2]))
                        {
                            indicatorAmount++;
                        }
                    }

                    DebugMsg("Out of all the lit indicators, {0} share at least a letter in the serial number. (5 * {0} = {1})", indicatorAmount, indicatorAmount * 5);
                    keyNum += indicatorAmount * 5;
                    indicatorAmount = 0;

                    foreach (var indicator in Bomb.GetOffIndicators())
                    {
                        if (Bomb.GetSerialNumberLetters().Contains(indicator[0]) || Bomb.GetSerialNumberLetters().Contains(indicator[1]) || Bomb.GetSerialNumberLetters().Contains(indicator[2]))
                        {
                            indicatorAmount++;
                        }
                    }
                    DebugMsg("Out of all the unlit indicators, {0} share at least a letter in the serial number.", indicatorAmount);
                    keyNum += indicatorAmount;
                    break;
                }

            case 7: // Blue
                if (!MidRecalc)
                    DebugMsg("...Corresponding to The Code.");
                if (Bomb.GetSerialNumberNumbers().First() == Bomb.GetSerialNumberNumbers().Last() && Bomb.GetBatteryCount() == 0)
                {
                    DebugMsg("Rule #1 is the first rule met in The Code. (First and last serial no. digits equal, no batteries)");
                    keyNum = 1;
                }
                else if (Bomb.GetIndicators().Contains("CLR"))
                {
                    DebugMsg("Rule #2 is the first rule met in The Code. (CLR indicator present)");
                    keyNum = 8;
                }
                else if (Bomb.GetSerialNumberLetters().Contains('X') || Bomb.GetSerialNumberLetters().Contains('Y') | Bomb.GetSerialNumberLetters().Contains('Z'))
                {
                    DebugMsg("Rule #3 is the first rule met in The Code. (X, Y or Z present in the serial number)");
                    keyNum = 20;
                }
                else if (Bomb.GetPortCount() >= 5)
                {
                    DebugMsg("Rule #4 is the first rule met in The Code. (At least 5 ports)");
                    keyNum = 30;
                }
                else if (Bomb.GetBatteryCount() == 0)
                {
                    DebugMsg("Rule #5 is the first rule met in The Code. (No batteries)");
                    keyNum = 42;
                }
                else if (Bomb.GetOnIndicators().Count() > Bomb.GetOffIndicators().Count())
                {
                    DebugMsg("Rule #6 is the first rule met in The Code. (More lit than unlit indicators)");
                    keyNum = 69;
                }
                else
                {
                    DebugMsg("No other rules met in The Code.");
                    keyNum = 3;
                }
                break;
            case 8: // Purple
                {
                    //char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Tennis.");
                    if (andAlsoSomeNumbers.Contains(Bomb.GetSerialNumber()[0]))
                        keyNum = Bomb.GetSerialNumberNumbers().First();
                    else
                        keyNum = Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[0]) + 6;
                    DebugMsg("The first character of the serial number is {0}. The binary you should obtain should be {1}.", Bomb.GetSerialNumber()[0], Enumerable.Range(0, 5).Select(a => (keyNum >> a) % 2).Join(""));
                    break;
                }

            case 9: // Magenta
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Radiator.");
                    foreach (var character in Bomb.GetSerialNumber())
                        if ("RADI4T07".Contains(character))
                            keyNum += 10;
                    DebugMsg("There are this many occurances of \"RADI4T07\" in the serial number: {0}", Bomb.GetSerialNumber().Count(a => "RADI4T07".Contains(a)));
                    keyNum += 5 * Bomb.GetBatteryHolderCount(2);
                    DebugMsg("There are this many pairs of AA batteries: {0}", Bomb.GetBatteryHolderCount(2));
                    keyNum -= 5 * Bomb.GetBatteryHolderCount(1);
                    DebugMsg("There are this many D batteries: {0}", Bomb.GetBatteryHolderCount(1));
                    if (keyNum < 0)
                        keyNum *= -1;
                    break;
                }

            case 10: // Brown
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Cooking.");
                    var mealIdx = Bomb.GetBatteryHolderCount() - Bomb.GetIndicators().Count() + Bomb.GetBatteryCount() * Bomb.GetPortCount() - Bomb.GetPortPlateCount();
                    DebugMsg("Battery Holders ({0}) - Indicators ({1}) + Batteries ({2}) * Ports ({3}) - Port Plates ({4}) = {5}", Bomb.GetBatteryHolderCount(), Bomb.GetIndicators().Count(), Bomb.GetBatteryCount(), Bomb.GetPortCount(), Bomb.GetPortPlateCount(), mealIdx);
                    mealIdx %= 5;
                    while (mealIdx <= 0)
                        mealIdx += 5;

                    string[] meals = { "Pizza", "Spaghetti Bolognese", "Chicken Casserole", "Chilli Con Carne", "Chicken Pie" };
                    int[] temps = { 250, 160, 200, 180, 180 };
                    DebugMsg("After keeping this within 1 to 5, look at the temperature to cook {0}.", meals[mealIdx - 1]);
                    keyNum = temps[mealIdx - 1];
                    break;
                }

            case 11: // White
                if (!MidRecalc)
                    DebugMsg("...Corresponding to Character Shift.");
                DebugMsg("X = Ports ({0}) + Letters In Serial ({1})", Bomb.GetPortCount(), Bomb.GetSerialNumberLetters().Count());
                keyNum = Bomb.GetPortCount() + Bomb.GetSerialNumberLetters().Count();
                break;
            case 12: // Gray
                if (!MidRecalc)
                    DebugMsg("...Corresponding to Fast Math.");
                if (Bomb.IsIndicatorOn(Indicator.MSA))
                {
                    DebugMsg("There is a lit MSA.");
                    keyNum += 20;
                }

                if (Bomb.IsPortPresent(Port.Serial))
                {
                    DebugMsg("There is a serial port.");
                    keyNum += 14;
                }

                if (Bomb.GetSerialNumber().Contains('F') || Bomb.GetSerialNumber().Contains('A') || Bomb.GetSerialNumber().Contains('S') || Bomb.GetSerialNumber().Contains('T'))
                {
                    DebugMsg("There is at least 1 letter from \"FAST\" in common with the serial number.");
                    keyNum -= 5;
                }

                if (Bomb.IsPortPresent(Port.RJ45))
                {
                    DebugMsg("There is an RJ port.");
                    keyNum += 27;
                }

                if (Bomb.GetBatteryCount() > 3)
                {
                    DebugMsg("There are more than 3 batteries.");
                    keyNum -= 15;
                }

                break;
            case 13: // Black
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to X01.");
                    int[,] firstTable =
                    {
                        { 74, 53, 79 },
                        { 62, 41, 70 },
                        { 42, 47, 86 },
                        { 38, 66, 51 },
                        { 80, 67, 58 }
                    };

                    int firstNumber = Bomb.GetBatteryCount(Battery.AA) + Bomb.GetSerialNumberNumbers().Count(),
                        secondNumber = Bomb.GetIndicators().Count() + Bomb.GetPortCount();
                    DebugMsg("The number of AA batteries and digits in the serial number equal {0}.", Bomb.GetBatteryCount(Battery.AA) + Bomb.GetSerialNumberNumbers().Count());
                    DebugMsg("The number of indicators and ports equal {0}.", Bomb.GetIndicators().Count() + Bomb.GetPortCount());
                    if (firstNumber < 3)
                        firstNumber = 0;
                    else if (firstNumber < 5)
                        firstNumber = 1;
                    else if (firstNumber < 6)
                        firstNumber = 2;
                    else if (firstNumber < 8)
                        firstNumber = 3;
                    else
                        firstNumber = 4;

                    if (secondNumber < 3)
                        secondNumber = 0;
                    else if (secondNumber < 6)
                        secondNumber = 1;
                    else
                        secondNumber = 2;

                    DebugMsg("Count {0} column(s) from the left and {1} row(s) from the top to get the key number.", secondNumber + 1, firstNumber + 1);

                    keyNum = firstTable[firstNumber, secondNumber];
                    break;
                }

            default: // Pink
                {
                    if (!MidRecalc)
                        DebugMsg("...Corresponding to Module Homework.");
                    keyNum = Bomb.GetSerialNumberNumbers().First();
                    DebugMsg("The first digit of the serial number is {0}", Bomb.GetSerialNumberNumbers().First());
                    foreach (var letter in Bomb.GetSerialNumberLetters())
                    {
                        if ("SCHOOL".Contains(letter))
                        {
                            DebugMsg("There is at least 1 character from the serial number in \"SCHOOL.\"");
                            keyNum += 3;
                            break;
                        }
                    }

                    bool randomBool = false;
                    foreach (var indicator in Bomb.GetIndicators())
                    {
                        foreach (var letter in indicator)
                        {
                            if ("STUDENT".Contains(letter))
                            {
                                DebugMsg("There is at least 1 indicator that contains a character in \"STUDENT.\"");
                                randomBool = true;
                                keyNum += 2;
                                break;
                            }
                        }

                        if (randomBool)
                            break;
                    }

                    if (Bomb.IsPortPresent(Port.Parallel))
                    {
                        DebugMsg("A parallel port is present.");
                        keyNum += 2;
                    }
                    if (Bomb.IsIndicatorPresent(Indicator.FRK) || Bomb.IsIndicatorPresent(Indicator.NSA))
                    {
                        DebugMsg("An FRK or NSA indicator is present.");
                        keyNum += 2;
                    }
                    foreach (var letter in Bomb.GetSerialNumber())
                    {
                        if ("AEIOU".Contains(letter))
                        {
                            DebugMsg("There is a vowel in the serial number.");
                            keyNum += 5;
                            break;
                        }
                    }

                    if (Bomb.GetBatteryCount(Battery.D) > 1)
                    {
                        DebugMsg("There is more than 1 D battery.");
                        keyNum += 2;
                    }

                    if (Bomb.GetOnIndicators().Contains("BOB"))
                    {
                        keyNum = 1;
                        DebugMsg("HOWEVER, Bob is still in elementary school. The modifcations from above should be discarded and the \"***\" footnote must be used.");
                    }
                    break;
                }
        }

        DebugMsg("The key number accumulated from the manual required is {0}.", keyNum);

        if (keyNum < 0)
            keyNum *= -1;
        keyNum %= 35;

        DebugMsg("The final key number, after taking the absolute value, modulo 35, and adding 1, is {0}. So use the pattern corresponding to that value.", keyNum + 1);
    }


    void GenerateModule()
    {
        for (int i = 0; i < 9; i++)
        {
            colors[i] = Random.Range(0, 15);
            btnRenderers[i].material = btnColors[colors[i]];
        }

        DebugMsg("The middle color is " + colorNames[colors[4]] + ".");
        CalculateKeyNumber(colors[4]);

        if (colorblindActive)
        {
            DebugMsg("Colorblind mode is active! Setting colorblind letters...");
            for (int i = 0; i < 9; i++)
                colorblindText[i].text = colorblindLetters[colors[i]];
        }
        else
        {
            for (int i = 0; i < 9; i++)
                colorblindText[i].text = "";
        }
    }

    void btnPress(int btnNum)
    {
        if (btnSelected == 99)
            btnSelected = btnNum;
        else
        {
            btnSelectables[btnNum].AddInteractionPunch();
            Audio.PlaySoundAtTransform("stroke" + Random.Range(1, 5), Module.transform);
            //DebugMsg("You connected two points.");

            if (btnSelected == btnNum)
                btnSelected = 99;

            else if ((btnSelected == 0 && btnNum == 1) || (btnSelected == 1 && btnNum == 0))
            {
                gaps[0, 0] = !(gaps[0, 0]);
            }
            else if ((btnSelected == 1 && btnNum == 2) || (btnSelected == 2 && btnNum == 1))
            {
                gaps[0, 1] = !(gaps[0, 1]);
            }
            else if ((btnSelected == 3 && btnNum == 4) || (btnSelected == 4 && btnNum == 3))
            {
                gaps[0, 2] = !(gaps[0, 2]);
            }
            else if ((btnSelected == 4 && btnNum == 5) || (btnSelected == 5 && btnNum == 4))
            {
                gaps[0, 3] = !(gaps[0, 3]);
            }
            else if ((btnSelected == 6 && btnNum == 7) || (btnSelected == 7 && btnNum == 6))
            {
                gaps[0, 4] = !(gaps[0, 4]);
            }
            else if ((btnSelected == 7 && btnNum == 8) || (btnSelected == 8 && btnNum == 7))
            {
                gaps[0, 5] = !(gaps[0, 5]);
            }
            else if ((btnSelected == 0 && btnNum == 3) || (btnSelected == 3 && btnNum == 0))
            {
                gaps[1, 0] = !(gaps[1, 0]);
            }
            else if ((btnSelected == 1 && btnNum == 4) || (btnSelected == 4 && btnNum == 1))
            {
                gaps[1, 1] = !(gaps[1, 1]);
            }
            else if ((btnSelected == 2 && btnNum == 5) || (btnSelected == 5 && btnNum == 2))
            {
                gaps[1, 2] = !(gaps[1, 2]);
            }
            else if ((btnSelected == 3 && btnNum == 6) || (btnSelected == 6 && btnNum == 3))
            {
                gaps[1, 3] = !(gaps[1, 3]);
            }
            else if ((btnSelected == 4 && btnNum == 7) || (btnSelected == 7 && btnNum == 4))
            {
                gaps[1, 4] = !(gaps[1, 4]);
            }
            else if ((btnSelected == 5 && btnNum == 8) || (btnSelected == 8 && btnNum == 5))
            {
                gaps[1, 5] = !(gaps[1, 5]);
            }
            else if ((btnSelected == 0 && btnNum == 4) || (btnSelected == 4 && btnNum == 0))
            {
                gaps[2, 0] = !(gaps[2, 0]);
            }
            else if ((btnSelected == 1 && btnNum == 5) || (btnSelected == 5 && btnNum == 1))
            {
                gaps[2, 1] = !(gaps[2, 1]);
            }
            else if ((btnSelected == 3 && btnNum == 7) || (btnSelected == 7 && btnNum == 3))
            {
                gaps[2, 2] = !(gaps[2, 2]);
            }
            else if ((btnSelected == 4 && btnNum == 8) || (btnSelected == 8 && btnNum == 4))
            {
                gaps[2, 3] = !(gaps[2, 3]);
            }
            else if ((btnSelected == 1 && btnNum == 3) || (btnSelected == 3 && btnNum == 1))
            {
                gaps[3, 0] = !(gaps[3, 0]);
            }
            else if ((btnSelected == 2 && btnNum == 4) || (btnSelected == 4 && btnNum == 2))
            {
                gaps[3, 1] = !(gaps[3, 1]);
            }
            else if ((btnSelected == 4 && btnNum == 6) || (btnSelected == 6 && btnNum == 4))
            {
                gaps[3, 2] = !(gaps[3, 2]);
            }
            else if ((btnSelected == 5 && btnNum == 7) || (btnSelected == 7 && btnNum == 5))
            {
                gaps[3, 3] = !(gaps[3, 3]);
            }
            else if ((btnSelected == 0 && btnNum == 2) || (btnSelected == 2 && btnNum == 0))
            {
                gaps[0, 0] = !(gaps[0, 0]);
                gaps[0, 1] = !(gaps[0, 1]);
            }
            else if ((btnSelected == 3 && btnNum == 5) || (btnSelected == 5 && btnNum == 3))
            {
                gaps[0, 2] = !(gaps[0, 2]);
                gaps[0, 3] = !(gaps[0, 3]);
            }
            else if ((btnSelected == 6 && btnNum == 8) || (btnSelected == 8 && btnNum == 6))
            {
                gaps[0, 4] = !(gaps[0, 4]);
                gaps[0, 5] = !(gaps[0, 5]);
            }
            else if ((btnSelected == 0 && btnNum == 6) || (btnSelected == 6 && btnNum == 0))
            {
                gaps[1, 0] = !(gaps[1, 0]);
                gaps[1, 3] = !(gaps[1, 3]);
            }
            else if ((btnSelected == 1 && btnNum == 7) || (btnSelected == 7 && btnNum == 1))
            {
                gaps[1, 1] = !(gaps[1, 1]);
                gaps[1, 4] = !(gaps[1, 4]);
            }
            else if ((btnSelected == 2 && btnNum == 8) || (btnSelected == 8 && btnNum == 2))
            {
                gaps[1, 2] = !(gaps[1, 2]);
                gaps[1, 5] = !(gaps[1, 5]);
            }
            else if ((btnSelected == 0 && btnNum == 8) || (btnSelected == 8 && btnNum == 0))
            {
                gaps[2, 0] = !(gaps[2, 0]);
                gaps[2, 3] = !(gaps[2, 3]);
            }
            else if ((btnSelected == 2 && btnNum == 6) || (btnSelected == 6 && btnNum == 2))
            {
                gaps[3, 1] = !(gaps[3, 1]);
                gaps[3, 2] = !(gaps[3, 2]);
            }

            btnSelected = 99;
        }
    }

    void DebugMsg(string msg, params object[] args)
    {
        Debug.LogFormat("[Brush Strokes #{0}] {1}", _moduleId, string.Format(msg, args));
    }

    void Submit()
    {
        if (solved) return;
        bool nopeThatsWrong = false;

        var recalcConditions = new Dictionary<int, bool> {
            { 2, Bomb.GetStrikes() != strikeCountUponGeneration } // 2 is yellow, corresponding to Modern Cipher.
        };
        if (recalcConditions.ContainsKey(colors[4]) && recalcConditions[colors[4]])
        {
            DebugMsg("Brush Strokes needs to be recalculated due to a change from the bomb.");
            CalculateKeyNumber(colors[4], true);
        }

        string firstPart;
        var lineConnectionsHoriz = new[] { "1-2", "2-3", "4-5", "5-6", "7-8", "8-9", };
        var lineConnectionsVert = new[] { "1-4", "2-5", "3-6", "4-7", "5-8", "6-9", };
        var lineConnectionsDiagTLBR = new[] { "1-5", "2-6", "4-8", "5-9", "?-?", "?-?", };
        var lineConnectionsDiagTRBL = new[] { "4-2", "5-3", "7-5", "8-6", "?-?", "?-?", };
        for (int i = 0; i < 4; i++)
        {
            for (int x = 0; x < 6; x++)
            {
                if (gaps[i, x] != symbols[keyNum, i, x])
                {
                    if (!nopeThatsWrong)
                    {
                        DebugMsg("Bruh... sh Strokes got a strike.");
                        Module.HandleStrike();
                        Audio.PlaySoundAtTransform("bruhmoment", Module.transform);
                    }

                    nopeThatsWrong = true;
                    string[] selectedList;

                    if (i == 0)
                    {
                        selectedList = lineConnectionsHoriz;
                        firstPart = "horizontal";
                    }
                    else if (i == 1)
                    {
                        selectedList = lineConnectionsVert;
                        firstPart = "vertical";
                    }
                    else if (i == 2)
                    {
                        selectedList = lineConnectionsDiagTLBR;
                        firstPart = "top-left to bottom-right diagonal";
                    }
                    else
                    {
                        selectedList = lineConnectionsDiagTRBL;
                        firstPart = "top-right to bottom-left diagonal";
                    }
                    if (gaps[i, x])
                        DebugMsg("The {0} line {1} shouldn't be there, but you drew a line there.", firstPart, selectedList[x]);
                    else
                        DebugMsg("The {0} line {1} should be there, but you did not draw a line there.", firstPart, selectedList[x]);
                }
            }
        }

        StartCoroutine(TurnOnStrokes());

        if (!nopeThatsWrong)
        {
            for (int i = 0; i < 9; i++)
                colorblindText[i].text = "SOLVED!!!"[i].ToString();

            Module.HandlePass();
            solved = true;
            DebugMsg("Congratulations, that key symbol you drew was completely right. Module solved.");
            foreach (var btnRenderer in btnRenderers)
            {
                btnRenderer.material = btnColors[13];
            }
        }

        else
        {
            for (int i = 0; i < 6; i++)
                for (int x = 0; x < 4; x++)
                    gaps[x, i] = false;

            keyNum = 0;
            GenerateModule();
        }

        btnSelected = 99;
    }

    IEnumerator Count()
    {
        yield return new WaitForSeconds(5);

        Submit();
    }

    IEnumerator TurnOnStrokes()
    {

        if (solved)
        {
            for (int i = 0; i < 6; i++)
                horizontalStrokes[i].color = new Color32(0, 255, 0, 255);

            for (int i = 0; i < 6; i++)
                verticalStrokes[i].color = new Color32(0, 255, 0, 255);

            for (int i = 0; i < 4; i++)
                tlbrStrokes[i].color = new Color32(0, 255, 0, 255);

            for (int i = 0; i < 4; i++)
                trblStrokes[i].color = new Color32(0, 255, 0, 255);
        }

        for (int i = 0; i < 6; i++)
            if (gaps[0, i])
                horizontalStrokes[i].enabled = true;

        for (int i = 0; i < 6; i++)
            if (gaps[1, i])
                verticalStrokes[i].enabled = true;

        for (int i = 0; i < 4; i++)
            if (gaps[2, i])
                tlbrStrokes[i].enabled = true;

        for (int i = 0; i < 4; i++)
            if (gaps[3, i])
                trblStrokes[i].enabled = true;

        yield return new WaitForSeconds(2);

        if (!solved)
        {
            for (int i = 0; i < 6; i++)
                horizontalStrokes[i].enabled = false;

            for (int i = 0; i < 6; i++)
                verticalStrokes[i].enabled = false;

            for (int i = 0; i < 4; i++)
                tlbrStrokes[i].enabled = false;

            for (int i = 0; i < 4; i++)
                trblStrokes[i].enabled = false;
        }

    }

    //twitch plays
    private bool stringIsDigit(string s)
    {
        int temp = 0;
        int.TryParse(s, out temp);
        if (temp != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool inputIsValid(string cmd)
    {
        char[] validchars = { ' ', ',', ';', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        char[] cmdchars = cmd.ToCharArray();
        for (int j = 7; j < cmdchars.Length; j++)
        {
            if (!validchars.Contains(cmdchars[j]))
            {
                return false;
            }
        }
        string[] parameters = cmd.Split(' ', ';', ',');
        if (parameters.Length % 2 == 0)
        {
            return false;
        }
        for (int i = 1; i < parameters.Length; i++)
        {
            if (!stringIsDigit(parameters[i]))
            {
                return false;
            }
            else
            {
                int temp = 0;
                int.TryParse(parameters[i], out temp);
                if (!((temp >= 1) && (temp <= 9)))
                {
                    return false;
                }
            }
        }
        return true;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} connect 1 3;3 9;9 7 [Connects the specified points (in this example they make a backwards C), points are in reading order 1-9] | !{0} colorblind [Enables colorblind mode]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            colorblindActive = !colorblindActive; // Consensed version by eliminating if-else statements
            if (colorblindActive)
            {
                DebugMsg("Colorblind mode is active! Setting colorblind letters...");
                for (int i = 0; i < 9; i++)
                    colorblindText[i].text = colorblindLetters[colors[i]];
            }
            else
            {
                for (int i = 0; i < 9; i++)
                    colorblindText[i].text = "";
            }
            yield break;
        }
        string[] parameters = command.Split(' ', ';', ',');
        if (Regex.IsMatch(parameters[0], @"^\s*connect\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (inputIsValid(command) && parameters.Length != 1)
            {
                yield return null;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (i != 0)
                    {
                        int temp = 0;
                        int.TryParse(parameters[i], out temp);
                        temp--;
                        btnSelectables[temp].OnInteract();
                        btnSelectables[temp].OnInteractEnded();
                        //btnPress(temp);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                //StartCoroutine(Count());
                yield return "strike";
                yield return "solve";
            }
            yield break;
        }
    }
}
