using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Json5Core
{
    internal static class Formatter
    {
        public static string PrettyPrint(string input, int spaces)
        {
            StringBuilder output = new StringBuilder(input.Length * 2);
            int depth = 0;
            
            ReadOnlySpan<char> span = input.AsSpan();

            int len = span.Length;

            for (int i = 0; i < len; i++)
            {
                char ch = span[i];

                if (ch == '"')
                {
                    output.Append(ch);
                    while (++i < len)
                    {
                        ch = span[i];
                        output.Append(ch);
                        if (ch == '\\')
                        {
                            output.Append(span[++i]);
                            continue;
                        }
                        if (ch == '"') break;
                    }
                    continue;
                }

                switch (ch)
                {
                    case '{':
                    case '[':
                        output.Append(ch)
                            .Append('\n');
                        AppendSpaces(output, ++depth * spaces);
                        break;

                    case '}':
                    case ']':
                        output.Append('\n');
                        AppendSpaces(output, --depth * spaces);
                        output.Append(ch);
                        break;

                    case ',':
                        output.Append(ch)
                            .Append('\n');
                        AppendSpaces(output, depth * spaces);
                        break;

                    case ':':
                        output.Append(" : ");
                        break;

                    default:
                        if (!char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendSpaces(StringBuilder sb, int count)
        {
            sb.Append(' ', count);
        }
    }
}