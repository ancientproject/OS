namespace System
{
    using Globalization;
    using IL2CPU.API.Attribs;
    // TODO, currently support only integer-type numbers and formatting only to hex
    [Plug(TargetName = "System.Number, System.Private.CoreLib")]
    public static class NumberImpl
    {
        public static string FormatInt32(int value, string format, NumberFormatInfo _)
        {
            var fmt = _getFormat(format.ToCharArray(), out var digits);

            if(fmt != 'X')
                throw new Exception($"int::format('{fmt}') formatting not support.");
            var hexBase = (char)(fmt - ('X' - 'A' + 10));
            return i32toHex(value, hexBase, digits);
        }
        public static string FormatUInt32(uint value, string format, NumberFormatInfo _)
        {
            var fmt = _getFormat(format.ToCharArray(), out var digits);

            if(fmt != 'X')
                throw new Exception($"int::format('{fmt}') formatting not support.");
            var hexBase = (char)(fmt - ('X' - 'A' + 10));
            return u32toHex(value, hexBase, digits);
        }
        public static string FormatUInt64(ulong value, string format, NumberFormatInfo _)
        {
            var fmt = _getFormat(format.ToCharArray(), out var digits);

            if(fmt != 'X')
                throw new Exception($"int::format('{fmt}') formatting not support.");
            var hexBase = (char)(fmt - ('X' - 'A' + 10));
            return u64toHex(value, hexBase, digits);
        }
        public static string FormatInt64(long value, string format, NumberFormatInfo _)
        {
            var fmt = _getFormat(format.ToCharArray(), out var digits);

            if(fmt != 'X')
                throw new Exception($"int::format('{fmt}') formatting not support.");
            var hexBase = (char)(fmt - ('X' - 'A' + 10));
            return i64toHex(value, hexBase, digits);
        }

        public static string FormatDouble(double value, string format, NumberFormatInfo _) 
            => throw new NotImplementedException($"FormatDouble {value}:{format}, {_}");

        public static string FormatSingle(float value, string format, NumberFormatInfo _) 
            => throw new NotImplementedException($"FormatSingle {value}:{format}, {_}");

        private static unsafe string i64toHex(long value, int hexBase, int digits)
        {
            if (digits < 1)
                digits = 1;

            var bufferLength = Math.Max(digits, ((int)Math.Log(value, 2) >> 2) + 1);
            var result = new string('\0', bufferLength);
            fixed (char* buffer = result)
            {
                var p = buffer;
                while (--digits >= 0 || value != 0)
                {
                    byte digit = (byte)(value & 0xF);
                    *(--p) = (char)(digit + (digit < 10 ? (byte)'0' : hexBase));
                    value >>= 4;
                }
            }
            return result;
        }
        private static unsafe string u64toHex(ulong value, int hexBase, int digits)
        {
            if (digits < 1)
                digits = 1;

            var bufferLength = Math.Max(digits, ((int)Math.Log(value, 2) >> 2) + 1);
            var result = new string('\0', bufferLength);
            fixed (char* buffer = result)
            {
                var p = buffer;
                while (--digits >= 0 || value != 0)
                {
                    byte digit = (byte)(value & 0xF);
                    *(--p) = (char)(digit + (digit < 10 ? (byte)'0' : hexBase));
                    value >>= 4;
                }
            }
            return result;
        }
        private static unsafe string u32toHex(uint value, int hexBase, int digits)
        {
            if (digits < 1)
                digits = 1;

            var bufferLength = Math.Max(digits, ((int)Math.Log(value, 2) >> 2) + 1);
            var result = new string('\0', bufferLength);
            fixed (char* buffer = result)
            {
                var p = buffer;
                while (--digits >= 0 || value != 0)
                {
                    byte digit = (byte)(value & 0xF);
                    *(--p) = (char)(digit + (digit < 10 ? (byte)'0' : hexBase));
                    value >>= 4;
                }
            }
            return result;
        }
        private static unsafe string i32toHex(int value, int hexBase, int digits)
        {
            if (digits < 1)
                digits = 1;

            var bufferLength = Math.Max(digits, ((int)Math.Log(value, 2) >> 2) + 1);
            var result = new string('\0', bufferLength);
            fixed (char* buffer = result)
            {
                var p = buffer;
                while (--digits >= 0 || value != 0)
                {
                    byte digit = (byte)(value & 0xF);
                    *(--p) = (char)(digit + (digit < 10 ? (byte)'0' : hexBase));
                    value >>= 4;
                }
            }
            return result;
        }
        internal static char _getFormat(char[] format, out int digits)
        {
            char c = default;
            if (format.Length > 0)
            {
                // If the format begins with a symbol, see if it's a standard format
                // with or without a specified number of digits.
                c = format[0];
                if ((uint)(c - 'A') <= 'Z' - 'A' ||
                    (uint)(c - 'a') <= 'z' - 'a')
                {
                    // Fast path for sole symbol, e.g. "D"
                    if (format.Length == 1)
                    {
                        digits = -1;
                        return c;
                    }

                    if (format.Length == 2)
                    {
                        // Fast path for symbol and single digit, e.g. "X4"
                        int d = format[1] - '0';
                        if ((uint)d < 10)
                        {
                            digits = d;
                            return c;
                        }
                    }
                    else if (format.Length == 3)
                    {
                        // Fast path for symbol and double digit, e.g. "F12"
                        int d1 = format[1] - '0', d2 = format[2] - '0';
                        if ((uint)d1 < 10 && (uint)d2 < 10)
                        {
                            digits = d1 * 10 + d2;
                            return c;
                        }
                    }

                    // Fallback for symbol and any length digits.  The digits value must be >= 0 && <= 99,
                    // but it can begin with any number of 0s, and thus we may need to check more than two
                    // digits.  Further, for compat, we need to stop when we hit a null char.
                    int n = 0;
                    int i = 1;
                    while (i < format.Length && (((uint)format[i] - '0') < 10) && n < 10)
                    {
                        n = (n * 10) + format[i++] - '0';
                    }

                    // If we're at the end of the digits rather than having stopped because we hit something
                    // other than a digit or overflowed, return the standard format info.
                    if (i == format.Length || format[i] == '\0')
                    {
                        digits = n;
                        return c;
                    }
                }
            }

            // Default empty format to be "G"; custom format is signified with '\0'.
            digits = -1;
            return format.Length == 0 || c == '\0' ? // For compat, treat '\0' as the end of the specifier, even if the specifier extends beyond it.
                'G' :
                '\0';
        }
    }
}
