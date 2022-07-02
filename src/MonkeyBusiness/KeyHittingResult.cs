namespace MonkeyCage.MonkeyBusiness
{
    public class KeyHittingResult
    {
        public KeyHittingResult(string targetText, string textFound, int keysHit)
        {
            TargetText = targetText;
            TextFound = textFound;
            KeysHit = keysHit;
        }

        public string TargetText { get; }
        public string TextFound { get; }
        public int KeysHit { get; }

        public bool IsSuccess
        {
            get
            {
                return string.Equals(TextFound, TargetText, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
