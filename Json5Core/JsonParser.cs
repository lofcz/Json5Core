using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Json5Core
{

    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    /// </summary>
    internal sealed class JsonParser
    {
        enum Token
        {
            None = -1,           // Used to denote no Lookahead available
            Curly_Open,
            Curly_Close,
            Squared_Open,
            Squared_Close,
            Colon,
            Comma,
            String,
            Number,
            True,
            False,
            Null//, 
            //Key
            ,
            PosInfinity,
            NegInfinity,
            NaN
        }

        // slower than StringBuilder
        //class myStringBuilder
        //{

        //    int _index = 0;
        //    char[] _str = new char[32*1024];

        //    public void Append(char c)
        //    {
        //        _str[_index++] = c;
        //    }

        //    public void Clear()
        //    {
        //        _index = 0;
        //    }

        //    public override string ToString()
        //    {
        //        return new string(_str, 0, _index - 1);
        //    }
        //}

        readonly char[] json;
        readonly StringBuilder s = new StringBuilder(); // used for inner string parsing " \"\r\n\u1234\'\t " 
        //readonly myStringBuilder s = new myStringBuilder(); 
        Token lookAheadToken = Token.None;
        int index;
        bool allownonquotedkey;
        //bool AllowJson5String = false;
        int _len;
        Json5SafeDictionary<string, bool>? _lookup;
        Json5SafeDictionary<Type, bool>? _seen;
        bool _parseJsonType;
        IList<string> warnings;

        internal JsonParser(string json, bool AllowNonQuotedKeys, IList<string> warnings)
        {
            allownonquotedkey = AllowNonQuotedKeys;
            //this.AllowJson5String = AllowJson5String;
            this.json = json.ToCharArray();
            _len = json.Length;
            this.warnings = warnings;
        }

        private void SetupLookup()
        {
            _lookup = new Json5SafeDictionary<string, bool>();
            _seen = new Json5SafeDictionary<Type, bool>();
            _lookup.Add("$types", true);
            _lookup.Add("$type", true);
            _lookup.Add("$i", true);
            _lookup.Add("$map", true);
            _lookup.Add("$schema", true);
            _lookup.Add("k", true);
            _lookup.Add("v", true);
        }

        public unsafe object Decode(Type? type)
        {
            SetupLookup();

            BuildLookup(type);
            
            fixed (char* p = json)
            {
                if (type != null)
                {
                    if (!CheckForTypeInJson(p))
                    {
                        _parseJsonType = true;
                        SetupLookup();

                        BuildLookup(type);

                        // reset if no properties found
                        if (_parseJsonType == false || _lookup.Count() == 7)
                            _lookup = null;
                    }
                }
                
                object? retV = ParseValue(p);
                SkipWhitespace(p);
                if (index != _len)
                {
                    throw new Exception($"Invalid character '{ p[index] }' at index { index }");
                }
                return retV;
            }
        }

        private unsafe bool CheckForTypeInJson(char* p)
        {
            int idx = 0;
            int len = _len > 1000 ? 1000 : _len;
            while (idx < len)
            {
                if (p[idx + 0] == '$' &&
                    p[idx + 1] == 't' &&
                    p[idx + 2] == 'y' &&
                    p[idx + 3] == 'p' &&
                    p[idx + 4] == 'e' &&
                    p[idx + 5] == 's'
                    )
                    return true;
                idx++;
            }

            return false;
        }

        private void BuildGenericTypeLookup(Type t)
        {
            if (_seen.TryGetValue(t, out bool _))
                return;

            foreach (Type? e in t.GetGenericArguments())
            {
                if (e.IsPrimitive())
                    continue;

                bool isstruct = e.IsValueType() && !e.IsEnum();

                if ((e.IsClass() || isstruct || e.IsAbstract()) && e != typeof(string) && e != typeof(DateTime) && e != typeof(Guid))
                {
                    BuildLookup(e);
                }
            }
        }

        private void BuildArrayTypeLookup(Type t)
        {
            if (_seen.TryGetValue(t, out bool _))
                return;

            bool isstruct = t.IsValueType() && !t.IsEnum();

            if ((t.IsClass() || isstruct) && t != typeof(string) && t != typeof(DateTime) && t != typeof(Guid))
            {
                BuildLookup(t.GetElementType());
            }
        }

        private void BuildLookup(Type objtype)
        {
            // build lookup
            if (objtype == null)
                return;

            if (objtype == typeof(NameValueCollection) || objtype == typeof(StringDictionary))
                return;

            //if (objtype == typeof(DataSet) || objtype == typeof(DataTable)) 
            //    return;

            if (typeof(IDictionary).IsAssignableFrom(objtype))
                return;

            if (_seen.TryGetValue(objtype, out bool _))
                return;

            if (objtype.IsGenericType())
                BuildGenericTypeLookup(objtype);

            else if (objtype.IsArray)
            {
                Type t = objtype;
                BuildArrayTypeLookup(objtype);
            }
            else
            {
                _seen.Add(objtype, true);

                foreach (KeyValuePair<string, myPropInfo> m in Reflection.Instance.Getproperties(objtype, objtype.FullName, true))
                {
                    Type t = m.Value.pt;

                    _lookup.Add(m.Key.ToLowerInvariant(), true);

                    if (t.IsArray)
                        BuildArrayTypeLookup(t);

                    if (t.IsGenericType())
                    {
                        // skip if dictionary
                        if (typeof(IDictionary).IsAssignableFrom(t))
                        {
                            _parseJsonType = false;
                            return;
                        }
                        BuildGenericTypeLookup(t);
                    }
                    if (t.FullName.IndexOf("System.") == -1)
                        BuildLookup(t);
                }
            }
        }

        private bool InLookup(string name)
        {
            if (_lookup == null)
                return true;

            return _lookup.TryGetValue(name.ToLowerInvariant(), out bool v);
        }

        bool _parseType;
        private unsafe Dictionary<string, object> ParseObject(char* p)
        {
            Dictionary<string, object> obj = new Dictionary<string, object>();

            ConsumeToken(); // {

            bool wasComma = true;
            while (true)
            {
                switch (LookAhead(p))
                {

                    case Token.Comma:
                        if (wasComma) throw new Exception("Unexpected comma at index " + index);
                        wasComma = true;
                        ConsumeToken();
                        break;

                    case Token.Curly_Close:
                        ConsumeToken();
                        return obj;

                    default:
                        if (!wasComma) throw new Exception("Missing comma at index " + index);
                        wasComma = false;
                        // name
                        string name = ParseKey(p);

                        Token n = NextToken(p);
                        // :
                        if (n != Token.Colon)
                        {
                            throw new Exception("Expected colon at index " + index);
                        }

                        if (_parseJsonType)
                        {
                            if (name == "$types")
                            {
                                _parseType = true;
                                Dictionary<string, object> types = (Dictionary<string, object>)ParseValue(p);
                                _parseType = false;
                                // parse $types 
                                // performance hit here
                                if (_lookup == null)
                                    SetupLookup();

                                foreach (string? v in types.Keys)
                                    BuildLookup(Reflection.Instance.GetTypeFromCache(v, true));

                                obj[name] = types;

                                break;
                            }

                            if (name == "$schema")
                            {
                                _parseType = true;
                                object? value = ParseValue(p);
                                _parseType = false;
                                obj[name] = value;
                                break;
                            }

                            if (_parseType || InLookup(name))
                                obj[name] = ParseValue(p);
                            else
                                SkipValue(p);
                        }
                        else
                        {
                            obj[name] = ParseValue(p);
                        }
                        break;
                }
            }
        }

        private unsafe void SkipValue(char* p)
        {
            // optimize skipping
            switch (LookAhead(p))
            {
                case Token.Number:
                    ParseNumber(p, true);
                    break;

                case Token.String:
                    SkipString(p);
                    break;

                case Token.Curly_Open:
                    SkipObject(p);
                    break;

                case Token.Squared_Open:
                    SkipArray(p);
                    break;

                case Token.True:
                case Token.False:
                case Token.Null:
                case Token.PosInfinity:
                case Token.NegInfinity:
                case Token.NaN:
                    ConsumeToken();
                    break;
            }
        }

        private unsafe void SkipObject(char* p)
        {
            ConsumeToken(); // {

            while (true)
            {
                switch (LookAhead(p))
                {

                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.Curly_Close:
                        ConsumeToken();
                        return;

                    default:
                        // name
                        SkipString(p);

                        Token n = NextToken(p);
                        // :
                        if (n != Token.Colon)
                        {
                            throw new Exception("Expected colon at index " + index);
                        }
                        SkipValue(p);
                        break;
                }
            }
        }

        private unsafe void SkipArray(char* p)
        {
            ConsumeToken(); // [

            while (true)
            {
                switch (LookAhead(p))
                {
                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.Squared_Close:
                        ConsumeToken();
                        return;

                    default:
                        SkipValue(p);
                        break;
                }
            }
        }

        private unsafe void SkipString(char* p)
        {
            ConsumeToken();

            int len = _len;

            // escaped string
            while (index < len)
            {
                char c = p[index++];
                if (c == '"')
                    return;

                if (c == '\\')
                {
                    c = p[index++];

                    if (c == 'u')
                        index += 4;
                }
            }
        }

        private unsafe List<object> ParseArray(char* p)
        {
            List<object> array = [];
            ConsumeToken(); // [

            while (true)
            {
                switch (LookAhead(p))
                {
                    case Token.Comma:
                        ConsumeToken();
                        break;

                    case Token.Squared_Close:
                        ConsumeToken();
                        return array;

                    default:
                        array.Add(ParseValue(p));
                        break;
                }
            }
        }

        private unsafe object ParseValue(char* p)//, bool val)
        {
            switch (LookAhead(p))
            {
                case Token.Number:
                    return ParseNumber(p, false);

                case Token.String:
                    return ParseString(p);

                case Token.Curly_Open:
                    return ParseObject(p);

                case Token.Squared_Open:
                    return ParseArray(p);

                case Token.True:
                    ConsumeToken();
                    return true;

                case Token.False:
                    ConsumeToken();
                    return false;

                case Token.Null:
                    ConsumeToken();
                    return null;

                case Token.PosInfinity:
                    ConsumeToken();
                    return double.PositiveInfinity;

                case Token.NegInfinity:
                    ConsumeToken();
                    return double.NegativeInfinity;

                case Token.NaN:
                    ConsumeToken();
                    return double.NaN;
            }

            throw new Exception("Unrecognized token at index " + index);
        }

        private unsafe string ParseKey(char* p)
        {
            if (allownonquotedkey == false || p[index - 1] == '"' || p[index - 1] == '\'')
                return ParseString(p);

            return ParseIdentifierNameWithoutQuoteString(p);
        }

        private unsafe string ParseIdentifierNameWithoutQuoteString(char* p)
        {
            bool IsUnicodeLetter(char c) => CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter or UnicodeCategory.LetterNumber;
            bool IsUnicodeCombiningMark(char c) => CharUnicodeInfo.GetUnicodeCategory(c) is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark;
            bool IsUnicodeDigit(char c) => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber;
            bool IsUnicodeConnectorPunctuation(char c) => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation;

            ConsumeToken();

            if (s.Length > 0)
                s.Length = 0;

            bool first = true;
            while (true)
            {
                bool isEscape = false;
                char c = p[index++];
                if (IsUnicodeLetter(c) || c is '$' or '_')
                {
                    s.Append(c);
                    first = false;
                }
                else if (c == '\\')
                {
                    isEscape = true;
                    c = p[index++];
                    switch (c)
                    {
                        case 'u':
                        {
                            int remainingLength = _len - index;
                            if (remainingLength < 4) throw new Exception("\\u escape sequence ended prematurely (end of input)");

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode(p[index], p[index + 1], p[index + 2], p[index + 3]);
                            char c1 = (char)codePoint;
                            if (c1 is not '$' and not '_' && !IsUnicodeLetter(c1))
                            {
                                if (first) throw new Exception($"Invalid starting character '{ "\\u" + codePoint.ToString("X4") }' in IdentifierName at index { index - 1 }");
                                if (codePoint is not 0x200C and not 0x200D && !IsUnicodeCombiningMark(c1) && !IsUnicodeDigit(c1) && !IsUnicodeConnectorPunctuation(c1)) throw new Exception($"Invalid continuation character '{ "\\u" + codePoint.ToString("X4") }' in IdentifierName at index { index - 1 }");
                            }
                            s.Append(c1);

                            // skip 4 chars
                            index += 4;
                        }
                        break;

                        default: goto error;
                    }
                    first = false;
                }
                else
                {
                    if (first) goto error;
                    if (IsUnicodeCombiningMark(c) || IsUnicodeDigit(c) || IsUnicodeConnectorPunctuation(c) || c is '\u200C' or '\u200D')
                    {
                        s.Append(c);
                    }
                    else break;
                }
                continue;
                error:
                if (first) throw new Exception($"Invalid { (isEscape ? "escape" : "character") } '{ (isEscape ? "\\" : "") + c }' in IdentifierName at index { (isEscape ? index - 1 : index) }");
                break;
            }
            index--;

            return s.ToString();
        }

        private unsafe string ParseString(char* p)
        {
            char quote = p[index - 1];
            if (quote != '\'' && quote != '"') throw new Exception("Invalid token at index " + index);
            ConsumeToken();

            if (s.Length > 0)
                s.Length = 0;

            //if (AllowJson5String)
            //    return ParseJson5String(p);

            int len = _len;
            int run = 0;

            // non escaped string
            while (index + run < len)
            {
                char c = p[index + run++];
                if (c == '\\')
                    break;
                if (c == '\n' || c == '\r') throw new Exception("Illegal newline character in string at index " + index);
                if (c == '\u2028' || c == '\u2029') break;
                if (c == quote)//'\"')
                {
                    string? str = UnsafeSubstring(p, index, run - 1);
                    index += run;
                    return str;
                }
            }

            // escaped string
            while (index < len)
            {
                char c = p[index++];
                if (c == quote)//'"')
                    return s.ToString();

                if (c != '\\')
                {
                    if (c == '\n' || c == '\r') throw new Exception("Illegal newline character in string at index " + (index - 1));
                    if (c == '\u2028') warnings?.Add($"Warning: invalid ECMAScript at index { index - 1 } with character \\u2028 in string.");
                    else if (c == '\u2029') warnings?.Add($"Warning: invalid ECMAScript at index { index - 1 } with character \\u2029 in string.");
                    s.Append(c);
                }
                else
                {
                    c = p[index++];
                    switch (c)
                    {
                        //case '\'':
                        //    s.Append('\'');
                        //    break;
                        //case '"':
                        //    s.Append('"');
                        //    break;

                        //case '\\':
                        //    s.Append('\\');
                        //    break;

                        //case '/':
                        //    s.Append('/');
                        //    break;

                        case 'b':
                            s.Append('\b');
                            break;

                        case 'f':
                            s.Append('\f');
                            break;

                        case 'n':
                            s.Append('\n');
                            break;

                        case 'r':
                            s.Append('\r');
                            break;

                        case 't':
                            s.Append('\t');
                            break;

                        case 'u':
                        {
                            int remainingLength = _len - index;
                            if (remainingLength < 4) throw new Exception("\\u escape sequence ended prematurely (end of input)");

                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode(p[index], p[index + 1], p[index + 2], p[index + 3]);
                            s.Append((char)codePoint);

                            // skip 4 chars
                            index += 4;
                        }
                        break;

                        case 'v':
                            s.Append('\v');
                            break;

                        case '0':
                            if (index < len && p[index] is >= '1' and <= '9') throw new Exception("Invalid: octal escapes are not allowed (at index " + (index - 1) + ")");
                            s.Append('\0');
                            break;

                        case 'x':
                        {
                            int remainingLength = _len - index;
                            if (remainingLength < 2) throw new Exception("\\x escape sequence ended prematurely (end of input)");

                            // parse the 16 bit hex into an integer codepoint
                            uint codePoint = ParseUnicode16Bit(p[index], p[index + 1]);
                            s.Append((char)codePoint);

                            // skip 2 chars
                            index += 2;
                        }
                        break;

                        case 'a':
                            s.Append('\a');
                            break;

                        case >= '1' and <= '9':
                            throw new Exception("Illegal escape \\" + c + " (at index " + (index - 1) + ")");

                        default:
                            if (c == '\r')
                            {
                                c = p[index];
                                if (c == '\n')
                                {
                                    index++;
                                    c = p[index];
                                }
                            }
                            else if (c != '\n' && c != '\u2028' && c != '\u2029')
                                s.Append(c);
                            break;
                    }
                }
            }

            throw new Exception("Did not reach end of string");
        }

        private unsafe string ParseJson5String(char* p)
        {
            throw new NotImplementedException();
        }

        private uint ParseSingleChar(char c1, uint multipliyer, int additionalIndex)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            else throw new Exception($"Invalid hexadecimal character '{ c1 }' at index { index + additionalIndex }");
            return p1;
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseSingleChar(c1, 0x1000, 0);
            uint p2 = ParseSingleChar(c2, 0x100, 1);
            uint p3 = ParseSingleChar(c3, 0x10, 2);
            uint p4 = ParseSingleChar(c4, 1, 3);

            return p1 + p2 + p3 + p4;
        }

        private uint ParseUnicode16Bit(char c1, char c2)
        {
            uint p1 = ParseSingleChar(c1, 0x10, 0);
            uint p2 = ParseSingleChar(c2, 1, 1);

            return p1 + p2;
        }

        private unsafe object ParseNumber(char* p, bool skip)
        {
            ConsumeToken();

            // Need to start back one place because the first digit is also a token and would have been consumed
            int startIndex = index - 1;
            bool dec = false;
            bool dob = false;
            bool run = true;
            bool hasDigits = false;
            bool hasDigits2 = false;
            if (p[startIndex] == '.') dec = true;
            else if (p[startIndex] is >= '0' and <= '9') hasDigits = true;
            do
            {
                if (index == _len)
                    break;

                bool signedNeg = p[startIndex] == '-';
                bool signed = signedNeg || p[startIndex] == '+';

                char c = p[index];
                char c1 = p[index + 1];

                if ((!signed && (c == 'x' || c == 'X')) || (signed && (c1 == 'x' || c1 == 'X')))
                {
                    index++;
                    if (signed) index++;
                    return ReadHexNumber(p, signedNeg);
                }

                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        index++;
                        if (dob) hasDigits2 = true;
                        else hasDigits = true;
                        break;
                    case '+':
                    case '-':
                        index++;
                        break;
                    case 'e':
                    case 'E':
                        if (!hasDigits) throw new Exception($"Unexpected character '{ c }' at index { index }");
                        dob = true;
                        index++;
                        break;
                    case '.':
                        index++;
                        dec = true;
                        break;
                    //break;
                    default:
                        run = false;
                        break;
                }

                if (index == _len)
                    run = false;

            } while (run);
            if (index == _len && (!hasDigits || (!hasDigits2 && dob))) throw new Exception("Unfinished number at end of input");
            if (!hasDigits) throw new Exception($"Unexpected character '{ p[index] }' at index { index }");
            if (!hasDigits2 && dob) throw new Exception($"Unexpected character '{ p[index] }' at index { index }");

            if (skip)
                return 0;

            int force = -1;
            redo:
            if (dob || force == 0)
            {
                string s = UnsafeSubstring(p, startIndex, index - startIndex);
                double retV = double.Parse(s, NumberFormatInfo.InvariantInfo);
                if (retV == 0 && s[0] == '-') return -0.0;
                return retV;
            }
            if ((dec == false && index - startIndex < 20) || force == 1)
            {
                long retV;
                try
                {
                    retV = Helper.CreateLong(json, startIndex, index - startIndex);
                }
                catch
                {
                    force = 2;
                    goto redo;
                }
                if (retV == 0 && p[startIndex] == '-') return -0.0;
                return retV;
            }

            if ((index - startIndex < 30) || force == 2)
            {
                string s = UnsafeSubstring(p, startIndex, index - startIndex);
                decimal retV;
                try
                {
                    retV = decimal.Parse(s, NumberFormatInfo.InvariantInfo);
                }
                catch
                {
                    force = 0;
                    goto redo;
                }
                if (retV == 0 && s[0] == '-') return -0.0m;
                return retV;
            }
            force = 2;
            goto redo;
        }

        private unsafe object ReadHexNumber(char* p, bool neg)
        {
            int digitsCount = 0;
            long num = 0L;
            decimal alternateNum = 0.0m;
            double alternateNum2 = 0.0;
            bool run = true;
            while (run && index < _len)
            {
                char c = p[index];
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        digitsCount++;
                        index++;
                        if (digitsCount < 15) num = (num << 4) + (c - '0');
                        else if (digitsCount == 15) alternateNum = num * 16m + (c - '0');
                        else if (digitsCount is > 15 and < 25) alternateNum = alternateNum * 16m + (c - '0');
                        else if (digitsCount == 25) alternateNum2 = (double)alternateNum * 16d + (c - '0');
                        else alternateNum2 = alternateNum2 * 16d + (c - '0');
                        break;
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        digitsCount++;
                        index++;
                        if (digitsCount < 15) num = (num << 4) + (c - 'a') + 10;
                        else if (digitsCount == 15) alternateNum = num * 16m + (c - 'a' + 10);
                        else if (digitsCount is > 15 and < 25) alternateNum = alternateNum * 16m + (c - 'a' + 10);
                        else if (digitsCount == 25) alternateNum2 = (double)alternateNum * 16d + (c - 'a' + 10);
                        else alternateNum2 = alternateNum2 * 16d + (c - 'a' + 10);
                        break;
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        digitsCount++;
                        index++;
                        if (digitsCount < 15) num = (num << 4) + (c - 'A') + 10;
                        else if (digitsCount == 15) alternateNum = num * 16m + (c - 'A' + 10);
                        else if (digitsCount is > 15 and < 25) alternateNum = alternateNum * 16m + (c - 'A' + 10);
                        else if (digitsCount == 25) alternateNum2 = (double)alternateNum * 16d + (c - 'A' + 10);
                        else alternateNum2 = alternateNum2 * 16d + (c - 'A' + 10);
                        break;
                    default:
                        run = false;
                        break;
                }
            }
            if (index == _len && digitsCount == 0) throw new Exception("Unfinished number at end of input");
            if (digitsCount == 0) throw new Exception($"Unexpected character '{ p[index] }' at index { index }");
            if (neg)
            {
                if (digitsCount < 15 && num == 0) return -0.0d;
                num = -num;
                alternateNum = -alternateNum;
                alternateNum2 = -alternateNum2;
            }

            if (digitsCount < 15) return num;
            if (digitsCount < 25) return alternateNum;
            return alternateNum2;
        }

        private unsafe Token LookAhead(char* p)
        {
            if (lookAheadToken != Token.None) return lookAheadToken;

            return lookAheadToken = NextTokenCore(p);
        }

        private void ConsumeToken()
        {
            lookAheadToken = Token.None;
        }

        private unsafe Token NextToken(char* p)
        {
            Token result = lookAheadToken != Token.None ? lookAheadToken : NextTokenCore(p);

            lookAheadToken = Token.None;

            return result;
        }

        private unsafe void SkipWhitespace(char* p)
        {
            // Skip past whitespace
            do
            {
                char c = p[index];

                if (c == '/' && p[index + 1] == '/') // c++ style single line comments
                {
                    index++;
                    index++;
                    do
                    {
                        c = p[index];
                        if (c == '\r' || c == '\n' || c == '\u2028' || c == '\u2029') break; // read till end of line
                    }
                    while (++index < _len);
                }

                if (c == '/' && p[index + 1] == '*') // c style multi line comments
                {
                    int startIndex = index;
                    index++;
                    index++;
                    bool foundEnd = false;
                    do
                    {
                        c = p[index];
                        if (c == '*' && p[index + 1] == '/')
                        {
                            index += 2;
                            c = p[index];
                            foundEnd = true;
                            break; // read till end of comment
                        }
                    }
                    while (++index < _len);
                    if (!foundEnd) throw new Exception("Unfinished comment declaration starting at index " + startIndex);
                }

                if (c == '/') throw new Exception("Illegal comment declaration at index " + index);

                if (c != ' ' && c != '\t' && c != '\n' && c != '\r' && c != '\v' && c != '\f' && c != '\u00A0' && c != '\u2028' && c != '\u2029' && c != '\uFEFF' && CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.SpaceSeparator)
                    break;
            } while (++index < _len);
        }

        private unsafe Token NextTokenCore(char* p)
        {
            int len = _len;

            if (index == len)
            {
                throw new Exception("Reached end of string unexpectedly");
            }

            SkipWhitespace(p);

            if (index == len)
            {
                throw new Exception("Reached end of string unexpectedly");
            }

            char c = p[index];

            index++;

            switch (c)
            {
                case '{':
                    return Token.Curly_Open;

                case '}':
                    return Token.Curly_Close;

                case '[':
                    return Token.Squared_Open;

                case ']':
                    return Token.Squared_Close;

                case ',':
                    return Token.Comma;

                case '\'':
                case '"':
                    return Token.String;

                case '-':
                    if (len - index >= 8 &&
                        p[index + 0] == 'I' &&
                        p[index + 1] == 'n' &&
                        p[index + 2] == 'f' &&
                        p[index + 3] == 'i' &&
                        p[index + 4] == 'n' &&
                        p[index + 5] == 'i' &&
                        p[index + 6] == 't' &&
                        p[index + 7] == 'y')
                    {
                        index += 8;
                        return Token.NegInfinity;
                    }

                    if (len - index >= 3 &&
                        p[index + 0] == 'N' &&
                        p[index + 1] == 'a' &&
                        p[index + 2] == 'N')
                    {
                        index += 3;
                        return Token.NaN;
                    }
                    return Token.Number;
                case '+':
                    if (len - index >= 8 &&
                        p[index + 0] == 'I' &&
                        p[index + 1] == 'n' &&
                        p[index + 2] == 'f' &&
                        p[index + 3] == 'i' &&
                        p[index + 4] == 'n' &&
                        p[index + 5] == 'i' &&
                        p[index + 6] == 't' &&
                        p[index + 7] == 'y')
                    {
                        index += 8;
                        return Token.PosInfinity;
                    }

                    if (len - index >= 3 &&
                        p[index + 0] == 'N' &&
                        p[index + 1] == 'a' &&
                        p[index + 2] == 'N')
                    {
                        index += 3;
                        return Token.NaN;
                    }
                    return Token.Number;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                    return Token.Number;

                case ':':
                    return Token.Colon;
                case 'I':
                    if (len - index >= 7 &&
                        p[index + 0] == 'n' &&
                        p[index + 1] == 'f' &&
                        p[index + 2] == 'i' &&
                        p[index + 3] == 'n' &&
                        p[index + 4] == 'i' &&
                        p[index + 5] == 't' &&
                        p[index + 6] == 'y')
                    {
                        index += 7;
                        return Token.PosInfinity;
                    }
                    break;

                case 'f':
                    if (len - index >= 4 &&
                        p[index + 0] == 'a' &&
                        p[index + 1] == 'l' &&
                        p[index + 2] == 's' &&
                        p[index + 3] == 'e')
                    {
                        index += 4;
                        return Token.False;
                    }
                    break;

                case 't':
                    if (len - index >= 3 &&
                        p[index + 0] == 'r' &&
                        p[index + 1] == 'u' &&
                        p[index + 2] == 'e')
                    {
                        index += 3;
                        return Token.True;
                    }
                    break;

                case 'n':
                    if (len - index >= 3 &&
                        p[index + 0] == 'u' &&
                        p[index + 1] == 'l' &&
                        p[index + 2] == 'l')
                    {
                        index += 3;
                        return Token.Null;
                    }
                    break;

                case 'N':
                    if (len - index >= 2 &&
                    p[index] == 'a' &&
                    (p[index + 1] == 'N'))
                    {
                        index += 2;
                        return Token.NaN;
                    }
                    break;
            }

            if (allownonquotedkey)//&& tok == Token.String)
            {
                index--;
                return Token.String;
            }

            //return tok;

            throw new Exception("Could not find token at index " + --index + " got '" + p[index] + "'");
        }

        private static unsafe string UnsafeSubstring(char* p, int startIndex, int length)
        {
            return new string(p, startIndex, length);
        }
    }
}
