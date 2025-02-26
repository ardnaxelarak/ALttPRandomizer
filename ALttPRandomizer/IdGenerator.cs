namespace ALttPRandomizer {
    using System;

    public class IdGenerator {
        private const string chars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int length = 10;
        private static readonly Random random = new Random();

        public string GenerateId() {
            var str = new char[length];

            for (int i = 0; i < length; i++) {
                str[i] = chars[random.Next(chars.Length)];
            }

            return new string(str);
        }
    }
}
