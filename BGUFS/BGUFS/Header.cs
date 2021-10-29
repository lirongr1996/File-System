using System;

namespace BGUFS
{
    [Serializable]
    class Header
    {
        private String filename;
        private long size;
        private DateTime date;
        private bool link;
        private Header existingfile;
        private String hash;
        private long len;
        private long index;


        public Header(String filename, long size, DateTime date, bool link, Header linkedfile)
        {
            this.filename = filename;
            this.size = size;
            this.date = date;
            this.link = link;
            this.existingfile = linkedfile;
        }

        public void SetLen(long l)
        {
            len = l;
        }
        public long GetLen()
        {
            return len;
        }
        public void SetHash(String h)
        {
            this.hash = h;
        }
        public String GetHash()
        {
            return hash;
        }


        public void SetFileName(String newName)
        {
            this.filename = newName;
        }

        public void Setindex(long i)
        {
            index = i;
        }
        public long GetIndex()
        {
            return index;
        }

        public String GetFileName()
        {
            return this.filename;
        }
        public long GetSize()
        {
            return this.size;
        }

        public void SetSize(long l)
        {
            this.size = l;
        }

        public DateTime GetDate()
        {
            return this.date;
        }
        public bool IsLink()
        {
            return this.link;
        }
        public String GetLinkName()
        {
            return this.existingfile.GetFileName();
        }
        public Header GetLink()
        {
            return this.existingfile;
        }


        public String toString()
        {
            String str = this.filename + "," + this.size + "," + this.date + ",";
            if (this.link == false)
                return str + "regular";
            return str + "link," + this.existingfile.GetFileName();
        }
    }
}
