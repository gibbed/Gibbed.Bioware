/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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
 *    
 */

using System;
using Gibbed.IO;

namespace Gibbed.Bioware.FileFormats.Script.Instructions
{
    [OpcodeHandler(Script.Opcode.JNZ)]
    public class JumpIfNonZero : IInstruction
    {
        public int Size
        {
            get { return 6; }
        }

        public int Destination;

        public void Encode(byte[] code, ref int offset, out OperandType operand, IState state)
        {
            throw new NotImplementedException();
        }

        public void Decode(byte[] code, ref int offset, OperandType operand, IState state)
        {
            if (operand != OperandType.None)
            {
                throw new InvalidOperationException();
            }

            var value = BitConverter.ToInt32(code, offset + 2).BigEndian();
            this.Destination = state.OffsetToIndex(offset + value);
            offset += 6;
        }

        public void Print(IPrintState state)
        {
            state.Print("jnz", state.GetLabel(this.Destination));
        }
    }
}
