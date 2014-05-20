using System;
using Xunit;

namespace Where.Machine.Core.Tests
{

    public class SystemLogsQueryBuilderTests
    {
        [Fact]
        public void WheresMyBuilder()
        {
            var expectedWhere =
                @"WHERE ((Name = @p1) AND (Age < @p2)) AND (Age > @p3) AND (Name != @p4) AND (TheDate = @p5) AND (WhatIsIt = @p6)";

            var search = new TestSearch
            {
                Name = "Person"
            };

            var builder = new WhereBuilder<TestType>();

            var theDate = DateTime.Now.AddDays(-1);
            builder.Add(x => x.Name == "test" && x.Age < 3);
            builder.Add(x => x.Age > 11);
            builder.Add(x => x.Name != search.Name);

            builder.Add(x => x.TheDate == theDate);
            builder.Add(x => x.WhatIsIt == TestEnum.TheSetting);
            builder.AddParam("@pageIndex", 20);

            Assert.Contains(expectedWhere, builder.Build());

            Assert.Equal(20, builder.GetParamsForTesting()["@pageIndex"]);
            Assert.Equal("test", builder.GetParamsForTesting()["@p1"]);
            Assert.Equal(3, builder.GetParamsForTesting()["@p2"]);
            Assert.Equal(11, builder.GetParamsForTesting()["@p3"]);
            Assert.Equal("Person", builder.GetParamsForTesting()["@p4"]);
            Assert.Equal(theDate, builder.GetParamsForTesting()["@p5"]);
            Assert.Equal(0, builder.GetParamsForTesting()["@p6"]);

        }

    }

    public class TestSearch
    {
        public string Name { get; set; }
    }

    public enum TestEnum
    {
        TheSetting
    }

    public class TestType
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime TheDate { get; set; }
        public TestEnum WhatIsIt { get; set; }
    }
}