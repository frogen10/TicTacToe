namespace Common
{
    public class RoomInfo
    {
        public string RoomName { get; set; }
        public int MemberCount { get; set; }
        public override string ToString()
        {
            return RoomName + " (" + MemberCount + " members)";
        }
    }
}
