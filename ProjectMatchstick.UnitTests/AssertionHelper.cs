using ProjectMatchstick.Services.Generation.Steps;
using Xunit.Sdk;

namespace ProjectMatchstick.UnitTests;

public class AssertionHelper
{
    public static void AssertUniqueItems<T>(List<T> list) where T : class
    {
        foreach (var pat1 in list)
        {
            foreach (var pat2 in list)
            {
                if (pat1 == pat2)
                {
                    continue;
                }

                Assert.NotEqual(pat1, pat2);
            }
        }
    }

    public static void AssertContainEqual<T>(List<T> list1, List<T> list2) where T : struct
    {
        Assert.Equal(list1.Count, list2.Count);
        
        var list1Counter = new Dictionary<T, int>();

        foreach (var item in list1)
        {
            if (list1Counter.ContainsKey(item))
            {
                list1Counter[item]++;
            }
            else
            {
                list1Counter[item] = 1;
            }
        }

        foreach (var item in list2)
        {
            if (!list1Counter.TryGetValue(item, out int value))
            {
                throw new XunitException("The lists do not contain the same values.");
            }

            list1Counter[item] = --value;
        }

        foreach (var item in list1)
        {
            if (list1Counter[item] != 0)
            {
                throw new XunitException("The lists do not contain the same values.");
            }
        }
    }
}
