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
 */

using System;
using Gibbed.IO;

namespace Gibbed.Bioware.FileFormats.Script.Instructions
{
    [OpcodeHandler(Script.Opcode.ACTION)]
    public class CallNativeRoutine : IInstruction
    {
        public int Size
        {
            get { return 5; }
        }

        public short Routine;
        public byte ArgumentCount;

        public void Encode(byte[] code, ref int offset, out OperandType operand, IState state)
        {
            operand = OperandType.None;
            Array.Copy(BitConverter.GetBytes(this.Routine.LittleEndian()), 0, code, offset + 2, 2);
            code[offset + 4] = this.ArgumentCount;
            offset += 5;
        }

        public void Decode(byte[] code, ref int offset, OperandType operand, IState state)
        {
            if (operand != OperandType.None)
            {
                throw new InvalidOperationException();
            }

            this.Routine = BitConverter.ToInt16(code, offset + 2).BigEndian();
            this.ArgumentCount = code[offset + 4];
            offset += 5;
        }

        public void Print(IPrintState state)
        {
            state.Print("syscall", this.Routine, this.ArgumentCount);
        }
    }
}
