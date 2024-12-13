namespace DecrypterCaesar.Models;

public class CaesarCipherModel
{
    public string InputText { get; set; }
    public int Key { get; set; }
    public string? OutputText { get; set; }

    public List<string> BruteForceResults { get; set; } = new List<string>();

    public List<FrequencyAnalysisResult> FrequencyAnalysisResults { get; set; } = new List<FrequencyAnalysisResult>();

    public string? MostLikelyPlainText { get; set; }
    public int MostLikelyShift { get; set; }
}