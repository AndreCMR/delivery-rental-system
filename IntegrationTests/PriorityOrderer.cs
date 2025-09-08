using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace IntegrationTests;

public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
    {
        return testCases.OrderBy(tc =>
            tc.TestMethod.Method.GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
              .FirstOrDefault()?.GetNamedArgument<int>("Priority") ?? 0);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestPriorityAttribute : Attribute
    {
        public int Priority { get; }
        public TestPriorityAttribute(int priority) => Priority = priority;
    }
}
