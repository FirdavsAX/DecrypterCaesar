namespace DecrypterCaesar.Models
{
    public class DecryptionResult
    {
        public string DecryptedText { get; set; }
        public int Shift { get; set; }
        public double VowelPercentage { get; set; }
        public double ConsonantPercentage { get; set; }
    }
}
