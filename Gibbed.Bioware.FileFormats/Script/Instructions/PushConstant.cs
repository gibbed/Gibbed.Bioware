/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Bioware.FileFormats.Script.Instructions
{
    [OpcodeHandler(Script.Opcode.CONST)]
    public class PushConstant : IInstruction
    {
        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        public OperandType Operand;
        public object Value;

        public void Encode(byte[] code, ref int offset, out OperandType operand, IState state)
        {
            throw new NotImplementedException();
        }

        public void Decode(byte[] code, ref int offset, OperandType operand, IState state)
        {
            this.Operand = operand;
            switch (operand)
            {
                case OperandType.Integer:
                {
                    this.Value = BitConverter.ToInt32(code, offset + 2).BigEndian();
                    offset += 6;
                    return;
                }

                case OperandType.Float:
                {
                    this.Value = BitConverter.ToSingle(code, offset + 2).BigEndian();
                    offset += 6;
                    return;
                }

                case OperandType.String:
                case OperandType.Resource:
                {
                    var length = BitConverter.ToInt16(code, offset + 2).BigEndian();
                    this.Value = Encoding.UTF8.GetString(code, offset + 4, length);
                    offset += 4 + length;
                    return;
                }

                case OperandType.Object:
                {
                    this.Value = BitConverter.ToInt32(code, offset + 2).BigEndian();
                    offset += 6;
                    return;
                }
            }

            throw new NotSupportedException();
        }

        public void Print(IPrintState state)
        {
            switch (this.Operand)
            {
                case OperandType.Integer:
                case OperandType.Float:
                case OperandType.Object:
                {
                    state.Print("push", this.Operand, this.Value);
                    return;
                }

                case OperandType.String:
                {
                    var value = (string)this.Value;
                    state.Print("push", this.Operand, "\"" + value + "\"");
                    return;
                }

                case OperandType.Resource:
                {
                    var value = (string)this.Value;
                    state.Print("push", this.Operand, "'" + value + "'");
                    return;
                }
            }

            throw new NotSupportedException();
        }
    }
}
