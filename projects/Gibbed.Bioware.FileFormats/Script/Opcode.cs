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

namespace Gibbed.Bioware.FileFormats.Script
{
    public enum Opcode : byte
    {
        Invalid = 0,
        CPDOWNSP = 1,
        RSADD = 2,
        CPTOPSP = 3,
        CONST = 4,
        ACTION = 5,
        LOGAND = 6,
        LOGOR = 7,
        INCOR = 8,
        EXCOR = 9,
        BOOLAND = 10,
        EQUAL = 11,
        NEQUAL = 12,
        GEQ = 13,
        GT = 14,
        LT = 15,
        LEQ = 16,
        SHLEFT = 17,
        SHRIGHT = 18,
        USHRIGHT = 19,
        ADD = 20,
        SUB = 21,
        MUL = 22,
        DIV = 23,
        MOD = 24,
        NEG = 25,
        COMP = 26,
        MOVSP = 27,
        STORE_STATEALL = 28,
        JMP = 29,
        JSR = 30,
        JZ = 31,
        RETN = 32,
        DESTRUCT = 33,
        NOT = 34,
        DECISP = 35,
        INCISP = 36,
        JNZ = 37,
        CPDOWNBP = 38,
        CPTOPBP = 39,
        DECIBP = 40,
        INCIBP = 41,
        SAVEBP = 42,
        RESTOREBP = 43,
        STORE_STATE = 44,
        NOP = 45,

        // added in Dragon Age
        U46 = 46,
        U47 = 47,

        // array access
        CPSPTOAL = 48, // foo[1] = bar
        CPBPTOAL = 49,
        CPALTOPSP = 50, // bar = foo[1]
        CPALTOPBP = 51,

        U52 = 52, // equivilent to STORE_STATEALL?

        U53 = 53, // causes the script engine to blow up / infinite loop?

        // added in Dragon Age 2
        U54 = 54,
        U55 = 55,
        U56 = 56,
        U57 = 57,
        U58 = 58,
    }
}
