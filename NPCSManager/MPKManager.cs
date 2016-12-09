using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NPCSManager {
    public class MPKManager {
        BinaryReader Packget;

        public MPKManager(Stream Input) {
            Packget = new BinaryReader(Input);
        }

        long FirstFile = 0x00;
        public File[] Open() {
            Seek(0);
            FirstFile = Packget.BaseStream.Length;
            if (Packget.ReadInt32() != 0x4B504D)
                throw new Exception("Invalid Format");
            Seek(0x06);
            if (Packget.ReadInt16() != 0x02)
                throw new Exception("Unsuported Version");
            ushort FCount = Packget.ReadUInt16();
            List<File> Entries = new List<File>();
            for (ushort i = 0; i < FCount; i++) {
                Seek(0x48 + (0x100 * i));
                long Offset = Packget.ReadInt64();
                if (Offset < FirstFile)
                    FirstFile = Offset;
                Seek(Packget.BaseStream.Position + 8);
                long Lenght = Packget.ReadInt64();
                string Name = GetFileName();
                File Entry = new File() {
                    Path = Name,
                    Content = new VirtStream(Packget.BaseStream, Offset, Lenght) as Stream
                };
                Entries.Add(Entry);
            }
            return Entries.ToArray();
        }

        public void InjectFiles(File[] Files, Stream Output) {
            if (FirstFile == 0)
                GetFirstFile();
            Seek(0);
            Seek(0, Output);
            CopyRegion(Packget.BaseStream, Output, 0, FirstFile);
            if (Packget.ReadInt32() != 0x4B504D)
                throw new Exception("Invalid Format");
            Seek(0x06);
            if (Packget.ReadInt16() != 0x02)
                throw new Exception("Unsuported Version");
            ushort FCount = Packget.ReadUInt16();
            BinaryWriter Writer = new BinaryWriter(Output);
            long LastFile = FirstFile;
            for (ushort i = 0; i < FCount; i++) {
                Seek(0x60 + (0x100 * i));
                Seek(0x48 + (0x100 * i), Output);
                string Name = GetFileName();
                File File = GetFileByName(Files, Name);
                if (File.Path == null)
                    continue;

                Writer.Write(LastFile);
                long Len = File.Content.Length;
                Writer.Write(Len);
                Writer.Write(Len);
                Seek(LastFile, Output);
                CopyRegion(File.Content, Output, 0, Len);
                LastFile += Len;
            }
            Output.Close();
        }

        private void GetFirstFile() {
            Seek(0);
            FirstFile = Packget.BaseStream.Length;
            Seek(0x06);
            ushort FCount = Packget.ReadUInt16();
            for (ushort i = 0; i < FCount; i++) {
                Seek(0x48 + (0x100 * i));
                long Offset = Packget.ReadInt64();
                if (Offset < FirstFile)
                    FirstFile = Offset;
            }
        }

        private File GetFileByName(File[] files, string name) {
            foreach (File File in files)
                if (File.Path == name)
                    return File;
            return new File() { Path = null };
        }

        private void CopyRegion(Stream Input, Stream Output, long Start, long Length) {
            long Pos = Input.Position;
            byte[] Buffer = new byte[(1024 * 1024) * 10];
            while (Input.Position < Start + Length) {
                int i = Input.Read(Buffer, 0, Buffer.Length);
                Output.Write(Buffer, 0, i);
            }
            Input.Position = Pos;
            Input.Flush();
        }

        private string GetFileName() {
            byte[] Buffer = new byte[0xE0];
            Packget.Read(Buffer, 0, Buffer.Length);
            int Len = 1;
            while (Buffer[Buffer.Length - Len] == 0x00)
                Len++;
            Len = Buffer.Length - Len;
            return Encoding.UTF8.GetString(Buffer, 0, Len + 1);
        }

        private void Seek(long i, Stream Stm = null) {
            if (Stm == null) {
                Packget.BaseStream.Position = i;
                Packget.BaseStream.Flush();
            }else {
                Stm.Position = i;
                Stm.Flush();
            }
        }
    }

}
