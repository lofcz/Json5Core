using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Json5Core
{
    public static class Helper
    {
		public static bool IsNullable(Type t)
        {
			if (!t.IsGenericType()) return false;
			Type g = t.GetGenericTypeDefinition();
            return (g == typeof(Nullable<>));
        }

        public static Type UnderlyingTypeOf(Type t)
        {
            return Reflection.Instance.GetGenericArguments(t)[0];
        }

        public static DateTimeOffset CreateDateTimeOffset(int year, int month, int day, int hour, int min, int sec, int milli, int extraTicks, TimeSpan offset)
        {
            DateTimeOffset dt = new DateTimeOffset(year, month, day, hour, min, sec, milli, offset);

            if (extraTicks > 0)
                dt += TimeSpan.FromTicks(extraTicks);

            return dt;
        }

        public static bool BoolConv(object v)
        {
            bool oset = false;
            switch (v)
            {
                case bool b:
                    oset = b;
                    break;
                case long l:
                    oset = l > 0;
                    break;
                case string s:
                {
                    s = s.ToLowerInvariant();
                    if (s == "1" || s == "true" || s == "yes" || s == "on")
                        oset = true;
                    break;
                }
            }

            return oset;
        }

        public static long AutoConv(object value, JsonParameters param)
        {
            return value switch
            {
                string s when param.AutoConvertStringToNumbers => CreateLong(s, 0, s.Length),
                string _ => throw new Exception("AutoConvertStringToNumbers is disabled for converting string : " + value),
                long l => l,
                _ => Convert.ToInt64(value)
            };
        }

        public static unsafe long CreateLong(string s, int index, int count)
        {
            long num = 0;
            int neg = 1;
            fixed (char* v = s)
            {
                char* str = v;
                str += index;
                if (*str == '-')
                {
                    neg = -1;
                    str++;
                    count--;
                }
                if (*str == '+')
                {
                    str++;
                    count--;
                }
                while (count > 0)
                {
                    num = num * 10
                       //(num << 4) - (num << 2) - (num << 1) 
                       + (*str - '0');
                    str++;
                    count--;
                }
            }
            return num * neg;
        }

        public static unsafe long CreateLong(char[] s, int index, int count)
        {
            long num = 0;
            int neg = 1;
            fixed (char* v = s)
            {
                char* str = v;
                str += index;
                if (*str == '-')
                {
                    neg = -1;
                    str++;
                    count--;
                }
                if (*str == '+')
                {
                    str++;
                    count--;
                }
                while (count > 0)
                {
                    num = num * 10
                        //(num << 4) - (num << 2) - (num << 1) 
                        + (*str - '0');
                    str++;
                    count--;
                }
            }
            return num * neg;
        }

        public static unsafe int CreateInteger(string s, int index, int count)
        {
            int num = 0;
            int neg = 1;
            fixed (char* v = s)
            {
                char* str = v;
                str += index;
                if (*str == '-')
                {
                    neg = -1;
                    str++;
                    count--;
                }
                if (*str == '+')
                {
                    str++;
                    count--;
                }
                while (count > 0)
                {
                    num = num * 10
                        //(num << 4) - (num << 2) - (num << 1) 
                        + (*str - '0');
                    str++;
                    count--;
                }
            }
            return num * neg;
        }

        public static object CreateEnum(Type pt, object v)
        {
            // FEATURE : optimize create enum
            return Enum.Parse(pt, v.ToString(), true);
        }

        public static Guid CreateGuid(string s)
        {
            return s.Length > 30 ? new Guid(s) : new Guid(Convert.FromBase64String(s));
        }

        public static StringDictionary CreateSD(Dictionary<string, object> d)
        {
            StringDictionary nv = new StringDictionary();

            foreach (KeyValuePair<string, object> o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        public static NameValueCollection CreateNV(Dictionary<string, object> d)
        {
            NameValueCollection nv = new NameValueCollection();

            foreach (KeyValuePair<string, object> o in d)
                nv.Add(o.Key, (string)o.Value);

            return nv;
        }

        public static object CreateDateTimeOffset(string value)
        {
            //                   0123456789012345678 9012 9/3 0/4  1/5
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  _   +   00:00

            // ISO8601 roundtrip formats have 7 digits for ticks, and no space before the '+'
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  +   00:00  
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnnnnnn  Z  

            int ms = 0;
            int usTicks = 0; // ticks for xxx.x microseconds

            int year = CreateInteger(value, 0, 4);
            int month = CreateInteger(value, 5, 2);
            int day = CreateInteger(value, 8, 2);
            int hour = CreateInteger(value, 11, 2);
            int min = CreateInteger(value, 14, 2);
            int sec = CreateInteger(value, 17, 2);

            int p = 20;

            if (value.Length > 21 && value[19] == '.')
            {
                ms = CreateInteger(value, p, 3);
                p = 23;

                // handle 7 digit case
                if (value.Length > 25 && char.IsDigit(value[p]))
                {
                    usTicks = CreateInteger(value, p, 4);
                    p = 27;
                }
            }

            switch (value[p])
            {
                // UTC
                case 'Z':
                    return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, TimeSpan.Zero);
                case ' ':
                    ++p;
                    break;
            }

            // +00:00
            int th = CreateInteger(value, p + 1, 2);
            int tm = CreateInteger(value, p + 1 + 2 + 1, 2);

            if (value[p] == '-')
                th = -th;

            return CreateDateTimeOffset(year, month, day, hour, min, sec, ms, usTicks, new TimeSpan(th, tm, 0));
        }

        public static DateTime CreateDateTime(string value, bool UseUTCDateTime)
        {
            if (value.Length < 19)
                return DateTime.MinValue;

            bool utc = false;
            //                   0123456789012345678 9012 9/3
            // datetime format = yyyy-MM-ddTHH:mm:ss .nnn  Z
            int ms = 0;

            int year = CreateInteger(value, 0, 4);
            int month = CreateInteger(value, 5, 2);
            int day = CreateInteger(value, 8, 2);
            int hour = CreateInteger(value, 11, 2);
            int min = CreateInteger(value, 14, 2);
            int sec = CreateInteger(value, 17, 2);
            if (value.Length > 21 && value[19] == '.')
                ms = CreateInteger(value, 20, 3);

            if (value[^1] == 'Z')
                utc = true;

            switch (UseUTCDateTime)
            {
                case false when utc == false:
                    return new DateTime(year, month, day, hour, min, sec, ms);
                case true when utc:
                    return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc);
                default:
                    return new DateTime(year, month, day, hour, min, sec, ms, DateTimeKind.Utc).ToLocalTime();
            }
        }
    }
}
