using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPCSManager
{
    public partial class NPCSManager
    {
        private byte[] Script;
        public NPCSManager(byte[] Script) {
            this.Script = Script;
        }

        public string[] Import() {                
            if (!(Script[0] == 0x53 && Script[1] == 0x43 && Script[2] == 0x33 && Script[3] == 0x00))//SC3
                throw new Exception("Bad Format");

            List<string> Strings = new List<string>();
            uint OffPos = BitConverter.ToUInt32(Script, 0x04), OffEnd = BitConverter.ToUInt32(Script, 0x08);
            for (; OffPos < OffEnd; OffPos += 4) {
                uint Offset = BitConverter.ToUInt32(GetDW(OffPos), 0);
                int Lenght = ((OffPos + 4 < OffEnd) ? (int)BitConverter.ToUInt32(GetDW(OffPos + 4), 0) : Script.Length) - (int)Offset;
                List <byte> Str = new List<byte>();
                while (Lenght-- > 0)
                    Str.Add(Script[Offset++]);
                string String = DecodeStr(Str.ToArray());
                Strings.Add(String);
            }
            return Strings.ToArray();
        }

        public byte[] Export(string[] Strs) {
            uint OffPos = BitConverter.ToUInt32(Script, 0x04), OffEnd = BitConverter.ToUInt32(Script, 0x08);
            uint BasePos = BitConverter.ToUInt32(GetDW(OffPos), 0);
            byte[] OutScript = CutAt(BasePos);
            List<byte> StrTable = new List<byte>();
            for (uint i = 0; OffPos < OffEnd; OffPos += 4, i++) {
                byte[] Str = EncodeStr(Strs[i]);
                BitConverter.GetBytes((uint)StrTable.Count + BasePos).CopyTo(OutScript, OffPos);
                Append(ref StrTable, Str);
            }
            byte[] Result = new byte[OutScript.LongLength + StrTable.Count];
            OutScript.CopyTo(Result, 0);
            StrTable.ToArray().CopyTo(Result, OutScript.LongLength);
            return Result;
        }

        private void Append(ref List<byte> Arr1, byte[] Arr2) {
            for (int i = 0; i < Arr2.Length; i++)
                Arr1.Add(Arr2[i]);
        }

        private byte[] CutAt(uint Pos) {
            byte[] Out = new byte[Pos];
            for (uint i = 0; i < Pos; i++)
                Out[i] = Script[i];
            return Out;
        }

        const byte OPEN_NAME = 0x01;
        const byte CLOSE_NAME = 0x02;
        const byte END_PAGE = 0x03;
        const byte OPEN_COLOR = 0x04;
        const byte CLOSE_COLOR = 0x00;
        private string DecodeStr(byte[] Bytes) {
            bool InName = false;
            bool HaveName = false;
            bool InColor = false;

            string Name = string.Empty;
            string Dialog = string.Empty;

            for (int i = 0; i < Bytes.Length;) {
                switch (Bytes[i]) {
                    case OPEN_NAME:
                        InName = true;
                        HaveName = true;
                        i++;
                        break;
                    case END_PAGE:
                        i++;
                        if (Bytes[i] == 0xFF)
                            i++;
                        break;
                    case 0xFF:
                    case CLOSE_NAME:
                        InName = false;
                        i++;
                        break;
                    case 0x11://unk
                        Dialog += string.Format("[0x{0:X2}{1:X2}{2:X2}]", Bytes[i], Bytes[i + 1], Bytes[i+2]);
                        i += 3;
                        break;
                    case OPEN_COLOR:
                        InColor = true;
                        Dialog += string.Format("<#{0:X2}{1:X2}{2:X2}", Bytes[i+1], Bytes[i+2], Bytes[i+3]);
                        i += 4;
                        break;
                    case CLOSE_COLOR:
                        InColor = false;
                        Dialog += "#>";
                        i++;
                        break;
                    default:
                        bool d = false;
                        try {
                            char C = Decode(BitConverter.ToInt16(new byte[] { Bytes[i + 1], Bytes[i] }, 0) & 0xFFFF);

                            if (C == '�') {
                                Dialog += string.Format("[0x{0:X2}{1:X2}]", Bytes[i], Bytes[i + 1]);
                            } else if (InName)
                                Name += C;
                            else
                                Dialog += C;
                            i += 2;
                        }
                        catch { d = true; }
                        if (d)
                            System.Diagnostics.Debugger.Break();
                        break;
                }
            }
            if (HaveName)
                return string.Format("[{0}]: {1}", Name, Dialog);
            else
                return Dialog;
        }


        //[Name]: Sample<#FFFFFFColor[0x1122...]#>text
        private byte[] EncodeStr(string Dialog) {
            List<byte> Arr = new List<byte>();
            if (Dialog.StartsWith("[")) {
                int EndsAt = Dialog.IndexOf("]:");
                if (EndsAt < 0)
                    throw new Exception("Bad dialog format");
                string Name = Dialog.Substring(1, EndsAt - 1);
                int Starts = EndsAt + 2;
                if (Dialog[Starts] == ' ')
                    Starts++;
                Arr.Add(OPEN_NAME);
                CompileStr(ref Arr, Name);
                Arr.Add(CLOSE_NAME);
                Dialog = Dialog.Substring(Starts, Dialog.Length - Starts);
            }
            string Cache = string.Empty;
            for (int i = 0; true; ) {check:  ;
                if (!(i < Dialog.Length))
                    break;

                if (EqualsAt(Dialog, i, "<#")) {
                    CompileStr(ref Arr, Cache);
                    Cache = string.Empty;
                    Arr.Add(OPEN_COLOR);
                    i += 2;
                    byte R = Convert.ToByte(Dialog.Substring(i, 2), 16);
                    i += 2;
                    byte G = Convert.ToByte(Dialog.Substring(i, 2), 16);
                    i += 2;
                    byte B = Convert.ToByte(Dialog.Substring(i, 2), 16);
                    i += 2;
                    Arr.Add(R);
                    Arr.Add(G);
                    Arr.Add(B);
                    goto check;
                }
                if (EqualsAt(Dialog, i, "#>")) {
                    i += 2;
                    CompileStr(ref Arr, Cache);
                    Cache = string.Empty;
                    Arr.Add(CLOSE_COLOR);
                    goto check;
                }
                if (EqualsAt(Dialog, i, "[0x")) {
                    i += 3;
                    CompileStr(ref Arr, Cache);
                    Cache = string.Empty;
                    while (Dialog[i] != ']') {
                        byte b = Convert.ToByte(Dialog.Substring(i, 2), 16);
                        Arr.Add(b);
                        i += 2;
                    }
                    i++;
                    goto check;
                }
                Cache += Dialog[i++];
            }
            if (Cache != string.Empty)
                CompileStr(ref Arr, Cache);
            Arr.Add(END_PAGE);
            Arr.Add(0xFF);
            return Arr.ToArray();
        }

        private bool EqualsAt(string Str, int pos, string val) {
            if (pos + val.Length > Str.Length)
                return false;
            return Str.Substring(pos, val.Length) == val;
        }

        private void CompileStr(ref List<byte> Arr, string Str) {
            for (int i = 0; i < Str.Length; i++) {
                byte[] b = BitConverter.GetBytes((ushort)Encode(Str[i]));
                Arr.Add(b[1]);
                Arr.Add(b[0]);
            }
        }

        private byte[] GetDW(uint offPos) =>
            new byte[] { Script[offPos], Script[offPos + 1], Script[offPos + 2], Script[offPos + 3] };
    }
}
