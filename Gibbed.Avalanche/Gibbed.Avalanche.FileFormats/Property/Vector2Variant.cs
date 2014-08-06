﻿/* Copyright (c) 2012 Rick (rick 'at' gibbed 'dot' us)
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
using System.Globalization;

namespace Gibbed.Avalanche.FileFormats.Property
{
    public class Vector2Variant : IVariant
    {
        public float X;
        public float Y;

        public string Tag
        {
            get { return "vec2"; }
        }

        public void Parse(string text)
        {
            var parts = text.Split(',');

            if (parts.Length != 2)
            {
                throw new InvalidOperationException("vec2 requires 2 float values delimited by a comma");
            }

            this.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
            this.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
        }

        public string Compose()
        {
            return string.Format("{0},{1}",
                                 this.X.ToString(CultureInfo.InvariantCulture),
                                 this.Y.ToString(CultureInfo.InvariantCulture));
        }
    }
}
