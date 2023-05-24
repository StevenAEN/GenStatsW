using GenStatsW;
using System.Windows;

namespace GenStatsW.Utils
{
    public static class NavigationService
    {
        public static void NavigateTo<T>(Window currentWindow) where T : Window, new()
        {
            Window newWindow = new T();
            newWindow.Show();
            currentWindow.Close();
        }
    }
}
