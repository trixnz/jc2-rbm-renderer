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
using System.IO;
using System.Linq;

namespace Gibbed.Avalanche.FileFormats
{
    public class ShaderLibraryFile
    {
        public class Shader
        {
            public uint NameHash;
            public string Name;
            public uint DataHash;
            public byte[] Data;

            public override string ToString()
            {
                return string.IsNullOrEmpty(this.Name) == true ? base.ToString() : this.Name;
            }
        }

        public string Name;
        public string BuildTime;
        public List<Shader> VertexShaders = new List<Shader>();
        public List<Shader> FragmentShaders = new List<Shader>();

        public Shader GetVertexShader(string name)
        {
            return this.VertexShaders.SingleOrDefault(
                v => v.Name == name);
        }

        public byte[] GetVertexShaderData(string name)
        {
            var shader = this.GetVertexShader(name);
            if (shader == null)
            {
                return null;
            }
            return shader.Data;
        }

        public Shader GetFragmentShader(string name)
        {
            return this.FragmentShaders.SingleOrDefault(
                v => v.Name == name);
        }

        public byte[] GetFragmentShaderData(string name)
        {
            var shader = this.GetFragmentShader(name);
            if (shader == null)
            {
                return null;
            }
            return shader.Data;
        }

        public void Deserialize(Stream input)
        {
            var libraryFormat = new DataFormatFile();
            using (var libraryFormatData = new MemoryStream(ShaderLibraryResources.BinaryFormat))
            {
                libraryFormat.Deserialize(libraryFormatData);
            }

            var data = new DataFormatFile();
            data.Deserialize(input);

            var root = data.FindEntry(0x1002D37C);
            if (root == null)
            {
                throw new InvalidOperationException("ShaderLibrary entry not present in shader library file");
            }

            // hack :)
            libraryFormat.Endian = data.Endian;

            object library = libraryFormat.ParseEntry(root, input);
            try
            {
                var structure = (DataFormat.Structure)library;
                this.Name = (string)structure.Values["Name"];
                this.BuildTime = (string)structure.Values["BuildTime"];

                this.VertexShaders.Clear();
                foreach (var element in (List<object>)structure.Values["VertexShaders"])
                {
                    var shaderData = (DataFormat.Structure)element;
                    var shader = new Shader
                    {
                        NameHash = (uint)shaderData.Values["NameHash"],
                        Name = (string)shaderData.Values["Name"],
                        DataHash = (uint)shaderData.Values["DataHash"],
                        Data = (byte[])shaderData.Values["BinaryData"]
                    };
                    this.VertexShaders.Add(shader);
                }

                this.FragmentShaders.Clear();
                foreach (var element in (List<object>)structure.Values["FragmentShaders"])
                {
                    var shaderData = (DataFormat.Structure)element;
                    var shader = new Shader
                    {
                        NameHash = (uint)shaderData.Values["NameHash"],
                        Name = (string)shaderData.Values["Name"],
                        DataHash = (uint)shaderData.Values["DataHash"],
                        Data = (byte[])shaderData.Values["BinaryData"]
                    };
                    this.FragmentShaders.Add(shader);
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("something was missing from the parsed data", e);
            }
        }
    }
}
