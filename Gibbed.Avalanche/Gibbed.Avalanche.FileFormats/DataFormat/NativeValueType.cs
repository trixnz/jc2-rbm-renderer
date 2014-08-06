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

namespace Gibbed.Avalanche.FileFormats.DataFormat
{
    public enum NativeValueType : uint
    {
        UInt8 = 0x0CA2821D,
        Int8 = 0x580D0A62,
        UInt16 = 0x86D152BD,
        Int16 = 0xD13FCF93,
        UInt32 = 0x075E4E4F,
        Int32 = 0x192FE633,
        Float = 0x7515A207,
        String = 0x34B4DBF8,
    }
}
