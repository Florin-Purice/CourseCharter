namespace WoTMapWPF
{
    public class PathFileDefinition
    {
        public string? Name { get; set; }
        public string? ImageMD5 { get; set; }
        public Path? Path { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj is not PathFileDefinition)
                return false;
            PathFileDefinition other = (PathFileDefinition)obj;
            if (Name == other.Name &&
                ImageMD5 == other.ImageMD5)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
