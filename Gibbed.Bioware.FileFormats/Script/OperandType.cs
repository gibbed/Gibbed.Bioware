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
    public enum OperandType : byte
    {
        None = 0,

        Integer = 3,
        Float = 4,
        String = 5,
        Object = 6,

        User0 = 16,
        User1 = 17,
        User2 = 18,
        User3 = 19,
        User4 = 20,
        User5 = 21,
        User6 = 22,
        User7 = 23,
        User8 = 24,
        User9 = 25,

        IntegerInteger = 32,
        FloatFloat = 33,
        ObjectObject = 34,
        StringString = 35,
        StructureStructure = 36,
        IntegerFloat = 37,
        FloatInteger = 38,

        _EventEvent = 48,
        _LocationLocation = 49,
        _CommandCommand = 50,
        _EffectEffect = 51,
        _ItemPropertyItemProperty = 52,
        _PlayerPlayer = 53,

        VectorVector = 58,
        VectorFloat = 59,
        FloatVector = 60,

        IntegerArray = 64,
        FloatArray = 65,
        StringArray = 66,
        ObjectArray = 67,
        ResourceArray = 68,
        
        User0Array = 80,
        User1Array = 81,
        User2Array = 82,
        User3Array = 83,
        User4Array = 84,
        User5Array = 85,
        User6Array = 86,
        User7Array = 87,
        User8Array = 88,
        User9Array = 89,

        Resource = 96,
    }
}
