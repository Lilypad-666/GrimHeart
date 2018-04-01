using System;
using System.Linq;
using System.Text;

class Session {
    public static string randomString(int length) {
        Random r = new Random();
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[r.Next(s.Length)]).ToArray());
    }
}
