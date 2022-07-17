﻿//------------------------------------------------------------------------------
// <copyright file="ExpressionDrawer.cs">
//     Copyright (c) Adam Wulkiewicz.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicalDebugging
{
    class ExpressionParser
    {
        public static int LoadSize(Debugger debugger, string name)
        {
            Expression expr = debugger.GetExpression(name);
            return expr.IsValidValue
                 ? Math.Max(Util.ParseInt(expr.Value, debugger.HexDisplayMode), 0)
                 : 0;
        }

        public static bool TryLoadInt(Debugger debugger, string name, out int result)
        {
            result = 0;
            Expression expr = debugger.GetExpression(name);
            if (!expr.IsValidValue)
                return false;
            result = Util.ParseInt(expr.Value, debugger.HexDisplayMode);
            return true;
        }

        public static bool TryLoadDouble(Debugger debugger, string name, out double result)
        {
            result = 0.0;
            string castedName = "(double)" + name;
            if (IsLanguageBasic(debugger))
                castedName = "CType(" + name + ", Double)";
            Expression expr = debugger.GetExpression(castedName);
            if (!expr.IsValidValue)
                return false;
            result = Util.ParseDouble(expr.Value);
            return true;
        }

        /*struct AddressDifference
        {
            long Value;
            bool IsValid;
        }*/

        // Valid difference of addresses of variables valName1 and valName2
        // or long.MinValue
        // detect invalid address difference with IsInvalidAddressDifference()
        public static long GetAddressDifference(Debugger debugger, string valName1, string valName2)
        {
            ulong addr1 = GetValueAddress(debugger, valName1);
            ulong addr2 = GetValueAddress(debugger, valName2);
            if (addr1 == 0 || addr2 == 0)
                return long.MinValue;
            return (addr2 >= addr1)
                 ? (long)(addr2 - addr1)
                 : -(long)(addr1 - addr2);
        }

        public static long GetPointerDifference(Debugger debugger, string pointerName1, string pointerName2)
        {
            ulong addr1 = GetPointer(debugger, pointerName1);
            ulong addr2 = GetPointer(debugger, pointerName2);
            if (addr1 == 0 || addr2 == 0)
                return long.MinValue;
            return (addr2 >= addr1)
                 ? (long)(addr2 - addr1)
                 : -(long)(addr1 - addr2);
        }

        public static long InvalidAddressDifference()
        {
            return long.MinValue;
        }

        public static bool IsInvalidAddressDifference(long diff)
        {
            return diff == long.MinValue;
        }

        // C++ and C# only!

        // TODO: C# classes
        // For value-types, structs, etc.
        // "typeof(" + type + ").IsValueType" == "true"
        // "&(" + name + ")" is of type SomeType*
        // - address: "&(" + name + ")"
        // - size: "sizeof(" + type + ")"
        // For non-value-types, e.g. classes
        // "typeof(" + type + ").IsValueType" == "false"
        // "&(" + name + ")" is of type IntPtr*
        // - address: "*(&(" + name + "))"
        // - size: "System.Runtime.InteropServices.Marshal.ReadInt32(typeof(" + type + ").TypeHandle.Value, 4)"
        // - size: "*(((int*)(void*)typeof(" + type + ").TypeHandle.Value) + 1)"

        public static ulong GetPointer(Debugger debugger, string pointerName)
        {
            Expression ptrExpr = debugger.GetExpression("(void*)(" + pointerName + ")");
            if (!ptrExpr.IsValidValue)
                return 0;
            string addr = ptrExpr.Value;

            // NOTE: Hexadecimal value is automatically detected, this is probably not needed.
            // But automatically detect the format just in case of various versions
            // of VS displayed it differently regardless of debugger mode.
            return Util.ParseULong(addr/*, true*/);
        }

        // Valid address of variable valName or 0
        public static ulong GetValueAddress(Debugger debugger, string valName)
        {
            return GetPointer(debugger, "&(" + valName + ")");
        }

        // Valid size or 0
        // NOTE: In C++ the actual byte size depends on CHAR_BIT
        public static int GetValueSizeof(Debugger debugger, string valName)
        {
            string typeName = valName; // In C++ value and type is interchangeable when passed into sizeof
            //if (!IsLanguageCpp(debugger))
            if (IsLanguageCs(debugger)) // Change this when getting address in Basic works
            {
                Expression valExpr = debugger.GetExpression(valName);
                if (!valExpr.IsValidValue)
                    return 0;
                typeName = valExpr.Type;
            }
            return GetTypeSizeof(debugger, typeName);
        }

        // Valid size or 0
        public static int GetTypeSizeof(Debugger debugger, string valType)
        {
            if (IsLanguageBasic(debugger)) // Change this when getting address in Basic works
                //sizeOfStr = "System.Runtime.InteropServices.Marshal.SizeOf(GetType(" + valType + "))";
                return 0;

            string sizeOfStr = "sizeof(" + valType + ")";
            Expression valSizeExpr = debugger.GetExpression(sizeOfStr);
            return valSizeExpr.IsValidValue
                 ? Util.ParseInt(valSizeExpr.Value, debugger.HexDisplayMode)
                 : 0;
        }

        // Valid name or null
        public static string GetValueType(Debugger debugger, string valName)
        {
            Expression valExpr = debugger.GetExpression(valName);
            return valExpr.IsValidValue
                 ? valExpr.Type
                 : null;
        }

        public static bool IsInvalidType(string type)
        {
            return type == null;
        }

        public static bool IsInvalidType(string type1, string type2)
        {
            return type1 == null || type2 == null;
        }

        public static bool IsInvalidSize(int size)
        {
            return size <= 0;
        }

        public static bool IsInvalidSize(int size1, int size2)
        {
            return size1 <= 0 || size2 <= 0;
        }

        public static bool IsInvalidOffset(long size, long offset)
        {
            return ExpressionParser.IsInvalidAddressDifference(offset)
                || offset < 0
                || offset >= size;
        }

        public static bool IsInvalidOffset(long size, long offset1, long offset2)
        {
            return IsInvalidOffset(size, offset1)
                || IsInvalidOffset(size, offset2);
        }

        private static bool IsLanguageCpp(Debugger debugger)
        {
            return debugger.CurrentStackFrame.Language.StartsWith("C+");
        }

        private static bool IsLanguageCs(Debugger debugger)
        {
            return debugger.CurrentStackFrame.Language.StartsWith("C#");
        }

        private static bool IsLanguageBasic(Debugger debugger)
        {
            return debugger.CurrentStackFrame.Language.StartsWith("Ba");
        }
    }
}
