using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XanaBot
{
    public static class CFormat
    {
        /// <summary>
        /// Check if the arg is in the correct format
        /// </summary>
        /// <param name="requiredType"></param>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        internal static object CoerceArgument(Type requiredType, string inputValue)
        {
            var requiredTypeCode = Type.GetTypeCode(requiredType);
            string exceptionMessage =
                string.Format("Impossible de convertir la valeur demandée {0} vers le type {1}.",
                inputValue, requiredType.Name);

            object result = null;
            switch (requiredTypeCode)
            {
                case TypeCode.String:
                    result = inputValue;
                    break;
                case TypeCode.Int16:
                    short number16;
                    if (Int16.TryParse(inputValue, out number16))
                    {
                        result = number16;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Int32:
                    int number32;
                    if (Int32.TryParse(inputValue, out number32))
                    {
                        result = number32;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Int64:
                    long number64;
                    if (Int64.TryParse(inputValue, out number64))
                    {
                        result = number64;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Boolean:
                    bool trueFalse;
                    if (bool.TryParse(inputValue, out trueFalse))
                    {
                        result = trueFalse;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Byte:
                    byte byteValue;
                    if (byte.TryParse(inputValue, out byteValue))
                    {
                        result = byteValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Char:
                    char charValue;
                    if (char.TryParse(inputValue, out charValue))
                    {
                        result = charValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.DateTime:
                    DateTime dateValue;
                    if (DateTime.TryParse(inputValue, out dateValue))
                    {
                        result = dateValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Decimal:
                    Decimal decimalValue;
                    if (Decimal.TryParse(inputValue, out decimalValue))
                    {
                        result = decimalValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Double:
                    Double doubleValue;
                    if (Double.TryParse(inputValue, out doubleValue))
                    {
                        result = doubleValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.Single:
                    Single singleValue;
                    if (Single.TryParse(inputValue, out singleValue))
                    {
                        result = singleValue;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.UInt16:
                    UInt16 uInt16Value;
                    if (UInt16.TryParse(inputValue, out uInt16Value))
                    {
                        result = uInt16Value;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.UInt32:
                    UInt32 uInt32Value;
                    if (UInt32.TryParse(inputValue, out uInt32Value))
                    {
                        result = uInt32Value;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                case TypeCode.UInt64:
                    UInt64 uInt64Value;
                    if (UInt64.TryParse(inputValue, out uInt64Value))
                    {
                        result = uInt64Value;
                    }
                    else
                    {
                        throw new ArgumentException(exceptionMessage);
                    }
                    break;
                default:
                    throw new ArgumentException(exceptionMessage);
            }
            return result;
        }

        /// <summary>
        /// Adds space before the line.
        /// </summary>
        /// <param name="nb"></param>
        /// <returns></returns>
        public static string Indent(int nb)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nb; i++)
            {
                sb.Append(' ');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Draw a progress bar
        /// </summary>
        /// <param name="complete"></param>
        /// <param name="maxVal"></param>
        /// <param name="barSize"></param>
        /// <param name="progressCharacter"></param>
        public static void DrawProgressBar(double complete, double maxVal, int barSize, char progressCharacter, ConsoleColor primaryColor = ConsoleColor.Green,
            ConsoleColor secondaryColor = ConsoleColor.DarkGreen)
        {
            Console.CursorVisible = false;
            int left = Console.CursorLeft;
            double perc = complete / maxVal;
            int chars = (int)Math.Floor(perc / (1d / barSize));
            string p1 = String.Empty, p2 = String.Empty;

            for (int i = 0; i < chars; i++) p1 += progressCharacter;
            for (int i = 0; i < barSize - chars; i++) p2 += progressCharacter;

            Console.ForegroundColor = primaryColor;
            CFormat.Write(p1);
            Console.ForegroundColor = secondaryColor;
            CFormat.Write(p2);
            Console.ForegroundColor = primaryColor;
            CFormat.Write(String.Format(" {0}%", (perc * 100).ToString("N2")));

            Console.ResetColor();
            Console.CursorVisible = true;
            Console.CursorLeft = left;
        }

        /// <summary>
        /// Simply jumps a line.
        /// </summary>
        public static void JumpLine()
        {
            Console.WriteLine("");
        }

        /// <summary>
        /// Prints a line with time and sender. It can be colored.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="col"></param>
        public static void Print(string text, string sender, DateTime time, ConsoleColor color = ConsoleColor.Gray)
        {
            if (Console.ForegroundColor != color)
            {
                Console.ForegroundColor = color;
            }

            Console.WriteLine(time.ToLongTimeString() + " [" + sender + "] " + text);
        }

        /// <summary>
        /// Writes a line text. It can be colored.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="col"></param>
        public static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (Console.ForegroundColor != color)
            {
                Console.ForegroundColor = color;
            }

            Console.WriteLine(text);
        }

        /// <summary>
        /// Writes text. It can be colored.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="col"></param>
        public static void Write(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (Console.ForegroundColor != color)
            {
                Console.ForegroundColor = color;
            }

            Console.Write(text);
        }

        public static string AddPluralS(double number)
        {
            if (number >= -1 && number <= 1)
            {
                return "";
            }
            else
            {
                return "s";
            }
        }
    }
}
