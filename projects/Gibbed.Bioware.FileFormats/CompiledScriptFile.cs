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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.Bioware.FileFormats
{
    public class CompiledScriptFile
    {
        public List<Script.IInstruction> Instructions
            = new List<Script.IInstruction>();

        public void Deserialize(Stream input)
        {
            this.Instructions.Clear();

            var basePosition = input.Position;

            if (input.ReadString(8, Encoding.ASCII) != "NCS V1.0")
            {
                throw new FormatException();
            }
            else if (input.ReadValueU8() != 0x42)
            {
                throw new FormatException();
            }

            var size = input.ReadValueU32(Endian.Big);
            if (basePosition + size > input.Length)
            {
                throw new InvalidOperationException();
            }

            var code = new byte[size - 13];
            if (input.Read(code, 0, code.Length) != code.Length)
            {
                throw new FormatException();
            }

            var count = CountInstructions(code);

            var offsets = new int[count];
            int offset;
            
            offset = 0;
            for (int i = 0; offset >= 0 && offset < code.Length; i++)
            {
                offsets[i] = offset;
                offset += GetInstructionSize(code, offset);
            }

            if (offset != code.Length)
            {
                throw new InvalidOperationException();
            }

            var state = new State(offsets);

            offset = 0;
            var instructions = new List<Script.IInstruction>();
            for (int i = 0; offset >= 0 && offset < code.Length; i++)
            {
                if (code[offset + 0] > 58)
                {
                    throw new InvalidOperationException("invalid opcode");
                }

                var op = (Script.Opcode)code[offset + 0];
                var type = (Script.OperandType)code[offset + 1];

                var instruction = Script.OpcodeHandlerCache
                    .CreateInstruction(op);
                instruction.Decode(code, ref offset, type, state);
                instructions.Add(instruction);
            }

            this.Instructions.AddRange(instructions);
        }

        private static int CountInstructions(byte[] code)
        {
            int count = 0;
            int offset;
            
            for (offset = 0; offset >= 0 && offset < code.Length; )
            {
                offset += GetInstructionSize(code, offset);
                count++;
            }

            if (offset != code.Length)
            {
                throw new InvalidOperationException();
            }

            return count;
        }

        /// <summary>
        /// Get the size of an instruction. This includes the opcode
        /// and operand type.
        /// </summary>
        /// <param name="code">Bytecode</param>
        /// <param name="offset">Offset of the instruction</param>
        /// <returns>Size of instruction</returns>
        private static int GetInstructionSize(byte[] code, int offset)
        {
            if (code[offset + 0] > 58)
            {
                throw new InvalidOperationException("invalid opcode");
            }

            var op = (Script.Opcode)code[offset + 0];
            var type = (Script.OperandType)code[offset + 1];

            switch (op)
            {
                case Script.Opcode.RSADD:
                case Script.Opcode.LOGAND:
                case Script.Opcode.LOGOR:
                case Script.Opcode.INCOR:
                case Script.Opcode.BOOLAND:
                case Script.Opcode.GEQ:
                case Script.Opcode.GT:
                case Script.Opcode.LT:
                case Script.Opcode.LEQ:
                case Script.Opcode.SHLEFT:
                case Script.Opcode.SHRIGHT:
                case Script.Opcode.ADD:
                case Script.Opcode.SUB:
                case Script.Opcode.MUL:
                case Script.Opcode.DIV:
                case Script.Opcode.MOD:
                case Script.Opcode.NEG:
                case Script.Opcode.COMP:
                case Script.Opcode.RETN:
                case Script.Opcode.NOT:
                case Script.Opcode.SAVEBP:
                case Script.Opcode.RESTOREBP:
                case Script.Opcode.NOP:
                {
                    return 2;
                }

                case Script.Opcode.ACTION:
                {
                    return 5;
                }

                case Script.Opcode.MOVSP:
                case Script.Opcode.JMP:
                case Script.Opcode.JSR:
                case Script.Opcode.JZ:
                case Script.Opcode.DECISP:
                case Script.Opcode.INCISP:
                case Script.Opcode.JNZ:
                case Script.Opcode.INCIBP:
                {
                    return 6;
                }

                case Script.Opcode.CPDOWNSP:
                case Script.Opcode.CPTOPSP:
                case Script.Opcode.DESTRUCT:
                case Script.Opcode.CPTOPBP:
                case Script.Opcode.CPSPTOAL:
                case Script.Opcode.CPBPTOAL:
                case Script.Opcode.CPALTOPSP:
                case Script.Opcode.CPALTOPBP:
                case Script.Opcode.U55:
                case Script.Opcode.U57:
                {
                    return 8;
                }

                case Script.Opcode.CONST:
                {
                    switch (type)
                    {
                        case Script.OperandType.Integer:
                        case Script.OperandType.Float:
                        case Script.OperandType.Object:
                        {
                            return 6;
                        }

                        case Script.OperandType.String:
                        case Script.OperandType.Resource:
                        {
                            int length = BitConverter.ToUInt16(code, offset + 2).BigEndian();
                            return 4 + length;
                        }
                    }

                    throw new NotSupportedException("Unhandled operand " + type.ToString());
                }

                case Script.Opcode.EQUAL:
                case Script.Opcode.NEQUAL:
                {
                    if (type == Script.OperandType.StructureStructure)
                    {
                        return 4;
                    }
                    else
                    {
                        return 2;
                    }
                }
            }

            throw new NotSupportedException("Unhandled opcode " + op.ToString());
        }

        private class State : Script.IState
        {
            public int[] Offsets;

            public State(int[] offsets)
            {
                this.Offsets = offsets;
            }

            public int IndexToOffset(int index)
            {
                return this.Offsets[index];
            }

            public int OffsetToIndex(int offset)
            {
                var index = Array.IndexOf<int>(this.Offsets, offset);
                if (index == -1)
                {
                    throw new InvalidOperationException();
                }
                return index;
            }
        }
    }
}
