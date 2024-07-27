using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace SendmessageRcvMsg
{
    ///
    /// This enum is used to indicate what kind of checksum you will be calculating.
    /// 
    public enum CRC8_POLY
    {
        CRC8 = 0xd5,
        CRC8_CCITT = 0x07,
        CRC8_DALLAS_MAXIM = 0x31,
        CRC8_SAE_J1850 = 0x1D,
        CRC_8_WCDMA = 0x9b,
    };

    /// 
    /// Class for calculating CRC8 checksums...
    /// CRC8Calc
    public class CRC
    {
        private byte[] table = new byte[256];

        public byte Checksum(byte[] val)
        {
            if (val == null)
                throw new ArgumentNullException("val");

            byte c = 0;

            foreach (byte b in val)
            {
                c = table[c ^ b];
            }

            return c;
        }

        public byte[] Table
        {
            get
            {
                return this.table;
            }
            set
            {
                this.table = value;
            }
        }

        public byte[] GenerateTable(CRC8_POLY polynomial)
        {
            byte[] csTable = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                int curr = i;

                for (int j = 0; j < 8; ++j)
                {
                    if ((curr & 0x80) != 0)
                    {
                        curr = (curr << 1) ^ (int)polynomial;
                    }
                    else
                    {
                        curr <<= 1;
                    }
                }

                csTable[i] = (byte)curr;
            }

            return csTable;
        }

        public CRC(CRC8_POLY polynomial)
        {
            this.table = this.GenerateTable(polynomial);
        }

        public static int GetByteCount(string hexString)
        {
            int numHexChars = 0;
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    numHexChars++;
            }
            // if odd number of characters, discard last character
            if (numHexChars % 2 != 0)
            {
                numHexChars--;
            }
            return numHexChars / 2; // 2 characters per byte
        }

        //public static byte[] ToByteArray(this BitArray bits)
        //{
        //    int numBytes = bits.Count / 8;
        //    if (bits.Count % 8 != 0) numBytes++;

        //    byte[] bytes = new byte[numBytes];
        //    int byteIndex = 0, bitIndex = 0;

        //    for (int i = 0; i < bits.Count; i++)
        //    {
        //        if (bits[i])
        //            bytes[byteIndex] |= (byte)(1 << (7 - bitIndex));

        //        bitIndex++;
        //        if (bitIndex == 8)
        //        {
        //            bitIndex = 0;
        //            byteIndex++;
        //        }
        //    }

        //    return bytes;
        //}

        ///// <summary>
        ///// Creates a byte array from the hexadecimal string. Each two characters are combined
        ///// to create one byte. First two hexadecimal characters become first byte in returned array.
        ///// Non-hexadecimal characters are ignored. 
        ///// </summary>
        ///// <param name="hexString">string to convert to byte array</param>
        ///// <param name="discarded">number of characters in string ignored</param>
        ///// <returns>byte array, in the same left-to-right order as the hexString</returns>
        public static byte[] GetBytes(string hexString, out int discarded)
        {
            discarded = 0;
            string newString = "";
            char c;
            // remove all none A-F, 0-9, characters
            for (int i = 0; i < hexString.Length; i++)
            {
                c = hexString[i];
                if (IsHexDigit(c))
                    newString += c;
                else
                    discarded++;
            }
            // if odd number of characters, discard last character
            if (newString.Length % 2 != 0)
            {
                discarded++;
                newString = newString.Substring(0, newString.Length - 1);
            }

            int byteLength = newString.Length / 2;
            byte[] bytes = new byte[byteLength];
            string hex;
            int j = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                hex = new String(new Char[] { newString[j], newString[j + 1] });
                bytes[i] = HexToByte(hex);
                j = j + 2;
            }
            return bytes;
        }
        public static string ToString(byte[] bytes)
        {
            string hexString = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                hexString += bytes[i].ToString("X2");
            }
            return hexString;
        }
        /// <summary>
        /// Determines if given string is in proper hexadecimal string format
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static bool InHexFormat(string hexString)
        {
            bool hexFormat = true;

            foreach (char digit in hexString)
            {
                if (!IsHexDigit(digit))
                {
                    hexFormat = false;
                    break;
                }
            }
            return hexFormat;
        }

        /// <summary>
        /// Returns true is c is a hexadecimal digit (A-F, a-f, 0-9)
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>true if hex digit, false if not</returns>
        public static bool IsHexDigit(Char c)
        {
            int numChar;
            int numA = Convert.ToInt32('A');
            int num1 = Convert.ToInt32('0');
            c = Char.ToUpper(c);
            numChar = Convert.ToInt32(c);
            if (numChar >= numA && numChar < (numA + 6))
                return true;
            if (numChar >= num1 && numChar < (num1 + 10))
                return true;
            return false;
        }
        /// <summary>
        /// Converts 1 or 2 character string into equivalant byte value
        /// </summary>
        /// <param name="hex">1 or 2 character string</param>
        /// <returns>byte</returns>
        private static byte HexToByte(string hex)
        {
            if (hex.Length > 2 || hex.Length <= 0)
                throw new ArgumentException("hex must be 1 or 2 characters in length");
            byte newByte = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return newByte;
        }

    }
}
