namespace FleetMind.Api.Configuration
{
    public class FileStorageOptions
    {
        public const string SectionName = "FileStorage";

        public long MaxFileSizeBytes { get; set; }
        public string[] AllowedExtensions { get; set; } = [];
        public string LocalStoragePath { get; set; } = string.Empty;
    }
}
