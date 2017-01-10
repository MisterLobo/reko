#region License
/* 
 * Copyright (C) 1999-2017 John K�ll�n.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using Reko.Core;
using Reko.Core.Machine;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.Text;
using Gee.External.Capstone;
using Gee.External.Capstone.PowerPc;
using CapInstruction = Gee.External.Capstone.PowerPc.PowerPcInstruction;

namespace Reko.Arch.PowerPC
{
    public class PowerPcInstruction : 
        Instruction<CapInstruction, PowerPcRegister, PowerPcInstructionGroup, PowerPcInstructionDetail>,
        MachineInstruction
    {
        private const InstructionClass Transfer = InstructionClass.Transfer;
        private const InstructionClass CondTransfer = InstructionClass.Conditional | InstructionClass.Transfer;
        private const InstructionClass LinkTransfer = InstructionClass.Call | InstructionClass.Transfer;
        private const InstructionClass LinkCondTransfer = InstructionClass.Call | InstructionClass.Transfer | InstructionClass.Conditional;

        private static Dictionary<CapInstruction, InstructionClass> classOf;

		public PowerPcInstruction()
        { 
        }

        public new Address Address  { get { return Core.Address.Ptr32((uint)base.Address); } } //$TODO: how to determine if we're 32- or 64- bit?

        int MachineInstruction.Length { get { return base.Bytes.Length; } }

        public bool IsValid { get { return base.Id != CapInstruction.INVALID; } }

        public int OpcodeAsInteger { get { return (int)Id; } }

        public InstructionClass InstructionClass
        {
            get {
                InstructionClass cl;
                if (!classOf.TryGetValue(Id, out cl))
                    cl = InstructionClass.Linear;
                return cl; 
            }
        }

        public int Operands
        {
            get { return base.ArchitectureDetail.Operands.Length; }
        }
	
        public CapInstruction Opcode
        {
            get { return Id; }
        }

        public MachineOperand op1 { get { return GetOperand(0); } }
        public MachineOperand op2 { get { return GetOperand(1); } }
        public MachineOperand op3 { get { return GetOperand(2); } }
        public MachineOperand op4 { get { return GetOperand(3); } }
        public MachineOperand op5 { get { return GetOperand(4); } }

        public bool Contains(Address addr)
        {
            var uAddr = addr.ToLinear();
            var uThisAddr = addr.ToLinear();
            return uThisAddr <= uAddr && uAddr < uThisAddr + (uint) Bytes.Length;
        }

        public MachineOperand GetOperand(int i)
        {
            //$TODO: how to map from Capstone operands to Reko operands?
            // The line below will throw
            return (MachineOperand)(object)ArchitectureDetail.Operands[i];
        }

        public void Render(MachineInstructionWriter writer)
        {
            writer.WriteOpcode(base.Mnemonic);
            if (base.Operands == 0)
                return;
            writer.Tab();
            var sep = ",";
            foreach (var op in ArchitectureDetail.Operands)
            {

            }
        /* Ye Olde Code. Remove when ready.
            var op = string.Format("{0}{1}", 
                opcode,
                setsCR0 ? "." : "");
            writer.WriteOpcode(op);
            if (op1 != null)
            {
                writer.Tab();
                op1.Write(true, writer);
                if (op2 != null)
                {
                    writer.Write(',');
                    op2.Write(true, writer);
                    if (op3 != null)
                    {
                        writer.Write(',');
                        op3.Write(true, writer);
                        if (op4 != null)
                        {
                            writer.Write(",");
                            op4.Write(true, writer);
                            if (op5 != null)
                            {
                                writer.Write(",");
                                op5.Write(true, writer);
                            }
                        }
                    }
                }
            }
            */
        }

        public override string ToString()
        {
            var sr = new StringRenderer();
            Render(sr);
            return sr.ToString();
        }

        public string ToString(IPlatform platform)
        {
            var sr = new StringRenderer(platform);
            Render(sr);
            return sr.ToString();
        }

        static PowerPcInstruction()
        {
            classOf = new Dictionary<CapInstruction, InstructionClass>
            {
                { CapInstruction.INVALID,   InstructionClass.Invalid },

                { CapInstruction.B,         Transfer },
                { CapInstruction.BC,        CondTransfer },
                { CapInstruction.BCL,       LinkCondTransfer },
                { CapInstruction.BCLR,      Transfer },
                { CapInstruction.BCLRL,     LinkTransfer },
                { CapInstruction.BCCTR,     CondTransfer },
                { CapInstruction.BCTRL,     LinkTransfer },
                { CapInstruction.BDNZ,      CondTransfer },
                { CapInstruction.BDNZF,     CondTransfer },
                { CapInstruction.BDNZFL,    LinkCondTransfer },
                { CapInstruction.BDNZL,     LinkCondTransfer },
                { CapInstruction.BDNZT,     CondTransfer },
                { CapInstruction.BDNZTL,    LinkCondTransfer },
                { CapInstruction.BDZ,       CondTransfer },
                { CapInstruction.BDZF,      CondTransfer },
                { CapInstruction.BDZFL,     LinkCondTransfer },
                { CapInstruction.BDZL,      CondTransfer },
                { CapInstruction.BDZT,      CondTransfer },
                { CapInstruction.BDZTL,     LinkCondTransfer },

                //$REVIEW: smx-smx: remove the comment and find out why this won't work?
                /*
                { CapInstruction.BEQ,       CondTransfer },
                { CapInstruction.BEQL,      LinkCondTransfer },
                { CapInstruction.BEQLR,     CondTransfer },
                { CapInstruction.BEQLRL,    LinkCondTransfer },
                { CapInstruction.BGE,       CondTransfer },
                { CapInstruction.BGEL,      LinkCondTransfer },
                { CapInstruction.BGELR,     CondTransfer },
                { CapInstruction.BGELRL,    LinkCondTransfer },
                { CapInstruction.BGT,       CondTransfer },
                { CapInstruction.BGTL,      LinkCondTransfer },
                { CapInstruction.BGTLR,     CondTransfer },
                { CapInstruction.BGTLRL,    LinkCondTransfer },
                { CapInstruction.BL,        LinkTransfer },
                { CapInstruction.BLE,       CondTransfer },
                { CapInstruction.BLEL,      LinkCondTransfer },
                { CapInstruction.BLELR,     CondTransfer },
                { CapInstruction.BLELRL,    LinkCondTransfer },
                { CapInstruction.BLR,       Transfer },
                { CapInstruction.BLRL,      LinkTransfer },
                { CapInstruction.BLT,       CondTransfer },
                { CapInstruction.BLTL,      LinkCondTransfer },
                { CapInstruction.BLTLR,     CondTransfer },
                { CapInstruction.BLTLRL,    LinkCondTransfer },
                { CapInstruction.BNE,       CondTransfer },
                { CapInstruction.BNEL,      LinkCondTransfer },
                { CapInstruction.BNELR,     CondTransfer },
                { CapInstruction.BNELRL,    LinkCondTransfer },
                { CapInstruction.BNS,       CondTransfer },
                { CapInstruction.BNSL,      LinkCondTransfer },
                { CapInstruction.BNSLR,     CondTransfer },
                { CapInstruction.BNSLRL,    LinkCondTransfer },
                { CapInstruction.BSO,       CondTransfer },
                { CapInstruction.BSOL,      LinkCondTransfer },
                { CapInstruction.BSOLR,     CondTransfer },
                { CapInstruction.BSOLRL,    LinkCondTransfer },
                */
            };
        }
	}

    public class AddressOperand : MachineOperand
    {
        public Address Address;

        public AddressOperand(Address a)
            : base(PrimitiveType.Pointer32)	//$BUGBUG: 64-bit pointers?
        {
            Address = a;
        }

        public override void Write(bool fExplicit, MachineInstructionWriter writer)
        {
            writer.WriteAddress("$" + Address.ToString(), Address);
        }
    }

    public class ConditionOperand : MachineOperand
    {
        public uint condition;

        public ConditionOperand(uint condition) : base(PrimitiveType.Byte)
        {
            this.condition = condition;
        }

        public override void Write(bool fExplicit, MachineInstructionWriter writer)
        {
            if (condition > 3)
                writer.Write("cr{0}+", condition >> 2);
            var s = "";
            switch (condition & 3)
            {
            case 0: s = "lt"; break;
            case 1: s = "gt"; break;
            case 2: s = "eq"; break;
            case 3: s = "so"; break;
            }
            writer.Write(s);
        }
    }
}
