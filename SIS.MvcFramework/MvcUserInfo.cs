namespace SIS.MvcFramework
{
    public class MvcUserInfo
    {
        private const string Separator = "___________";

        public MvcUserInfo()
        {
        }

        public MvcUserInfo(string serializedInfo)
        {
            string[] infoParts = serializedInfo.Split(Separator); //правим десериализация
            this.Username = infoParts[0];
            this.Role = infoParts[1];
            this.Info = infoParts[2];
        }

        public string Username { get; set; }

        public string Role { get; set; }

        public string Info { get; set; } //Това ще влезне в бисквитката. Тук слагаме каквото преценим, един вид бонус поленце

        public bool IsLoggedIn => this.Username != null;

        public override string ToString() //правим сериализация
        {
            return $"{this.Username}{Separator}{this.Role}{Separator}{this.Info}";
        }
    }
}