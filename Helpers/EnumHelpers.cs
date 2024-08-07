namespace Helpers;

public static class EnumHelpers
{
    public static string Description(this Enum value)
    {
        var type = value.GetType();
        var member = type.GetMember(value.ToString());

        if (member == null || member.Length <= 0) return value.ToString();

        var attributes = member[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

        if ((attributes != null && attributes.Any()))
        {
            return ((System.ComponentModel.DescriptionAttribute)attributes.ElementAt(0)).Description;
        }

        return value.ToString();
    }
}