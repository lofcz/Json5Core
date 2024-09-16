using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using System.Collections;
using System.Threading;
using Json5Core;
using System.Collections.Specialized;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Linq;
using System.Dynamic;
using System.Collections.ObjectModel;

//namespace UnitTests
//{
public class tests
{
#region [  helpers  ]
    static int thousandtimes = 1000;
    static int fivetimes = 5;

    //static bool exotic = false;
    //static bool dsser = false;

    public enum Gender
    {
        Male,
        Female
    }

    public class colclass
    {
        public colclass()
        {
            items = new List<baseclass>();
            date = DateTime.Now;
            multilineString = @"
            AJKLjaskljLA
       ahjksjkAHJKS سلام فارسی
       AJKHSKJhaksjhAHSJKa
       AJKSHajkhsjkHKSJKash
       ASJKhasjkKASJKahsjk
            ";
            isNew = true;
            booleanValue = true;
            ordinaryDouble = 0.001;
            gender = Gender.Female;
            intarray = new int[5] { 1, 2, 3, 4, 5 };
        }
        public bool booleanValue { get; set; }
        public DateTime date { get; set; }
        public string multilineString { get; set; }
        public List<baseclass> items { get; set; }
        public decimal ordinaryDecimal { get; set; }
        public double ordinaryDouble { get; set; }
        public bool isNew { get; set; }
        public string laststring { get; set; }
        public Gender gender { get; set; }
		public DataSet dataset { get; set; }
        public Hashtable hash { get; set; } 
		public Dictionary<string, baseclass> stringDictionary { get; set; }
        public Dictionary<baseclass, baseclass> objectDictionary { get; set; }
        public Dictionary<int, baseclass> intDictionary { get; set; }
        public Guid? nullableGuid { get; set; }
        public decimal? nullableDecimal { get; set; }
        public double? nullableDouble { get; set; }
        public baseclass[] arrayType { get; set; }
        public byte[] bytes { get; set; }
        public int[] intarray { get; set; }
    }

    public static colclass CreateObject(bool exotic, bool dataset)
    {
        colclass c = new colclass
        {
            booleanValue = true,
            ordinaryDecimal = 3
        };

        if (exotic)
        {
            c.nullableGuid = Guid.NewGuid();
#if !SILVERLIGHT
            c.hash = new Hashtable();
            c.hash.Add(new class1("0", "hello", Guid.NewGuid()), new class2("1", "code", "desc"));
            c.hash.Add(new class2("0", "hello", "pppp"), new class1("1", "code", Guid.NewGuid()));
#endif
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
			if (dataset)
                c.dataset = CreateDataset();
#endif
			c.bytes = new byte[1024];
            c.stringDictionary = new Dictionary<string, baseclass>();
            c.objectDictionary = new Dictionary<baseclass, baseclass>();
            c.intDictionary = new Dictionary<int, baseclass>();
            c.nullableDouble = 100.003;


            c.nullableDecimal = 3.14M;



            c.stringDictionary.Add("name1", new class2("1", "code", "desc"));
            c.stringDictionary.Add("name2", new class1("1", "code", Guid.NewGuid()));

            c.intDictionary.Add(1, new class2("1", "code", "desc"));
            c.intDictionary.Add(2, new class1("1", "code", Guid.NewGuid()));

            c.objectDictionary.Add(new class1("0", "hello", Guid.NewGuid()), new class2("1", "code", "desc"));
            c.objectDictionary.Add(new class2("0", "hello", "pppp"), new class1("1", "code", Guid.NewGuid()));

            c.arrayType = new baseclass[2];
            c.arrayType[0] = new class1();
            c.arrayType[1] = new class2();
        }


        c.items.Add(new class1("1", "1", Guid.NewGuid()));
        c.items.Add(new class2("2", "2", "desc1"));
        c.items.Add(new class1("3", "3", Guid.NewGuid()));
        c.items.Add(new class2("4", "4", "desc2"));

        c.laststring = "" + DateTime.Now;

        return c;
    }

    public class baseclass
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class class1 : baseclass
    {
        public class1() { }
        public class1(string name, string code, Guid g)
        {
            Name = name;
            Code = code;
            guid = g;
        }
        public Guid guid { get; set; }
    }

    public class class2 : baseclass
    {
        public class2() { }
        public class2(string name, string code, string desc)
        {
            Name = name;
            Code = code;
            description = desc;
        }
        public string description { get; set; }
    }

    public class NoExt
    {
        [System.Xml.Serialization.XmlIgnore()]
        public string Name { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public baseclass[] objs { get; set; }
        public Dictionary<string, class1> dic { get; set; }
        public NoExt intern { get; set; }
    }

    public class Retclass
    {
        public object ReturnEntity { get; set; }
        public string Name { get; set; }
        public string Field1;
        public int Field2;
        public object obj;
        public string ppp { get { return "sdfas df "; } }
        public DateTime date { get; set; }
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
		public DataTable ds { get; set; }
#endif
    }

    public struct Retstruct
    {
        public object ReturnEntity { get; set; }
        public string Name { get; set; }
        public string Field1;
        public int Field2;
        public string ppp { get { return "sdfas df "; } }
        public DateTime date { get; set; }
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
		public DataTable ds { get; set; }
#endif
    }

    private static long CreateLong(string s)
    {
        long num = 0;
        bool neg = false;
        foreach (char cc in s)
        {
            if (cc == '-')
                neg = true;
            else if (cc == '+')
                neg = false;
            else
            {
                num *= 10;
                num += (int)(cc - '0');
            }
        }

        return neg ? -num : num;
    }

#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
	private static DataSet CreateDataset()
    {
        DataSet ds = new DataSet();
        for (int j = 1; j < 3; j++)
        {
            DataTable dt = new DataTable();
            dt.TableName = "Table" + j;
            dt.Columns.Add("col1", typeof(int));
            dt.Columns.Add("col2", typeof(string));
            dt.Columns.Add("col3", typeof(Guid));
            dt.Columns.Add("col4", typeof(string));
            dt.Columns.Add("col5", typeof(bool));
            dt.Columns.Add("col6", typeof(string));
            dt.Columns.Add("col7", typeof(string));
            ds.Tables.Add(dt);
            Random rrr = new Random();
            for (int i = 0; i < 100; i++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = rrr.Next(int.MaxValue);
                dr[1] = "" + rrr.Next(int.MaxValue);
                dr[2] = Guid.NewGuid();
                dr[3] = "" + rrr.Next(int.MaxValue);
                dr[4] = true;
                dr[5] = "" + rrr.Next(int.MaxValue);
                dr[6] = "" + rrr.Next(int.MaxValue);

                dt.Rows.Add(dr);
            }
        }
        return ds;
    }
#endif

    public class RetNestedclass
    {
        public Retclass Nested { get; set; }
    }

#endregion

#if !CORE_TEST
    [TestFixtureSetUp]
#else
    [OneTimeSetUp]
#endif
    public static void setup()
    {
        Json5Core.JSON.Parameters = new JSONParameters() { UseExtensions = true };
        JSON.Parameters.FixValues();
    }

    [Test]
    public static void objectarray()
    {
        object[] o = new object[] { 1, "sdaffs", DateTime.Now };
        string s = JSON.ToJSON(o);
        object p = JSON.ToObject(s);
    }

    [Test]
    public static void ClassTest()
    {
        Retclass r = new Retclass();
        r.Name = "hello";
        r.Field1 = "dsasdF";
        r.Field2 = 2312;
        r.date = DateTime.Now;
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
		r.ds = CreateDataset().Tables[0];
#endif

        string s = JSON.ToJSON(r);
        Console.WriteLine(JSON.Beautify(s));
        object o = JSON.ToObject(s);
        Console.WriteLine((o as Retclass).Field2);
        Assert.AreEqual(2312, (o as Retclass).Field2);
    }


    [Test]
    public static void StructTest()
    {
        Retstruct r = new Retstruct();
        r.Name = "hello";
        r.Field1 = "dsasdF";
        r.Field2 = 2312;
        r.date = DateTime.Now;
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
		r.ds = CreateDataset().Tables[0];
#endif

        string s = JSON.ToNiceJSON(r);
        Console.WriteLine(s);
        object o = JSON.ToObject(s);
        Assert.NotNull(o);
        Assert.AreEqual(2312, ((Retstruct)o).Field2);
    }

    [Test]
    public static void ParseTest()
    {
        Retclass r = new Retclass();
        r.Name = "hello";
        r.Field1 = "dsasdF";
        r.Field2 = 2312;
        r.date = DateTime.Now;
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)
		r.ds = CreateDataset().Tables[0];
#endif

        string s = JSON.ToJSON(r);
        Console.WriteLine(s);
        object o = JSON.Parse(s);

        Assert.IsNotNull(o);
    }

    [Test]
    public static void StringListTest()
    {
        List<string> ls = new List<string>();
        ls.AddRange(new string[] { "a", "b", "c", "d" });

        string s = JSON.ToJSON(ls);
        Console.WriteLine(s);
        object o = JSON.ToObject(s);

        Assert.IsNotNull(o);
    }

    [Test]
    public static void IntListTest()
    {
        List<int> ls = new List<int>();
        ls.AddRange(new int[] { 1, 2, 3, 4, 5, 10 });

        string s = JSON.ToJSON(ls);
        Console.WriteLine(s);
        object p = JSON.Parse(s);
        object o = JSON.ToObject(s); // long[] {1,2,3,4,5,10}

        Assert.IsNotNull(o);
    }

    [Test]
    public static void List_int()
    {
        List<int> ls = new List<int>();
        ls.AddRange(new int[] { 1, 2, 3, 4, 5, 10 });

        string s = JSON.ToJSON(ls);
        Console.WriteLine(s);
        object p = JSON.Parse(s);
        List<int> o = JSON.ToObject<List<int>>(s);

        Assert.IsNotNull(o);
    }

    [Test]
    public static void Variables()
    {
        string s = JSON.ToJSON(42);
        object o = JSON.ToObject(s);
        Assert.AreEqual(o, 42);

        s = JSON.ToJSON("hello");
        o = JSON.ToObject(s);
        Assert.AreEqual(o, "hello");

        s = JSON.ToJSON(42.42M);
        o = JSON.ToObject(s);
        Assert.AreEqual(42.42M, o);
    }

