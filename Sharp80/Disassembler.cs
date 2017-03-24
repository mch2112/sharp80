﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp80.Processor
{
    internal static class Disassembler
    {
        public const int NUM_DISASSEMBLY_LINES = 22;

        private static Z80.InstructionSet InstructionSet { get; set; }

        public static bool HistoricDisassemblyMode { get; set; }

        public static void Initialize(Z80.InstructionSet InstructionSet)
        {
            Disassembler.InstructionSet = InstructionSet;
        }
        public static string Disassemble(IReadOnlyList<byte> Memory, ushort Start, ushort End, bool MakeAssemblable)
        {
            ushort PC = Start;
            Instruction inst;

            // Eliminate trailing NOPs
            while (Memory[End] == 0 && End > Start) // NOP
                End--;
            if (End < 0xFFFF)
                End++;

            if (End > Sharp80.Memory.MEMORY_SIZE - 0x10)
                End = Sharp80.Memory.MEMORY_SIZE - 1;

            var sb = new StringBuilder(Sharp80.Memory.MEMORY_SIZE * 40);

            var li = new Dictionary<ushort, Instruction>();

            while (PC <= End)
            {
                li.Add(PC, inst = InstructionSet.GetInstruction(Memory[PC], Memory[PC + 1], Memory[PC + 3]));
                PC += inst.Size;
            }

            var header = $"; Disassembly from memory {Start:X4}H to {End:X4}H" +
                           Environment.NewLine;

            if (MakeAssemblable)
                return header +
                       string.Join(Environment.NewLine, li.Select(i => i.Value.AssemblableName(Memory, i.Key)));
            else
                return header +
                           string.Join(Environment.NewLine, li.Select(i => string.Format("{0}  {1}  {2}",
                                                                                         i.Key.ToHexString(),
                                                                                         Lib.GetSpacedHex(Memory, i.Key, i.Value.Size),
                                                                                         i.Value.FullName(Memory, i.Key)
                                                                                         )));
        }
    }
}