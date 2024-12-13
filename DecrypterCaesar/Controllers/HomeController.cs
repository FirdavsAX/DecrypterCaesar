using DecrypterCaesar.Models;
using DecrypterCaesar.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DecrypterCaesar.Controllers
{
    public class HomeController : Controller
    {
        private static List<(string DecryptedText, int Shift)> _mostLikelyResults;
        private static int _currentIndex = 0;

        // GET: CaesarCipher
        public ActionResult Index()
        {
            _mostLikelyResults = new List<(string DecryptedText, int Shift)>();
            _currentIndex = 0;
            return View(new CaesarCipherModel());
        }

        [HttpPost]
        public ActionResult ProcessText(CaesarCipherModel model, string actionType)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            switch (actionType)
            {
                case "Encrypt":
                    model.OutputText = CaesarCipherService.Encrypt(model.InputText, model.Key);
                    break;

                case "Decrypt":
                    model.OutputText = CaesarCipherService.Decrypt(model.InputText, model.Key);
                    break;

                case "BruteForceDecrypt":
                    model.BruteForceResults = CaesarCipherService
                        .BruteForceDecrypt(model.InputText)
                        .Select(r => $"{r.Shift}: {r.DecryptedText} (Vowels: {r.VowelPercentage}%, Consonants: {r.ConsonantPercentage}%)")
                        .ToList();
                    break;

                case "FrequencyAnalysis":
                    var bruteForceResults = CaesarCipherService.BruteForceDecrypt(model.InputText);

                    // Frequency Analysis
                    var frequencyAnalysis = CaesarCipherService.FrequencyAnalysis(model.InputText);
                    var totalLetters = frequencyAnalysis.Values.Sum();

                    var englishFrequency = new Dictionary<char, double>
                    {
                        { 'E', 12.7 }, { 'T', 9.1 }, { 'A', 8.2 }, { 'O', 7.5 },
                        { 'I', 7.0 }, { 'N', 6.7 }, { 'S', 6.3 }, { 'H', 6.1 },
                        { 'R', 6.0 }, { 'D', 4.3 }, { 'L', 4.0 }, { 'C', 2.8 },
                        { 'U', 2.8 }, { 'M', 2.4 }, { 'W', 2.4 }, { 'F', 2.2 },
                        { 'G', 2.0 }, { 'Y', 2.0 }, { 'P', 1.9 }, { 'B', 1.5 },
                        { 'V', 1.0 }, { 'K', 0.8 }, { 'J', 0.2 }, { 'X', 0.2 },
                        { 'Q', 0.1 }, { 'Z', 0.1 }
                    };

                    // Generate Frequency Analysis Results
                    model.FrequencyAnalysisResults = frequencyAnalysis
                        .Select(f => new FrequencyAnalysisResult
                        {
                            Letter = f.Key,
                            Count = f.Value,
                            Percentage = (double)f.Value / totalLetters * 100,
                            EnglishFrequency = englishFrequency.ContainsKey(char.ToUpper(f.Key)) ? englishFrequency[char.ToUpper(f.Key)] : 0
                        })
                        .ToList();

                    // Calculate similarity scores for brute force results
                    _mostLikelyResults = bruteForceResults
                        .Select(r =>
                        {
                            var resultFrequency = CaesarCipherService.FrequencyAnalysis(r.DecryptedText);
                            double chiSquare = englishFrequency.Sum(ef =>
                            {
                                var letter = ef.Key;
                                var expectedFreq = ef.Value / 100 * r.DecryptedText.Length; // Convert percentage to absolute count
                                var actualFreq = resultFrequency.ContainsKey(char.ToUpper(letter))
                                    ? resultFrequency[char.ToUpper(letter)]
                                    : 0;

                                return expectedFreq > 0
                                    ? Math.Pow(actualFreq - expectedFreq, 2) / expectedFreq
                                    : 0;
                            });

                            return (r.DecryptedText, r.Shift, SimilarityScore: chiSquare);
                        })
                        .OrderBy(r => r.SimilarityScore) // Sort by lowest Chi-Square score
                        .ThenByDescending(r => r.DecryptedText.Count(c => "aeiouAEIOU".Contains(c))) // Tiebreaker: prioritize vowel-rich text
                        .Select(r => (r.DecryptedText, r.Shift))
                        .ToList();

                    // Update model with the most likely plaintext and shift
                    if (_mostLikelyResults.Any())
                    {
                        model.MostLikelyPlainText = _mostLikelyResults[0].DecryptedText;
                        model.MostLikelyShift = _mostLikelyResults[0].Shift;
                    }
                    break;

                case "NextMostLikely":
                    if (_mostLikelyResults.Any())
                    {
                        _currentIndex = (_currentIndex + 1) % _mostLikelyResults.Count;
                        model.MostLikelyPlainText = _mostLikelyResults[_currentIndex].DecryptedText;
                        model.MostLikelyShift = _mostLikelyResults[_currentIndex].Shift;
                    }
                    break;
            }

            return View("Index", model);
        }
    }
}
