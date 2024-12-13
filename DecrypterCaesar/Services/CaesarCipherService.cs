using DecrypterCaesar.Models;
using System.Text;

namespace DecrypterCaesar.Services
{
    public static class CaesarCipherService
    {
        public static string Encrypt(string text, int key)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return ProcessText(text, key);
        }

        public static string Decrypt(string text, int key)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            return ProcessText(text, -key);
        }

        public static List<DecryptionResult> BruteForceDecrypt(string text)
        {
            var candidates = new List<DecryptionResult>();

            for (int i = 0; i < 26; i++)
            {
                var candidateText = ProcessText(text, -i);
                var (vowelPercentage, consonantPercentage) = AnalyzeText(candidateText);

                candidates.Add(new DecryptionResult
                {
                    DecryptedText = candidateText,
                    Shift = i,
                    VowelPercentage = vowelPercentage,
                    ConsonantPercentage = consonantPercentage
                });
            }

            return candidates;
        }

        public static Dictionary<char, int> FrequencyAnalysis(string text)
        {
            var frequency = new Dictionary<char, int>();

            foreach (char c in text.ToLower())
            {
                if (char.IsLetter(c))
                {
                    if (frequency.ContainsKey(c))
                    {
                        frequency[c]++;
                    }
                    else
                    {
                        frequency[c] = 1;
                    }
                }
            }

            return frequency;
        }

        private static string ProcessText(string text, int key)
        {
            key = key % 26; // Normalize the key to avoid unnecessary rotations
            var result = new StringBuilder();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char offset = char.IsUpper(c) ? 'A' : 'a';
                    result.Append((char)((c + key - offset + 26) % 26 + offset));
                }
                else
                {
                    result.Append(c); // Non-alphabetic characters remain unchanged
                }
            }
            return result.ToString();
        }

        private static (double VowelPercentage, double ConsonantPercentage) AnalyzeText(string text)
        {
            int vowelCount = 0, consonantCount = 0, totalAlpha = 0;
            var vowels = "aeiouAEIOU";

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    totalAlpha++;
                    if (vowels.Contains(c))
                    {
                        vowelCount++;
                    }
                    else
                    {
                        consonantCount++;
                    }
                }
            }

            if (totalAlpha == 0)
            {
                return (0, 0);
            }

            double vowelPercentage = (double)vowelCount / totalAlpha * 100;
            double consonantPercentage = (double)consonantCount / totalAlpha * 100;

            return (vowelPercentage, consonantPercentage);
        }
    }
}