    [Test]
    public static void Dictionary_String_RetClass()
    {
        Dictionary<string, Retclass> r = new Dictionary<string, Retclass>();
        r.Add("11", new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add("12", new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        string s = JSON.ToJSON(r);
        Console.WriteLine(JSON.Beautify(s));
        Dictionary<string, Retclass> o = JSON.ToObject<Dictionary<string, Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void Dictionary_String_RetClass_noextensions()
    {
        Dictionary<string, Retclass> r = new Dictionary<string, Retclass>();
        r.Add("11", new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add("12", new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        string s = JSON.ToJSON(r, new Json5Core.JSONParameters { UseExtensions = false });
        Console.WriteLine(JSON.Beautify(s));
        Dictionary<string, Retclass> o = JSON.ToObject<Dictionary<string, Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void Dictionary_int_RetClass()
    {
        Dictionary<int, Retclass> r = new Dictionary<int, Retclass>();
        r.Add(11, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add(12, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        string s = JSON.ToJSON(r);
        Console.WriteLine(JSON.Beautify(s));
        Dictionary<int, Retclass> o = JSON.ToObject<Dictionary<int, Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void Dictionary_int_RetClass_noextensions()
    {
        Dictionary<int, Retclass> r = new Dictionary<int, Retclass>();
        r.Add(11, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add(12, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        string s = JSON.ToJSON(r, new Json5Core.JSONParameters { UseExtensions = false });
        Console.WriteLine(JSON.Beautify(s));
        Dictionary<int, Retclass> o = JSON.ToObject<Dictionary<int, Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void Dictionary_Retstruct_RetClass()
    {
        Dictionary<Retstruct, Retclass> r = new Dictionary<Retstruct, Retclass>();
        r.Add(new Retstruct { Field1 = "111", Field2 = 1, date = DateTime.Now }, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add(new Retstruct { Field1 = "222", Field2 = 2, date = DateTime.Now }, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        string s = JSON.ToJSON(r);
        Console.WriteLine(JSON.Beautify(s));
        Dictionary<Retstruct, Retclass> o = JSON.ToObject<Dictionary<Retstruct, Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void Dictionary_Retstruct_RetClass_noextentions()
    {
        Dictionary<Retstruct, Retclass> r = new Dictionary<Retstruct, Retclass>();
        r.Add(new Retstruct { Field1 = "111", Field2 = 1, date = DateTime.Now }, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add(new Retstruct { Field1 = "222", Field2 = 2, date = DateTime.Now }, new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        string s = JSON.ToJSON(r, new Json5Core.JSONParameters { UseExtensions = false });
        Console.WriteLine(JSON.Beautify(s));
        Dictionary<Retstruct, Retclass> o = JSON.ToObject<Dictionary<Retstruct, Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void List_RetClass()
    {
        List<Retclass> r = new List<Retclass>();
        r.Add(new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add(new Retclass { Field1 = "222", Field2 = 3, date = DateTime.Now });
        string s = JSON.ToJSON(r);
        Console.WriteLine(JSON.Beautify(s));
        List<Retclass> o = JSON.ToObject<List<Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void List_RetClass_noextensions()
    {
        List<Retclass> r = new List<Retclass>();
        r.Add(new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now });
        r.Add(new Retclass { Field1 = "222", Field2 = 3, date = DateTime.Now });
        string s = JSON.ToJSON(r, new Json5Core.JSONParameters { UseExtensions = false });
        Console.WriteLine(JSON.Beautify(s));
        List<Retclass> o = JSON.ToObject<List<Retclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void Perftest()
    {
        string s = "123456";

        DateTime dt = DateTime.Now;
        int c = 1000000;

        for (int i = 0; i < c; i++)
        {
            long o = CreateLong(s);
        }

        Console.WriteLine("convertlong (ms): " + DateTime.Now.Subtract(dt).TotalMilliseconds);

        dt = DateTime.Now;

        for (int i = 0; i < c; i++)
        {
            long o = long.Parse(s);
        }

        Console.WriteLine("long.parse (ms): " + DateTime.Now.Subtract(dt).TotalMilliseconds);

        dt = DateTime.Now;

        for (int i = 0; i < c; i++)
        {
            long o = Convert.ToInt64(s);
        }

        Console.WriteLine("convert.toint64 (ms): " + DateTime.Now.Subtract(dt).TotalMilliseconds);
    }

    [Test]
    public static void FillObject()
    {
        NoExt ne = new NoExt();
        ne.Name = "hello";
        ne.Address = "here";
        ne.Age = 10;
        ne.dic = new Dictionary<string, class1>();
        ne.dic.Add("hello", new class1("asda", "asdas", Guid.NewGuid()));
        ne.objs = new baseclass[] { new class1("a", "1", Guid.NewGuid()), new class2("b", "2", "desc") };

        string str = JSON.ToJSON(ne, new Json5Core.JSONParameters { UseExtensions = false, UsingGlobalTypes = false });
        string strr = JSON.Beautify(str);
        Console.WriteLine(strr);
        object dic = JSON.Parse(str);
        object oo = JSON.ToObject<NoExt>(str);

        NoExt nee = new NoExt();
        nee.intern = new NoExt { Name = "aaa" };
        JSON.FillObject(nee, strr);
    }

    [Test]
    public static void AnonymousTypes()
    {
        var q = new { Name = "asassa", Address = "asadasd", Age = 12 };
        string sq = JSON.ToJSON(q, new JSONParameters { EnableAnonymousTypes = true, UseExtensions = true });
        Console.WriteLine(sq);
        Assert.AreEqual("{\"Name\":\"asassa\",\"Address\":\"asadasd\",\"Age\":12}", sq);
    }

    [Test]
    public static void Speed_Test_Deserialize()
    {
        Console.Write("Json5Core deserialize");
        JSON.Parameters = new JSONParameters() { UseExtensions = true };
        colclass c = CreateObject(false, false);
        double t = 0;
        Stopwatch stopwatch = new Stopwatch();
        for (int pp = 0; pp < fivetimes; pp++)
        {
            stopwatch.Restart();
            colclass deserializedStore;
            string jsonText = JSON.ToJSON(c);
            //Console.WriteLine(" size = " + jsonText.Length);
            for (int i = 0; i < thousandtimes; i++)
            {
                deserializedStore = (colclass)JSON.ToObject(jsonText);
            }
            stopwatch.Stop();
            t += stopwatch.ElapsedMilliseconds;
            Console.Write("\t" + stopwatch.ElapsedMilliseconds);
        }
        Console.WriteLine("\tAVG = " + t / fivetimes);
    }

    [Test]
    public static void Speed_Test_Serialize()
    {
        Console.Write("Json5Core serialize");
        JSON.Parameters = new JSONParameters() { UseExtensions = true };
        //Json5Core.JSON.Parameters.UsingGlobalTypes = false;
        colclass c = CreateObject(false, false);
        double t = 0;
        Stopwatch stopwatch = new Stopwatch();
        for (int pp = 0; pp < fivetimes; pp++)
        {
            stopwatch.Restart();
            string jsonText = null;
            for (int i = 0; i < thousandtimes; i++)
            {
                jsonText = JSON.ToJSON(c);
            }
            stopwatch.Stop();
            t += stopwatch.ElapsedMilliseconds;
            Console.Write("\t" + stopwatch.ElapsedMilliseconds);
        }
        Console.WriteLine("\tAVG = " + t / fivetimes);
    }

    [Test]
    public static void List_NestedRetClass()
    {
        List<RetNestedclass> r = new List<RetNestedclass>();
        r.Add(new RetNestedclass { Nested = new Retclass { Field1 = "111", Field2 = 2, date = DateTime.Now } });
        r.Add(new RetNestedclass { Nested = new Retclass { Field1 = "222", Field2 = 3, date = DateTime.Now } });
        string s = JSON.ToJSON(r);
        Console.WriteLine(JSON.Beautify(s));
        List<RetNestedclass> o = JSON.ToObject<List<RetNestedclass>>(s);
        Assert.AreEqual(2, o.Count);
    }

    [Test]
    public static void NullTest()
    {
        string s = JSON.ToJSON(null);
        Assert.AreEqual("null", s);
        object o = JSON.ToObject(s);
        Assert.AreEqual(null, o);
        o = JSON.ToObject<class1>(s);
        Assert.AreEqual(null, o);
    }

    [Test]
    public static void DisableExtensions()
    {
        JSONParameters p = new Json5Core.JSONParameters { UseExtensions = false, SerializeNullValues = false };
        string s = JSON.ToJSON(new Retclass { date = DateTime.Now, Name = "aaaaaaa" }, p);
        Console.WriteLine(JSON.Beautify(s));
        Retclass o = JSON.ToObject<Retclass>(s);
        Assert.AreEqual("aaaaaaa", o.Name);
    }

    [Test]
    public static void ZeroArray()
    {
        string s = JSON.ToJSON(new object[] { });
        object o = JSON.ToObject(s);
        object[] a = o as object[];
        Assert.AreEqual(0, a.Length);
    }

    [Test]
    public static void BigNumber()
    {
        double d = 4.16366160299608e18;
        string s = JSON.ToJSON(d);
        double o = JSON.ToObject<double>(s);
        Assert.AreEqual(d, o);

        object dd = JSON.ToObject("100000000000000000000000000000000000000000");
        Assert.AreEqual(1e41, dd);
    }

    [Test]
    public static void GermanNumbers()
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de");
        decimal d = 3.141592654M;
        string s = JSON.ToJSON(d);
        decimal o = JSON.ToObject<decimal>(s);
        Assert.AreEqual(d, o);

        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en");
    }

    private static void GenerateJsonForAandB(out string jsonA, out string jsonB)
    {
        Console.WriteLine("Begin constructing the original objects. Please ignore trace information until I'm done.");

        // set all parameters to false to produce pure JSON
        JSON.Parameters = new JSONParameters { EnableAnonymousTypes = false, SerializeNullValues = false, UseExtensions = false, UseFastGuid = false, UseOptimizedDatasetSchema = false, UseUTCDateTime = false, UsingGlobalTypes = false };

        ConcurrentClassA a = new ConcurrentClassA { PayloadA = new PayloadA() };
        ConcurrentClassB b = new ConcurrentClassB { PayloadB = new PayloadB() };

        // A is serialized with extensions and global types
        jsonA = JSON.ToJSON(a, new JSONParameters { EnableAnonymousTypes = false, SerializeNullValues = false, UseExtensions = true, UseFastGuid = false, UseOptimizedDatasetSchema = false, UseUTCDateTime = false, UsingGlobalTypes = true });
        // B is serialized using the above defaults
        jsonB = JSON.ToJSON(b);

        Console.WriteLine("Ok, I'm done constructing the objects. Below is the generated json. Trace messages that follow below are the result of deserialization and critical for understanding the timing.");
        Console.WriteLine(jsonA);
        Console.WriteLine(jsonB);
    }

    [Test]
    public void UsingGlobalsBug_singlethread()
    {
        JSONParameters p = JSON.Parameters;
        string jsonA;
        string jsonB;
        GenerateJsonForAandB(out jsonA, out jsonB);

        object ax = JSON.ToObject(jsonA); // A has type information in JSON-extended
        ConcurrentClassB bx = JSON.ToObject<ConcurrentClassB>(jsonB); // B needs external type info

        Assert.IsNotNull(ax);
        Assert.IsInstanceOf<ConcurrentClassA>(ax);
        Assert.IsNotNull(bx);
        Assert.IsInstanceOf<ConcurrentClassB>(bx);
        JSON.Parameters = p;
    }

    [Test]
    public static void NullOutput()
    {
        ConcurrentClassA c = new ConcurrentClassA();
        string s = JSON.ToJSON(c, new JSONParameters { UseExtensions = false, SerializeNullValues = false });
        Console.WriteLine(JSON.Beautify(s));
        Assert.AreEqual(false, s.Contains(",")); // should not have a comma


    }

    [Test]
    public void UsingGlobalsBug_multithread()
    {
        JSONParameters p = JSON.Parameters;
        string jsonA;
        string jsonB;
        GenerateJsonForAandB(out jsonA, out jsonB);

        object ax = null;
        object bx = null;

        /*
         * Intended timing to force CannotGetType bug in 2.0.5:
         * the outer class ConcurrentClassA is deserialized first from json with extensions+global types. It reads the global types and sets _usingglobals to true.
         * The constructor contains a sleep to force parallel deserialization of ConcurrentClassB while in A's constructor.
         * The deserialization of B sets _usingglobals back to false.
         * After B is done, A continues to deserialize its PayloadA. It finds type "2" but since _usingglobals is false now, it fails with "Cannot get type".
         */

        Exception exception = null;

        Thread thread = new Thread(() =>
                                {
                                    try
                                    {
                                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " A begins deserialization");
                                        ax = JSON.ToObject(jsonA); // A has type information in JSON-extended
                                        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " A is done");
                                    }
                                    catch (Exception ex)
                                    {
                                        exception = ex;
                                    }
                                });

        thread.Start();

        Thread.Sleep(500); // wait to allow A to begin deserialization first

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " B begins deserialization");
        bx = JSON.ToObject<ConcurrentClassB>(jsonB); // B needs external type info
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " B is done");

        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " waiting for A to continue");
        thread.Join(); // wait for completion of A due to Sleep in A's constructor
        Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " threads joined.");

        Assert.IsNull(exception, exception == null ? "" : exception.Message + " " + exception.StackTrace);

        Assert.IsNotNull(ax);
        Assert.IsInstanceOf<ConcurrentClassA>(ax);
        Assert.IsNotNull(bx);
        Assert.IsInstanceOf<ConcurrentClassB>(bx);
        JSON.Parameters = p;
    }



    public class ConcurrentClassA
    {
        public ConcurrentClassA()
        {
            Console.WriteLine("ctor ConcurrentClassA. I will sleep for 2 seconds.");
            Thread.Sleep(2000);
            Thread.MemoryBarrier(); // just to be sure the caches on multi-core processors do not hide the bug. For me, the bug is present without the memory barrier, too.
            Console.WriteLine("ctor ConcurrentClassA. I am done sleeping.");
        }

        public PayloadA PayloadA { get; set; }
    }

    public class ConcurrentClassB
    {
        public ConcurrentClassB()
        {
            Console.WriteLine("ctor ConcurrentClassB.");
        }

        public PayloadB PayloadB { get; set; }
    }

    public class PayloadA
    {
        public PayloadA()
        {
            Console.WriteLine("ctor PayLoadA.");
        }
    }

    public class PayloadB
    {
        public PayloadB()
        {
            Console.WriteLine("ctor PayLoadB.");
        }
    }

    public class commaclass
    {
        public string Name = "aaa";
    }

    public class arrayclass
    {
        public int[] ints { get; set; }
        public string[] strs;
    }
    [Test]
    public static void ArrayTest()
    {
        arrayclass a = new arrayclass();
        a.ints = new int[] { 3, 1, 4 };
        a.strs = new string[] { "a", "b", "c" };
        string s = JSON.ToJSON(a);
        object o = JSON.ToObject(s);
    }


#if !SILVERLIGHT
    [Test]
    public static void SingleCharNumber()
    {
        sbyte zero = 0;
        string s = JSON.ToJSON(zero);
        object o = JSON.ToObject(s);
        Assert.That(zero, Is.EqualTo(o));
    }

#endif
#if !SILVERLIGHT && (NETFRAMEWORK || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NET4)

    [Test]
    public static void Datasets()
    {
        DataSet ds = CreateDataset();

        string s = JSON.ToNiceJSON(ds);
        //Console.WriteLine(s);

        DataSet o = JSON.ToObject<DataSet>(s);
        Assert.IsNotNull(o);
        Assert.AreEqual(typeof(DataSet), o.GetType());
        Assert.AreEqual(2, o.Tables.Count);

        object p = JSON.ToObject(s, typeof(DataSet));
        Assert.IsNotNull(p);
        Assert.AreEqual(typeof(DataSet), p.GetType());
        Assert.AreEqual(2, (p as DataSet).Tables.Count);


        s = JSON.ToNiceJSON(ds.Tables[0]);
        Console.WriteLine(s);

        DataTable oo = JSON.ToObject<DataTable>(s);
        Assert.IsNotNull(oo);
        Assert.AreEqual(typeof(DataTable), oo.GetType());
        Assert.AreEqual(100, oo.Rows.Count);
    }

#endif
#if !SILVERLIGHT

	[Test]
    public static void DynamicTest()
    {
        string s = "{\"Name\":\"aaaaaa\",\"Age\":10,\"dob\":\"2000-01-01 00:00:00Z\",\"inner\":{\"prop\":30},\"arr\":[1,{\"a\":2},3,4,5,6]}";
        dynamic d = JSON.ToDynamic(s);
        dynamic ss = d.Name;
        dynamic oo = d.Age;
        dynamic dob = d.dob;
        dynamic inp = d.inner.prop;
        dynamic i = d.arr[1].a;

        Assert.AreEqual("aaaaaa", ss);
        Assert.AreEqual(10, oo);
        Assert.AreEqual(30, inp);
        Assert.AreEqual("2000-01-01 00:00:00Z", dob);

        s = "{\"ints\":[1,2,3,4,5]}";

        d = JSON.ToDynamic(s);
        dynamic o = d.ints[0];
        Assert.AreEqual(1, o);

        s = "[1,2,3,4,5,{\"key\":90}]";
        d = JSON.ToDynamic(s);
        o = d[2];
        Assert.AreEqual(3, o);
        dynamic p = d[5].key;
        Assert.AreEqual(90, p);
    }

    [Test]
    public static void GetDynamicMemberNamesTests()
    {
        string s = "{\"Name\":\"aaaaaa\",\"Age\":10,\"dob\":\"2000-01-01 00:00:00Z\",\"inner\":{\"prop\":30},\"arr\":[1,{\"a\":2},3,4,5,6]}";
        dynamic d = Json5Core.JSON.ToDynamic(s);
        Assert.AreEqual(5, d.GetDynamicMemberNames().Count);
        Assert.AreEqual(6, d.arr.Count);
        Assert.AreEqual("aaaaaa", d["Name"]);
    }
#endif

	[Test]
    public static void CommaTests()
    {
        string s = JSON.ToJSON(new commaclass(), new JSONParameters() { UseExtensions = true });
        Console.WriteLine(JSON.Beautify(s));
        Assert.AreEqual(true, s.Contains("\"$type\":\"1\","));

        var objTest = new
        {
            A = "foo",
            B = (object)null,
            C = (object)null,
            D = "bar",
            E = 12,
            F = (object)null
        };

        JSONParameters p = new JSONParameters
        {
            EnableAnonymousTypes = true,
            SerializeNullValues = false,
            UseExtensions = false,
            UseFastGuid = true,
            UseOptimizedDatasetSchema = true,
            UseUTCDateTime = false,
            UsingGlobalTypes = false,
            UseEscapedUnicode = false
        };

        string json = JSON.ToJSON(objTest, p);
        Console.WriteLine(JSON.Beautify(json));
        Assert.AreEqual("{\"A\":\"foo\",\"D\":\"bar\",\"E\":12}", json);

        var o2 = new { A = "foo", B = "bar", C = (object)null };
        json = JSON.ToJSON(o2, p);
        Console.WriteLine(JSON.Beautify(json));
        Assert.AreEqual("{\"A\":\"foo\",\"B\":\"bar\"}", json);

        var o3 = new { A = (object)null };
        json = JSON.ToJSON(o3, p);
        Console.WriteLine(JSON.Beautify(json));
        Assert.AreEqual("{}", json);

        var o4 = new { A = (object)null, B = "foo" };
        json = JSON.ToJSON(o4, p);
        Console.WriteLine(JSON.Beautify(json));
        Assert.AreEqual("{\"B\":\"foo\"}", json);
    }

    [Test]
    public static void embedded_list()
    {
        string s = JSON.ToJSON(new { list = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, } });//.Where(i => i % 2 == 0) });
    }

    [Test]
    public static void Formatter()
    {
        string s = "[{\"foo\":\"'[0]\\\"{}\\u1234\\r\\n\",\"bar\":12222,\"coo\":\"some' string\",\"dir\":\"C:\\\\folder\\\\\"}]";
        string o = JSON.Beautify(s);
        Console.WriteLine(o);
        string x = @"[
   {
      ""foo"" : ""'[0]\""{}\u1234\r\n"",
      ""bar"" : 12222,
      ""coo"" : ""some' string"",
      ""dir"" : ""C:\\folder\\""
   }
]";
        Assert.AreEqual(x, o);
    }

    [Test]
    public static void EmptyArray()
    {
        string str = "[]";
        List<class1> o = JSON.ToObject<List<class1>>(str);
        Assert.AreEqual(typeof(List<class1>), o.GetType());
        class1[] d = JSON.ToObject<class1[]>(str);
        Assert.AreEqual(typeof(class1[]), d.GetType());
    }

    public class diclist
    {
        public Dictionary<string, List<string>> d;
    }

    [Test]
    public static void DictionaryWithListValue()
    {
        diclist dd = new diclist();
        dd.d = new Dictionary<string, List<string>>();
        dd.d.Add("a", new List<string> { "1", "2", "3" });
        dd.d.Add("b", new List<string> { "4", "5", "7" });
        string s = JSON.ToJSON(dd, new JSONParameters { UseExtensions = false });
        Console.WriteLine(s);
        diclist o = JSON.ToObject<diclist>(s);
        Assert.AreEqual(3, o.d["a"].Count);

        s = JSON.ToJSON(dd.d, new JSONParameters { UseExtensions = false });
        Dictionary<string, List<string>> oo = JSON.ToObject<Dictionary<string, List<string>>>(s);
        Assert.AreEqual(3, oo["a"].Count);
        Dictionary<string, string[]> ooo = JSON.ToObject<Dictionary<string, string[]>>(s);
        Assert.AreEqual(3, ooo["b"].Length);
    }

    [Test]
    public static void HashtableTest()
    {
        Hashtable h = new Hashtable();
        h.Add(1, "dsjfhksa");
        h.Add("dsds", new class1());

        string s = JSON.ToNiceJSON(h, new JSONParameters() { UseExtensions = true });

        Hashtable o = JSON.ToObject<Hashtable>(s);
        Assert.AreEqual(typeof(Hashtable), o.GetType());
        Assert.AreEqual(typeof(class1), o["dsds"].GetType());
    }
    
    [Test]
    public static void HashsetTest()
    {
        HashSet<string> h = new HashSet<string>
        {
            "string1",
            "string2"
        };

        string s = JSON.ToNiceJSON(h);
        HashSet<string> o = JSON.ToObject<HashSet<string>>(s);
        
        Assert.AreEqual(typeof(HashSet<string>), o.GetType());
        Assert.AreEqual(o.Count, h.Count);
    }
    
    [Test]
    public static void HashsetNestedTest()
    {
        HashSet<HashSet<string>> h = new HashSet<HashSet<string>>
        {
            new HashSet<string>
            {
                "item1",
                "item2"
            },
            new HashSet<string>
            {
                "item3",
                "item4"
            },
        };

        string s = JSON.ToNiceJSON(h);
        HashSet<HashSet<string>> o = JSON.ToObject<HashSet<HashSet<string>>>(s);
        
        Assert.AreEqual(typeof(HashSet<HashSet<string>>), o.GetType());
        Assert.AreEqual(o.Count, h.Count);
    }
    
    [Test]
    public static void HashsetTestWithinDictionary()
    {
        Dictionary<string, HashSet<string>> dict = new Dictionary<string, HashSet<string>>
        {
            {
                "testKey", new HashSet<string>
                {
                    "string1",
                    "string2"
                }
            }
        };

        string s = JSON.ToNiceJSON(dict);
        Dictionary<string, HashSet<string>> o = JSON.ToObject<Dictionary<string, HashSet<string>>>(s);
        
        Assert.AreEqual(typeof(Dictionary<string, HashSet<string>>), o.GetType());
        Assert.AreEqual(o.Count, 1);

        HashSet<string> hashSet = dict["testKey"] as HashSet<string>;
        
        Assert.AreEqual(hashSet.Count, 2);
    }

    public abstract class abstractClass
    {
        public string myConcreteType { get; set; }
        public abstractClass()
        {

        }

        public abstractClass(string type) // : base(type)
        {

            this.myConcreteType = type;

        }
    }

    public abstract class abstractClass<T> : abstractClass
    {
        public T Value { get; set; }
        public abstractClass() { }
        public abstractClass(T value, string type) : base(type) { this.Value = value; }
    }
    public class OneConcreteClass : abstractClass<int>
    {
        public OneConcreteClass() { }
        public OneConcreteClass(int value) : base(value, "INT") { }
    }
    public class OneOtherConcreteClass : abstractClass<string>
    {
        public OneOtherConcreteClass() { }
        public OneOtherConcreteClass(string value) : base(value, "STRING") { }
    }

    [Test]
    public static void AbstractTest()
    {
        OneConcreteClass intField = new OneConcreteClass(1);
        OneOtherConcreteClass stringField = new OneOtherConcreteClass("lol");
        List<abstractClass> list = new List<abstractClass>() { intField, stringField };

        string json = JSON.ToNiceJSON(list, new JSONParameters() { UseExtensions = true });
        Console.WriteLine(json);
        List<abstractClass> objects = JSON.ToObject<List<abstractClass>>(json);
    }

    [Test]
    public static void NestedDictionary()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict["123"] = 12345;

        Dictionary<string, object> table = new Dictionary<string, object>();
        table["dict"] = dict;

        string st = JSON.ToJSON(table);
        Console.WriteLine(JSON.Beautify(st));
        Dictionary<string, object> tableDst = JSON.ToObject<Dictionary<string, object>>(st);
        Console.WriteLine(JSON.Beautify(JSON.ToJSON(tableDst)));
    }

    public class ignorecase
    {
        public string Name;
        public int Age;
    }
    public class ignorecase2
    {
        public string name;
        public int age;
    }
    [Test]
    public static void IgnoreCase()
    {
        string json = "{\"name\":\"aaaa\",\"age\": 42}";

        ignorecase o = JSON.ToObject<ignorecase>(json);
        Assert.AreEqual("aaaa", o.Name);
        ignorecase2 oo = JSON.ToObject<ignorecase2>(json.ToUpper());
        Assert.AreEqual("AAAA", oo.name);
    }

    public class coltest
    {
        public string name;
        public NameValueCollection nv;
        public StringDictionary sd;
    }

    [Test]
    public static void SpecialCollections()
    {
        NameValueCollection nv = new NameValueCollection();
        nv.Add("1", "a");
        nv.Add("2", "b");
        string s = JSON.ToJSON(nv);
        Console.WriteLine(s);
        NameValueCollection oo = JSON.ToObject<NameValueCollection>(s);
        Assert.AreEqual("a", oo["1"]);
        StringDictionary sd = new StringDictionary();
        sd.Add("1", "a");
        sd.Add("2", "b");
        s = JSON.ToJSON(sd);
        Console.WriteLine(s);
        StringDictionary o = JSON.ToObject<StringDictionary>(s);
        Assert.AreEqual("b", o["2"]);

        coltest c = new coltest();
        c.name = "aaa";
        c.nv = nv;
        c.sd = sd;
        s = JSON.ToJSON(c);
        Console.WriteLine(s);
        object ooo = JSON.ToObject(s);
        Assert.AreEqual("a", (ooo as coltest).nv["1"]);
        Assert.AreEqual("b", (ooo as coltest).sd["2"]);
    }

    public class constch
    {
        public enumt e = enumt.B;
        public string Name = "aa";
        public const int age = 11;
    }

    [Test]
    public static void consttest()
    {
        string s = JSON.ToJSON(new constch());
        object o = JSON.ToObject(s);
    }


    public enum enumt
    {
        A = 65,
        B = 90,
        C = 100
    }

    [Test]
    public static void enumtest()
    {
        string s = JSON.ToJSON(new constch(), new JSONParameters { UseValuesOfEnums = true, UseExtensions = true });
        Console.WriteLine(s);
        object o = JSON.ToObject(s);
    }

    public class ignoreatt : Attribute
    {
    }

    public class ignore
    {
        public string Name { get; set; }
        [System.Xml.Serialization.XmlIgnore]
        public int Age1 { get; set; }
        [ignoreatt]
        public int Age2;
    }
    public class ignore1 : ignore
    {
    }

    [Test]
    public static void IgnoreAttributes()
    {
        ignore i = new ignore { Age1 = 10, Age2 = 20, Name = "aa" };
        string s = JSON.ToJSON(i);
        Console.WriteLine(s);
        Assert.IsFalse(s.Contains("Age1"));
        i = new ignore1 { Age1 = 10, Age2 = 20, Name = "bb" };
        JSONParameters j = new JSONParameters() { UseExtensions = true };
        j.IgnoreAttributes.Add(typeof(ignoreatt));
        s = JSON.ToJSON(i, j);
        Console.WriteLine(s);
        Assert.IsFalse(s.Contains("Age1"));
        Assert.IsFalse(s.Contains("Age2"));
    }

    public class nondefaultctor
    {
        public nondefaultctor(int a)
        { age = a; }
        public int age;
    }

    [Test]
    public static void NonDefaultConstructor()
    {
        nondefaultctor o = new nondefaultctor(10);
        string s = JSON.ToJSON(o);
        Console.WriteLine(s);
        nondefaultctor obj = JSON.ToObject<nondefaultctor>(s, new JSONParameters { ParametricConstructorOverride = true, UsingGlobalTypes = true, UseExtensions = true });
        Assert.AreEqual(10, obj.age);
        Console.WriteLine("list of objects");
        List<nondefaultctor> l = new List<nondefaultctor> { o, o, o };
        s = JSON.ToJSON(l);
        Console.WriteLine(s);
        List<nondefaultctor> obj2 = JSON.ToObject<List<nondefaultctor>>(s, new JSONParameters { ParametricConstructorOverride = true, UsingGlobalTypes = true, UseExtensions = true });
        Assert.AreEqual(3, obj2.Count);
        Assert.AreEqual(10, obj2[1].age);
    }

    private delegate object CreateObj();
    private static SafeDictionary<Type, CreateObj> _constrcache = new SafeDictionary<Type, CreateObj>();
    internal static object FastCreateInstance(Type objtype)
    {
        try
        {
            CreateObj c = null;
            if (_constrcache.TryGetValue(objtype, out c))
            {
                return c();
            }

            if (objtype.IsClass)
            {
                DynamicMethod dynMethod = new DynamicMethod("_fcc", objtype, null, true);
                ILGenerator ilGen = dynMethod.GetILGenerator();
                ilGen.Emit(OpCodes.Newobj, objtype.GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Ret);
                c = (CreateObj)dynMethod.CreateDelegate(typeof(CreateObj));
                _constrcache.Add(objtype, c);
            }
            else // structs
            {
                DynamicMethod dynMethod = new DynamicMethod("_fcs", typeof(object), null, true);
                ILGenerator ilGen = dynMethod.GetILGenerator();
                LocalBuilder lv = ilGen.DeclareLocal(objtype);
                ilGen.Emit(OpCodes.Ldloca_S, lv);
                ilGen.Emit(OpCodes.Initobj, objtype);
                ilGen.Emit(OpCodes.Ldloc_0);
                ilGen.Emit(OpCodes.Box, objtype);
                ilGen.Emit(OpCodes.Ret);
                c = (CreateObj)dynMethod.CreateDelegate(typeof(CreateObj));
                _constrcache.Add(objtype, c);
            }
            return c();
        }
        catch (Exception exc)
        {
            throw new Exception(string.Format("Failed to fast create instance for type '{0}' from assembly '{1}'",
                objtype.FullName, objtype.AssemblyQualifiedName), exc);
        }
    }

    private static SafeDictionary<Type, Func<object>> lamdic = new SafeDictionary<Type, Func<object>>();
    static object lambdaCreateInstance(Type type)
    {
        Func<object> o = null;
        if (lamdic.TryGetValue(type, out o))
            return o();
        o = Expression.Lambda<Func<object>>(
                Expression.Convert(Expression.New(type), typeof(object)))
            .Compile();
        lamdic.Add(type, o);
        return o();
    }

    [Test]
    public static void CreateObjPerfTest()
    {
        //FastCreateInstance(typeof(colclass));
        //lambdaCreateInstance(typeof(colclass));
        int count = 100000;
        Console.WriteLine("count = " + count.ToString("#,#"));
        DateTime dt = DateTime.Now;
        for (int i = 0; i < count; i++)
        {
            object o = new colclass();
        }
        Console.WriteLine("normal new T() time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);

        dt = DateTime.Now;
        for (int i = 0; i < count; i++)
        {
            object o = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(colclass));
        }
        Console.WriteLine("FormatterServices time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);

        dt = DateTime.Now;
        for (int i = 0; i < count; i++)
        {
            object o = FastCreateInstance(typeof(colclass));
        }
        Console.WriteLine("IL newobj time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);

        dt = DateTime.Now;
        for (int i = 0; i < count; i++)
        {
            object o = lambdaCreateInstance(typeof(colclass));
        }
        Console.WriteLine("lambda time ms = " + DateTime.Now.Subtract(dt).TotalMilliseconds);
    }


    public class o1
    {
        public int o1int;
        public o2 o2obj;
        public o3 child;
    }
    public class o2
    {
        public int o2int;
        public o1 parent;
    }
    public class o3
    {
        public int o3int;
        public o2 child;
    }


    [Test]
    public static void CircularReferences()
    {
        o1 o = new o1 { o1int = 1, child = new o3 { o3int = 3 }, o2obj = new o2 { o2int = 2 } };
        o.o2obj.parent = o;
        o.child.child = o.o2obj;

        string s = JSON.ToJSON(o, new JSONParameters() { UseExtensions = true });
        Console.WriteLine(JSON.Beautify(s));
        o1 p = JSON.ToObject<o1>(s);
        Assert.AreEqual(p, p.o2obj.parent);
        Assert.AreEqual(p.o2obj, p.child.child);
    }

    public class lol
    {
        public List<List<object>> r;
    }
    public class lol2
    {
        public List<object[]> r;
    }
    [Test]
    public static void ListOfList()
    {
        List<List<object>> o = new List<List<object>> { new List<object> { 1, 2, 3 }, new List<object> { "aa", 3, "bb" } };
        string s = JSON.ToJSON(o);
        Console.WriteLine(s);
        object i = JSON.ToObject(s);
        lol p = new lol { r = o };
        s = JSON.ToJSON(p);
        Console.WriteLine(s);
        i = JSON.ToObject(s);
        Assert.AreEqual(3, (i as lol).r[0].Count);

        List<object[]> oo = new List<object[]> { new object[] { 1, 2, 3 }, new object[] { "a", 4, "b" } };
        s = JSON.ToJSON(oo);
        Console.WriteLine(s);
        object ii = JSON.ToObject(s);
        lol2 l = new lol2() { r = oo };

        s = JSON.ToJSON(l);
        Console.WriteLine(s);
        object iii = JSON.ToObject(s);
        Assert.AreEqual(3, (iii as lol2).r[0].Length);
    }
    //[Test]
    //public static void Exception()
    //{
    //    var e = new Exception("hello");

    //    var s = Json5Core.JSON.ToJSON(e);
    //    Console.WriteLine(s);
    //    var o = Json5Core.JSON.ToObject(s);
    //    Assert.AreEqual("hello", (o as Exception).Message);
    //}
    //public class ilistclass
    //{
    //    public string name;
    //    public IList<colclass> list { get; set; }
    //}

    //[Test]
    //public static void ilist()
    //{
    //    ilistclass i = new ilistclass();
    //    i.name = "aa";
    //    i.list = new List<colclass>();
    //    i.list.Add(new colclass() { gender = Gender.Female, date = DateTime.Now, isNew = true });

    //    var s = Json5Core.JSON.ToJSON(i);
    //    Console.WriteLine(s);
    //    var o = Json5Core.JSON.ToObject(s);
    //}


    //[Test]
    //public static void listdic()
    //{ 
    //    string s = @"[{""1"":""a""},{""2"":""b""}]";
    //    var o = Json5Core.JSON.ToDynamic(s);// ToObject<List<Dictionary<string, object>>>(s);
    //    var d = o[0].Count;
    //    Console.WriteLine(d.ToString());
    //}


    public class Y
    {
        public byte[] BinaryData;
    }

    public class A
    {
        public int DataA;
        public A NextA;
    }

    public class B : A
    {
        public string DataB;
    }

    public class C : A
    {
        public DateTime DataC;
    }

    public class Root
    {
        public Y TheY;
        public List<A> ListOfAs = new List<A>();
        public string UnicodeText;
        public Root NextRoot;
        public int MagicInt { get; set; }
        public A TheReferenceA;

        public void SetMagicInt(int value)
        {
            MagicInt = value;
        }
    }

    [Test]
    public static void complexobject()
    {
        Root r = new Root();
        r.TheY = new Y { BinaryData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF } };
        r.ListOfAs.Add(new A { DataA = 10 });
        r.ListOfAs.Add(new B { DataA = 20, DataB = "Hello" });
        r.ListOfAs.Add(new C { DataA = 30, DataC = DateTime.Today });
        r.UnicodeText = "Žlutý kůň ∊ WORLD";
        r.ListOfAs[2].NextA = r.ListOfAs[1];
        r.ListOfAs[1].NextA = r.ListOfAs[2];
        r.TheReferenceA = r.ListOfAs[2];
        r.NextRoot = r;

        JSONParameters jsonParams = new JSONParameters() { UseExtensions = true };
        jsonParams.UseEscapedUnicode = false;

        Console.WriteLine("JSON:\n---\n{0}\n---", JSON.ToJSON(r, jsonParams));

        Console.WriteLine();

        Console.WriteLine("Nice JSON:\n---\n{0}\n---", JSON.ToNiceJSON(JSON.ToObject<Root>(JSON.ToNiceJSON(r, jsonParams)), jsonParams));
    }

    [Test]
    public static void TestMilliseconds()
    {
        JSONParameters jpar = new JSONParameters() { UseExtensions = true };
        jpar.DateTimeMilliseconds = false;
        DateTime dt = DateTime.Now;
        string s = JSON.ToJSON(dt, jpar);
        Console.WriteLine(s);
        DateTime o = JSON.ToObject<DateTime>(s, jpar);
        Assert.AreNotEqual(dt.Millisecond, o.Millisecond);

        jpar.DateTimeMilliseconds = true;
        s = JSON.ToJSON(dt, jpar);
        Console.WriteLine(s);
        o = JSON.ToObject<DateTime>(s, jpar);
        Assert.AreEqual(dt.Millisecond, o.Millisecond);
    }

    public struct Foo
    {
        public string name;
    };

    public class Bar
    {
        public Foo foo;
    };

    [Test]
    public static void StructProperty()
    {
        Bar b = new Bar();
        b.foo = new Foo();
        b.foo.name = "Buzz";
        string json = JSON.ToJSON(b);
        Bar bar = JSON.ToObject<Bar>(json);
    }

    [Test]
    public static void NullVariable()
    {
        int? i = JSON.ToObject<int?>("10");
        Assert.AreEqual(10, i);
        long? l = JSON.ToObject<long?>("100");
        Assert.AreEqual(100L, l);
        DateTime? d = JSON.ToObject<DateTime?>("\"2000-01-01 10:10:10\"");
        Assert.AreEqual(2000, d.Value.Year);
    }

    public class readonlyclass
    {
        public readonlyclass()
        {
            ROName = "bb";
            Age = 10;
        }
        private string _ro = "aa";
        public string ROAddress { get { return _ro; } }
        public string ROName { get; private set; }
        public int Age { get; set; }
    }

    [Test]
    public static void ReadonlyTest()
    {
        string s = JSON.ToJSON(new readonlyclass(), new JSONParameters { ShowReadOnlyProperties = true, UseExtensions = true });
        readonlyclass o = JSON.ToObject<readonlyclass>(s.Replace("aa", "cc"));
        Assert.AreEqual("aa", o.ROAddress);
    }

    public class container
    {
        public string name = "aa";
        public List<inline> items = new List<inline>();
    }
    public class inline
    {
        public string aaaa = "1111";
        public int id = 1;
    }

    [Test]
    public static void InlineCircular()
    {
        container o = new container();
        inline i = new inline();
        o.items.Add(i);
        o.items.Add(i);

        string s = JSON.ToNiceJSON(o, JSON.Parameters);
        Console.WriteLine("*** circular replace");
        Console.WriteLine(s);

        s = JSON.ToNiceJSON(o, new JSONParameters { InlineCircularReferences = true, UseExtensions = true });
        Console.WriteLine("*** inline objects");
        Console.WriteLine(s);
    }


    [Test]
    public static void lowercaseSerilaize()
    {
        Retclass r = new Retclass();
        r.Name = "Hello";
        r.Field1 = "dsasdF";
        r.Field2 = 2312;
        r.date = DateTime.Now;
        string s = JSON.ToNiceJSON(r, new JSONParameters { SerializeToLowerCaseNames = true,UseExtensions = true });
        Console.WriteLine(s);
        object o = JSON.ToObject(s);
        Assert.IsNotNull(o);
        Assert.AreEqual("Hello", (o as Retclass).Name);
        Assert.AreEqual(2312, (o as Retclass).Field2);
    }


    public class nulltest
    {
        public string A;
        public int b;
        public DateTime? d;
    }

    [Test]
    public static void null_in_dictionary()
    {
        Dictionary<string, object> d = new Dictionary<string, object>();
        d.Add("a", null);
        d.Add("b", 12);
        d.Add("c", null);

        string s = JSON.ToJSON(d);
        Console.WriteLine(s);
        s = JSON.ToJSON(d, new JSONParameters() { SerializeNullValues = false, UseExtensions = true });
        Console.WriteLine(s);
        Assert.AreEqual("{\"b\":12}", s);

        s = JSON.ToJSON(new nulltest(), new JSONParameters { SerializeNullValues = false, UseExtensions = false });
        Console.WriteLine(s);
        Assert.AreEqual("{\"b\":0}", s);
    }


    public class InstrumentSettings
    {
        public string dataProtocol { get; set; }
        public static bool isBad { get; set; }
        public static bool isOk;

        public InstrumentSettings()
        {
            dataProtocol = "Wireless";
        }
    }

    [Test]
    public static void statictest()
    {
        InstrumentSettings s = new InstrumentSettings();
        JSONParameters pa = new JSONParameters() { UseExtensions = true };
        pa.UseExtensions = false;
        InstrumentSettings.isOk = true;
        InstrumentSettings.isBad = true;

        string jsonStr = JSON.ToNiceJSON(s, pa);

        InstrumentSettings o = JSON.ToObject<InstrumentSettings>(jsonStr);
    }

    public class arrayclass2
    {
        public int[] ints { get; set; }
        public string[] strs;
        public int[][] int2d { get; set; }
        public int[][][] int3d;
        public baseclass[][] class2d;
    }

    [Test]
    public static void ArrayTest2()
    {
        arrayclass2 a = new arrayclass2();
        a.ints = new int[] { 3, 1, 4 };
        a.strs = new string[] { "a", "b", "c" };
        a.int2d = new int[][] { new int[] { 1, 2, 3 }, new int[] { 2, 3, 4 } };
        a.int3d = new int[][][] {        new int[][] {
            new int[] { 0, 0, 1 },
            new int[] { 0, 1, 0 }
        },
        null,
        new int[][] {
            new int[] { 0, 0, 2 },
            new int[] { 0, 2, 0 },
            null
        }
    };
        a.class2d = new baseclass[][]{
        new baseclass[] {
            new baseclass () { Name = "a", Code = "A" },
            new baseclass () { Name = "b", Code = "B" }
        },
        new baseclass[] {
            new baseclass () { Name = "c" }
        },
        null
    };
        string s = JSON.ToJSON(a);
        arrayclass2 o = JSON.ToObject<arrayclass2>(s);
        CollectionAssert.AreEqual(a.ints, o.ints);
        CollectionAssert.AreEqual(a.strs, o.strs);
        CollectionAssert.AreEqual(a.int2d[0], o.int2d[0]);
        CollectionAssert.AreEqual(a.int2d[1], o.int2d[1]);
        CollectionAssert.AreEqual(a.int3d[0][0], o.int3d[0][0]);
        CollectionAssert.AreEqual(a.int3d[0][1], o.int3d[0][1]);
        Assert.AreEqual(null, o.int3d[1]);
        CollectionAssert.AreEqual(a.int3d[2][0], o.int3d[2][0]);
        CollectionAssert.AreEqual(a.int3d[2][1], o.int3d[2][1]);
        CollectionAssert.AreEqual(a.int3d[2][2], o.int3d[2][2]);
        for (int i = 0; i < a.class2d.Length; i++)
        {
            baseclass[] ai = a.class2d[i];
            baseclass[] oi = o.class2d[i];
            if (ai == null && oi == null)
            {
                continue;
            }
            for (int j = 0; j < ai.Length; j++)
            {
                baseclass aii = ai[j];
                baseclass oii = oi[j];
                if (aii == null && oii == null)
                {
                    continue;
                }
                Assert.AreEqual(aii.Name, oii.Name);
                Assert.AreEqual(aii.Code, oii.Code);
            }
        }
    }

    [Test]
    public static void Dictionary_String_Object_WithList()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        dict.Add("C", new List<float>() { 1.1f, 2.2f, 3.3f });
        string json = JSON.ToJSON(dict);

        Dictionary<string, List<float>> des = JSON.ToObject<Dictionary<string, List<float>>>(json);
        Assert.IsInstanceOf(typeof(List<float>), des["C"]);
    }

    [Test]
    public static void exotic_deserialize()
    {
        Console.WriteLine();
        Console.Write("Json5Core deserialize");
        colclass c = CreateObject(true, true);
        Stopwatch stopwatch = new Stopwatch();
        for (int pp = 0; pp < fivetimes; pp++)
        {
            colclass deserializedStore;
            string jsonText = null;

            stopwatch.Restart();
            jsonText = JSON.ToJSON(c);
            //Console.WriteLine(" size = " + jsonText.Length);
            for (int i = 0; i < thousandtimes; i++)
            {
				deserializedStore = (colclass)JSON.ToObject(jsonText);
            }
            stopwatch.Stop();
            Console.Write("\t" + stopwatch.ElapsedMilliseconds);
        }
    }

    [Test]
    public static void exotic_serialize()
    {
        Console.WriteLine();
        Console.Write("Json5Core serialize");
        colclass c = CreateObject(true, true);
        Stopwatch stopwatch = new Stopwatch();
        for (int pp = 0; pp < fivetimes; pp++)
        {
            string jsonText = null;
            stopwatch.Restart();
            for (int i = 0; i < thousandtimes; i++)
            {
                jsonText = JSON.ToJSON(c);
            }
            stopwatch.Stop();
            Console.Write("\t" + stopwatch.ElapsedMilliseconds);
        }
    }

    [Test]
    public static void BigData()
    {
        Console.WriteLine();
        Console.Write("Json5Core bigdata serialize");
        colclass c = CreateBigdata();
        Console.WriteLine("\r\ntest obj created");
        double t = 0;
        Stopwatch stopwatch = new Stopwatch();
        for (int pp = 0; pp < fivetimes; pp++)
        {
            string jsonText = null;
            stopwatch.Restart();

            jsonText = JSON.ToJSON(c);

            stopwatch.Stop();
            t += stopwatch.ElapsedMilliseconds;
            Console.Write("\t" + stopwatch.ElapsedMilliseconds);
        }
        Console.WriteLine("\tAVG = " + t / fivetimes);
    }

    private static colclass CreateBigdata()
    {
        colclass c = new colclass();
        Random r = new Random((int)DateTime.Now.Ticks);

        for (int i = 0; i < 200 * thousandtimes; i++)
        {
            c.items.Add(new class1(r.Next().ToString(), r.Next().ToString(), Guid.NewGuid()));
        }
        return c;
    }

    [Test]
    public static void comments()
    {
        string s = @"
{
    // help
    ""property"" : 2,
    // comment
    ""str"":""hello"" //hello
}
";
        object o = JSON.Parse(s);
        Assert.AreEqual(2, (o as IDictionary).Count);
    }

#if !CORE_TEST || NETFRAMEWORK || !NETCOREAPP3_0_OR_GREATER
    public class ctype
    {
        public System.Net.IPAddress ip;
    }
    [Test]
    public static void CustomTypes()
    {
        var ip = new ctype();
        ip.ip = System.Net.IPAddress.Loopback;

        JSON.RegisterCustomType(typeof(System.Net.IPAddress),
            (x) => { return x.ToString(); },
            (x) => { return System.Net.IPAddress.Parse(x); });

        var s = JSON.ToJSON(ip);

        var o = JSON.ToObject<ctype>(s);
        Assert.AreEqual(ip.ip, o.ip);
    }
#else
    public class ctype
    {
        public HashCode hashcode;
    }
    [Test]
    public static void CustomTypes()
    {
        ctype hashcode = new ctype();
        hashcode.hashcode = new HashCode();

        //this code is not necessarily efficient:
        JSON.RegisterCustomType(typeof(HashCode),
            (x) =>
            {
                unsafe
                {
                    HashCode y = (HashCode)x;
                    int* y_ = (int*)&y;
                    return 
                    y_[0].ToString("X8") +
                    y_[1].ToString("X8") +
                    y_[2].ToString("X8") +
                    y_[3].ToString("X8") +
                    y_[4].ToString("X8") +
                    y_[5].ToString("X8") +
                    y_[6].ToString("X8") +
                    y_[7].ToString("X8");
                }
            },
            (x) => 
            {
                unsafe
                {
                    string x_ = (string)x;
                    uint y0 = uint.Parse(x_[(0 * 8)..(1 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y1 = uint.Parse(x_[(1 * 8)..(2 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y2 = uint.Parse(x_[(2 * 8)..(3 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y3 = uint.Parse(x_[(3 * 8)..(4 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y4 = uint.Parse(x_[(4 * 8)..(5 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y5 = uint.Parse(x_[(5 * 8)..(6 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y6 = uint.Parse(x_[(6 * 8)..(7 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint y7 = uint.Parse(x_[(7 * 8)..(8 * 8)], System.Globalization.NumberStyles.HexNumber);
                    uint[] ys = { y0, y1, y2, y3, y4, y5, y6, y7 };
                    fixed (uint* y = ys) return *(HashCode*)&y;
                }
            });

        string s = JSON.ToJSON(hashcode);

        ctype o = JSON.ToObject<ctype>(s);
        Assert.AreEqual(hashcode.hashcode.ToHashCode(), o.hashcode.ToHashCode());
    }
#endif

    [Test]
    public static void stringint()
    {
        long o = JSON.ToObject<long>("\"42\"");
    }

    [Test]
    public static void anonymoustype()
    {
        JSONParameters jsonParameters = new JSONParameters { EnableAnonymousTypes = true, UseExtensions = true };
        List<DateTimeOffset> data = new List<DateTimeOffset>();
        data.Add(new DateTimeOffset(DateTime.Now));

        var anonTypeWithDateTimeOffset = data.Select(entry => new { DateTimeOffset = entry }).ToList();
        string json = JSON.ToJSON(anonTypeWithDateTimeOffset.First(), jsonParameters); // this will throw

        var obj = new
        {
            Name = "aa",
            Age = 42,
            Code = "007"
        };

        json = JSON.ToJSON(obj, jsonParameters);
        Assert.True(json.Contains("\"Name\""));
    }

    [Test]
    public static void Expando()
    {
        dynamic obj = new ExpandoObject();
        obj.UserView = "10080";
        obj.UserCatalog = "test";
        obj.UserDate = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        obj.UserBase = "";

        string s = JSON.ToJSON(obj);
        Assert.True(s.Contains("UserView\":\"10080"));
    }


    public class item
    {
        public string name;
        public int age;
    }

    [Test]
    public static void array()
    {
        string j = @"
[
{""name"":""Tom"",""age"":1},
{""name"":""Dick"",""age"":1},
{""name"":""Harry"",""age"":3}
]
";

        List<item> o = JSON.ToObject<List<item>>(j);
        Assert.AreEqual(3, o.Count);

        item[] oo = JSON.ToObject<item[]>(j);
        Assert.AreEqual(3, oo.Count());
    }

    [Test]
    public static void NaN()
    {
        double d = double.NaN;
        float f = float.NaN;


        string s = JSON.ToJSON(d);
        double o = JSON.ToObject<double>(s);
        Assert.AreEqual(d, o);

        s = JSON.ToJSON(f);
        float oo = JSON.ToObject<float>(s);
        Assert.AreEqual(f, oo);

        float pp = JSON.ToObject<Single>(s);
    }

    [Test]
    public static void nonstandardkey()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict["With \"Quotes\""] = "With \"Quotes\"";
        JSONParameters p = new JSONParameters() { UseExtensions = true };
        p.EnableAnonymousTypes = false;
        p.SerializeNullValues = false;
        p.UseExtensions = false;
        string s = JSON.ToJSON(dict, p);
        Dictionary<string, string> d = JSON.ToObject<Dictionary<string, string>>(s);
        Assert.AreEqual(1, d.Count);
        Assert.AreEqual("With \"Quotes\"", d.Keys.First());
    }

    [Test]
    public static void bytearrindic()
    {
        string s = JSON.ToJSON(new Dictionary<string, byte[]>
                {
                    { "Test", new byte[10] },
                    { "Test 2", new byte[0] }
                });

        Dictionary<string, byte[]> d = JSON.ToObject<Dictionary<string, byte[]>>(s);
    }

#region twitter
    public class Twitter
    {
        public Query query { get; set; }
        public Result result { get; set; }

        public class Query
        {
            public Parameters @params { get; set; }
            public string type { get; set; }
            public string url { get; set; }
        }

        public class Parameters
        {
            public int accuracy { get; set; }
            public bool autocomplete { get; set; }
            public string granularity { get; set; }
            public string query { get; set; }
            public bool trim_place { get; set; }
        }

        public class Result
        {
            public Place[] places { get; set; }
        }

        public class Place
        {
            public Attributes attributes { get; set; }
            public BoundingBox bounding_box { get; set; }
            public Place[] contained_within { get; set; }

            public string country { get; set; }
            public string country_code { get; set; }
            public string full_name { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string place_type { get; set; }
            public string url { get; set; }
        }

        public class Attributes
        {
        }

        public class BoundingBox
        {
            public double[][][] coordinates { get; set; }
            public string type { get; set; }
        }
    }
#endregion
    [Test]
    public static void twitter()
    {
#region tw data
        string ss = @"{
  ""query"": {
    ""params"": {
      ""accuracy"": 0,
      ""autocomplete"": false,
      ""granularity"": ""neighborhood"",
      ""query"": ""Toronto"",
      ""trim_place"": false
    },
    ""type"": ""search"",
    ""url"": ""https://api.twitter.com/1.1/geo/search.json?accuracy=0&query=Toronto&granularity=neighborhood&autocomplete=false&trim_place=false""
  },
  ""result"": {
    ""places"": [
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -96.647415,
                44.566715
              ],
              [
                -96.630435,
                44.566715
              ],
              [
                -96.630435,
                44.578118
              ],
              [
                -96.647415,
                44.578118
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -104.057739,
                    42.479686
                  ],
                  [
                    -96.436472,
                    42.479686
                  ],
                  [
                    -96.436472,
                    45.945716
                  ],
                  [
                    -104.057739,
                    45.945716
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""South Dakota, US"",
            ""id"": ""d06e595eb3733f42"",
            ""name"": ""South Dakota"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/d06e595eb3733f42.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, SD"",
        ""id"": ""3e8542a1e9f82870"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/3e8542a1e9f82870.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -80.622815,
                40.436469
              ],
              [
                -80.596567,
                40.436469
              ],
              [
                -80.596567,
                40.482566
              ],
              [
                -80.622815,
                40.482566
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -84.820305,
                    38.403423
                  ],
                  [
                    -80.518454,
                    38.403423
                  ],
                  [
                    -80.518454,
                    42.327132
                  ],
                  [
                    -84.820305,
                    42.327132
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""Ohio, US"",
            ""id"": ""de599025180e2ee7"",
            ""name"": ""Ohio"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/de599025180e2ee7.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, OH"",
        ""id"": ""53d949149e8cd438"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/53d949149e8cd438.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -79.639128,
                43.403221
              ],
              [
                -78.90582,
                43.403221
              ],
              [
                -78.90582,
                43.855466
              ],
              [
                -79.639128,
                43.855466
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -95.155919,
                    41.676329
                  ],
                  [
                    -74.339383,
                    41.676329
                  ],
                  [
                    -74.339383,
                    56.852398
                  ],
                  [
                    -95.155919,
                    56.852398
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""Canada"",
            ""country_code"": ""CA"",
            ""full_name"": ""Ontario, Canada"",
            ""id"": ""89b2eb8b2b9847f7"",
            ""name"": ""Ontario"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/89b2eb8b2b9847f7.json""
          }
        ],
        ""country"": ""Canada"",
        ""country_code"": ""CA"",
        ""full_name"": ""Toronto, Ontario"",
        ""id"": ""8f9664a8ccd89e5c"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/8f9664a8ccd89e5c.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -90.867234,
                41.898723
              ],
              [
                -90.859467,
                41.898723
              ],
              [
                -90.859467,
                41.906811
              ],
              [
                -90.867234,
                41.906811
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -96.639485,
                    40.375437
                  ],
                  [
                    -90.140061,
                    40.375437
                  ],
                  [
                    -90.140061,
                    43.501196
                  ],
                  [
                    -96.639485,
                    43.501196
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""Iowa, US"",
            ""id"": ""3cd4c18d3615bbc9"",
            ""name"": ""Iowa"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/3cd4c18d3615bbc9.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, IA"",
        ""id"": ""173d6f9c3249b4fd"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/173d6f9c3249b4fd.json""
      },
      {
        ""attributes"": {},
        ""bounding_box"": {
          ""coordinates"": [
            [
              [
                -95.956873,
                37.792724
              ],
              [
                -95.941288,
                37.792724
              ],
              [
                -95.941288,
                37.803752
              ],
              [
                -95.956873,
                37.803752
              ]
            ]
          ],
          ""type"": ""Polygon""
        },
        ""contained_within"": [
          {
            ""attributes"": {},
            ""bounding_box"": {
              ""coordinates"": [
                [
                  [
                    -102.051769,
                    36.993016
                  ],
                  [
                    -94.588387,
                    36.993016
                  ],
                  [
                    -94.588387,
                    40.003166
                  ],
                  [
                    -102.051769,
                    40.003166
                  ]
                ]
              ],
              ""type"": ""Polygon""
            },
            ""country"": ""United States"",
            ""country_code"": ""US"",
            ""full_name"": ""Kansas, US"",
            ""id"": ""27c45d804c777999"",
            ""name"": ""Kansas"",
            ""place_type"": ""admin"",
            ""url"": ""https://api.twitter.com/1.1/geo/id/27c45d804c777999.json""
          }
        ],
        ""country"": ""United States"",
        ""country_code"": ""US"",
        ""full_name"": ""Toronto, KS"",
        ""id"": ""b90e4628bff4ad82"",
        ""name"": ""Toronto"",
        ""place_type"": ""city"",
        ""url"": ""https://api.twitter.com/1.1/geo/id/b90e4628bff4ad82.json""
      }
    ]
  }
}";
#endregion
        Twitter o = JSON.ToObject<Twitter>(ss);
    }

    [Test]
    public static void datetimeoff()
    {
        DateTimeOffset dt = new DateTimeOffset(DateTime.Now);
        //JSON.RegisterCustomType(typeof(DateTimeOffset), 
        //    (x) => { return x.ToString(); },
        //    (x) => { return DateTimeOffset.Parse(x); }
        //);

        // test with UTC format ('Z' in output rather than HH:MM timezone)
        string s = JSON.ToJSON(dt, new JSONParameters { UseUTCDateTime = true, UseExtensions = true });
        Console.WriteLine(s);
        DateTimeOffset d = JSON.ToObject<DateTimeOffset>(s);
        // ticks will differ, so convert both to UTC and use ISO8601 roundtrip format to compare
        Assert.AreEqual(dt.ToUniversalTime().ToString("O"), d.ToUniversalTime().ToString("O"));

        s = JSON.ToJSON(dt, new JSONParameters { UseUTCDateTime = false, UseExtensions = true });
        Console.WriteLine(s);
        d = JSON.ToObject<DateTimeOffset>(s);
        Assert.AreEqual(dt.ToUniversalTime().ToString("O"), d.ToUniversalTime().ToString("O"));

        // test deserialize of output from DateTimeOffset.ToString()
        // DateTimeOffset roundtrip format, UTC 
        dt = new DateTimeOffset(DateTime.UtcNow);
        s = '"' + dt.ToString("O") + '"';
        Console.WriteLine(s);
        d = JSON.ToObject<DateTimeOffset>(s);
        Assert.AreEqual(dt.ToUniversalTime().ToString("O"), d.ToUniversalTime().ToString("O"));

        // DateTimeOffset roundtrip format, non-UTC
        dt = new DateTimeOffset(new DateTime(2017, 5, 22, 10, 06, 53, 123, DateTimeKind.Unspecified), TimeSpan.FromHours(11.5));
        s = '"' + dt.ToString("O") + '"';
        Console.WriteLine(s);
        d = JSON.ToObject<DateTimeOffset>(s);
        Assert.AreEqual(dt.ToUniversalTime().ToString("O"), d.ToUniversalTime().ToString("O"));

        // previous Json5Core serialization format for DateTimeOffset. Millisecond resolution only.
        s = '"' + dt.ToString("yyyy-MM-ddTHH:mm:ss.fff zzz") + '"';
        Console.WriteLine(s);
        DateTimeOffset ld = JSON.ToObject<DateTimeOffset>(s);
        Assert.AreEqual(dt.ToUniversalTime().ToString("O"), ld.ToUniversalTime().ToString("O"));
    }

    class X
    {
        private int i;
        public X(int i) { this.i = i; }
        public int I { get { return this.i; } }
    }

    [Test]
    public static void ReadonlyProperty()
    {
        X x = new X(10);
        string s = JSON.ToJSON(x, new JSONParameters { ShowReadOnlyProperties = true, UseExtensions = true });
        Assert.True(s.Contains("\"I\":"));
        X o = JSON.ToObject<X>(s, new JSONParameters { ParametricConstructorOverride = true, UseExtensions = true });
        // no set available -> I = 0
        Assert.AreEqual(0, o.I);
    }


    public class il
    {
        public IList list;
        public string name;
    }

    [Test]
    public static void ilist()
    {
        il i = new il();
        i.list = new List<baseclass>();
        i.list.Add(new class1("1", "1", Guid.NewGuid()));
        i.list.Add(new class2("4", "5", "hi"));
        i.name = "hi";

        string s = JSON.ToNiceJSON(i, new JSONParameters() { UseExtensions = true });//, new JSONParameters { UseExtensions = true });
        Console.WriteLine(s);

        il o = JSON.ToObject<il>(s);
    }


    public interface iintfc
    {
        string name { get; set; }
        int age { get; set; }
    }

    public class intfc : iintfc
    {
        public string address = "fadfsdf";
        private int _age;
        public int age
        {
            get
            {
                return _age;
            }

            set
            {
                _age = value;
            }
        }
        private string _name;
        public string name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }
    }

    public class it
    {
        public iintfc i { get; set; }
        public string name = "bb";

    }

    [Test]
    public static void interface_test()
    {
        it ii = new it();

        intfc i = new intfc();
        i.age = 10;
        i.name = "aa";

        ii.i = i;

        string s = JSON.ToJSON(ii);

        object o = JSON.ToObject(s);

    }

    [Test]
    public static void nested_dictionary()
    {
        Dictionary<int, Dictionary<string, double>> dic = new Dictionary<int, Dictionary<string, double>>();
        dic.Add(0, new Dictionary<string, double> { { "PX_LAST", 1.1 }, { "PX_LOW", 1.0 } });
        dic.Add(1, new Dictionary<string, double> { { "PX_LAST", 2.1 }, { "PX_LOW", 2.0 } });

        string s = JSON.ToJSON(dic);
        Dictionary<int, Dictionary<string, double>> obj = JSON.ToObject<Dictionary<int, Dictionary<string, double>>>(s);
        Assert.AreEqual(2, obj[0].Count());
    }

    [Test]
    public static void DynamicEnumerate()
    {
        string j =
        @"[
   {
      ""Prop1"" : ""Info 1"",
      ""Prop2"" : ""More Info 1""
   },
   {
      ""Prop1"" : ""Info 2"",
      ""Prop2"" : ""More Info 2""
   }
]";

        dynamic testObject = JSON.ToDynamic(j);
        foreach (dynamic o in testObject)
        {
            Console.WriteLine(o.Prop1);
            Assert.True(o.Prop1 != "");
        }
    }

    public class AC { public AC() { } public decimal Lo { get; set; } public decimal Ratio { get; set; } }

    [Test]
    public static void DictListTest()
    {
        Dictionary<string, List<AC>> dictList = new Dictionary<string, List<AC>>();
        dictList.Add("P", new List<AC>());
        dictList["P"].Add(new AC() { Lo = 1.5m, Ratio = 2.5m });
        dictList["P"].Add(new AC() { Lo = 2.5m, Ratio = 3.5m });
        string jsonstr = JSON.ToJSON(dictList, new JSONParameters { UseExtensions = false });

        Console.WriteLine();
        Console.WriteLine(jsonstr);

        Dictionary<string, List<AC>> dictList2 = Json5Core.JSON.ToObject<Dictionary<string, List<AC>>>(jsonstr);

        Assert.True(dictList2["P"].Count == 2);
        Assert.True(dictList2["P"][0].GetType() == typeof(AC));
        foreach (KeyValuePair<string, List<AC>> k in dictList2)
        {
            Console.Write(k.Key);
            foreach (AC v in k.Value)
                Console.WriteLine(":\tLo:{0}\tRatio:{1}", v.Lo, v.Ratio);
        }
    }


    public class ac<T>
    {
        public T age;
    }
    [Test]
    public static void autoconvert()
    {
        long v = long.MaxValue;
        //v = 42;
        //byte v = 42;
        ac<long> o = JSON.ToObject<ac<long>>("{\"age\":\"" + v + "\"}");
        Assert.AreEqual(v, o.age);
    }

    [Test]
    public static void timespan()
    {
        TimeSpan t = new TimeSpan(2, 2, 2, 2);
        string s = JSON.ToJSON(t);
        TimeSpan o = JSON.ToObject<TimeSpan>(s);
        Assert.AreEqual(o.Days, t.Days);
    }

    public class dmember
    {
        [System.Runtime.Serialization.DataMember(Name = "prop")]
        public string MyProperty;
        //[System.Runtime.Serialization.DataMember(Name = "id")]
        [Json5Core.DataMember(Name = "id")]
        public int docid;
    }

    public class TestObj
    {

        [Json5Core.DataMember(Name = "D")] 
        public int SomeData { get; set; } = -1;
    }

    [Test]
    public static void DataMember()
    {
        string s = "{\"prop\":\"Date\",\"id\":42}";
        Console.WriteLine(s);
        dmember o = Json5Core.JSON.ToObject<dmember>(s);

        Assert.AreEqual(42, o.docid);
        Assert.AreEqual("Date", o.MyProperty);

        string ss = Json5Core.JSON.ToJSON(o, new JSONParameters { UseExtensions = false });
        Console.WriteLine(ss);
        Assert.AreEqual(s, ss);

        TestObj popop = JSON.ToObject<TestObj>("{'D':9}");
        Assert.AreEqual(9, popop.SomeData);
    }

    [Test]
    public static void zerostring()
    {
        string t = "test\0test";
        Console.WriteLine(t);
        string s = Json5Core.JSON.ToJSON(t, new JSONParameters { UseEscapedUnicode = false, UseExtensions = true });
        Assert.True(s.Contains("\\0"));
        Console.WriteLine(s);
        string o = Json5Core.JSON.ToObject<string>(s);
        Assert.True(o.Contains("\0"));
        Console.WriteLine("" + o);
    }

    [Test]
    public static void spacetest()
    {
        colclass c = new colclass();

        string s = JSON.ToNiceJSON(c);
        Console.WriteLine(s);
        s = JSON.Beautify(s, 2);
        Console.WriteLine(s);
        s = JSON.ToNiceJSON(c, new JSONParameters { FormatterIndentSpaces = 8, UseExtensions = true });
        Console.WriteLine(s);
    }

    public class DigitLimit
    {
        public float Fmin;
        public float Fmax;
        public decimal MminDec;
        public decimal MmaxDec;


        public decimal Mmin;
        public decimal Mmax;
        public double Dmin;
        public double Dmax;
        public double DminDec;
        public double DmaxDec;
        public double Dni;
        public double Dpi;
        public double Dnan;
        public float FminDec;
        public float FmaxDec;
        public float Fni;
        public float Fpi;
        public float Fnan;
        public long Lmin;
        public long Lmax;
        public ulong ULmax;
        public int Imin;
        public int Imax;
        public uint UImax;


        //public IntPtr Iptr1 = new IntPtr(0); //Serialized to a Dict, exception on deserialization
        //public IntPtr Iptr2 = new IntPtr(0x33445566); //Serialized to a Dict, exception on deserialization
        //public UIntPtr UIptr1 = new UIntPtr(0); //Serialized to a Dict, exception on deserialization
        //public UIntPtr UIptr2 = new UIntPtr(0x55667788); //Serialized to a Dict, exception on deserialization
    }

    [Test]
    public static void dec()
    {

    }

    [Test]
    public static void digitlimits()
    {
        DigitLimit d = new DigitLimit();
        d.Fmin = float.MinValue;// serializer loss on tostring() 
        d.Fmax = float.MaxValue;// serializer loss on tostring()
        d.MminDec = -7.9228162514264337593543950335m;
        d.MmaxDec = +7.9228162514264337593543950335m;

        d.Mmin = decimal.MinValue;
        d.Mmax = decimal.MaxValue;
        d.Dmin = double.MinValue;
        d.Dmax = double.MaxValue;
        d.DminDec = -double.Epsilon;
        d.DmaxDec = double.Epsilon;
        d.Dni = double.NegativeInfinity;
        d.Dpi = double.PositiveInfinity;
        d.Dnan = double.NaN;
        d.FminDec = -float.Epsilon;
        d.FmaxDec = float.Epsilon;
        d.Fni = float.NegativeInfinity;
        d.Fpi = float.PositiveInfinity;
        d.Fnan = float.NaN;
        d.Lmin = long.MinValue;
        d.Lmax = long.MaxValue;
        d.ULmax = ulong.MaxValue;
        d.Imin = int.MinValue;
        d.Imax = int.MaxValue;
        d.UImax = uint.MaxValue;


        string s = JSON.ToNiceJSON(d);
        Console.WriteLine(s);
        DigitLimit o = JSON.ToObject<DigitLimit>(s);


        //ok
        Assert.AreEqual(d.Dmax, o.Dmax);
        Assert.AreEqual(d.DmaxDec, o.DmaxDec);
        Assert.AreEqual(d.Dmin, o.Dmin);
        Assert.AreEqual(d.DminDec, o.DminDec);
        Assert.AreEqual(d.Dnan, o.Dnan);
        Assert.AreEqual(d.Dni, o.Dni);
        Assert.AreEqual(d.Dpi, o.Dpi);
        Assert.AreEqual(d.FmaxDec, o.FmaxDec);
        Assert.AreEqual(d.FminDec, o.FminDec);
        Assert.AreEqual(d.Fnan, o.Fnan);
        Assert.AreEqual(d.Fni, o.Fni);
        Assert.AreEqual(d.Fpi, o.Fpi);
        Assert.AreEqual(d.Imax, o.Imax);
        Assert.AreEqual(d.Imin, o.Imin);
        Assert.AreEqual(d.Lmax, o.Lmax);
        Assert.AreEqual(d.Lmin, o.Lmin);
        Assert.AreEqual(d.Mmax, o.Mmax);
        Assert.AreEqual(d.Mmin, o.Mmin);
        Assert.AreEqual(d.UImax, o.UImax);
        Assert.AreEqual(d.ULmax, o.ULmax);

        Assert.AreEqual(d.Fmax, o.Fmax);
        Assert.AreEqual(d.Fmin, o.Fmin);
        Assert.AreEqual(d.MmaxDec, o.MmaxDec);
        Assert.AreEqual(d.MminDec, o.MminDec);
    }


    public class TestData
    {
        [System.Runtime.Serialization.DataMember(Name = "foo")]
        //[Json5Core.DataMember(Name = "foo")]
        public string Foo { get; set; }

        //[System.Runtime.Serialization.DataMember(Name = "bar")]
        public string Bar { get; set; }
    }

    [Test]
    public static void ConvertTest()
    {
        TestData data = new TestData
        {
            Foo = "foo_value",
            Bar = "bar_value"
        };
        string jsonData = JSON.ToJSON(data);
        Console.WriteLine(jsonData);

        TestData data2 = JSON.ToObject<TestData>(jsonData);

        // OK, since data member name is "foo" which is all in lower case
        Assert.AreEqual(data.Foo, data2.Foo);

        // Fails, since data member name is "Bar", but the library looks for "bar" when setting the value
        Assert.AreEqual(data.Bar, data2.Bar);
    }


    public class test { public string name = "me"; }
    [Test]
    public static void ArrayOfObjectExtOff()
    {
        string s = JSON.ToJSON(new test[] { new test(), new test() }, new JSONParameters { UseExtensions = false });
        test[] o = JSON.ToObject<test[]>(s);
        Console.WriteLine(o.GetType().ToString());
        Assert.AreEqual(typeof(test[]), o.GetType());
    }
    [Test]
    public static void ArrayOfObjectsWithoutTypeInfoToObjectTyped()
    {
        string s = JSON.ToJSON(new test[] { new test(), new test() });
        test[] o = JSON.ToObject<test[]>(s);
        Console.WriteLine(o.GetType().ToString());
        Assert.AreEqual(typeof(test[]), o.GetType());
    }
    [Test]
    public static void ArrayOfObjectsWithTypeInfoToObject()
    {
        string s = JSON.ToJSON(new test[] { new test(), new test() }, new JSONParameters() { UseExtensions = true });
        Console.WriteLine(s);
        object o = JSON.ToObject(s);
        Console.WriteLine(o.GetType().ToString());
        List<object> i = o as List<object>;
        Assert.AreEqual(typeof(test), i[0].GetType());
    }

    public class nskeys
    {
        public string name;
        public int age;
        public string address;
    }
    [Test]
    public static void NonStandardKey()
    {
        //var s = "{\"name\":\"m:e\", \"age\":42, \"address\":\"here\"}";
        //var o = JSON.ToObject<nskeys>(s);


        string s = "{name:\"m:e\", age   \t:42, \"address\":\"here\"}";
        nskeys o = JSON.ToObject<nskeys>(s, new JSONParameters { UseExtensions = true });
        //Console.WriteLine("t1");
        Assert.AreEqual("m:e", o.name);
        Assert.AreEqual("here", o.address);
        Assert.AreEqual(42, o.age);

        s = "{name  \t  :\"me\", age : 42, address  :\"here\"}";
        o = JSON.ToObject<nskeys>(s, new JSONParameters { UseExtensions = true });
        //Console.WriteLine("t2");
        Assert.AreEqual("me", o.name);
        Assert.AreEqual("here", o.address);
        Assert.AreEqual(42, o.age);

        s = "{    name   :\"me\", age : 42, address :    \"here\"}";
        o = JSON.ToObject<nskeys>(s, new JSONParameters { UseExtensions = true });
        //Console.WriteLine("t3");
        Assert.AreEqual("me", o.name);
        Assert.AreEqual("here", o.address);
        Assert.AreEqual(42, o.age);
    }

    public class cis
    {
        public string age;
    }

    [Test]
    public static void ConvertInt2String()
    {
        string s = "{\"age\":42}";
        cis o = JSON.ToObject<cis>(s);
    }

    [Test]
    public static void dicofdic()
    {
        string s = "{ 'Section1' : { 'Key1' : 'Value1', 'Key2' : 'Value2', 'Key3' : 'Value3', 'Key4' : 'Value4', 'Key5' : 'Value5' } }".Replace("\'", "\"");
        Dictionary<string, Dictionary<string, string>> o = JSON.ToObject<Dictionary<string, Dictionary<string, string>>>(s);
        Dictionary<string, string> v = o["Section1"];

        Assert.AreEqual(5, v.Count);
        Assert.AreEqual("Value2", v["Key2"]);
    }

    public class readonlyProps
    {
        public List<string> Collection { get; }

        public readonlyProps(List<string> collection)
        {
            Collection = collection;
        }

        public readonlyProps()
        {
        }
    }

    [Test]
    public static void ReadOnlyProperty() // rbeurskens 
    {
        readonlyProps dto = new readonlyProps(new List<string> { "test", "test2" });

        JSON.Parameters.ShowReadOnlyProperties = true;
        string s = JSON.ToJSON(dto);
        readonlyProps o = JSON.ToObject<readonlyProps>(s);

        Assert.IsNotNull(o);
        CollectionAssert.AreEqual(dto.Collection, o.Collection);
    }

    public class nsb
    {
        public bool one = false; // number 1
        public bool two = false; // string 1
        public bool three = false; // string true
        public bool four = false; // string on
        public bool five = false; // string yes
    }
    [Test]
    public static void NonStrictBoolean()
    {
        string s = "{'one':1,'two':'1','three':'true','four':'on','five':'yes'}".Replace("\'", "\"");

        nsb o = JSON.ToObject<nsb>(s);
        Assert.AreEqual(true, o.one);
        Assert.AreEqual(true, o.two);
        Assert.AreEqual(true, o.three);
        Assert.AreEqual(true, o.four);
        Assert.AreEqual(true, o.five);
    }

    private class npc
    {
        public int a = 1;
        public int b = 2;
    }
    [Test]
    public static void NonPublicClass()
    {
        npc p = new npc();
        p.a = 10;
        p.b = 20;
        string s = JSON.ToJSON(p);
        npc o = (npc)JSON.ToObject(s);
        Assert.AreEqual(10, o.a);
        Assert.AreEqual(20, o.b);
    }

    public class Item
    {
        public int Id { get; set; }
        public string Data { get; set; }
    }

    public class TestObject
    {
        public int Id { get; set; }
        public string Stuff { get; set; }
        public virtual ObservableCollection<Item> Items { get; set; }
    }


    [Test]
    public static void noncapacitylist()
    {
        TestObject testObject = new TestObject
        {
            Id = 1,
            Stuff = "test",
            Items = new ObservableCollection<Item>()
        };

        testObject.Items.Add(new Item { Id = 1, Data = "Item 1" });
        testObject.Items.Add(new Item { Id = 2, Data = "Item 2" });

        string jsonData = Json5Core.JSON.ToNiceJSON(testObject);
        Console.WriteLine(jsonData);

        TestObject copyObject = new TestObject();
        Json5Core.JSON.FillObject(copyObject, jsonData);
    }

    [Test]
    public static void Dates()
    {
        string s = "\"2018-09-01T09:38:27\"";

        DateTime d = JSON.ToObject<DateTime>(s, new JSONParameters { UseUTCDateTime = false, UseExtensions = true });

        Assert.AreEqual(9, d.Hour);
    }

    [Test]
    public static void diclistdouble()
    {
        Dictionary<int, List<double>> d = new Dictionary<int, List<double>>();
        d.Add(1, new List<double> { 1.1, 2.2, 3.3 });
        d.Add(2, new List<double> { 4.4, 5.5, 6.6 });
        string s = JSON.ToJSON(d, new JSONParameters { UseExtensions = false });

        Dictionary<int, List<double>> o = JSON.ToObject<Dictionary<int, List<double>>>(s, new JSONParameters { AutoConvertStringToNumbers = true, UseExtensions = true });

        Assert.AreEqual(2, o.Count);
        Assert.AreEqual(1.1, o[1][0]);
    }

    [Test]
    public static void dicarraydouble()
    {
        Dictionary<int, double[]> d = new Dictionary<int, double[]>();
        d.Add(1, new List<double> { 1.1, 2.2, 3.3 }.ToArray());
        d.Add(2, new List<double> { 4.4, 5.5, 6.6 }.ToArray());
        string s = JSON.ToJSON(d, new JSONParameters { UseExtensions = false });
        Console.WriteLine(s);

        Dictionary<int, double[]> o = JSON.ToObject<Dictionary<int, double[]>>(s, new JSONParameters { AutoConvertStringToNumbers = true, UseExtensions = true });

        Assert.AreEqual(2, o.Count);
        Assert.AreEqual(1.1, o[1][0]);
    }

    public class nt
    {
        public int a;
    }


    [Test]
    public static void numberchecks()
    {
        string s = "{'a':+1234567}".Replace("'", "\"");
        nt o = JSON.ToObject<nt>(s);
        Assert.AreEqual(1234567L, o.a);

        s = "{'a':-1234567}".Replace("'", "\"");
        o = JSON.ToObject<nt>(s);
        Assert.AreEqual(-1234567L, o.a);
    }

    public class rofield
    {
        public static readonly int age = 10;
        public string name = "a";
    }

    [Test]
    public static void readonlyfield()
    {
        rofield o = new rofield();

        string s = JSON.ToJSON(o, new JSONParameters { ShowReadOnlyProperties = false, UseExtensions = true });
        Console.WriteLine(s);
        Assert.False(s.Contains("age"));

        s = JSON.ToJSON(o, new JSONParameters { ShowReadOnlyProperties = true, UseExtensions = true });
        Console.WriteLine(s);
        Assert.True(s.Contains("age"));
    }

    [Test]
    public static void intarr()
    {
        int[] o = JSON.ToObject<int[]>("[1,2,-3]");
        Assert.AreEqual(o[2], -3);
    }


    public class Circle
    {
        public Point Center { get; set; }
        public int Radius { get; set; }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point() { X = Y = 0; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (obj is Point p) return p.X == X && p.Y == Y;
            return false;
        }

        public override int GetHashCode()
        {
            return X + Y;//.GetHashCode() * 23 + Y.GetHashCode() * 17;
        }
    }

    [Test]
    public static void refchecking1()
    {
        Point p = new Point(0, 1);
        Circle[] circles = new Circle[]
        {
            new Circle() { Center = new Point(0, 0), Radius = 1 },
            new Circle() { Center = p, Radius = 2 },
            new Circle() { Center = p, Radius = 3 }
        };
        JSONParameters jp = new JSONParameters { OverrideObjectHashCodeChecking = true, UseExtensions = true };
        string json = JSON.ToNiceJSON(circles);//, jp);
        Console.WriteLine(json);
        Circle[] oc = JSON.ToObject<Circle[]>(json, jp);
        Assert.AreEqual(3, oc.Length);
        Assert.AreEqual(oc[2].Center.Y, 1);
    }
    [Test]
    public static void refchecking2()
    {
        Circle[] circles = new Circle[]
        {
            new Circle() { Center = new Point(0, 0), Radius = 1 },
            new Circle() { Center = new Point(0, 1), Radius = 2 },
            new Circle() { Center = new Point(0, 1), Radius = 3 }
        };
        JSONParameters jp = new JSONParameters { OverrideObjectHashCodeChecking = true, InlineCircularReferences = true, UseExtensions = true };

        string json = JSON.ToNiceJSON(circles, jp);
        Console.WriteLine(json);
        Circle[] oc = JSON.ToObject<Circle[]>(json, jp);
        Assert.AreEqual(3, oc.Length);
        Assert.AreEqual(oc[2].Center.Y, 1);
    }

    [Test]
    public static void HackTest()
    {
        //        var s = @"{'$type':'System.Configuration.Install.AssemblyInstaller,System.Configuration.Install, Version=4.0.0.0,culture=neutral,PublicKeyToken=b03f5f7f11d50a3a',
        //'Path':'file:///"
        //.Replace("\'", "\"") + typeof(JSON).Assembly.Location.Replace("\\","/") + "\"}";
        string s = @"{
    '$types':{
        'System.Windows.Data.ObjectDataProvider, PresentationFramework, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = 31bf3856ad364e35':'1',
        'System.Diagnostics.Process, System, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089':'2',
        'System.Diagnostics.ProcessStartInfo, System, Version = 4.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089':'3'
    },
    '$type':'1',
    'ObjectInstance':{
        '$type':'2',
        'StartInfo':{
            '$type':'3',
            'FileName':'cmd',
            'Arguments':'/c notepad hacked'
        }
    },
    'MethodName':'Start'
}".Replace("'", "\"");

        bool fail = false;
        try
        {
            object o = JSON.ToObject(s, new JSONParameters { BadListTypeChecking = true, UseExtensions = true });
            Console.WriteLine(o.GetType().Name);
            //Assert.AreEqual(o.GetType().Name, "");
            fail = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            //Assert.Pass();
        }
        if (fail)
            Assert.Fail();
    }

    [Test]
    public static void TestNull()
    {
        NullTestClass o1 = new NullTestClass();
        o1.Test = null;
        string s = JSON.ToJSON(o1, new JSONParameters() { UseExtensions = true });
        Console.WriteLine(s);
        NullTestClass o2 = JSON.ToObject<NullTestClass>(s);
        Assert.AreEqual(o1.Test, o2.Test);
    }

    public class NullTestClass
    {
        public object Test
        {
            get; set;
        }

        public NullTestClass()
        {
            this.Test = new object();
        }
    }

    [Test]
    public static void json5_non_leading_zero_decimal()
    {
        string s = "{'a':.314}".Replace("'", "\"");
        Dictionary<string, object> o = (Dictionary<string,object>)JSON.Parse(s);
        Assert.AreEqual(0.314, (decimal)o["a"]);

        s = "{'a':-.314}".Replace("'", "\"");
        o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(-0.314, (decimal)o["a"]);

        s = "{'a':0.314}".Replace("'", "\"");
        o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(0.314, (decimal)o["a"]);
    }

    [Test]
    public static void json5_trailing_dot_decimal()
    {
        string s = "{'a':314.}".Replace("'", "\"");
        Dictionary<string, object> o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(314, (decimal)o["a"]);
    }

    [Test]
    public static void json5_infinity()
    {
        Assert.AreEqual(double.PositiveInfinity, JSON.ToObject<double>("Infinity"));
        Assert.AreEqual(double.PositiveInfinity, JSON.ToObject<double>("+Infinity"));
        Assert.AreEqual(double.NegativeInfinity, JSON.ToObject<double>("-Infinity"));

        string s = "{'a':Infinity,'b':1,}".Replace("'", "\"");
        Dictionary<string, object> o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(double.PositiveInfinity, (double)o["a"]);
        Assert.AreEqual(1, (long)o["b"]);

        s = "{'a':+Infinity,'b':1,}".Replace("'", "\"");
        o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(double.PositiveInfinity, (double)o["a"]);
        Assert.AreEqual(1, (long)o["b"]);

        s = "{'a':-Infinity,'b':1,}".Replace("'", "\"");
        o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(double.NegativeInfinity, (double)o["a"]);
        Assert.AreEqual(1, (long)o["b"]);
    }

    [Test]
    public static void json5_trailing_comma()
    {
        string s = "[1,2,3,]";
        List<object> o = (List<object>)JSON.Parse(s);
        Assert.AreEqual(3, o.Count);

        s = "{'a':1, 'b':2, 'c': 3,}".Replace("'", "\"");
        Dictionary<string, object> oo = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(3, oo.Count);
    }

    [Test]
    public static void json5_nan()
    {
        Assert.AreEqual(double.NaN, JSON.ToObject<double>("NaN"));
        Assert.AreEqual(double.NaN, JSON.ToObject<double>("+NaN"));
        Assert.AreEqual(double.NaN, JSON.ToObject<double>("-NaN"));
        string s = "{'a':-NaN,'b':1,}".Replace("'", "\"");
        Dictionary<string, object> o = (Dictionary<string, object>)JSON.Parse(s);
        Console.WriteLine(o["a"]);
        Assert.AreEqual(double.NaN, (double)o["a"]);
        Assert.AreEqual(1, (long)o["b"]);
    }

    [Test]
    public static void json5_comments()
    {
        object oo = JSON.ToObject("/*comment*/null");
        string s = @"{
// comment
    'a' : /*hello
*/ 1,
'b':2,
}
".Replace("'", "\"");
        Dictionary<string, object> o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(1, (long)o["a"]);
        Assert.AreEqual(2, (long)o["b"]);
    }

    [Test]
    public static void json5_hex_numbers()
    {
        Assert.AreEqual(0xff, JSON.ToObject<long>("0xff"));
        Assert.AreEqual(0xffff, JSON.ToObject<long>("0xffff"));
        Assert.AreEqual(0xffffff, JSON.ToObject<long>("0xffffff"));
        Assert.AreEqual(0xffffffff, JSON.ToObject<long>("0xffffffff"));
        Assert.AreEqual(0x12345678, JSON.ToObject<long>("0x12345678"));
        string s = @"{
// comment
    'a' : /*hello
*/ 0x11,
'b': 0XFF2,
}
".Replace("'", "\"");
        Dictionary<string, object> o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(0x11, (long)o["a"]);
        Assert.AreEqual(0xFF2, (long)o["b"]);
    }

    [Test]
    public static void json5_single_double_strings()
    {
        Assert.AreEqual("non escaped normal", JSON.Parse("\"non escaped normal\""));
        Assert.AreEqual("non escaped normal - don't", JSON.Parse("\"non escaped normal - don't\""));

        Assert.AreEqual("non escaped single", JSON.Parse("'non escaped single'"));
        Assert.AreEqual("non escaped single \"", JSON.Parse("'non escaped single \"'"));

        Assert.AreEqual("escaped single \"with double inside\"", JSON.Parse("'escaped single \"with double inside\"'"));
        Assert.AreEqual("escaped single 'with single inside'", JSON.Parse("'escaped single \\\'with single inside\\\''"));
        Assert.AreEqual("don't", JSON.Parse("\"don't\"")); // "don't"
        Assert.AreEqual("don't", JSON.Parse(@"'don\'t'")); // 'don\'t'
        string s = @"{
                   // comment
                   'a' : /*hello
                          */ 0x11,
                   'b': 0XFF2,
                   'c': 'hello there'
                  }";
        Dictionary<string, object> o = (Dictionary<string, object>)JSON.Parse(s);
        Assert.AreEqual(0x11, (long)o["a"]);
        Assert.AreEqual(0xFF2, (long)o["b"]);
        Assert.AreEqual("hello there", (string)o["c"]);

        //Assert.Fail();
    }

    [Test]
    public static void json5_string_escapes()
    {
        Assert.AreEqual("AC/DC", JSON.Parse(@"'\A\C\/\D\C'"));
        //Assert.AreEqual("123456789", JSON.Parse(@"'\1\2\3\4\5\6\7\8\9'"));
    }

    [Test]
    public static void json5_string_breaks()
    {
        string s = @"'this is a cont\
inuous line.\
'";
        object ss = JSON.Parse(s);
        Console.WriteLine(ss);
        Assert.AreEqual("this is a continuous line.", ss);

        s = @"'abc\
   message'";
        Assert.AreEqual("abc   message", JSON.Parse(s));

        try
        {
            JSON.Parse(@"'
hello
there
'");
            Assert.Fail();
        }
        catch (Exception ex)
        {
            Assert.AreEqual(typeof(Exception), ex.GetType());
            Assert.AreEqual("Illegal newline character in string at index 1", ex.Message);
        }
        //Assert.Fail();
    }

    public struct sstruct
    {
        public static int num1 { get; set; }
        public static int num2;
    }

    [Test]
    public static void structstaticproperty()
    {
        sstruct o = new sstruct();        

        string s = JSON.ToJSON(o);
        Console.WriteLine(s);
    }

    [Test]
    public static void UTCDateTrue()
    {
        DateTime dt = new DateTime(2021, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        string js = JSON.ToJSON(dt);
        Console.WriteLine(js);
        Assert.AreEqual(12 , JSON.ToObject<DateTime>(js, new JSONParameters() { UseUTCDateTime = true }).Hour);
    }
    [Test]
    public static void UTCDateFalse()
    {
        DateTime dt = new DateTime(2021, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        string js = JSON.ToJSON(dt);
        Console.WriteLine(js);
		DateTime dt2 = DateTime.SpecifyKind(dt.ToLocalTime(), DateTimeKind.Utc);
        Assert.AreEqual(dt2.Hour, JSON.ToObject<DateTime>(js, new JSONParameters() { UseUTCDateTime = false }).Hour);
    }
    //[Test]
    //public static void ma()
    //{
    //    var a = new int[2, 3] { { 1, 2, 3 },{ 4, 5, 6 } };
    //    //var b = new int[2][3];//{ { 1, 2, 3 }, { 4, 5, 6 } };

    //    Console.WriteLine(a.Rank);
    //    Console.WriteLine(a.GetLength(0));
    //    Console.WriteLine(a.GetLength(1));

    //    var s = JSON.ToJSON(a);
    //    Console.WriteLine(s);
    //    var o = JSON.ToObject<int[,]>(s);

    //}

    //public class WordEntry
    //{
    //    public List<Guid> Class { set; get; }
    //    public List<int> EdgePaths { set; get; }
    //    public List<int> RelatedWords { set; get; }
    //    public String Word { set; get; }
    //    public bool Plural { set; get; }
    //    //public Tense TenseState { set; get; }
    //    public Guid RootForm { set; get; }
    //    public Guid ID { set; get; }
    //    public Single UseFrequency { set; get; }
    //    public List<int> PartsofSpeech { set; get; }
    //}

    //[Test]
    //public static void emptylist()
    //{
    //    var s = "{ 'Class': ['K2JFO+FwG0CfeuTFE283AQ=='], 'EdgePaths': [-1537686140], 'RelatedWords': [], 'Word': 'Tum-ti-tum', 'Plural': false, 'TenseState': 'present', 'RootForm': 'AAAAAAAAAAAAAAAAAAAAAA==', 'ID': '78LPEHC0wkiQQu6DvX9wzQ==', 'UseFrequency': 0, 'PartsofSpeech': [] }";

    //    var o = JSON.ToObject<WordEntry>(s.Replace("\'","\""));

    //}

    //public static void paramobjfunc()
    //{
    //    var str = "";
    //    var o = JSON.ToObject(str, new JSONParameters
    //    {
    //        CreateParameterConstructorObject = (t) =>
    //        {
    //            if (t == typeof(NullTestClass))
    //                return new NullTestClass();
    //            else return null;
    //        }
    //    });
    //}


    //[Test]
    //public static void autoconvtest()
    //{
    //    var j = JSON.ToObject<int>("\"G\"", new JSONParameters { AutoConvertStringToNumbers = false });
    //    var i = JSON.ToObject<Item>("{\"Id\":\"G\"}", new JSONParameters { AutoConvertStringToNumbers = false });
    //}

    //tests from https://github.com/json5/json5/blob/32bb2cdae4864b2ac80a6d9b4045efc4cc54f47a/test/parse.js:
    //they are licensed under https://github.com/json5/json5/blob/32bb2cdae4864b2ac80a6d9b4045efc4cc54f47a/LICENSE.md for their usage and the license of this project for their implementation
    [Test]
    public static void json5_additional_tests_1()
    {
        JSONParameters old = JSON.Parameters;
        JSON.Parameters = new JSONParameters();

        try
        {
            Assert.AreEqual("{}", JSON.ToJSON(JSON.Parse("{}")));
            Console.WriteLine("parses empty objects");

            Assert.AreEqual("{\"a\":1}", JSON.ToJSON(JSON.Parse("{\"a\":1}")));
            Console.WriteLine("parses double string property names");

            Assert.AreEqual("{\"a\":1}", JSON.ToJSON(JSON.Parse("{'a':1}")));
            Console.WriteLine("parses single string property names");

            Assert.AreEqual("{\"a\":1}", JSON.ToJSON(JSON.Parse("{a:1}")));
            Console.WriteLine("parses unquoted property names");

            Assert.AreEqual("{\"$_\":1,\"_$\":2,\"a\u200C\":3}", JSON.ToJSON(JSON.Parse("{$_:1,_$:2,a\u200C:3}")));
            Console.WriteLine("parses special character property names");

            Assert.AreEqual("{\"ùńîċõďë\":9}", JSON.ToJSON(JSON.Parse("{ùńîċõďë:9}")));
            Console.WriteLine("parses unicode property names");

            Assert.AreEqual("{\"ab\":1,\"$_\":2,\"_$\":3}", JSON.ToJSON(JSON.Parse("{\\u0061\\u0062:1,\\u0024\\u005F:2,\\u005F\\u0024:3}")));
            Console.WriteLine("parses escaped property names");

            Assert.AreEqual("{\"abc\":1,\"def\":2}", JSON.ToJSON(JSON.Parse("{abc:1,def:2}")));
            Console.WriteLine("parses multiple properties");

            Assert.AreEqual("{\"a\":{\"b\":2}}", JSON.ToJSON(JSON.Parse("{a:{b:2}}")));
            Console.WriteLine("parses nested objects");

            Assert.AreEqual("[]", JSON.ToJSON(JSON.Parse("[]")));
            Console.WriteLine("parses empty arrays");

            Assert.AreEqual("[1]", JSON.ToJSON(JSON.Parse("[1]")));
            Console.WriteLine("parses array values");

            Assert.AreEqual("[1,2]", JSON.ToJSON(JSON.Parse("[1,2]")));
            Console.WriteLine("parses multiple array values");

            Assert.AreEqual("[1,[2,3]]", JSON.ToJSON(JSON.Parse("[1,[2,3]]")));
            Console.WriteLine("parses nested arrays");

            Assert.AreEqual("null", JSON.ToJSON(JSON.Parse("null")));
            Console.WriteLine("parses nulls");

            Assert.AreEqual("true", JSON.ToJSON(JSON.Parse("true")));
            Console.WriteLine("parses true");

            Assert.AreEqual("false", JSON.ToJSON(JSON.Parse("false")));
            Console.WriteLine("parses false");

            Assert.AreEqual("[0,0,0]", JSON.ToJSON(JSON.Parse("[0,0.,0e0]")));
            Console.WriteLine("parses leading zeroes");

            Assert.AreEqual("[1,23,456,7890]", JSON.ToJSON(JSON.Parse("[1,23,456,7890]")));
            Console.WriteLine("parses integers");

            Assert.AreEqual("[-1,2,-0.1,-0]", JSON.ToJSON(JSON.Parse("[-1,+2,-.1,-0]")));
            Console.WriteLine("parses signed numbers");

            Assert.AreEqual("[0.1,0.23]", JSON.ToJSON(JSON.Parse("[.1,.23]")));
            Console.WriteLine("parses leading decimal points");

            Assert.AreEqual("[1.0,1.23]", JSON.ToJSON(JSON.Parse("[1.0,1.23]")));
            Console.WriteLine("parses fractional numbers");

            Assert.AreEqual("[1,10,10,1,1.1,0.1,10]", JSON.ToJSON(JSON.Parse("[1e0,1e1,1e01,1.e0,1.1e0,1e-1,1e+1]")));
            Console.WriteLine("parses exponents");

            Assert.AreEqual("[1,16,255,255]", JSON.ToJSON(JSON.Parse("[0x1,0x10,0xff,0xFF]")));
            Console.WriteLine("parses hexadecimal numbers");

            Assert.AreEqual("[Infinity,-Infinity]", JSON.ToJSON(JSON.Parse("[Infinity,-Infinity]")));
            Console.WriteLine("parses signed and unsigned Infinity");

            Assert.AreEqual("NaN", JSON.ToJSON(JSON.Parse("NaN")));
            Console.WriteLine("parses NaN");

            Assert.AreEqual("NaN", JSON.ToJSON(JSON.Parse("-NaN")));
            Console.WriteLine("parses signed NaN");

            Assert.AreEqual("1", JSON.ToJSON(JSON.Parse("1")));
            Console.WriteLine("parses 1");

            Assert.AreEqual("1.23E+100", JSON.ToJSON(JSON.Parse("+1.23e100"))); //changed
            Console.WriteLine("parses +1.23e100");

            Assert.AreEqual("1", JSON.ToJSON(JSON.Parse("0x1"))); //changed
            Console.WriteLine("parses bare hexadecimal number");

            Assert.AreEqual("-1375488932539311409843695", JSON.ToJSON(JSON.Parse("-0x0123456789abcdefABCDEF"))); //changed
            Console.WriteLine("parses bare long hexadecimal number");

            Assert.AreEqual("\"abc\"", JSON.ToJSON(JSON.Parse("\"abc\"")));
            Console.WriteLine("parses double quoted strings");

            Assert.AreEqual("\"abc\"", JSON.ToJSON(JSON.Parse("'abc'")));
            Console.WriteLine("parses single quoted strings");

            Assert.AreEqual("[\"\\\"\",\"'\"]", JSON.ToJSON(JSON.Parse("['\"',\"'\"]")));
            Console.WriteLine("parses quotes in strings");

            Assert.AreEqual("\b\f\n\r\t\v\0\x0f\u01FF\a'\"", JSON.Parse("'\\b\\f\\n\\r\\t\\v\\0\\x0f\\u01fF\\\n\\\r\n\\\r\\\u2028\\\u2029\\a\\'\\\"'"));
            Console.WriteLine("parses escaped characters");

            Assert.AreEqual("\"\\b\\f\\n\\r\\t\\v\\0\\u000F\u01ff\\a'\\\"\"", JSON.ToJSON(JSON.Parse("'\\b\\f\\n\\r\\t\\v\\0\\x0f\\u01fF\\\n\\\r\n\\\r\\\u2028\\\u2029\\a\\'\\\"'")));
            Console.WriteLine("parses escaped characters (pt. 2)");

            List<string> warnings = new List<string>();
            Assert.AreEqual("\"\\u2028\\u2029\"", JSON.ToJSON(JSON.Parse("'\u2028\u2029'", warnings)));
            Assert.AreEqual(new[] { "Warning: invalid ECMAScript at index 1 with character \\u2028 in string.", "Warning: invalid ECMAScript at index 2 with character \\u2029 in string." }, warnings);
            Console.WriteLine("parses line and paragraph separators with a warning");

            Assert.AreEqual("{}", JSON.ToJSON(JSON.Parse("{//comment\n}")));
            Console.WriteLine("parses single-line comments");

            Assert.AreEqual("{}", JSON.ToJSON(JSON.Parse("{}//comment")));
            Console.WriteLine("parses single-line comments at end of input");

            Assert.AreEqual("{}", JSON.ToJSON(JSON.Parse("{/*comment\n** */}")));
            Console.WriteLine("parses multi-line comments");

            Assert.AreEqual("{}", JSON.ToJSON(JSON.Parse("{\t\v\f \u00A0\uFEFF\n\r\u2028\u2029\u2003}")));
            Console.WriteLine("parses whitespace");
        }
        finally
        {
            JSON.Parameters = old;
        }
    }

    private static void AssertException(Type expectedType, string expectedMessage, Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (ex?.GetType() != expectedType) throw;
            if (ex?.Message != expectedMessage) throw;
            return;
        }
        throw new Exception("No error was thrown!");
    }

    //tests from https://github.com/json5/json5/blob/32bb2cdae4864b2ac80a6d9b4045efc4cc54f47a/test/errors.js:
    //they are licensed under https://github.com/json5/json5/blob/32bb2cdae4864b2ac80a6d9b4045efc4cc54f47a/LICENSE.md for their usage and the license of this project for their implementation
    [Test]
    public static void json5_additional_tests_2()
    {
        JSONParameters old = JSON.Parameters;
        JSON.Parameters = new JSONParameters();

        try
        {
            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse(""));
            Console.WriteLine("throws on empty documents");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("//a"));
            Console.WriteLine("throws on documents with only comments");

            AssertException(typeof(Exception), "Illegal comment declaration at index 0", () => JSON.Parse("/a"));
            Console.WriteLine("throws on incomplete single line comments");

            AssertException(typeof(Exception), "Unfinished comment declaration starting at index 0", () => JSON.Parse("/*"));
            Console.WriteLine("throws on unterminated multiline comments");

            AssertException(typeof(Exception), "Unfinished comment declaration starting at index 0", () => JSON.Parse("/**"));
            Console.WriteLine("throws on unterminated multiline comment closings");

            AssertException(typeof(Exception), "Invalid token at index 0", () => JSON.Parse("a"));
            Console.WriteLine("throws on invalid characters in values");

            AssertException(typeof(Exception), "Invalid escape '\\a' in IdentifierName at index 2", () => JSON.Parse("{\\a:1}"));
            Console.WriteLine("throws on invalid characters in identifier start escapes");

            AssertException(typeof(Exception), "Invalid starting character '\\u0021' in IdentifierName at index 2", () => JSON.Parse("{\\u0021:1}"));
            Console.WriteLine("throws on invalid identifier start characters");

            AssertException(typeof(Exception), "Expected colon at index 3", () => JSON.Parse("{a\\a:1}"));
            Console.WriteLine("throws on invalid characters in identifier continue escapes");

            AssertException(typeof(Exception), "Invalid continuation character '\\u0021' in IdentifierName at index 3", () => JSON.Parse("{a\\u0021:1}"));
            Console.WriteLine("throws on invalid identifier continue characters");

            AssertException(typeof(Exception), "Unexpected character 'a' at index 1", () => JSON.Parse("-a"));
            Console.WriteLine("throws on invalid characters following a sign");

            AssertException(typeof(Exception), "Unexpected character 'a' at index 1", () => JSON.Parse(".a"));
            Console.WriteLine("throws on invalid characters following a leading decimal point");

            AssertException(typeof(Exception), "Unexpected character 'a' at index 2", () => JSON.Parse("1ea"));
            Console.WriteLine("throws on invalid characters following an exponent indicator");

            AssertException(typeof(Exception), "Unexpected character 'a' at index 3", () => JSON.Parse("1e-a"));
            Console.WriteLine("throws on invalid characters following an exponent sign");

            AssertException(typeof(Exception), "Unexpected character 'g' at index 2", () => JSON.Parse("0xg"));
            Console.WriteLine("throws on invalid characters following a hexadecimal indicator");

            AssertException(typeof(Exception), "Illegal newline character in string at index 1", () => JSON.Parse("\"\n\""));
            Console.WriteLine("throws on invalid new lines in strings");

            AssertException(typeof(Exception), "Did not reach end of string", () => JSON.Parse("\""));
            Console.WriteLine("throws on unterminated strings");

            AssertException(typeof(Exception), "Invalid character '!' in IdentifierName at index 2", () => JSON.Parse("{!:1}"));
            Console.WriteLine("throws on invalid identifier start characters in property names");

            AssertException(typeof(Exception), "Expected colon at index 2", () => JSON.Parse("{a!1}"));
            Console.WriteLine("throws on invalid characters following a property name");

            AssertException(typeof(Exception), "Missing comma at index 4", () => JSON.Parse("{a:1!}"));
            Console.WriteLine("throws on invalid characters following a property value");

            AssertException(typeof(Exception), "Invalid token at index 2", () => JSON.Parse("[1!]"));
            Console.WriteLine("throws on invalid characters following an array value");

            AssertException(typeof(Exception), "Invalid token at index 0", () => JSON.Parse("tru!"));
            Console.WriteLine("throws on invalid characters in literals");

            AssertException(typeof(Exception), "Did not reach end of string", () => JSON.Parse("\"\\"));
            Console.WriteLine("throws on unterminated escapes");

            AssertException(typeof(Exception), "Invalid hexadecimal character 'g' at index 3", () => JSON.Parse("\"\\xg\""));
            Console.WriteLine("throws on invalid first digits in hexadecimal escapes");

            AssertException(typeof(Exception), "Invalid hexadecimal character 'g' at index 4", () => JSON.Parse("\"\\x0g\""));
            Console.WriteLine("throws on invalid second digits in hexadecimal escapes");

            AssertException(typeof(Exception), "Invalid hexadecimal character 'g' at index 6", () => JSON.Parse("\"\\u000g\""));
            Console.WriteLine("throws on invalid unicode escapes");

            AssertException(typeof(Exception), "Illegal escape \\1 (at index 2)", () => JSON.Parse("\"\\1\""));
            AssertException(typeof(Exception), "Illegal escape \\2 (at index 2)", () => JSON.Parse("\"\\2\""));
            AssertException(typeof(Exception), "Illegal escape \\3 (at index 2)", () => JSON.Parse("\"\\3\""));
            AssertException(typeof(Exception), "Illegal escape \\4 (at index 2)", () => JSON.Parse("\"\\4\""));
            AssertException(typeof(Exception), "Illegal escape \\5 (at index 2)", () => JSON.Parse("\"\\5\""));
            AssertException(typeof(Exception), "Illegal escape \\6 (at index 2)", () => JSON.Parse("\"\\6\""));
            AssertException(typeof(Exception), "Illegal escape \\7 (at index 2)", () => JSON.Parse("\"\\7\""));
            AssertException(typeof(Exception), "Illegal escape \\8 (at index 2)", () => JSON.Parse("\"\\8\""));
            AssertException(typeof(Exception), "Illegal escape \\9 (at index 2)", () => JSON.Parse("\"\\9\""));
            Console.WriteLine("throws on escaped digits other than 0");

            AssertException(typeof(Exception), "Invalid: octal escapes are not allowed (at index 2)", () => JSON.Parse("'\\01'"));
            Console.WriteLine("throws on octal escapes");

            AssertException(typeof(Exception), "Invalid character '2' at index 2", () => JSON.Parse("1 2"));
            Console.WriteLine("throws on multiple values");

            AssertException(typeof(Exception), "Invalid token at index 0", () => JSON.Parse("\x01"));
            Console.WriteLine("throws with control characters escaped in the message");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("{"));
            Console.WriteLine("throws on unclosed objects before property names");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("{a"));
            Console.WriteLine("throws on unclosed objects after property names");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("{a:"));
            Console.WriteLine("throws on unclosed objects before property values");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("{a:1"));
            Console.WriteLine("throws on unclosed objects after property values");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("["));
            Console.WriteLine("throws on unclosed arrays before values");

            AssertException(typeof(Exception), "Reached end of string unexpectedly", () => JSON.Parse("[1"));
            Console.WriteLine("throws on unclosed arrays after values");
        }
        finally
        {
            JSON.Parameters = old;
        }
    }

    [Test]
    public static void json5_additional_tests_3()
    {
        JSONParameters old = JSON.Parameters;
        JSON.Parameters = new JSONParameters();

        try
        {
            AssertException(typeof(Exception), "Unfinished comment declaration starting at index 1", () => JSON.Parse("{/*}"));
            Console.WriteLine("{/*} is illegal");

            AssertException(typeof(Exception), "Illegal comment declaration at index 1", () => JSON.Parse("{/\n}"));
            Console.WriteLine("{/\n} is illegal");

            AssertException(typeof(Exception), "Unfinished comment declaration starting at index 1", () => JSON.Parse("{/**}"));
            Console.WriteLine("{/**} is illegal");

            AssertException(typeof(Exception), "Unfinished number at end of input", () => JSON.Parse("-"));
            Console.WriteLine("- is illegal");

            AssertException(typeof(Exception), "Unexpected character '}' at index 4", () => JSON.Parse("{a:-}"));
            Console.WriteLine("- is illegal in object");

            AssertException(typeof(Exception), "Unfinished number at end of input", () => JSON.Parse("."));
            Console.WriteLine(". is illegal");

            AssertException(typeof(Exception), "Unexpected character '}' at index 4", () => JSON.Parse("{a:.}"));
            Console.WriteLine(". is illegal in object");

            AssertException(typeof(Exception), "Unfinished number at end of input", () => JSON.Parse("0x"));
            Console.WriteLine("0x is illegal");

            AssertException(typeof(Exception), "Unexpected character '}' at index 5", () => JSON.Parse("{a:0x}"));
            Console.WriteLine("0x is illegal in object");

            AssertException(typeof(Exception), "\\x escape sequence ended prematurely (end of input)", () => JSON.Parse("\"\\x\""));
            Console.WriteLine("\"\\x\" is illegal due to the end of the string");

            AssertException(typeof(Exception), "\\u escape sequence ended prematurely (end of input)", () => JSON.Parse("\"\\u\""));
            Console.WriteLine("\"\\u\" is illegal due to the end of the string");

            Assert.AreEqual("\0", JSON.Parse("\"\\0\""));
            Console.WriteLine("Test for \\0");

            Assert.AreEqual("{}", JSON.ToJSON(JSON.Parse("{} ")));
            Console.WriteLine("Test for {}SPACE");

            Assert.AreEqual("-0", JSON.ToJSON(JSON.Parse("-0x0")));
            Console.WriteLine("Test for -0x0 --> -0");

            Assert.AreEqual(
#if NETFRAMEWORK || NET4 || NETCOREAPP && !NETCOREAPP3_0_OR_GREATER
                "9.0144042682896313E+28"
#else
                "9.014404268289631E+28"
#endif
                , JSON.ToJSON(JSON.Parse("+0x0123456789abcdefABCDEF0000")));
            Console.WriteLine("Test for long hex number");

			try
			{
				AssertException(typeof(FormatException), "Input string was not in a correct format.", () => JSON.Parse("..2"));
				Console.WriteLine(".. is illegal");
			}
			catch
			{
				AssertException(typeof(FormatException), "The input string '..2' was not in a correct format.", () => JSON.Parse("..2"));
				Console.WriteLine(".. is illegal");
			}

			try
			{
				AssertException(typeof(FormatException), "Input string was not in a correct format.", () => JSON.Parse("{a:..2}"));
				Console.WriteLine(".. is illegal inside of an object");
			}
			catch
			{
				AssertException(typeof(FormatException), "The input string '..2' was not in a correct format.", () => JSON.Parse("{a:..2}"));
				Console.WriteLine(".. is illegal inside of an object");
			}

            Assert.AreEqual(1e41, JSON.Parse("100000000000000000000000000000000000000000"));
            Console.WriteLine("Parses 100000000000000000000000000000000000000000");
        }
        finally
        {
            JSON.Parameters = old;
        }
    }

}// UnitTests.Tests
 //}

