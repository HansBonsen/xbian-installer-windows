using System;
using System.Collections.Generic;
using System.Text;

namespace installer
{
    class version
    {
        private string versionName;
        private string[] locations;

        public version(string versionName, string[] locations)
        {
            this.versionName = versionName;
            this.locations = locations;
        }

        public string getVersionName()
        {
            return this.versionName;
        }

        public string getArchiveName()
        {
            string versionNameWithoutSpaces = versionName.Replace(" ", string.Empty);
            return versionNameWithoutSpaces + ".img.gz";
        }

        public string getRandomMirror()
        {
            Random random = new Random();
            int i = random.Next(0, this.locations.Length - 1);
            return this.locations[i];
        }
    }
}
