﻿/*The contents of this file are subject to the Mozilla Public License Version 1.1
(the "License"); you may not use this file except in compliance with the
License. You may obtain a copy of the License at http://www.mozilla.org/MPL/

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
the specific language governing rights and limitations under the License.

The Original Code is the TSOClient.

The Initial Developer of the Original Code is
ddfczm. All Rights Reserved.

Contributor(s): ______________________________________.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TSO.Files.utils;

namespace TSO.Files.formats.iff.chunks
{
    /// <summary>
    /// This chunk type holds the filename of a semi-global iff file used by this object.
    /// </summary>
    public class GLOB : IffChunk
    {
        public string Name;

        /// <summary>
        /// Reads a GLOB chunk from a stream.
        /// </summary>
        /// <param name="iff">An Iff instance.</param>
        /// <param name="stream">A Stream object holding a GLOB chunk.</param>
        public override void Read(Iff iff, Stream stream)
        {
            using (var io = IoBuffer.FromStream(stream, ByteOrder.LITTLE_ENDIAN))
            {
                StringBuilder temp = new StringBuilder();
                var num = io.ReadByte();
                if (num < 48)
                { //less than smallest ASCII value for valid filename character, so assume this is a pascal string
                    temp.Append(io.ReadCString(num));
                }
                else
                { //we're actually a null terminated string!
                    temp.Append((char)num);
                    while (stream.Position < stream.Length)
                    {
                        char read = (char)io.ReadByte();
                        if (read == 0) break;
                        else temp.Append(read);
                    }
                }
                Name = temp.ToString();
            }
        }
    }
}
