using UnityEngine;
using System;
using Random = UnityEngine.Random;
using KModkit;
using System.Linq;
using System.Collections;
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
    private readonly static string vowels = "AEIOU";
    private readonly static string[] colorNames = { "red", "orange", "yellow", "lime", "green", "cyan", "sky", "blue", "purple", "magenta", "brown", "white", "gray", "black", "pink" };
    private int keyNum = 0;

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
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 1st symbol
        {
            { true, true, false, false, false, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, false, false, true, false },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 3rd symbol
        {
            { true, true, true, false, true, false },
            { false, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, true, true, true, true },
            { false, false, true, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 5th symbol
        {
            { true, true, false, false, true, true },
            { false, true, true, false, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 7th symbol
        {
            { true, true, true, true, true, true },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, true, true, true, false },
            { false, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 9th symbol
        {
            { true, true, true, true, false, false },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false },
            { false, false, false, true, false, false }
        },
        {
            { true, true, false, false, true, true },
            { false, false, false, false, false, false },
            { true, false, false, true, false, false },
            { false, true, true, false, false, false }
        },// 11th symbol
        {
            { true, true, false, false, true, true },
            { false, false, false, false, false, false },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, false, false, true, true },
            { false, false, true, false, false, false },
            { true, false, false, true, false, false },
            { false, false, false, false, false, false }
        },// 13th symbol
        {
            { true, true, true, true, false, false },
            { true, false, false, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, true, true, true, true },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 15th symbol
        {
            { true, true, true, true, true, true },
            { true, false, false, false, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, true, true, true, false },
            { false, false, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 17th symbol
        {
            { false, false, false, false, true, true },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { false, true, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 19th symbol
        {
            { true, true, false, false, true, true },
            { true, false, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, false, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 21st symbol
        {
            { true, true, true, true, true, true },
            { false, false, true, false, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, false, true, false, true },
            { true, false, true, true, true, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 23rd symbol
        {
            { false, false, false, false, true, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, true, true, false, false },
            { false, true, false, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 25th symbol
        {
            { true, false, false, false, false, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { false, true, true, false, false, true },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 27th symbol
        {
            { true, false, false, true, true, false },
            { true, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, false, false, true, false },
            { false, true, true, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 29th symbol
        {
            { true, true, false, true, true, false },
            { true, false, false, true, true, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { true, true, true, false, true, false },
            { false, true, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 31st symbol
        {
            { false, true, false, false, true, true },
            { true, true, true, true, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
        {
            { false, true, false, false, true, true },
            { false, true, true, false, false, true },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },// 31st symbol
        {
            { false, false, false, false, false, false },
            { false, true, false, false, true, false },
            { false, false, false, false, false, false },
            { false, false, false, false, false, false }
        },
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

    void GenerateModule()
    {
        for (int i = 0; i < 9; i++)
        {
            colors[i] = Random.Range(0, 15);
            btnRenderers[i].material = btnColors[colors[i]];
        }

        DebugMsg("The middle color is " + colorNames[colors[4]] + ".");

        if (colors[4] == 0) // if center point is red...
        {
            keyNum = Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumberLetters().First()) + 1;
            keyNum += Bomb.GetSerialNumberNumbers().Last() * Bomb.GetBatteryCount();

            if (Bomb.IsPortPresent(Port.StereoRCA) && !Bomb.IsPortPresent(Port.RJ45))
                keyNum *= 2;

            keyNum %= 100;
        }

        else if (colors[4] == 1) // if center point is orange...
        {
            if (literallyJustTheEntireAlphabet.Contains(Bomb.GetSerialNumber()[0]))
                keyNum = (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[0]) + 1) % 16 * 16;
            else
                keyNum = Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[0]) * 16;

            if (literallyJustTheEntireAlphabet.Contains(Bomb.GetSerialNumber()[1]))
                keyNum += (Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[1]) + 1) % 16;
            else
                keyNum += Array.IndexOf(andAlsoSomeNumbers, Bomb.GetSerialNumber()[1]);
        }

        else if (colors[4] == 2) // if center point is yellow...
        {
            foreach (var number in Bomb.GetSerialNumberNumbers())
            {
                keyNum += number;
            }

            keyNum += Bomb.GetStrikes();
        }

        else if (colors[4] == 3) // if center point is lime...
        {
            if (Bomb.GetBatteryCount() > 4)
            {
                if (Bomb.GetOffIndicators().Count() == 0)
                    keyNum = 20;
                else
                    keyNum = 60;
            }

            else
            {
                if (Bomb.GetOnIndicators().Count() == 0)
                    keyNum = 40;
                else
                    keyNum = 90;
            }

            if (Bomb.GetBatteryCount() == Bomb.GetIndicators().Count())
            {
                if (Bomb.GetSerialNumber().Contains('B') || Bomb.GetSerialNumber().Contains('U') || Bomb.GetSerialNumber().Contains('R') || Bomb.GetSerialNumber().Contains('G') || Bomb.GetSerialNumber().Contains('1') || Bomb.GetSerialNumber().Contains('4') || Bomb.GetSerialNumber().Contains('R'))
                    keyNum += 1;
            }

            else if (!(Bomb.GetSerialNumber().Contains('A') || Bomb.GetSerialNumber().Contains('L') || Bomb.GetSerialNumber().Contains('5') || Bomb.GetSerialNumber().Contains('3') || Bomb.GetSerialNumber().Contains('M')))
            {
                keyNum += 8;
            }
        }

        else if (colors[4] == 4) // if center point is green...
        {
            int firstNumber, secondNumber, thirdNumber, fourthNumber;
            int[,] tableOne =
            {
                { 260, 66, 164, 152 },
                { 73, 194, 99, 202 },
                { 116, 158, 240, 195 },
                { 269, 204, 121, 1 }
            };

            int[,] tableTwo =
            {
                { 252, 87 },
                { 220, 155 }
            };

            if (Bomb.GetSerialNumberNumbers().Count() == 2)
            {
                firstNumber = Bomb.GetSerialNumberNumbers().First() * 10 + Bomb.GetSerialNumberNumbers().Last() - Bomb.GetSerialNumberNumbers().First() * Bomb.GetSerialNumberNumbers().Last();
                secondNumber = Bomb.GetSerialNumberNumbers().Last() * 10 + Bomb.GetSerialNumberNumbers().First() - Bomb.GetSerialNumberNumbers().Last() * Bomb.GetSerialNumberNumbers().First();

                if (firstNumber > secondNumber)
                {
                    int swap = secondNumber;
                    secondNumber = firstNumber;
                    firstNumber = swap;
                }

                firstNumber %= 3;
                secondNumber %= 4;

                keyNum = tableOne[secondNumber, firstNumber];
            }

            else if (Bomb.GetSerialNumberNumbers().Count() == 3)
            {
                firstNumber = Bomb.GetSerialNumberNumbers().First();
                secondNumber = Bomb.GetSerialNumberNumbers().Skip(1).First();
                thirdNumber = Bomb.GetSerialNumberNumbers().Skip(2).First();

                if (firstNumber * secondNumber > thirdNumber * secondNumber - firstNumber)
                {
                    firstNumber += Bomb.GetBatteryCount();
                    secondNumber += Bomb.GetBatteryCount();
                    thirdNumber += Bomb.GetBatteryCount();
                }

                int even = 0, odd = 0;

                if (firstNumber % 2 == 0)
                    even++;
                else
                    odd++;
                if (secondNumber % 2 == 0)
                    even++;
                else
                    odd++;
                if (thirdNumber % 2 == 0)
                    even++;
                else
                    odd++;
                if (even == 2)
                    keyNum = 220;
                else if (odd == 2)
                {
                    if (firstNumber < thirdNumber && firstNumber < secondNumber)
                        keyNum = tableTwo[secondNumber % 2, thirdNumber % 2];
                    else if (secondNumber < firstNumber && secondNumber < thirdNumber)
                        keyNum = tableTwo[firstNumber % 2, thirdNumber % 2];
                    else
                        keyNum = tableTwo[firstNumber % 2, secondNumber % 2];
                }
                else if (even == 3)
                    keyNum = 220;
                else
                {
                    secondNumber += 2;
                    if (firstNumber < thirdNumber && firstNumber < secondNumber)
                        keyNum = tableTwo[secondNumber % 2, thirdNumber % 2];
                    else if (secondNumber < firstNumber && secondNumber < thirdNumber)
                        keyNum = tableTwo[firstNumber % 2, thirdNumber % 2];
                    else
                        keyNum = tableTwo[firstNumber % 2, secondNumber % 2];
                }
            }

            else if (Bomb.GetSerialNumberNumbers().Count() == 4)
            {
                firstNumber = Bomb.GetSerialNumberNumbers().First();
                secondNumber = Bomb.GetSerialNumberNumbers().Skip(1).First();
                thirdNumber = Bomb.GetSerialNumberNumbers().Skip(2).First();
                fourthNumber = Bomb.GetSerialNumberNumbers().Last();

                if (secondNumber == 0)
                    secondNumber = 1;
                if (fourthNumber == 0)
                    secondNumber = 1;

                int intDivided = firstNumber / secondNumber;
                float floatDivided = firstNumber / secondNumber;

                if (intDivided == floatDivided)
                    firstNumber = intDivided;
                else
                    firstNumber = firstNumber + secondNumber;

                intDivided = thirdNumber / fourthNumber;
                floatDivided = thirdNumber / fourthNumber;

                if (intDivided == floatDivided)
                    secondNumber = intDivided;
                else
                    secondNumber = thirdNumber + fourthNumber;

                firstNumber %= 4;
                secondNumber %= 4;

                keyNum = tableOne[secondNumber, firstNumber];
            }
        }

        else if (colors[4] == 5) // if center point is cyan...
        {
            if (Bomb.IsPortPresent(Port.Parallel))
                keyNum += 1;
            for (int i = 0; i < Bomb.GetSerialNumberLetters().Count(); i++)
            {
                if (vowels.Contains(Bomb.GetSerialNumberLetters().Skip(i).First()))
                {
                    keyNum += 1;
                    break;
                }

                if (i == Bomb.GetSerialNumberLetters().Count() - 1)
                    keyNum += 2;
            }
            keyNum += 2;
            if (Bomb.IsPortPresent(Port.Serial))
                keyNum += 2;

            if (Bomb.IsIndicatorOn(Indicator.FRK))
                keyNum = keyNum * 2 % 10 + 3;
            if (Bomb.GetSerialNumberNumbers().Count() > Bomb.GetSerialNumberLetters().Count())
                keyNum = keyNum * 2 % 10 + 4;
            if (Bomb.GetSerialNumberNumbers().Last() % 2 == 0)
                keyNum = keyNum * 2 % 10 + 5;
            else
                keyNum = keyNum * 3 % 10 + 8;
            if (Bomb.IsIndicatorOff(Indicator.NSA))
                keyNum = keyNum * 3 % 10 + 6;
            if (Bomb.GetSerialNumberNumbers().Count() < Bomb.GetSerialNumberLetters().Count())
                keyNum = keyNum * 3 % 10 + 7;

            keyNum %= 10;
        }

        else if (colors[4] == 6) // if center point is sky...
        {
            keyNum = Bomb.CountUniquePorts() * 7;
            int indicatorAmount = 0;

            foreach (var indicator in Bomb.GetOnIndicators())
            {
                if (Bomb.GetSerialNumberLetters().Contains(indicator[0]) || Bomb.GetSerialNumberLetters().Contains(indicator[1]) || Bomb.GetSerialNumberLetters().Contains(indicator[2]))
                {
                    indicatorAmount++;
                }
            }

            keyNum += indicatorAmount * 5;
            indicatorAmount = 0;

            foreach (var indicator in Bomb.GetOffIndicators())
            {
                if (Bomb.GetSerialNumberLetters().Contains(indicator[0]) || Bomb.GetSerialNumberLetters().Contains(indicator[1]) || Bomb.GetSerialNumberLetters().Contains(indicator[2]))
                {
                    indicatorAmount++;
                }
            }

            keyNum += indicatorAmount;
        }

        else if (colors[4] == 7) // if center point is blue...
        {
            if (Bomb.GetSerialNumberNumbers().First() == Bomb.GetSerialNumberNumbers().Last() && Bomb.GetBatteryCount().Equals(0))
                keyNum = 1;
            else if (Bomb.GetIndicators().Contains("CLR"))
                keyNum = 8;
            else if (Bomb.GetSerialNumberLetters().Contains('X') && Bomb.GetSerialNumberLetters().Contains('Y') && Bomb.GetSerialNumberLetters().Contains('Z'))
                keyNum = 20;
            else if (Bomb.GetPortCount() >= 5)
                keyNum = 30;
            else if (Bomb.GetBatteryCount() == 0)
                keyNum = 42;
            else if (Bomb.GetOnIndicators().Count() > Bomb.GetOffIndicators().Count())
                keyNum = 69;
            else
                keyNum = 3;
        }

        else if (colors[4] == 8) // if center point is purple...
        {
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if (numbers.Contains(Bomb.GetSerialNumber()[0]))
                keyNum = Bomb.GetSerialNumberNumbers().First();
            else
                keyNum = Array.IndexOf(literallyJustTheEntireAlphabet, Bomb.GetSerialNumber()[0]) + 6;
        }

        else if (colors[4] == 9) // if center point is magenta...
        {
            foreach (var character in Bomb.GetSerialNumber())
                if ("RADI4T07".Contains(character))
                    keyNum += 10;
            keyNum += 5 * Bomb.GetBatteryHolderCount(2);
            keyNum -= 5 * Bomb.GetBatteryHolderCount(1);
            if (keyNum < 0)
                keyNum *= -1;
        }

        else if (colors[4] == 10) // if center point is brown...
        {
            keyNum = Bomb.GetBatteryHolderCount() - Bomb.GetIndicators().Count() + Bomb.GetBatteryCount() * Bomb.GetPortCount() - Bomb.GetPortPlateCount();
            keyNum %= 5;
            while (keyNum <= 0)
                keyNum += 5;

            int[] temps = { 250, 160, 200, 180, 180 };
            keyNum = temps[keyNum - 1];
        }

        else if (colors[4] == 11) // if center point is white...
            keyNum = Bomb.GetPortCount() + Bomb.GetSerialNumberLetters().Count();

        else if (colors[4] == 12) // if center point is gray...
        {
            if (Bomb.IsIndicatorOn(Indicator.MSA))
                keyNum += 20;
            if (Bomb.IsPortPresent(Port.Serial))
                keyNum += 14;
            if (Bomb.GetSerialNumber().Contains('F') || Bomb.GetSerialNumber().Contains('A') || Bomb.GetSerialNumber().Contains('S') || Bomb.GetSerialNumber().Contains('T'))
                keyNum -= 5;
            if (Bomb.IsPortPresent(Port.RJ45))
                keyNum += 27;
            if (Bomb.GetBatteryCount() > 3)
                keyNum -= 15;
        }

        else if (colors[4] == 13) // if center point is black...
        {
            int[,] firstTable =
            {
                { 74, 53, 79 },
                { 62, 41, 70 },
                { 42, 47, 86 },
                { 38, 66, 51 },
                { 80, 67, 58 }
            };

            int firstNumber = Bomb.GetBatteryCount(Battery.AA) + Bomb.GetSerialNumberNumbers().Count(), secondNumber = Bomb.GetIndicators().Count() + Bomb.GetPortCount();

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

            keyNum = firstTable[firstNumber, secondNumber];
        }

        else // if center point is pink...
        {
            keyNum = Bomb.GetSerialNumberNumbers().First();

            foreach (var letter in Bomb.GetSerialNumberLetters())
            {
                if ("SCHOOL".Contains(letter))
                {
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
                        randomBool = true;
                        keyNum += 2;
                        break;
                    }
                }

                if (randomBool)
                    break;
            }

            if (Bomb.IsPortPresent(Port.Parallel))
                keyNum += 2;

            if (Bomb.IsIndicatorPresent(Indicator.FRK) || Bomb.IsIndicatorPresent(Indicator.NSA))
                keyNum += 2;

            foreach (var letter in Bomb.GetSerialNumber())
            {
                if ("AEIOU".Contains(letter))
                {
                    keyNum += 5;
                    break;
                }
            }

            if (Bomb.GetBatteryCount(Battery.D) > 1)
            {
                keyNum += 2;
            }

            if (Bomb.GetOnIndicators().Contains("BOB"))
                keyNum = 1;
        }

        DebugMsg("The key number accumulated from the manual required is " + (keyNum) + ".");

        if (keyNum < 0)
            keyNum *= -1;
        keyNum %= 35;

        DebugMsg("The final key number is " + (keyNum + 1) + ".");

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
            DebugMsg("You connected two points.");

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

            else
            {
                DebugMsg("You can't do that.");
            }

            btnSelected = 99;
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Brush Strokes #{0}] {1}", _moduleId, msg);
    }

    void Submit()
    {
        if (solved) return;
        bool nopeThatsWrong = false;
        string firstPart;

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

                    if (i == 0)
                        firstPart = "horizontal";
                    else if (i == 1)
                        firstPart = "vertical";
                    else if (i == 2)
                        firstPart = "top-left to bottom-right diagonal";
                    else
                        firstPart = "top-right to bottom-left diagonal";

                    if (gaps[i, x])
                        DebugMsg("The " + (x + 1) + "th " + firstPart + " line in reading order shouldn't be there.");
                    else
                        DebugMsg("The " + (x + 1) + "th " + firstPart + " line in reading order should be there.");
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
            DebugMsg("Congratulations, that was right. Module solved.");
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
                        btnPress(temp);
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                StartCoroutine(Count());
                yield return "strike";
                yield return "solve";
            }
            yield break;
        }
    }
}
