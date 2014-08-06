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

namespace Gibbed.Avalanche.FileFormats.Property
{
    public static class VariantFactory
    {
        public static IVariant GetVariant(string type)
        {
            switch (type)
            {
                case "int":
                {
                    return new IntegerVariant();
                }

                case "float":
                {
                    return new FloatVariant();
                }

                case "string":
                {
                    return new StringVariant();
                }

                case "vec2":
                {
                    return new Vector2Variant();
                }

                case "vec":
                {
                    return new Vector3Variant();
                }

                case "vec4":
                {
                    return new Vector4Variant();
                }

                case "mat3x3":
                {
                    return new Matrix3x3Variant();
                }

                case "mat":
                {
                    return new Matrix4x3Variant();
                }

                case "mat4x4":
                {
                    return new Matrix4x4Variant();
                }

                case "vec_int":
                {
                    return new IntegersVariant();
                }

                case "vec_float":
                {
                    return new FloatsVariant();
                }
            }

            throw new ArgumentException("unknown variant type", "type");
        }
    }
}
