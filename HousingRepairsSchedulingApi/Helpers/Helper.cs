namespace HousingRepairsSchedulingApi.Helpers;

using System;

public static class Helper
{
    public static void VerifyLengthIsUnder255Characters(string text)
    {
        if (text.Length > 255)
        {
            throw new Exception("Repair description text exceeded 255 character limit");
        }
    }
}
