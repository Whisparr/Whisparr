namespace NzbDrone.Core.Movies
{
    public class Credit
    {
        public string CreditForeignId { get; set; }
        public string Department { get; set; }
        public string Job { get; set; }
        public string Character { get; set; }
        public int Order { get; set; }
        public CreditType Type { get; set; }
        public Performer Performer { get; set; }
    }

    public enum CreditType
    {
        Cast,
        Crew
    }
}
