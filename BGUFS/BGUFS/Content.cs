using System;
using System.IO;

namespace BGUFS
{
    [Serializable]
    class Content
    {
        private int deleted;
        private String file;
        private long extraSpace;
        private long size;
        private String hash;
        private long index;
        private long len;

        public Content()
        {
            this.deleted = 0;
            this.extraSpace = 0;
        }

        public Content(string file, long size)
        {
            this.file = file;
            this.deleted = 0;
            this.extraSpace = 0;
            this.size = size;
        }

        public void SetHash(String h)
        {
            this.hash = h;
        }
        public String GetHash()
        {
            return hash;
        }

        public int GetDeleted()
        {
            return this.deleted;
        }
        public void SetDeleted()
        {
            if (this.deleted == 1)
                this.deleted = 0;
            else
                this.deleted = 1;
        }

        public String GetFile()
        {
            return this.file;
        }

        public long getSize()
        {
            return this.size + this.extraSpace;
        }

        public long getSizeFile()
        {
            return this.size;
        }

        public void Setfile(string file1, long size)
        {
            long temp = size;
            this.file = file1;
            this.deleted = 0;
            this.extraSpace = this.size - temp;
            this.size = temp;
        }


    }
}
