using System.ComponentModel;

namespace ConsoleTests;

[MathCore.Generators.Enums.EnumTransformAttribute]
public enum TestEnum
{
    /// <summary>
    /// Значение 1
    /// Строке 2
    /// Строке 3
    /// Строке 4
    /// </summary>
    /// <remarks>Тестовое значение 1</remarks>
    [Description("V1")]
    Value1,
    /// <remarks>Тестовое значение 2</remarks>
    [Description("V21")]
    Value2,
    /// <summary>Значение 2</summary>
    [Description("V321")]
    Value3,
    [Description("V4321")]
    Value4,
}

internal static class TestEnumTest
{
    public static void Test(TestEnum e)
    {
        //TestEnumExtensions
    }
}