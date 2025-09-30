namespace FitPass.Domain.Strings;

public static class EmailBodies
{
    public static string Welcome(string firstName)
    {
        return $"Hello {firstName}," +
        $"\n\nWe're excited to welcome you to {CommonStrings.AppName}. " +
        "Our goal is simple: make managing your gym passes and memberships effortless, so you can focus on what matters the most - your fitness." +
        $"\n\nWith {CommonStrings.AppName}, you can:" +
        "\n - Store and access all your gym passes in one secure place" +
        "\n - Breeze through check-ins without searching for cards or receipts" +
        "\n - Keep track of active membershups and upcoming renewals" +
        "\n\nGetting started is quick. Add your first pass today, and experience the convenience of having everything ready at your fingeripts." +
        "\n{ButtonPlaceHolder}: Add your first Pass!" +
        $"\n\nThank you for choosing {CommonStrings.AppName} to support your fitness journey. We're glad to have you with us." +
        "\n\nBest Regards," +
        $"\n\nThe {CommonStrings.AppName} Team";
    }
}