namespace RecettesIndex.Services;

/// <summary>
/// How long ago, in words.
/// </summary>
/// <remarks>
/// « il y a 3 mois » says more at a glance than a date does, which is what both the
/// Sources list and the dashboard's sleeping-books card are after: not when exactly, but
/// whether it has been a while.
/// </remarks>
public static class RelativeTime
{
    public static string Since(DateTime from, DateTime now)
    {
        var days = (int)(now.Date - from.Date).TotalDays;

        if (days <= 0) return "aujourd'hui";
        if (days == 1) return "hier";
        if (days < 30) return $"il y a {days} jours";

        var months = days / 30;
        if (months < 12)
        {
            return months == 1 ? "il y a 1 mois" : $"il y a {months} mois";
        }

        // Plancher à un an : 364 jours donnent 12 mois par la division précédente, et
        // 364 / 365 donnerait « il y a 0 ans ».
        var years = Math.Max(1, days / 365);
        return years == 1 ? "il y a 1 an" : $"il y a {years} ans";
    }
}
