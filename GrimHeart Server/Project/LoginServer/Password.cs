using System;

public class Password {
    public static String hashPassword(String password) {
        const int WorkFactor = 14;
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public static bool verifyPassword(String password, String passwordFromDB) {
        return BCrypt.Net.BCrypt.Verify(password, passwordFromDB);
    }
}