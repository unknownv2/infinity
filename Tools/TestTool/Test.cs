#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NoDev.InfinityToolLib.Tools;

namespace NoDev.Infinity.Tools.TestTool
{
    [Guid("6b2aa52e-1e76-4993-b80f-93a143d7075e")]
    internal partial class Test : Tool
    {
        private const long ConstInt64 = 38;
        private static uint StaticUInt32 = 39;
        private static readonly ushort StaticReadonlyUInt16 = 40;
        private string ClassString = "a string";

        private string PropertyTest
        {
            get { return "replace me"; }
        }

        internal Test()
        {
            InitializeComponent();
        }

        private void BasicTest()
        {
            var t1 = 1;
            var t2 = (byte)2;
            var t3 = (long)3;
            var t4 = 4UL;
            var t5 = "test";
            var t6 = @"verbatim test";
            var t7 = "new\r\nline";
            var t8 = @"multiline
veratim
string";
            const string t9 = "in-line constant string should be removed";
            var t10 = t9;
            var t11 = (IntPtr)(ConstInt64 + 132);
        }

        private void NullableTypeTest()
        {
            var t1 = (uint?)133;
            var t2 = (IntPtr?)134;
        }

        private void OperatorTest()
        {
            var t1 = 5 + 6;
            var t2 = 7 + 8L;
            var t3 = (byte)(9 + 10UL);
            const short t4 = 11;
            var t5 = t4 * 12;
            var t6 = 13L;
            t6 += 14;
            var t7 = -t6;
            var t8 = (15 + t6) * t6 >> (int)(16L + 17);
            var t9 = "concat" + " test";
            var t10 = "=" + t9 + "=";
            var t11 = @"verbatim 
concat" + " test";
            var t12 = "implicit conversion test " + 18;
        }

        private void LambdaTest()
        {
            Func<long, short> t1 = l1 => (short)(l1 - 21);
            Func<byte, ulong> t2 = b1 =>
            {
                var b2 = (b1 + 22) / (b1 * 23);
                return (ulong)b2;
            };
        }

        private void ArrayTest()
        {
            var t1 = new byte[] { 41, 42, 43 };
            var t2 = new uint[] { 44, t1[0], 45 };
            var t3 = new long[2,4]
            {
                { 55, 56, 57, 58 },
                { 59, 60, 61, 62 }
            };
            var t8 = new long[,,]
            {
                {
                    { 82, 83, 84 },
                    { 85, 86, 87 }
                },
                {
                    { 88, 89, 90 },
                    { 91, 92, 93 }
                },
                {
                    { 94, 95, 96 },
                    { 97, 98, 99 }
                },
                {
                    { 100, 101, 102 },
                    { 103, 104, 105 }
                }
            };
            var t4 = t3[63, 64 + 65];
            var t5 = new[]
            {
                new short[] { 71, 72, 73 },
                new short[] { 74, 75, 76 }
            };
            var t6 = new short[77][][];
            var t7 = new ushort[78, 79, 80];
            var t9 = new object[,]
            {
                { "object array", 117, ConstInt64, 118.912 },
                { 119.912f, (decimal)120.912 + 121, null, 122UL }
            };
            var t10 = new object[,]
            {
                { "another array", null },
                { ConstInt64, StaticReadonlyUInt16 }
            };
        }

        private void ObjectCreationTest()
        {
            var t1 = new List<string>(81);
        }

        private void IndexerTest()
        {
            var t1 = new List<long> { 46, 47 };
            var t2 = new Dictionary<string, List<short>>
            {
                { "key1", new List<short> { 48, 49 } },
                { "key2", new List<short> { 50, 51 } }
            };
            var t3 = new object[] { "a string", null, 50, 51L, (short)52 };
            var t4 = t2["key1"][0];
            var t5 = this[66 + 67];
            this[68] = 69 + 70;
        }

        private void LinqTest()
        {
            var t1 = new[] { 106, 107 };
            var t2 =
                from t3 in t1
                where t3 > 108 + 109
                select 110L + 111;
        }

        private void UnaryOperatorTest()
        {
            var t1 = ~112U;
            var t2 = ~(113L ^ 114);
            var t3 = -(~115 + 116U);
        }

        private void TernaryOperatorTest()
        {
            var t1 = (124 + StaticUInt32) == StaticUInt32 ? 125L : 126L;
            var t2 = new object();
            var t3 = t2 ?? 127 + StaticUInt32;
        }

        private void SwitchTest()
        {
            var t1 = 0;

            // why is this even allowed?
            switch (128)
            {
                case 129:
                    t1 = 130;
                    break;
                default:
                    t1 = 131;
                    break;
            }

            switch ((uint)(135 + ConstInt64))
            {
                case 136:
                    return;
            }
        }

        private void UncheckedTest()
        {
            var t1 = unchecked(137L);
            var t2 = unchecked(138 + StaticUInt32);
            var t3 = checked(139 + StaticUInt32);
        }

        private void MethodCallTest()
        {
            TakesAnInt16(19);
            TakesAString(20.ToString());
            TakesALotOfArguments<DateTime>(25, 26, 27, 28, 29, 30, 31, 32, (decimal)33.912, (float)34.912, 35.912, (36 + 37).ToString());
        }

#if DOESNT_EXIST
        private int ShouldRemain = 123;

        private void PrecompilerConditionalTest()
        {
            var t1 = "This should not be parsed.";
        }
#endif

        private short this[long l1]
        {
            get { return (short)(l1 + 53); }
            set { StaticUInt32 = 54; }
        }

        private void TakesAnInt16(short num)
        {
            
        }

        private void TakesAString(string str)
        {
            
        }

        private T TakesALotOfArguments<T>(
            byte b, sbyte sb, short s, ushort us, int i, uint ui, long l, 
            ulong ul, decimal de, float f, double d = 24, string str = "default")
        {
            return default(T);
        }
    }
}
