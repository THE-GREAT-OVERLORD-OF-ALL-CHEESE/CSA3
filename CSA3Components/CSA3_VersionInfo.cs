namespace CheeseMods.CSA3Components
{
    public static class CSA3_VersionInfo
    {
        [System.Serializable]
        public struct VersionNumber
        {
            public int major;
            public int minor;
            public int patch;

            public string VersionString => $"{major}.{minor}.{patch}";

            public VersionNumber(int major, int minor, int patch)
            {
                this.major = major;
                this.minor = minor;
                this.patch = patch;
            }
        }

        public static int Major => 3;
        public static int Minor => 0;
        public static int Patch => 5;

        public static VersionNumber CurrentVersion => new VersionNumber(Major, Minor, Patch);
    }
}
