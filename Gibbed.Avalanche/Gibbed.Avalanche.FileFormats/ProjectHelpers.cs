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

using System.IO;

namespace Gibbed.Avalanche.FileFormats
{
    public static class ProjectHelpers
    {
        public static ProjectData.HashList<uint> LoadListsFileNames(
            this ProjectData.Manager manager)
        {
            return manager.LoadLists(
                "*.filelist",
                s => Path.GetFileName(s).HashJenkins(),
                s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<uint> LoadListsFileNames(
            this ProjectData.Project project)
        {
            return project.LoadLists(
                "*.filelist",
                s => Path.GetFileName(s).HashJenkins(),
                s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<uint> LoadListsPropertyNames(
            this ProjectData.Manager manager)
        {
            return manager.LoadLists(
                "*.namelist",
                s => s.HashJenkins(),
                s => s);
        }

        public static ProjectData.HashList<uint> LoadListsPropertyNames(
            this ProjectData.Project project)
        {
            return project.LoadLists(
                "*.namelist",
                s => s.HashJenkins(),
                s => s);
        }
    }
}
