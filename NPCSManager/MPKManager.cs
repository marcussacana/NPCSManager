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

        public File[] Open() {
            Seek(0);
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
                Seek(Packget.BaseStream.Position + 8);
                long Lenght = Packget.ReadInt64();
                string Name = GetFileName();
                File Entry = new File() {
                    Path = Name,
                    Content = new FileStream(Packget.BaseStream, Offset, Lenght)
                };
                Entries.Add(Entry);
            }
            return Entries.ToArray();
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

        private void Seek(long i) {
            Packget.BaseStream.Position = i;
            Packget.BaseStream.Flush();
        }
    }

}
