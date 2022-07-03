namespace MonkeyCage.MonkeyBusiness
{
    public class KeyHittingResult
    {
        public KeyHittingResult(string targetText, string textFound, long keyPresses)
        {
            TargetText = targetText;
            TextFound = textFound;
            KeyPresses = keyPresses;
        }

        public string TargetText { get; }
        public string TextFound { get; }
        public long KeyPresses { get; }

        public bool IsSuccess
        {
            get
            {
                return string.Equals(TextFound, TargetText, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
