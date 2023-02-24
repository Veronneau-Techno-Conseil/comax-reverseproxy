using System.Text;

namespace CommunAxiom.Commons.Client.Hosting.Operator
{
    public class Helper
    {
        public static string Decode(string s) 
        {
            byte[] data = Convert.FromBase64String(s);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static string Encode(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(bytes);
        }
    }
}
