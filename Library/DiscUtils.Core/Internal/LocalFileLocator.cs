//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace DiscUtils.Internal
{
    internal sealed class LocalFileLocator : FileLocator
    {
        private readonly string _dir;

        public LocalFileLocator(string dir)
        {
            _dir = dir;
        }

        public override bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(_dir, fileName));
        }

        protected override Stream OpenFile(string fileName, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(Path.Combine(_dir, fileName), mode, access, share);
        }

        public override FileLocator GetRelativeLocator(string path)
        {
            return new LocalFileLocator(Path.Combine(_dir, path));
        }

        public override string GetFullPath(string path)
        {
            string combinedPath = Path.Combine(_dir, path);
            if (string.IsNullOrEmpty(combinedPath))
            {
                return Environment.CurrentDirectory;
            }
            return Path.GetFullPath(combinedPath);
        }

        public override string GetDirectoryFromPath(string path)
        {
#if NETSTANDARD
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.GetDirectoryName(path);
            }
            else
            {
                return path.Replace(path.Split('/').Last(), string.Empty);
            }
#else
            return Path.GetDirectoryName(path);
#endif
        }

        public override string GetFileFromPath(string path)
        {
#if NETSTANDARD
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.GetFileName(path);
            }
            else
            {
                return path.Split('/').Last();
            }
#else
            return Path.GetFileName(path);
#endif
        }

        public override DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(Path.Combine(_dir, path));
        }

        public override bool HasCommonRoot(FileLocator other)
        {
            LocalFileLocator otherLocal = other as LocalFileLocator;
            if (otherLocal == null)
            {
                return false;
            }

            // If the paths have drive specifiers, then common root depends on them having a common
            // drive letter.
            string otherDir = otherLocal._dir;
            if (otherDir.Length >= 2 && _dir.Length >= 2)
            {
                if (otherDir[1] == ':' && _dir[1] == ':')
                {
                    return char.ToUpperInvariant(otherDir[0]) == char.ToUpperInvariant(_dir[0]);
                }
            }

            return true;
        }

        public override string ResolveRelativePath(string path)
        {
            return Utilities.ResolveRelativePath(_dir, path);
        }
    }
}