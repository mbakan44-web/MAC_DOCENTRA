namespace PromtAiPdfPro.Helpers
{
    public static class NavigationHelper
    {
        /// <summary>
        /// Sayfalar arası taşınacak dosya yolu
        /// </summary>
        public static string? PendingFilePath { get; set; }

        /// <summary>
        /// Hedef sayfada yapılacak alt eylem (Örn: Protect, Unlock, Word, Image vb.)
        /// </summary>
        public static string? TargetAction { get; set; }
    }
}
