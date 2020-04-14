using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace XanaBot
{
    public enum ConsoleAnswerType
    {
        YesNo = 0,
        YesNoCancel = 1,
        TrueFalse = 2
    }

    public enum ConsoleAnswer
    {
        Yes = 0,
        No = 1,
        Cancel = 2,
        True = 3,
        False = 4,
        Undefined = 5
    }

    public enum ConsoleInputType
    {
        String = 0,
        Int = 1,
        Double = 2,
        Ulong = 3
    }

    public static class CInput
    {
        const string _readPrompt = "XanaBot> ";
        internal const string _badCommandMessage = "This command does not exist.";

        private static string[] _commandHistory = { "" };
        private static bool insertMode = false;

        private static string _readBuffer = "";
        private static int cursorPos;
        private static int historyIndex = 0;
        private static string newReadBuffer = "";

        public static object ReadFromConsole(string promptMessage = "", ConsoleInputType inputType = ConsoleInputType.String, bool canEscape = false, ConsoleColor color = ConsoleColor.White, int maxChars = -1, char charMask = Char.MinValue)
        {
            // set vars
            ConsoleColor initialColor = Console.ForegroundColor;
            bool maskChars = (charMask != Char.MinValue);
            bool pressedEnter = false;
            string prompt = (promptMessage == "" ? _readPrompt : promptMessage);
            cursorPos = 0;
            historyIndex = 0;
            _readBuffer = "";

            // Show a prompt
            Console.ForegroundColor = color;
            Console.Write(prompt);

            // Get input
            while (!pressedEnter)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                // ENTER KEY
                if (key.Key == ConsoleKey.Enter)
                {
                    ValidateHistory();

                    AddToHistory(_readBuffer);

                    Console.ForegroundColor = initialColor;
                    CFormat.WriteLine("");

                    if (inputType == ConsoleInputType.String)
                    {
                        return _readBuffer;
                    }
                    else if (inputType == ConsoleInputType.Int)
                    {
                        return int.Parse(_readBuffer); // we can parse safely because input keys were filtered
                    }
                    else if (inputType == ConsoleInputType.Double)
                    {
                        return Double.Parse(_readBuffer.Replace(".", ",")); // we can parse safely because input keys were filtered
                    }
                    else if (inputType == ConsoleInputType.Ulong)
                    {
                        return ulong.Parse(_readBuffer); // we can parse safely because input keys were filtered
                    }

                }

                // BACKSPACE KEY
                else if (key.Key == ConsoleKey.Backspace)
                {
                    ValidateHistory();

                    if (cursorPos <= 0)
                    {
                        continue;
                    }

                    if (cursorPos != _readBuffer.Length)
                    {
                        string beforeChar = _readBuffer.Substring(0, cursorPos - 1);
                        string afterChar = _readBuffer.Substring(cursorPos, _readBuffer.Length - cursorPos);
                        _readBuffer = beforeChar + afterChar;

                        MoveCursorBack();
                        UserWrite(afterChar + " ");
                        for (int i = 0; i < (afterChar.Length + 1); i++)
                        {
                            MoveCursorBack();
                        }
                    }
                    else
                    {
                        _readBuffer = _readBuffer.Substring(0, _readBuffer.Length - 1);
                        MoveCursorBack();
                        UserWrite(" ");
                        MoveCursorBack();
                    }
                }

                // DELETE KEY
                else if (key.Key == ConsoleKey.Delete)
                {
                    ValidateHistory();

                    if (cursorPos >= _readBuffer.Length)
                    {
                        continue;
                    }

                    string beforeChar = _readBuffer.Substring(0, cursorPos);
                    string afterChar = _readBuffer.Substring(cursorPos + 1, _readBuffer.Length - cursorPos - 1);
                    _readBuffer = beforeChar + afterChar;

                    UserWrite(afterChar + " ");
                    for (int i = 0; i < (afterChar.Length + 1); i++)
                    {
                        MoveCursorBack();
                    }
                }

                // DEBUT KEY
                else if (key.Key == ConsoleKey.Home)
                {
                    for (int i = cursorPos; i > 0; i--)
                    {
                        MoveCursorBack();
                    }
                }

                // END KEY
                else if (key.Key == ConsoleKey.End)
                {
                    for (int i = cursorPos; i < _readBuffer.Length; i++)
                    {
                        MoveCursorAhead();
                    }
                }

                // ESCAPE KEY
                else if (key.Key == ConsoleKey.Escape)
                {
                    if (canEscape)
                    {
                        return null;
                    }
                }

                // INSERT KEY
                else if (key.Key == ConsoleKey.Insert)
                {
                    insertMode = !insertMode;
                    if (insertMode)
                    {
                        Console.CursorSize = 100;
                    }
                    else
                    {
                        Console.CursorSize = 1;
                    }
                }

                // Do not seek for specific kees after this line: ---

                // ARROW KEYS and WHITE SPACE KEYS
                else if (key.Key != ConsoleKey.Spacebar && key.KeyChar == Char.MinValue)
                {
                    if (key.Key == ConsoleKey.RightArrow && cursorPos < _readBuffer.Length)
                    {
                        MoveCursorAhead();
                    }
                    else if (key.Key == ConsoleKey.LeftArrow && cursorPos > 0)
                    {
                        MoveCursorBack();
                    }
                    else if (key.Key == ConsoleKey.UpArrow)
                    {
                        OlderHistory();
                    }
                    else if (key.Key == ConsoleKey.DownArrow)
                    {
                        NewerHistory();
                    }
                }

                // ANY OTHER KEY
                else
                {
                    ValidateHistory();

                    // MAX CHARS CHECK
                    if (maxChars > 0 && _readBuffer.Length >= maxChars)
                    {
                        continue;
                    }

                    // INPUT TYPE CHECK
                    if (inputType == ConsoleInputType.Int)
                    {
                        if (!int.TryParse(key.KeyChar.ToString(), out int temp)
                            && (key.KeyChar.ToString() != "-" || (key.KeyChar.ToString() == "-" && _readBuffer.Length != 0)))
                            // If input is not a number in int mode, continue
                        {
                            continue;
                        }
                    }
                    else if (inputType == ConsoleInputType.Double)
                    {
                        if (!int.TryParse(key.KeyChar.ToString(), out int temp)
                            && (key.KeyChar.ToString() != "." || (key.KeyChar.ToString() == "." && _readBuffer.Contains(".")))
                            && (key.KeyChar.ToString() != "-" || (key.KeyChar.ToString() == "-" && (_readBuffer.Length != 0 || _readBuffer.Contains(".")))))
                            // good luck
                        {
                            continue;
                        }
                    }
                    else if (inputType == ConsoleInputType.Ulong)
                    {
                        if (!int.TryParse(key.KeyChar.ToString(), out int temp)
                            && (key.KeyChar.ToString() != "." || (key.KeyChar.ToString() == "." && _readBuffer.Contains("."))))
                            // good luck
                        {
                            continue;
                        }
                    }

                    if (cursorPos != _readBuffer.Length && !insertMode)
                    {
                        _readBuffer = _readBuffer.Substring(0, cursorPos) + key.KeyChar + _readBuffer.Substring(cursorPos, _readBuffer.Length - cursorPos);
                    }
                    else if (cursorPos != _readBuffer.Length && insertMode)
                    {
                        _readBuffer = _readBuffer.Substring(0, cursorPos) + key.KeyChar + _readBuffer.Substring(cursorPos + 1, _readBuffer.Length - cursorPos - 1);
                    }
                    else
                    {
                        _readBuffer += key.KeyChar;
                    }

                    if (maskChars)
                    {
                        UserWrite(charMask.ToString());
                    }
                    else
                    {
                        UserWrite(key.KeyChar.ToString());

                        if (cursorPos != _readBuffer.Length && !insertMode)
                        {
                            string aheadCursor = _readBuffer.Substring(cursorPos, _readBuffer.Length - cursorPos);
                            UserWrite(aheadCursor); // Re write text that was ahead cursor
                            for (int i = 0; i < (aheadCursor.Length); i++)
                            {
                                MoveCursorBack();
                            }
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine(_readBuffer);
            }

            return null;
        }

        public static ConsoleAnswer UserChoice(ConsoleAnswerType type)
        {
            switch (type)
            {
                case ConsoleAnswerType.YesNo:
                    string result = ReadFromConsole("(YES / NO): ").ToString().ToLower();
                    if (result.ToLower() == "y" || result.ToLower() == "yes")
                    {
                        return ConsoleAnswer.Yes;
                    }
                    else if (result.ToLower() == "n" || result.ToLower() == "no")
                    {
                        return ConsoleAnswer.No;
                    }
                    else
                    {
                        return UserChoice(type); // Re ask
                    }

                case ConsoleAnswerType.YesNoCancel:
                    string result1 = ReadFromConsole("(YES / NO / CANCEL): ").ToString().ToLower();
                    if (result1.ToLower() == "y" || result1.ToLower() == "yes")
                    {
                        return ConsoleAnswer.Yes;
                    }
                    else if (result1.ToLower() == "n" || result1.ToLower() == "no")
                    {
                        return ConsoleAnswer.No;
                    }
                    else if (result1.ToLower() == "c" || result1.ToLower() == "cancel")
                    {
                        return ConsoleAnswer.Cancel;
                    }
                    else
                    {
                        return UserChoice(type); // Re ask
                    }

                case ConsoleAnswerType.TrueFalse:
                    string result2 = ReadFromConsole("(TRUE / FALSE): ").ToString().ToLower();
                    if (result2.ToLower() == "t" || result2.ToLower() == "true")
                    {
                        return ConsoleAnswer.Yes;
                    }
                    else if (result2.ToLower() == "f" || result2.ToLower() == "false")
                    {
                        return ConsoleAnswer.No;
                    }
                    else
                    {
                        return UserChoice(type); // Re ask
                    }

                default:
                    CFormat.WriteLine("Could not get user choice, specifed answer type does not exist.", ConsoleColor.Red);
                    return ConsoleAnswer.Undefined;
            }
        }

        /// <summary>
        /// User must make a pick a number between 0 and maxNumber.
        /// </summary>
        /// <param name="maxNumber"></param>
        /// <returns></returns>
        public static int UserPickInt(int maxNumber)
        {
            var pickNumber = ReadFromConsole("Enter a number between 0 and " + maxNumber + ": ", ConsoleInputType.Int, true, ConsoleColor.White, maxNumber.ToString().Length);

            if (pickNumber == null)
            {
                CFormat.WriteLine("Canceled.");
                return -1;
            }

            if ((int)pickNumber >= 0 && (int)pickNumber <= maxNumber)
            {
                return (int)pickNumber;
            }
            else
            {
                UserPickInt(maxNumber); // re ask
                return -1; // in case of error. Should never reach this point.
            }
        }

        /// <summary>
        /// If user pressed another key than arrows while going through history, it is considered as new user input.
        /// </summary>
        private static void ValidateHistory()
        {
            if (newReadBuffer != "") // if user starts editing an history, it replaces what user was editing previously
            {
                _readBuffer = newReadBuffer;
                historyIndex = 0;
                newReadBuffer = "";
            }
        }

        private static void AddToHistory(string s)
        {
            _commandHistory = _commandHistory.Concat(new string[] { s }).ToArray();
        }

        private static void OlderHistory()
        {
            if (historyIndex + 2 > _commandHistory.Length || _commandHistory.Length <= 1)
            {
                return;
            }

            historyIndex++;
            newReadBuffer = _commandHistory[_commandHistory.Length - historyIndex];
            for (int i = cursorPos; i > 0; i--)
            {
                MoveCursorBack();
                UserWrite(" ");
                MoveCursorBack();
            }
            UserWrite(newReadBuffer);
        }

        private static void NewerHistory()
        {
            if (historyIndex == 0)
            {
                return;
            }

            if (historyIndex == 1)
            {
                historyIndex--;
                newReadBuffer = "";
                for (int i = cursorPos; i > 0; i--)
                {
                    MoveCursorBack();
                    UserWrite(" ");
                    MoveCursorBack();
                }
                UserWrite(_readBuffer);
            }
            else
            {
                historyIndex--;
                newReadBuffer = _commandHistory[_commandHistory.Length - historyIndex];
                for (int i = cursorPos; i > 0; i--)
                {
                    MoveCursorBack();
                    UserWrite(" ");
                    MoveCursorBack();
                }
                UserWrite(newReadBuffer);
            }
        }

        private static void MoveCursorBack()
        {
            if (Console.CursorLeft > 0) // if not reached start of the line
            {
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                cursorPos--;
            }
            else if (Console.CursorTop > 0)
            {
                Console.SetCursorPosition(Console.WindowWidth - 1, Console.CursorTop - 1);
                cursorPos--;
            }
        }

        private static void MoveCursorAhead()
        {
            if (Console.CursorLeft < Console.WindowWidth - 1) // if not reached start of the line
            {
                Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                cursorPos++;
            }
            else if (Console.CursorTop < Console.BufferHeight)
            {
                Console.SetCursorPosition(0, Console.CursorTop + 1);
                cursorPos++;
            }
        }

        private static void UserWrite(string s)
        {
            Console.Write(s);
            cursorPos += s.Length;
        }
    }
}
