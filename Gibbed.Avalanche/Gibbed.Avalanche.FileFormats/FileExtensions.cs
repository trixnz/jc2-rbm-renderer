/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Gibbed.Avalanche.FileFormats
{
    public class FileExtensions
    {
        public static KeyValuePair<string, string> Detect(byte[] guess, int read)
        {
            if (read == 0)
            {
                return new KeyValuePair<string, string>("unknown", "null");
            }

            if (read >= 2 &&
                guess[0] == 'M' &&
                guess[1] == 'Z')
            {
                return new KeyValuePair<string, string>("executables", "exe");
            }

            if (read >= 2 &&
                guess[0] == 'B' &&
                guess[1] == 'M')
            {
                return new KeyValuePair<string, string>("images", "bmp");
            }

            if (read >= 3 &&
                guess[0] == 'F' &&
                guess[1] == 'S' &&
                guess[2] == 'B')
            {
                return new KeyValuePair<string, string>("sounds", "fsb");
            }

            if (read >= 3 &&
                guess[0] == 'F' &&
                guess[1] == 'E' &&
                guess[2] == 'V')
            {
                return new KeyValuePair<string, string>("sounds", "fev");
            }

            if (read >= 3 &&
                guess[0] == 'D' &&
                guess[1] == 'D' &&
                guess[2] == 'S')
            {
                return new KeyValuePair<string, string>("images", "dds");
            }

            if (read >= 4 &&
                guess[0] == 'C' &&
                guess[1] == 'R' &&
                guess[2] == 'I' &&
                guess[3] == 'D')
            {
                return new KeyValuePair<string, string>("videos", "usm");
            }

            if (read >= 4 &&
                guess[0] == ' ' &&
                guess[1] == 'F' &&
                guess[2] == 'D' &&
                guess[3] == 'A')
            {
                return new KeyValuePair<string, string>("effects", "adf");
            }

            if (read >= 4 &&
                guess[0] == 0x57 &&
                guess[1] == 0xE0 &&
                guess[2] == 0xE0 &&
                guess[3] == 0x57)
            {
                return new KeyValuePair<string, string>("havok", "unknown");
            }

            if (read >= 5 &&
                guess[0] == '<' &&
                guess[1] == '?' &&
                guess[2] == 'x' &&
                guess[3] == 'm' &&
                guess[4] == 'l')
            {
                return new KeyValuePair<string, string>("xml", "xml");
            }

            if (read >= 7 &&
                guess[0] == '<' &&
                guess[1] == 'o' &&
                guess[2] == 'b' &&
                guess[3] == 'j' &&
                guess[4] == 'e' &&
                guess[5] == 'c' &&
                guess[6] == 't')
            {
                return new KeyValuePair<string, string>("bins", "xml");
            }

            if (read >= 8 + 4 &&
                guess[8] == 'C' &&
                guess[9] == 'T' &&
                guess[10] == 'A' &&
                guess[11] == 'B')
            {
                return new KeyValuePair<string, string>("shaders", "unknown");
            }

            if (read >= 3 &&
                (guess[0] == 1 || guess[0] == 2) &&
                (guess[1] == 1 || guess[1] == 4 || guess[1] == 5) &&
                guess[2] == 0)
            {
                return new KeyValuePair<string, string>("bins", "bin");
            }

            if (read >= 8 &&
                guess[4] == 'S' &&
                guess[5] == 'A' &&
                guess[6] == 'R' &&
                guess[7] == 'C')
            {
                return new KeyValuePair<string, string>("archives", "sarc");
            }

            if (read >= 16 &&
                BitConverter.ToUInt32(guess, 0) == 0 &&
                BitConverter.ToUInt32(guess, 4) == 0x1C &&
                Encoding.ASCII.GetString(guess, 8, 8) == "AnarkBGF")
            {
                return new KeyValuePair<string, string>("anark", "agui");
            }

            if (read >= 3 &&
                guess[0] == 'A' &&
                guess[1] == 'D' &&
                guess[2] == 'F')
            {
                return new KeyValuePair<string, string>("anark", "cgui");
            }

            if (read >= 4 &&
                (guess[0] == 9 || guess[0] == 12) &&
                guess[1] == 0 &&
                guess[2] == 0 &&
                guess[3] == 0)
            {
                return new KeyValuePair<string, string>("terrain", "unknown");
            }

            return new KeyValuePair<string, string>("unknown", "unknown");
        }
    }
}
