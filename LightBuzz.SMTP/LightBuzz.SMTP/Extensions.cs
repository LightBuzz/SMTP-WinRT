using Windows.ApplicationModel.Email;

namespace LightBuzz.SMTP
{
    internal static class Extensions
    {
        public static string ToXPriority(this EmailImportance importance)
        {
            switch(importance)
            {
                case EmailImportance.Normal:
                    return "3";
                case EmailImportance.High:
                    return "1";
                case EmailImportance.Low:
                    return "5";
            }

            return null;
        }
    }
}
