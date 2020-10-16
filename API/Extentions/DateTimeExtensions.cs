using System;

namespace API.Extentions
{
  public static class DateTimeExtensions
  {
    public static int CalculateAge(this DateTime dob)
    {
      var today = DateTime.Today;

      var age = today.Year - dob.Year;

      if (dob.Date > today.AddYears(-1)) age--;

      return age;
    }
  }
}